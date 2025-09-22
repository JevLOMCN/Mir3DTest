using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.Json;
using Mir3DClientEditor.FormValueEditors;
using StormLibSharp;

namespace Mir3DClientEditor.Dialogs
{
    public partial class ExportFiles : Form
    {
        private string? _selectedRootFolder;
        private readonly MPQExplorerControl? _mpq;
        private enum ViewMode { PakList, PakContents }
        private ViewMode _viewMode = ViewMode.PakList;
        private ArchiveHandle? _currentPak;
        private ColumnHeader? ColPakSize;

        private sealed class ArchiveHandle
        {
            public string FilePath = "";
            public MpqArchive Archive = null!;
            public List<MpqFileEntry> Files = new();
        }

        private sealed class MpqFileEntry
        {
            public string Path = "";
            public long Size;
        }

        private readonly List<ArchiveHandle> _archives = new();
        private string _filterText = "";
        private int _sortColumn = 0;
        private bool _sortAsc = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MPQExplorerControl? MpqControl { get; set; }

        public ExportFiles()
        {
            InitializeComponent();
            InitListView();
        }

        public ExportFiles(MPQExplorerControl mpq) : this()
        {
            _mpq = mpq ?? throw new ArgumentNullException(nameof(mpq));
            MpqControl = _mpq;
        }

        #region InitListView
        private void InitListView()
        {
            FilesListView.View = View.Details;
            FilesListView.FullRowSelect = true;
            FilesListView.GridLines = true;

            if (FilesListView.Columns.Count == 0)
            {
                FilesListView.Columns.Add("File", 450);
                FilesListView.Columns.Add("Pak", 260);
                FilesListView.Columns.Add("Size", 100, HorizontalAlignment.Right);
            }
        }
        #endregion

        #region Set Status
        private void SetExportMessage(string text)
        {
            if (ExportLabel == null) return;
            if (ExportLabel.IsHandleCreated)
                ExportLabel.BeginInvoke(new Action(() => ExportLabel.Text = text));
        }
        #endregion

        #region Files List View Click
        private void FilesListView_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (_sortColumn == e.Column) _sortAsc = !_sortAsc;
            else { _sortColumn = e.Column; _sortAsc = true; }

            if (_viewMode == ViewMode.PakList) ShowPakList();
            else if (_currentPak != null) ShowPakContents(_currentPak);
        }
        #endregion

        #region Clear Archives
        private void ClearArchives()
        {
            foreach (var a in _archives)
            {
                try { a.Archive?.Dispose(); } catch { /* ignore */ }
            }
            _archives.Clear();
        }
        #endregion

        #region Load Archives
        private async Task LoadArchivesAsync(IEnumerable<string> pakPaths)
        {
            ClearArchives();
            FilesListView.BeginUpdate();
            FilesListView.Items.Clear();
            FilesListView.EndUpdate();

            var paths = pakPaths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (paths.Length == 0)
            {
                SetExportMessage("No .pak files found.");
                return;
            }

            Cursor = Cursors.WaitCursor;
            SetExportMessage("Loading paks...");

            var errors = new List<string>();

            try
            {
                await Task.Run(() =>
                {
                    foreach (var pak in paths)
                    {
                        try
                        {
                            var h = new ArchiveHandle
                            {
                                FilePath = pak,
                                Archive = new MpqArchive(pak, FileAccess.Read) // use ReadWrite if you plan edits
                            };

                            var list = new List<MpqFileEntry>();
                            try
                            {
                                using var s = h.Archive.OpenFile("(listfile)");
                                var buf = new byte[s.Length];
                                s.Read(buf, 0, buf.Length);

                                var text = Encoding.UTF8.GetString(buf);

                                foreach (var line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var fileInsidePak = line.Replace('/', '\\');

                                    long size = 0;
                                    try
                                    {
                                        size = h.Archive.GetFileSize(fileInsidePak);
                                    }
                                    catch
                                    {
                                        // Some packers omit sizes — ignore.
                                    }

                                    list.Add(new MpqFileEntry
                                    {
                                        Path = fileInsidePak,
                                        Size = size
                                    });
                                }
                            }
                            catch (Exception exList)
                            {
                                // Pak without (listfile) — keep the pak, with zero files
                                errors.Add($"{Path.GetFileName(pak)}: {exList.Message}");
                            }

                            h.Files = list;
                            lock (_archives) _archives.Add(h);
                        }
                        catch (Exception exOpen)
                        {
                            errors.Add($"{Path.GetFileName(pak)}: {exOpen.Message}");
                        }
                    }
                });

                ShowPakList();

                var fileCount = _archives.Sum(a => a.Files.Count);
                SetExportMessage($"Loaded {paths.Length} pak(s), {fileCount} file(s).");

                if (errors.Count > 0)
                    MessageBox.Show(string.Join(Environment.NewLine, errors), "Some archives failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        #endregion

        #region Select Pak Button
        private async void BtnSelectPak_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Mir3D Pak (*.pak)|*.pak",
                Multiselect = true
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            _selectedRootFolder = null; // loose paks, so no root
            await LoadArchivesAsync(ofd.FileNames);
        }
        #endregion

        #region Select Pak Folder Button
        private async void BtnSelectFolder_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Select a folder that contains .pak files"
            };
            if (fbd.ShowDialog(this) != DialogResult.OK) return;

            _selectedRootFolder = fbd.SelectedPath; // will be used for relative paths and payload.root
            var paks = Directory.EnumerateFiles(fbd.SelectedPath, "*.pak", SearchOption.AllDirectories);
            await LoadArchivesAsync(paks);
        }
        #endregion

        private static string NormalizeDir(string? s)
        {
            if (string.IsNullOrEmpty(s) || s == ".") return string.Empty;
            return s!.Replace('/', '\\');
        }

        #region Export Button
        private async void ExportBtn_Click(object? sender, EventArgs e)
        {
            // --- Local helpers (thread-safe UI updates) ---
            void SetExportMessage(string text)
            {
                if (ExportLabel == null) return;
                if (ExportLabel.IsHandleCreated)
                    ExportLabel.BeginInvoke(new Action(() => ExportLabel.Text = text));
            }

            void SetExportLabelPath(string pakRelPath, string filePath, int processed, int total)
            {
                if (ExportLabel == null) return;
                string displayPath = $"{pakRelPath.Replace('\\', '/')}/{filePath.Replace('\\', '/')}";
                string msg = $"Current File: {displayPath} Total: {processed}/{total}";
                if (ExportLabel.IsHandleCreated)
                    ExportLabel.BeginInvoke(new Action(() => ExportLabel.Text = msg));
            }
            // ------------------------------------------------

            ExportBtn.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Value = 0;
            progressBar1.Maximum = 100;
            SetExportMessage(string.Empty); // clean start

            try
            {
                if (_archives.Count > 0)
                {
                    // Decide which archives to export
                    var selectedArchives = new List<ArchiveHandle>();

                    if (_viewMode == ViewMode.PakList && FilesListView.SelectedItems.Count > 0)
                    {
                        selectedArchives = FilesListView.SelectedItems
                            .Cast<ListViewItem>()
                            .Select(i => i.Tag as ArchiveHandle)
                            .Where(h => h != null)
                            .Distinct()
                            .ToList()!;
                    }
                    else if (_viewMode == ViewMode.PakContents && _currentPak != null)
                    {
                        selectedArchives.Add(_currentPak);
                    }
                    else
                    {
                        selectedArchives.AddRange(_archives);
                    }

                    if (selectedArchives.Count == 0)
                    {
                        MessageBox.Show("No paks selected to export.");
                        return;
                    }

                    using var fbd = new FolderBrowserDialog
                    {
                        Description = $"Choose a folder to export {selectedArchives.Count} pak(s)"
                    };
                    if (fbd.ShowDialog(this) != DialogResult.OK) return;

                    var exportRoot = fbd.SelectedPath;

                    int total = selectedArchives.Sum(a => a.Files?.Count ?? 0);
                    int processed = 0;

                    await Task.Run(() =>
                    {
                        foreach (var a in selectedArchives)
                        {
                            // Relative bits for output + label
                            var relFromMarker = MPQExplorerControl.MakeRelativeFromMarker(a.FilePath, "MMOGame");  // e.g. "MMOGame\\CookedPC\\00001.pak"
                            var relPakParent = Path.GetDirectoryName(relFromMarker) ?? string.Empty;              // e.g. "MMOGame\\CookedPC"
                            var pakFolderName = Path.GetFileName(a.FilePath);                                     // e.g. "00001.pak"

                            foreach (var f in a.Files)
                            {
                                try
                                {
                                    using var s2 = a.Archive.OpenFile(f.Path);
                                    var data = new byte[s2.Length];
                                    s2.Read(data, 0, data.Length);

                                    // decrypt if needed
                                    data = Mir3DCrypto.Crypto.Decrypt(data);

                                    // Preserve internal path structure
                                    var internalPath = f.Path.Replace('/', '\\');

                                    // FINAL OUTPUT: <exportRoot>\MMOGame\CookedPC\00001.pak\<internalPath>
                                    var outFullPath = Path.Combine(exportRoot, relPakParent, pakFolderName, internalPath);
                                    Directory.CreateDirectory(Path.GetDirectoryName(outFullPath)!);
                                    File.WriteAllBytes(outFullPath, data);
                                }
                                catch
                                {
                                    // optionally log/collect errors
                                }
                                finally
                                {
                                    processed++;
                                    var pct = total > 0 ? Math.Min(100, (int)((long)processed * 100 / total)) : 0;

                                    if (progressBar1.IsHandleCreated)
                                    {
                                        progressBar1.BeginInvoke(new Action(() =>
                                        {
                                            progressBar1.Value = pct;
                                        }));
                                    }

                                    // Update the single status label with your required format
                                    var pakRelForLabel = Path.Combine(relPakParent, pakFolderName); // e.g. "MMOGame\\CookedPC\\00001.pak"
                                    SetExportLabelPath(pakRelForLabel, f.Path, processed, total);
                                }
                            }
                        }
                    });

                    // Write PList.json next to the exported files (only for the selected paks)
                    if (ExportPListCheckBox.Checked)
                    {
                        try
                        {
                            var paks = selectedArchives
                                .Select(h =>
                                {
                                    var pakPathRel = MPQExplorerControl.MakeRelativeFromMarker(h.FilePath, "MMOGame");
                                    long pakSize = 0; try { pakSize = new FileInfo(h.FilePath).Length; } catch { }

                                    var files = (h.Files ?? new List<MpqFileEntry>())
                                        .Select(ff => new { path = ff.Path, size = ff.Size })
                                        .OrderBy(x => x.path, StringComparer.OrdinalIgnoreCase)
                                        .ToArray();

                                    return new
                                    {
                                        pak = Path.GetFileName(h.FilePath),
                                        pakPath = pakPathRel,
                                        pakSize = pakSize,
                                        fileCount = files.Length,
                                        totalSize = files.Aggregate(0L, (sum, x) => sum + x.size),
                                        files
                                    };
                                })
                                .OrderBy(p => p.pakPath, StringComparer.OrdinalIgnoreCase)
                                .ToArray();

                            // Root = first segment from first pakPath (fallback "MMOGame")
                            var rootName = "MMOGame";
                            if (paks.Length > 0)
                            {
                                var first = paks[0].pakPath;
                                var sep = first.IndexOf('\\');
                                if (sep > 0) rootName = first.Substring(0, sep);
                            }

                            var payload = new { root = rootName, pakCount = paks.Length, paks };
                            var json = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(Path.Combine(exportRoot, "PList.json"), json, System.Text.Encoding.UTF8);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to write PList.json:\n{ex.Message}", "PList",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    SetExportMessage("Export complete.");
                }
                else if (MpqControl != null)
                {
                    // Export via the main MPQ viewer (all from that control)
                    var progress = new Progress<MPQExplorerControl.ExportProgress>(p =>
                    {
                        if (p.Total > 0)
                        {
                            int pct = (int)Math.Min(100L, (long)p.Processed * 100 / p.Total);
                            progressBar1.Value = Math.Max(0, Math.Min(100, pct));
                        }

                        // Keep the single label in sync with required format
                        SetExportLabelPath(p.CurrentPak ?? "", p.CurrentFile ?? "", p.Processed, p.Total);
                    });

                    await MpqControl.ExportAllFilesFromAllPaksAsync(progress);

                    if (ExportPListCheckBox.Checked)
                    {
                        try { MpqControl.ExportAllPakInfoWithFolderPicker(); }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to export PList.json:\n{ex.Message}", "PList",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    SetExportMessage("Export complete.");
                }
                else
                {
                    MessageBox.Show("Load .pak files in this window, or open the dialog with MpqControl assigned.");
                }
            }
            finally
            {
                ExportBtn.Enabled = true;
                progressBar1.Visible = false;
            }
        }
        #endregion

        #region Process Backspace Key
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Back && _viewMode == ViewMode.PakContents)
            {
                ShowPakList();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Show Pak List / Contents
        private void ShowPakList()
        {
            _viewMode = ViewMode.PakList;
            _currentPak = null;

            ColFile.Text = "Pak";
            ColPak.Text = "Path";
            ColSize.Text = "Files";
            EnsurePakSizeColumn(false);

            var needle = _filterText?.Trim().ToLowerInvariant() ?? string.Empty;

            var rows = _archives.Select(a => new
            {
                Handle = a,
                PakName = Path.GetFileName(a.FilePath),
                PakDir = Path.GetDirectoryName(a.FilePath) ?? string.Empty,
                FileCnt = a.Files?.Count ?? 0,
                PakSizeBytes = SafeGetFileSize(a.FilePath)
            });

            if (!string.IsNullOrEmpty(needle))
                rows = rows.Where(r =>
                    (r.PakName + " " + r.Handle.FilePath).ToLowerInvariant().Contains(needle));

            rows = _sortColumn switch
            {
                0 => _sortAsc ? rows.OrderBy(r => r.PakName, StringComparer.OrdinalIgnoreCase)
                              : rows.OrderByDescending(r => r.PakName, StringComparer.OrdinalIgnoreCase),
                1 => _sortAsc ? rows.OrderBy(r => r.PakDir, StringComparer.OrdinalIgnoreCase)
                              : rows.OrderByDescending(r => r.PakDir, StringComparer.OrdinalIgnoreCase),
                2 => _sortAsc ? rows.OrderBy(r => r.FileCnt)
                              : rows.OrderByDescending(r => r.FileCnt),
                3 => _sortAsc ? rows.OrderBy(r => r.PakSizeBytes)
                              : rows.OrderByDescending(r => r.PakSizeBytes),
                _ => rows
            };

            FilesListView.BeginUpdate();
            FilesListView.Items.Clear();

            foreach (var r in rows)
            {
                var lvi = new ListViewItem(r.PakName);
                lvi.SubItems.Add(r.PakDir);
                lvi.SubItems.Add(r.FileCnt.ToString());
                lvi.SubItems.Add(FormatBytes(r.PakSizeBytes));
                lvi.Tag = r.Handle;
                FilesListView.Items.Add(lvi);
            }

            FilesListView.EndUpdate();

            FilesListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            FilesListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            if (ColPakSize != null)
                FilesListView.AutoResizeColumn(ColPakSize.Index, ColumnHeaderAutoResizeStyle.ColumnContent);

            SetExportMessage($"Loaded {_archives.Count} pak(s). Double-click a pak to view contents.");
        }

        private void ShowPakContents(ArchiveHandle a)
        {
            _viewMode = ViewMode.PakContents;
            _currentPak = a;

            // Re-title columns for file view
            ColFile.Text = "File";
            ColPak.Text = "Pak";
            ColSize.Text = "Size";

            var pakName = Path.GetFileName(a.FilePath);
            var needle = _filterText?.Trim().ToLowerInvariant() ?? string.Empty;

            FilesListView.BeginUpdate();
            FilesListView.Items.Clear();

            foreach (var f in a.Files)
            {
                if (!string.IsNullOrEmpty(needle) &&
                    !f.Path.ToLowerInvariant().Contains(needle))
                    continue;

                var lvi = new ListViewItem(f.Path);
                lvi.SubItems.Add(pakName);
                lvi.SubItems.Add(f.Size.ToString());
                lvi.Tag = f; // handy if you later add preview/export per file
                FilesListView.Items.Add(lvi);
            }

            FilesListView.EndUpdate();
            SetExportMessage($"{pakName} — {FilesListView.Items.Count} file(s). Backspace to return to pak list.");
        }
        #endregion

        #region Filter TextBox
        private void TxtFilter_TextChanged(object? sender, EventArgs e)
        {
            _filterText = TxtFilter.Text?.Trim() ?? string.Empty;
            if (_viewMode == ViewMode.PakList) ShowPakList();
            else if (_currentPak != null) ShowPakContents(_currentPak);
        }
        #endregion

        #region Form Close
        private void OnFormClosed(object sender, FormClosingEventArgs e)
        {
            ClearArchives();
        }
        private void BtnClose_Click(object? sender, EventArgs e) => this.Close();
        #endregion

        #region Files ListView Item Activate (Double-Click)
        private void FilesListView_ItemActivate(object sender, EventArgs e)
        {
            if (FilesListView.SelectedItems.Count == 0) return;

            if (_viewMode == ViewMode.PakList)
            {
                if (FilesListView.SelectedItems[0].Tag is ArchiveHandle a)
                    ShowPakContents(a);
            }
            else if (_viewMode == ViewMode.PakContents)
            {
                // Optional: open preview/editor for the selected file here.
                // var file = FilesListView.SelectedItems[0].Tag as MpqFileEntry;
            }
        }
        #endregion

        #region Pak Size
        private static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            int u = 0;
            while (value >= 1024 && u < units.Length - 1)
            {
                value /= 1024;
                u++;
            }
            return $"{value:0.##} {units[u]}";
        }

        private void EnsurePakSizeColumn(bool want)
        {
            if (want)
            {
                if (ColPakSize == null)
                    ColPakSize = new ColumnHeader { Text = "Size", Width = 110, TextAlign = HorizontalAlignment.Right };

                if (!FilesListView.Columns.Contains(ColPakSize))
                    FilesListView.Columns.Add(ColPakSize);
            }
            else
            {
                if (ColPakSize != null && FilesListView.Columns.Contains(ColPakSize))
                    FilesListView.Columns.Remove(ColPakSize);
            }
        }

        private static long SafeGetFileSize(string path)
        {
            try { return new FileInfo(path).Length; }
            catch { return 0L; }
        }
        #endregion

        #region PList
        private sealed class PakFileEntryDto
        {
            public string path { get; set; } = "";
            public long size { get; set; }
        }
        private sealed class PakInfoDto
        {
            public string pak { get; set; } = "";
            public string pakPath { get; set; } = "";
            public long pakSize { get; set; }
            public int fileCount { get; set; }
            public long totalSize { get; set; }
            public PakFileEntryDto[] files { get; set; } = Array.Empty<PakFileEntryDto>();
        }

        private static PakInfoDto BuildPakInfoFromHandle(ArchiveHandle h)
        {
            var pakPathRel = MPQExplorerControl.MakeRelativeFromMarker(h.FilePath, "MMOGame");
            long pakSize = 0; try { pakSize = new FileInfo(h.FilePath).Length; } catch { }

            var files = (h.Files ?? new List<MpqFileEntry>())
                .Select(f => new PakFileEntryDto { path = f.Path, size = f.Size })
                .OrderBy(x => x.path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new PakInfoDto
            {
                pak = Path.GetFileName(h.FilePath),
                pakPath = pakPathRel,                     // e.g. MMOGame\CookedPC\00001.pak
                pakSize = pakSize,
                fileCount = files.Length,
                totalSize = files.Aggregate(0L, (sum, x) => sum + x.size),
                files = files
            };
        }

        private void WritePListJsonForDialogArchives(string exportRoot)
        {
            var paks = _archives
                .Select(BuildPakInfoFromHandle)
                .OrderBy(p => p.pakPath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            // Root is the first segment of the first pakPath, default "MMOGame"
            var rootName = "MMOGame";
            if (paks.Length > 0)
            {
                var first = paks[0].pakPath;
                var sep = first.IndexOf('\\');
                if (sep > 0) rootName = first.Substring(0, sep);
            }

            var payload = new { root = rootName, pakCount = paks.Length, paks };
            var json = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(Path.Combine(exportRoot, "PList.json"), json, Encoding.UTF8);
        }
        #endregion
    }
}
