using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using StormLibSharp;
using Mir3DCrypto;

namespace GameLogin
{
    internal sealed class PListFileEntry { public string path { get; set; } = ""; public long size { get; set; } }
    internal sealed class PListPakEntry
    {
        public string pak { get; set; } = "";
        public string pakPath { get; set; } = ""; // e.g. MMOGame\\CookedPC\\00000.pak
        public long pakSize { get; set; }
        public int fileCount { get; set; }
        public long totalSize { get; set; }
        public List<PListFileEntry> files { get; set; } = new List<PListFileEntry>();
    }
    internal sealed class PListRoot
    {
        public string root { get; set; } = "MMOGame";
        public int pakCount { get; set; }
        public List<PListPakEntry> paks { get; set; } = new List<PListPakEntry>();
    }

    internal sealed class PatchRunLogger : IDisposable
    {
        private readonly Form _owner;
        private readonly Label _label;
        private readonly StreamWriter _writer;
        public string LogPath { get; }

        public PatchRunLogger(Form owner, Label label, string patchDir)
        {
            _owner = owner;
            _label = label;

            var logDir = Path.Combine(patchDir, "Logs");
            // Safely creates the Logs folder if missing (no-op if it already exists).  ─ MS docs
            Directory.CreateDirectory(logDir); // :contentReference[oaicite:4]{index=4}

            LogPath = Path.Combine(logDir, $"patch_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            _writer = new StreamWriter(new FileStream(LogPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = true // flush after each WriteLine; disposal also flushes.  ─ MS docs
            };
        }

        public void Write(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            try { _writer.WriteLine(line); } catch { /* ignore disk issues */ }
            UiSet(message);
        }

        public void WriteError(string message, Exception ex)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message} :: {ex.GetType().Name}: {ex.Message}";
            try
            {
                _writer.WriteLine(line);
                _writer.WriteLine(ex.StackTrace);
            }
            catch { /* ignore */ }
            UiSet(message);
        }

        private void UiSet(string text)
        {
            if (_owner == null || _owner.IsDisposed || _label == null || _label.IsDisposed) return;
            _owner.BeginInvoke((Action)(() =>
            {
                _label.Visible = true;
                _label.Text = text;
                _label.Refresh();
            })); // update UI on UI thread  ─ WinForms BeginInvoke
        }

        public void Dispose()
        {
            try { _writer?.Dispose(); } catch { }
        }
    }

    // ---------- Patcher ----------
    internal static class LauncherPatcher
    {
        private static readonly HttpClient Http = new HttpClient();

        // Paths
        private static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;
        private static string PatchDir => Path.Combine(BaseDir, "MMOGame", "Patch");
        private static string TempDir => Path.Combine(PatchDir, "temp_downloads");
        private static string LocalPlistPath => Path.Combine(PatchDir, "PList.json");

        // -------- Public entry points (label + file logging; no MessageBoxes) --------

        public static async Task RunOnStartupAsync(Form owner, Label statusLabel)
        {
            using var logger = new PatchRunLogger(owner, statusLabel, PatchDir);
            logger.Write("Auto Patch: Preparing…");

            if (AppConfig.Current?.Enabled != true)
            {
                logger.Write("Auto Patch: Disabled in config.");
                return;
            }

            await EnsureLocalPListAsync(logger);

            var res = await ApplyPatchAsync(showPrompt: false, keepBackups: AppConfig.Current.KeepBackups, logger: logger);
            await RefreshLocalPlistFromRemoteIfAvailable(res, logger);

            TryDeleteDir(TempDir);

            if (res.TotalChanged == 0)
                logger.Write("Auto Patch: Up to date.");
            else
                logger.Write($"Auto Patch: Finished: {res.Success} Updated, {res.Fail} Failed.");
        }

        public static async Task RunManualAsync(Form owner, Label statusLabel)
        {
            using var logger = new PatchRunLogger(owner, statusLabel, PatchDir);
            logger.Write("Auto Patch: Preparing…");

            await EnsureLocalPListAsync(logger);

            var res = await ApplyPatchAsync(showPrompt: false, keepBackups: AppConfig.Current.KeepBackups, logger: logger);
            await RefreshLocalPlistFromRemoteIfAvailable(res, logger);

            TryDeleteDir(TempDir);

            if (res.TotalChanged == 0)
                logger.Write("Auto Patch: Client already up to date.");
            else
                logger.Write($"Auto Patch: Done: {res.Success} Updated, {res.Fail} Failed.");
        }

        // -------- Core patch --------

        private sealed class PatchResult
        {
            public int Success { get; set; }
            public int Fail { get; set; }
            public int TotalChanged => Success + Fail;
            public string RefreshedRemoteJson { get; set; }  // null if remote plist not fetched
        }

        private static async Task<PatchResult> ApplyPatchAsync(bool showPrompt, bool keepBackups, PatchRunLogger logger)
        {
            var result = new PatchResult();

            // Resolve endpoints (handles folder or direct JSON; ensures trailing slash for folder)
            if (!TryGetPatchEndpoints(out var baseUri, out var plistUri))
            {
                logger.Write("Auto Patch: PatchUrl not set/invalid. Skipping.");
                return result;
            }

            // Clean temp at start
            TryDeleteDir(TempDir);
            Directory.CreateDirectory(TempDir);

            // Load local plist
            if (!File.Exists(LocalPlistPath))
            {
                logger.Write("Auto Patch: Local PList.json not found; skipping patch.");
                return result;
            }

            PListRoot local;
            try
            {
                local = JsonSerializer.Deserialize<PListRoot>(await File.ReadAllTextAsync(LocalPlistPath)) ?? new PListRoot();
            }
            catch (Exception ex)
            {
                logger.WriteError("Auto Patch: Failed to read local PList.json", ex);
                return result;
            }

            // Fetch remote plist (stream-friendly)
            try
            {
                logger.Write($"Auto Patch: Fetching PList.json… ({plistUri})");
                using (var plistResp = await Http.GetAsync(plistUri, HttpCompletionOption.ResponseHeadersRead)) // returns after headers
                {
                    if (!plistResp.IsSuccessStatusCode)
                    {
                        logger.Write($"Auto Patch: Remote PList fetch failed: {(int)plistResp.StatusCode} {plistResp.ReasonPhrase}");
                        return result;
                    }

                    var remoteJson = await plistResp.Content.ReadAsStringAsync();
                    var remote = JsonSerializer.Deserialize<PListRoot>(remoteJson) ?? new PListRoot();

                    // Diff by path + size
                    var localPaks = local.paks.ToDictionary(p => N(p.pakPath), StringComparer.OrdinalIgnoreCase);
                    var toDownload = new List<(PListPakEntry pak, PListFileEntry file)>();

                    foreach (var rpak in remote.paks)
                    {
                        var rKey = N(rpak.pakPath);
                        if (!localPaks.TryGetValue(rKey, out var lpak))
                        {
                            foreach (var f in rpak.files) toDownload.Add((rpak, f));
                            continue;
                        }
                        var lFiles = lpak.files.ToDictionary(f => N(f.path), StringComparer.OrdinalIgnoreCase);
                        foreach (var rf in rpak.files)
                        {
                            var key = N(rf.path);
                            if (!lFiles.TryGetValue(key, out var lf) || lf.size != rf.size)
                                toDownload.Add((rpak, rf));
                        }
                    }

                    if (toDownload.Count == 0)
                    {
                        // Keep local plist in sync anyway
                        result.RefreshedRemoteJson = remoteJson;
                        logger.Write("Auto Patch: No changes found.");
                        return result;
                    }

                    logger.Write($"Auto Patch: Files to update: {toDownload.Count}");

                    var touchedPaks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var (pak, file) in toDownload)
                    {
                        var pakFolderUri = new Uri(baseUri, N(pak.pakPath) + "/");
                        var fileUri = new Uri(pakFolderUri, N(file.path));

                        string tmp = Path.Combine(TempDir, Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(file.path));

                        try
                        {
                            logger.Write($"Auto Patch: Downloading {file.path}…");
                            using (var r = await Http.GetAsync(fileUri, HttpCompletionOption.ResponseHeadersRead))
                            {
                                if (!r.IsSuccessStatusCode)
                                {
                                    result.Fail++;
                                    logger.Write($"Auto Patch: Download Failed: {file.path} ({(int)r.StatusCode} {r.ReasonPhrase})");
                                    continue;
                                }
                                using (var inS = await r.Content.ReadAsStreamAsync())
                                using (var outF = File.Create(tmp))
                                {
                                    await inS.CopyToAsync(outF);
                                }
                            }

                            string pakOnDisk = Path.Combine(BaseDir, D(pak.pakPath));
                            if (!File.Exists(pakOnDisk))
                            {
                                result.Fail++;
                                logger.Write($"Auto Patch: Missing pak on disk: {pakOnDisk}");
                                continue;
                            }

                            // Optional backup
                            if (keepBackups && !touchedPaks.Contains(pakOnDisk))
                            {
                                touchedPaks.Add(pakOnDisk);
                                var bak = pakOnDisk + ".bak";
                                try { if (!File.Exists(bak)) File.Copy(pakOnDisk, bak, false); } catch { }
                            }

                            // Replace inside MPQ (encrypt-on-write; preserve flags)
                            try
                            {
                                byte[] data = await File.ReadAllBytesAsync(tmp);
                                data = Crypto.Encrypt(data);

                                uint flags = 0;
                                using (var archive = new MpqArchive(pakOnDisk, FileAccess.ReadWrite))
                                {
                                    try { using (var existing = archive.OpenFile(D(file.path))) { flags = existing.GetFlags(); } }
                                    catch { /* not present */ }

                                    archive.FileCreateFile(D(file.path), flags, data);
                                }

                                result.Success++;
                                logger.Write($"Auto Patch: Patched {file.path}");
                            }
                            catch (Exception exW)
                            {
                                result.Fail++;
                                logger.WriteError($"Auto Patch: Patch Failed: {file.path}", exW);
                            }
                        }
                        catch (Exception exD)
                        {
                            result.Fail++;
                            logger.WriteError($"Auto Patch: Download Error: {file.path}", exD);
                        }
                        finally
                        {
                            TryDeleteFile(tmp);
                        }
                    }

                    // After patch, overwrite local PList with remote so future diffs match
                    result.RefreshedRemoteJson = remoteJson;
                    logger.Write("Auto Patch: Updating local PList.json…");
                }
            }
            catch (HttpRequestException ex)
            {
                logger.WriteError("Auto Patch: HTTP error fetching PList", ex);
            }
            catch (Exception ex)
            {
                logger.WriteError("Auto Patch: Unexpected patch error", ex);
            }

            return result;
        }

        private static async Task RefreshLocalPlistFromRemoteIfAvailable(PatchResult res, PatchRunLogger logger)
        {
            if (!string.IsNullOrEmpty(res.RefreshedRemoteJson))
            {
                try
                {
                    await File.WriteAllTextAsync(LocalPlistPath, res.RefreshedRemoteJson);
                    logger.Write("Auto Patch: Local PList.json refreshed from remote.");
                }
                catch (Exception ex)
                {
                    logger.WriteError("Auto Patch: Failed writing local PList.json", ex);
                }
            }
        }

        // -------- Ensure / generate local PList when missing --------

        private static async Task EnsureLocalPListAsync(PatchRunLogger logger)
        {
            if (File.Exists(LocalPlistPath)) return;

            logger.Write("Auto Patch: No local PList.json found. Building from local paks…");
            try
            {
                Directory.CreateDirectory(PatchDir); // ensure Patch folder exists  ─ MS docs
                var payload = await BuildPListFromLocalPaksAsync(logger);
                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(LocalPlistPath, json);
                logger.Write("Auto Patch: Local PList.json generated.");
            }
            catch (Exception ex)
            {
                logger.WriteError("Auto Patch: Failed generating local PList.json", ex);
            }
        }

        private static async Task<PListRoot> BuildPListFromLocalPaksAsync(PatchRunLogger logger)
        {
            string mmoRoot = Path.Combine(BaseDir, "MMOGame");

            // ✅ scan all relevant pak roots
            string[] roots = new[]
            {
        Path.Combine(mmoRoot, "CookedPC"),
        Path.Combine(mmoRoot, "Config"),
        Path.Combine(mmoRoot, "Localization")
    }
            .Where(Directory.Exists)
            .ToArray();

            if (roots.Length == 0)
            {
                logger.Write("No MMOGame subfolders found (CookedPC/Config/Localization).");
                return new PListRoot { root = "MMOGame", pakCount = 0, paks = new List<PListPakEntry>() };
            }

            // enumerate .pak files under each root
            var pakFiles = roots.SelectMany(r => Directory.EnumerateFiles(r, "*.pak", SearchOption.AllDirectories))
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToArray();

            logger.Write($"Scanning {pakFiles.Length} pak(s) across {roots.Length} root(s)…");

            var paks = new List<PListPakEntry>();

            foreach (var pakPath in pakFiles)
            {
                try
                {
                    using (var archive = new MpqArchive(pakPath, FileAccess.ReadWrite))
                    {
                        // Read "(listfile)" and enumerate entries
                        string[] entries;
                        using (var lf = archive.OpenFile("(listfile)"))
                        {
                            var buf = new byte[lf.Length];
                            lf.Read(buf, 0, buf.Length);
                            var content = Encoding.UTF8.GetString(buf);
                            entries = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        }

                        var files = new List<PListFileEntry>(entries.Length);
                        long totalSize = 0;

                        foreach (var e in entries)
                        {
                            long sz = (long)archive.GetFileSize(e);
                            files.Add(new PListFileEntry { path = e, size = sz });
                            totalSize += sz;
                        }

                        long pakSize = 0;
                        try { pakSize = new FileInfo(pakPath).Length; } catch { }

                        paks.Add(new PListPakEntry
                        {
                            pak = Path.GetFileName(pakPath),
                            pakPath = MakeRelativeFromMarker(pakPath, "MMOGame"), // e.g. MMOGame\\Config\\00000.pak
                            pakSize = pakSize,
                            fileCount = files.Count,
                            totalSize = totalSize,
                            files = files.OrderBy(f => f.path, StringComparer.OrdinalIgnoreCase).ToList()
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.WriteError($"Failed scanning pak: {pakPath}", ex);
                }
            }

            var root = new PListRoot
            {
                root = "MMOGame",
                pakCount = paks.Count,
                paks = paks.OrderBy(p => p.pakPath, StringComparer.OrdinalIgnoreCase).ToList()
            };

            await Task.CompletedTask; // keep async signature
            return root;
        }

        // -------- Helpers --------

        // Build endpoints robustly: folder or direct JSON; ensure trailing slash for folder to avoid Uri "directory vs file" pitfalls.
        private static bool TryGetPatchEndpoints(out Uri baseFolderUri, out Uri plistUri)
        {
            baseFolderUri = null;
            plistUri = null;

            var raw = AppConfig.Current?.PatchUrl?.Trim();
            if (string.IsNullOrEmpty(raw)) return false;

            // User pointed directly at a JSON file
            if (raw.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                if (!Uri.TryCreate(raw, UriKind.Absolute, out var fileUri)) return false;
                var folder = fileUri.AbsoluteUri.Substring(0, fileUri.AbsoluteUri.LastIndexOf('/') + 1);
                baseFolderUri = new Uri(folder, UriKind.Absolute);
                plistUri = fileUri;
                return true;
            }

            // Treat as folder; make sure it ends with a slash so relative Uri resolves correctly.
            if (!raw.EndsWith("/")) raw += "/";
            if (!Uri.TryCreate(raw, UriKind.Absolute, out baseFolderUri)) return false;

            plistUri = new Uri(baseFolderUri, "PList.json");
            return true;
        }

        private static string N(string p) => (p ?? "").Replace('\\', '/').Trim();
        private static string D(string p) => (p ?? "").Replace('/', '\\');

        private static string MakeRelativeFromMarker(string fullPath, string marker)
        {
            if (string.IsNullOrEmpty(fullPath)) return string.Empty;
            string norm = fullPath.Replace('/', '\\');
            string m = marker.Replace('/', '\\');

            int idx = norm.IndexOf("\\" + m + "\\", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) idx = norm.IndexOf(m, StringComparison.OrdinalIgnoreCase);

            return idx >= 0 ? norm.Substring(idx).TrimStart('\\') : Path.GetFileName(norm);
        }

        private static void TryDeleteFile(string p) { try { if (File.Exists(p)) File.Delete(p); } catch { } }
        private static void TryDeleteDir(string p) { try { if (Directory.Exists(p)) Directory.Delete(p, true); } catch { } }
    }
}
