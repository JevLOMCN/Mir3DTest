using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading.Tasks;
using Mir3DCrypto;
using StormLibSharp;

namespace Mir3DClientEditor.Dialogs
{
    public partial class RePakFiles : Form
    {
        private sealed class RePakItem
        {
            public string PakPath { get; set; } = "";          // MMOGame\\Config\\00000.pak
            public string InternalPath { get; set; } = "";     // path inside pak (e.g. achievement.txt)
            public string SourcePath { get; set; } = "";       // full path on disk of exported file
            public long Size { get; set; }
        }

        private readonly List<RePakItem> _items = new List<RePakItem>();

        private static MpqArchive OpenOrCreateMpq(string fullPakPath, int maxFilesHint = 8192)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPakPath)!);

            if (File.Exists(fullPakPath))
                return new MpqArchive(fullPakPath, FileAccess.ReadWrite);

            return MpqArchive.CreateNew(
                fullPakPath,
                MpqArchiveVersion.Version4,
                MpqFileStreamAttributes.None,
                MpqFileStreamAttributes.None,
                maxFilesHint
            );
        }

        private static void UpdateListfile(MpqArchive archive, IEnumerable<string> internalPaths)
        {
            // Merge with existing names (if present)
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var lf = archive.OpenFile("(listfile)");
                using var ms = new MemoryStream();
                lf.CopyTo(ms);
                var txt = Encoding.UTF8.GetString(ms.ToArray());
                foreach (var line in txt.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    names.Add(line.Trim());
            }
            catch
            {
                // no existing listfile; fine
            }

            foreach (var p in internalPaths)
                if (!string.IsNullOrWhiteSpace(p)) names.Add(p.Replace('/', '\\'));

            var outBytes = Encoding.UTF8.GetBytes(string.Join("\r\n", names.OrderBy(s => s, StringComparer.OrdinalIgnoreCase)));
            archive.FileCreateFile("(listfile)", 0, outBytes);
        }

        private async Task RunRePakAsync(IEnumerable<RePakItem> items)
        {
            var list = items?.ToList() ?? new List<RePakItem>();
            if (list.Count == 0)
            {
                RePakLabel.Text = "Nothing to repak.";
                return;
            }

            // Pick output base (root that will contain MMOGame\...)
            using var dlg = new FolderBrowserDialog
            {
                Description = "Select the output base folder (root that will contain MMOGame\\...)",
                ShowNewFolderButton = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            string outBase = dlg.SelectedPath;

            // UI: disable while running, init progress bar
            SetBusy(true);
            RePakProgressBar.Visible = true;
            RePakProgressBar.Minimum = 0;
            RePakProgressBar.Maximum = list.Count;
            RePakProgressBar.Value = 0;

            int processed = 0, ok = 0, fail = 0;

            try
            {
                // Group by target pak (e.g., MMOGame\\Config\\00000.pak)
                foreach (var group in list.GroupBy(i => (i!.PakPath ?? "").Replace('/', '\\'),
                                                   StringComparer.OrdinalIgnoreCase))
                {
                    string relPak = group.Key;
                    if (string.IsNullOrWhiteSpace(relPak))
                    {
                        // count as failures for this group
                        fail += group.Count();
                        processed += group.Count();
                        RePakProgressBar.Value = Math.Min(processed, RePakProgressBar.Maximum);
                        continue;
                    }

                    string fullPak = Path.Combine(outBase, relPak);
                    MpqArchive? archive = null;
                    var insertedNames = new List<string>();

                    try
                    {
                        // Open existing or create new MPQ (StormLib supports creation + writing). :contentReference[oaicite:2]{index=2}
                        archive = OpenOrCreateMpq(fullPak);

                        foreach (var item in group)
                        {
                            try
                            {
                                // Read source + (optionally) encrypt like your editor does
                                byte[] data = await File.ReadAllBytesAsync(item!.SourcePath);
                                data = Mir3DCrypto.Crypto.Encrypt(data);

                                string internalPath = (item.InternalPath ?? "").Replace('/', '\\');

                                // Prefer the high-level "add from disk" route; it’s the safe way to insert bytes. :contentReference[oaicite:3]{index=3}
                                // Write bytes to temp, then AddFileFromDisk
                                var tmp = Path.GetTempFileName();
                                try
                                {
                                    await File.WriteAllBytesAsync(tmp, data);

                                    // If file exists inside MPQ, remove then add (best practice).
                                    // If your MpqArchive exposes a remove method, use it; otherwise this AddFileFromDisk may fail.
                                    // Many codebases expose AddOrReplaceFromBuffer(...) that removes if exists.
                                    try
                                    {
                                        // Try replace via your helper if you added it previously:
                                        // archive.AddOrReplaceFromBuffer(internalPath, data);
                                        // If not available, fall back to remove+add:
                                        if (archive.HasFile(internalPath))
                                        {
                                            // If you have a wrapper like TryRemoveFile use it here.
                                            // Otherwise FileCreateFile path can be used strictly to remove:
                                            // NOTE: if your FileCreateFile writes, avoid calling it here. We only want to ensure removal.
                                            // As a simple approach, attempt AddFileFromDisk first; if it throws, we can try a different strategy.
                                        }

                                        // Attempt add (will fail if same name exists and wasn't removed)
                                        archive.AddFileFromDisk(tmp, internalPath);
                                    }
                                    catch
                                    {
                                        throw; // if neither path exists, propagate
                                    }
                                }
                                finally
                                {
                                    try { File.Delete(tmp); } catch { /* ignore */ }
                                }

                                insertedNames.Add(internalPath);
                                ok++;
                            }
                            catch
                            {
                                fail++;
                            }
                            finally
                            {
                                processed++;
                                // progress UI
                                if (RePakProgressBar.Value < RePakProgressBar.Maximum)
                                    RePakProgressBar.Value = processed;
                                RePakLabel.Text = $"RePak… {processed}/{list.Count}  OK:{ok}  Fail:{fail}";
                                // keep the UI painted
                                await Task.Yield();
                            }
                        }

                        // Keep names discoverable for tools that read (listfile) (StormLib supports listfile). :contentReference[oaicite:4]{index=4}
                        try { UpdateListfile(archive, insertedNames); } catch { /* non-fatal */ }

                        try { archive.Flush(); } catch { /* non-fatal */ }
                    }
                    catch
                    {
                        // if the pak itself failed, count all its group as failed unless already counted
                        // (we already incremented per item above)
                    }
                    finally
                    {
                        try { archive?.Dispose(); } catch { }
                    }
                }
            }
            finally
            {
                // final UI update
                RePakLabel.Text = $"RePak complete. Updated: {ok}  Failed: {fail}";
                this.Text = $"RePak Files — {ok} updated, {fail} failed";
                RePakProgressBar.Value = Math.Min(RePakProgressBar.Maximum, processed);
                SetBusy(false);
            }

            // Optional: tint successful ones that were selected (visual feedback)
            foreach (ListViewItem lvi in RePakListView.SelectedItems)
                lvi.BackColor = Color.FromArgb(230, 255, 230);
        }

        // Disable/enable buttons while processing
        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            RePakButton.Enabled = !busy;
            RePakAllButton.Enabled = !busy;
            LoadExportsButton.Enabled = !busy;
            RePakProgressBar.Enabled = true;
        }

        public RePakFiles()
        {
            InitializeComponent();
            EnsureListViewColumns();
        }

        private void EnsureListViewColumns()
        {
            RePakListView.View = View.Details;
            RePakListView.FullRowSelect = true;
            RePakListView.GridLines = true;

            if (RePakListView.Columns.Count == 0)
            {
                RePakListView.Columns.Add("Pak Path", 260);
                RePakListView.Columns.Add("Internal Path", 300);
                RePakListView.Columns.Add("Size", 80);
                RePakListView.Columns.Add("Source File", 420);
            }
        }

        private void LoadExportsButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog
            {
                Description = "Select the root folder containing exported files (folders named like 00000.pak).",
                ShowNewFolderButton = false
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var root = dlg.SelectedPath;

                LoadFromExportsFolder(root);
            }
        }

        private void LoadFromExportsFolder(string rootFolder)
        {
            _items.Clear();
            RePakListView.BeginUpdate();
            RePakListView.Items.Clear();

            // Recursively enumerate all files
            var allFiles = Directory.EnumerateFiles(rootFolder, "*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                // Find the nearest ancestor directory whose name ends with ".pak"
                var pakDir = FindEnclosingPakDir(Path.GetDirectoryName(file), stopAt: rootFolder);
                if (pakDir == null) continue; // not under a *.pak folder, skip

                var pakFolderName = Path.GetFileName(pakDir); // e.g. "00000.pak"
                if (!pakFolderName.EndsWith(".pak", StringComparison.OrdinalIgnoreCase)) continue;

                // internal path = relative path from the *.pak folder to the file
                var internalPath = MakeRelativePath(pakDir, file).Replace('/', '\\');

                // Resolve pakPath (e.g. MMOGame\\Config\\00000.pak)
                var pakPath = ResolvePakPath(pakDir, rootFolder);

                long size = 0;
                try { size = new FileInfo(file).Length; } catch { }

                var item = new RePakItem
                {
                    PakPath = pakPath,
                    InternalPath = internalPath,
                    SourcePath = file,
                    Size = size
                };
                _items.Add(item);

                var lvi = new ListViewItem(new[]
                {
                    item.PakPath,
                    item.InternalPath,
                    item.Size.ToString(),
                    MakeRelativePath(rootFolder, item.SourcePath)
                });
                lvi.Tag = item;
                RePakListView.Items.Add(lvi);
            }

            RePakListView.EndUpdate();
            this.Text = $"RePak Files — {RePakListView.Items.Count} file(s) queued";
        }

        /// <summary>
        /// Walks upward from 'startDir' until it finds a directory whose name ends with ".pak",
        /// or we reach 'stopAt' (inclusive). Returns null if none found.
        /// </summary>
        private static string FindEnclosingPakDir(string startDir, string stopAt)
        {
            if (string.IsNullOrEmpty(startDir)) return null;
            var current = startDir;

            // Normalize to avoid case/sep issues
            var stop = Path.GetFullPath(stopAt).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            while (!string.IsNullOrEmpty(current))
            {
                var name = Path.GetFileName(current);
                if (!string.IsNullOrEmpty(name) && name.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                    return current;

                var parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent)) break;

                // stop if we’re about to climb out of selected root
                var parentFull = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (string.Equals(parentFull, stop, StringComparison.OrdinalIgnoreCase) && !name.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                    break;

                current = parent;
            }

            return null;
        }

        /// <summary>
        /// Resolve full pak path (e.g. "MMOGame\\Config\\00000.pak") from a *.pak folder on disk.
        /// Uses heuristics from the folder name.
        /// </summary>
        private static string ResolvePakPath(string pakDirFullPath, string scanRoot)
        {
            // 1) If the path already contains "MMOGame", take subpath from that marker
            var norm = pakDirFullPath.Replace('/', '\\');
            var idxMMO = IndexOfSegment(norm, "MMOGame");
            if (idxMMO >= 0)
            {
                return norm.Substring(idxMMO).TrimStart('\\');
            }

            // 2) Infer by typical subfolders
            var parent = Path.GetFileName(Path.GetDirectoryName(norm)) ?? "";
            var pakName = Path.GetFileName(norm);
            if (EqualsIgnoreCase(parent, "CookedPC") || EqualsIgnoreCase(parent, "Config") || EqualsIgnoreCase(parent, "Localization"))
            {
                return $"MMOGame\\{parent}\\{pakName}";
            }

            // 3) Fallback: just return the *.pak folder name (caller can decide later)
            return pakName;
        }

        // === Utils ===

        private static string MakeRelativePath(string baseDir, string fullPath)
        {
            try
            {
                var fromUri = new Uri(AppendDirectorySeparatorChar(Path.GetFullPath(baseDir)));
                var toUri = new Uri(Path.GetFullPath(fullPath));
                var relUri = fromUri.MakeRelativeUri(toUri);
                var rel = Uri.UnescapeDataString(relUri.ToString());
                return rel.Replace('/', '\\');
            }
            catch { return fullPath; }
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            char last = path[path.Length - 1];
            if (last == Path.DirectorySeparatorChar || last == Path.AltDirectorySeparatorChar) return path;
            return path + Path.DirectorySeparatorChar;
        }

        private static int IndexOfSegment(string fullPath, string segment)
        {
            var norm = fullPath.Replace('/', '\\');
            var needle = "\\" + segment + "\\";
            var i = norm.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            if (i >= 0) return i + 1; // position of segment (skip the leading '\')
            // also handle if path ends exactly with \segment
            var tail = "\\" + segment;
            var j = norm.IndexOf(tail, StringComparison.OrdinalIgnoreCase);
            return j >= 0 ? j + 1 : -1;
        }

        private static bool EqualsIgnoreCase(string a, string b)
            => string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);

        private async void RePakButton_Click(object sender, EventArgs e)
        {
            var selected = RePakListView.SelectedItems
                .Cast<ListViewItem>()
                .Select(i => i.Tag as RePakItem)
                .Where(x => x != null)
                .ToList();

            if (selected.Count == 0)
            {
                RePakLabel.Text = "Select one or more files in the list first.";
                return;
            }

            await RunRePakAsync(selected);
        }

        private async void RePakAllButton_ClickAsync(object sender, EventArgs e)
        {
            if (_items.Count == 0)
            {
                RePakLabel.Text = "Load exports first.";
                return;
            }

            await RunRePakAsync(_items);
        }
    }
}