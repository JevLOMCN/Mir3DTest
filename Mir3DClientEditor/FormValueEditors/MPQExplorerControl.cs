using Mir3DCrypto;
using StormLibSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir3DClientEditor.FormValueEditors
{
    public partial class MPQExplorerControl : UserControl
    {

        public List<MpqArchiveManager> _archives = new List<MpqArchiveManager>();

        public MPQExplorerControl()
        {
            InitializeComponent();
            TreeFolders.AfterSelect += TreeFolders_AfterSelect;
            DataGrid.ReadOnly = true;
            DataGrid.CellDoubleClick += DataGrid_CellDoubleClick;
            DataGrid.CellMouseClick += DataGrid_CellMouseClick;
            DataGrid.AllowDrop = true;
            DataGrid.DragDrop += DataGrid_DragDrop;
            DataGrid.DragOver += DataGrid_DragOver;
            Disposed += MPQExplorerControl_Disposed;
        }

        public class ExportProgress
        {
            public int Processed { get; set; }
            public int Total { get; set; }
            public string? CurrentPak { get; set; }   // e.g. "MMOGame\\CookedPC"
            public string? CurrentFile { get; set; }  // internal path inside the pak
        }

        private bool _exportBusyAllFiles;

        private sealed class PakFileEntry
        {
            public string path { get; set; } = "";
            public long size { get; set; }
        }

        private sealed class PakInfo
        {
            public string pak { get; set; } = "";
            public string pakPath { get; set; } = ""; // relative from MMOGame
            public long pakSize { get; set; }         // bytes on disk
            public int fileCount { get; set; }
            public long totalSize { get; set; }       // sum of contained file sizes
            public PakFileEntry[] files { get; set; } = Array.Empty<PakFileEntry>();
        }
        private PakInfo BuildPakInfo(MpqArchiveManager manager)
        {
            var files = manager.ListFiles ?? Array.Empty<MpqArchiveManagerFile>();

            long totalSize = files.Aggregate(0L, (sum, f) => sum + f.FileSize);

            long pakSize = 0;
            try { pakSize = new System.IO.FileInfo(manager.FilePath).Length; } catch { /* ignore */ }

            return new PakInfo
            {
                pak = System.IO.Path.GetFileName(manager.FilePath),
                pakPath = MakeRelativeFromMarker(manager.FilePath, "MMOGame"),
                pakSize = pakSize,
                fileCount = files.Length,
                totalSize = totalSize,
                files = files
                    .Select(f => new PakFileEntry { path = f.Path, size = f.FileSize })
                    .OrderBy(x => x.path, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            };
        }
        private void DataGrid_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void DataGrid_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    var path = (GetSelectedDirectory() + '\\' + Path.GetFileName(filePath)).TrimStart('\\');
                    var manager = _archives.Where(x => x.ListFiles.Any(x => x.Path == path)).FirstOrDefault();
                    var buffer = System.IO.File.ReadAllBytes(filePath);
                    buffer = Crypto.Encrypt(buffer);

                    if (manager == null)
                    {
                        // Is a new file
                        var smallArchive = _archives.OrderBy(x => x.ListFiles.Sum(x => x.FileSize)).First();
                        var listFiles = new List<MpqArchiveManagerFile>(smallArchive.ListFiles.Length + 1);
                        listFiles.AddRange(smallArchive.ListFiles);
                        listFiles.Add(new MpqArchiveManagerFile
                        {
                            FileSize = (uint)buffer.Length,
                            FileTime = DateTime.Now,
                            Flags = 0,
                            Manager = smallArchive,
                            Path = path
                        });
                        smallArchive.ListFiles = listFiles.ToArray();
                        smallArchive.Archive.FileCreateFile(path, 0, buffer);
                    }
                    else
                    {
                        if (MessageBox.Show("File already exists. Are you sure do you want replace it?", "Replace file", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            var file = manager.ListFiles.First(x => x.Path == path);
                            manager.Archive.FileCreateFile(path, file.Flags, buffer);
                        }
                    }
                }

                TreeFolders_AfterSelect(null, default(TreeViewEventArgs));
            }
        }

        private void DataGrid_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != -1 && e.RowIndex != -1 && e.Button == MouseButtons.Right)
            {
                ContextMenuStrip m = new ContextMenuStrip();
                var exportAs = new ToolStripMenuItem("Extract File");
                exportAs.Click += ExportAs_Click;
                m.Items.Add(exportAs);
                var point = PointToClient(Cursor.Position);
                Point locationOnForm = DataGrid.FindForm().PointToClient(DataGrid.Parent.PointToScreen(DataGrid.Location));
                point = new Point(point.X - locationOnForm.X, point.Y - locationOnForm.Y);
                m.Show(DataGrid, point);
            }
        }

        private void ExportAs_Click(object? sender, EventArgs e)
        {
            var rows = DataGrid.SelectedRows.Cast<DataGridViewRow>().ToList();
            if (rows.Count == 0) return;

            if (rows.Count == 1)
            {
                var row = rows[0];
                var cell = row.Cells[0];
                if (cell?.Value == null) return;

                var path = (GetSelectedDirectory() + '\\' + (string)cell.Value).TrimStart('\\');
                var manager = _archives.FirstOrDefault(x => x.ListFiles.Any(f => f.Path == path));
                if (manager == null) return;

                var saveDialog = new SaveFileDialog();
                saveDialog.FileName = System.IO.Path.GetFileName(path);

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                using var fileStream = manager.Archive.OpenFile(path);

                var flags = fileStream.GetFlags();
                var data = new byte[fileStream.Length];
                fileStream.Read(data, 0, data.Length);

                data = Crypto.Decrypt(data);

                System.IO.File.WriteAllBytes(saveDialog.FileName, data);
                return;
            }

            using (var folderDialog = new FolderBrowserDialog()
            {
                Description = $"Choose a folder to export {rows.Count} files"
            })
            {
                if (folderDialog.ShowDialog() != DialogResult.OK)
                    return;

                string targetDir = folderDialog.SelectedPath;
                int saved = 0, failed = 0, skipped = 0;

                foreach (var row in rows)
                {
                    try
                    {
                        var cell = row.Cells[0];
                        if (cell?.Value == null) { skipped++; continue; }

                        var path = (GetSelectedDirectory() + '\\' + (string)cell.Value).TrimStart('\\');
                        var manager = _archives.FirstOrDefault(x => x.ListFiles.Any(f => f.Path == path));
                        if (manager == null) { skipped++; continue; }

                        using var fileStream = manager.Archive.OpenFile(path);

                        var flags = fileStream.GetFlags(); // preserved
                        var data = new byte[fileStream.Length];
                        fileStream.Read(data, 0, data.Length);

                        data = Crypto.Decrypt(data);

                        var outName = System.IO.Path.GetFileName(path);
                        var outPath = GetUniquePath(targetDir, outName);

                        // Ensure directory exists (targetDir already exists; outPath may include subdirs if you change logic later)
                        var outDir = System.IO.Path.GetDirectoryName(outPath);
                        if (!string.IsNullOrEmpty(outDir) && !System.IO.Directory.Exists(outDir))
                            System.IO.Directory.CreateDirectory(outDir);

                        System.IO.File.WriteAllBytes(outPath, data);
                        saved++;
                    }
                    catch
                    {
                        failed++;
                    }
                }

                MessageBox.Show(
                    $"Export complete.\n\nSaved: {saved}\nFailed: {failed}\nSkipped: {skipped}",
                    "Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private static string GetUniquePath(string directory, string fileName)
        {
            string baseName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            string ext = System.IO.Path.GetExtension(fileName);
            string candidate = System.IO.Path.Combine(directory, fileName);

            int i = 2;
            while (System.IO.File.Exists(candidate))
            {
                candidate = System.IO.Path.Combine(directory, $"{baseName} ({i}){ext}");
                i++;
            }
            return candidate;
        }

        private void MPQExplorerControl_Disposed(object? sender, EventArgs e)
        {
            foreach (var archive in _archives)
                archive.Archive.Dispose();
        }

        private void DataGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var row = DataGrid.Rows[e.RowIndex];
            var cell = row.Cells[0];

            var path = (GetSelectedDirectory() + '\\' + (string)cell.Value).TrimStart('\\');
            var manager = _archives.Where(x => x.ListFiles.Any(x => x.Path == path)).FirstOrDefault();
            if (manager == null) return;

            using var fileStream = manager.Archive.OpenFile(path);

            var flags = fileStream.GetFlags();
            var data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);

            var form = new FMainEditor();
            form.SaveFile += (s, e) =>
            {
                manager.Archive.FileCreateFile(path, flags, e.Buffer);
            };
            form.LoadFile(path, data, LoadDependantFile);
            form.ShowDialog();
            form.Dispose();
        }

        private byte[]? LoadDependantFile(string file)
        {
            var fileManager = _archives.SelectMany(x => x.ListFiles)
                .Where(x => Path.GetFileNameWithoutExtension(x.Path) == file)
                .FirstOrDefault();

            if (fileManager == null)
                return null;

            using var fileStream = fileManager.Manager.Archive.OpenFile(fileManager.Path);

            var data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);

            data = Crypto.Decrypt(data);

            return data;
        }

        private string GetSelectedDirectory()
        {
            var pathParts = new List<string>();
            var node = TreeFolders.SelectedNode;
            do
            {
                pathParts.Insert(0, node.Name);
            } while ((node = node.Parent) != null);

            var path = string.Join('\\', pathParts.ToArray()).TrimStart('\\');

            return path;
        }

        private void TreeFolders_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            DataGrid.Rows.Clear();

            var path = GetSelectedDirectory();

            var files = _archives.SelectMany(x => x.ListFiles.Where(x => Path.GetDirectoryName(x.Path) == path).ToArray()).ToArray();

            foreach (var file in files)
            {
                var row = DataGrid.Rows[DataGrid.Rows.Add()];
                row.Cells[0].Value = Path.GetFileName(file.Path);
                row.Cells[1].Value = file.FileSize;
                row.Cells[2].Value = Path.GetExtension(file.Path);
                row.Cells[3].Value = file.FileTime == null ? "N/a" : file.FileTime.ToString();
                row.Cells[4].Value = file.Flags;
                row.Cells[5].Value = file.Manager.FilePath;
            }
        }

        public void LoadMPQ(IEnumerable<string> mpqs)
        {
            foreach (var mpq in mpqs)
            {
                try
                {
                    _archives.Add(new MpqArchiveManager
                    {
                        FilePath = mpq,
                        Archive = new MpqArchive(mpq, FileAccess.ReadWrite)
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error ocurred opening '{mpq}':\n{ex.Message}");
                }
            }

            LoadTree();
        }

        private void LoadTree()
        {
            textBox1.Text = string.Empty;

            foreach (var manager in _archives)
            {
                try
                {
                    using (var stream = manager.Archive.OpenFile("(listfile)"))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var content = Encoding.UTF8.GetString(buffer);
                        var files = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var listfiles = new List<MpqArchiveManagerFile>(files.Length);
                        foreach (var file in files)
                        {
                            listfiles.Add(new MpqArchiveManagerFile
                            {
                                Manager = manager,
                                Path = file,
                                FileSize = manager.Archive.GetFileSize(file),
                                Flags = manager.Archive.GetFileFlags(file),
                                FileTime = manager.Archive.GetFileTime(file)
                            });
                        }

                        manager.ListFiles = listfiles.ToArray();
                    }
                }
                catch (Exception e)
                {
                    manager.ListFiles = new MpqArchiveManagerFile[0];
                    MessageBox.Show($"Cant open {manager.FilePath} / (listfile): {e.Message}");
                }
            }

            FilterNodes(string.Empty);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            FilterNodes(textBox1.Text);
        }

        private void FilterNodes(string text)
        {
            DataGrid.Rows.Clear();
            TreeFolders.Nodes.Clear();
            var rootNode = TreeFolders.Nodes.Add("Root");

            foreach (var manager in _archives)
            {
                foreach (var file in manager.ListFiles)
                {
                    if (!string.IsNullOrEmpty(text) && !file.Path.ToLower().Contains(text.ToLower()))
                        continue;

                    var parts = Path.GetDirectoryName(file.Path).Split('\\');

                    if (parts.Length == 0 || (parts.Length == 1 && parts[0] == string.Empty))
                        continue;

                    var nodes = rootNode.Nodes;
                    foreach (var part in parts)
                    {
                        if (!nodes.ContainsKey(part))
                        {
                            var node = nodes.Add(part, part);
                            nodes = node.Nodes;
                        }
                        else
                        {
                            nodes = nodes[part].Nodes;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(text) && text.Length > 3)
            {
                TreeFolders.ExpandAll();
            }
            else
            {
                TreeFolders.SelectedNode = rootNode;
                rootNode.Expand();
            }
        }

        public void ExportAllPakInfoWithFolderPicker()
        {
            if (_archives == null || _archives.Count == 0)
            {
                MessageBox.Show("No pak archives loaded.", "Export File Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sfd = new SaveFileDialog
            {
                FileName = "PList.json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var paks = _archives
                .Select(BuildPakInfo)
                .OrderBy(p => p.pakPath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var payload = new
            {
                root = "MMOGame",
                pakCount = paks.Length,
                paks
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(sfd.FileName, json, System.Text.Encoding.UTF8);

            MessageBox.Show($"Exported {paks.Length} paks to:\n{sfd.FileName}", "Export File Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public async Task ExportAllFilesFromAllPaksAsync(IProgress<ExportProgress>? progress = null)
        {
            if (_exportBusyAllFiles) return;
            _exportBusyAllFiles = true;

            try
            {
                if (_archives == null || _archives.Count == 0)
                {
                    MessageBox.Show("No pak archives loaded.", "Export All Files",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string exportRoot;
                using (var fbd = new FolderBrowserDialog
                {
                    Description = $"Choose a folder to export files from {_archives.Count} paks"
                })
                {
                    if (fbd.ShowDialog() != DialogResult.OK) return;
                    exportRoot = fbd.SelectedPath;
                }

                int total = _archives.Sum(a => a.ListFiles?.Length ?? 0);
                int processed = 0;
                int saved = 0, failed = 0;

                await Task.Run(() =>
                {
                    foreach (var manager in _archives)
                    {
                        var relPakPath = MakeRelativeFromMarker(manager.FilePath, "MMOGame").Replace('/', '\\'); // "MMOGame\\CookedPC\\00000.pak"
                        var relPakDir = System.IO.Path.GetDirectoryName(relPakPath) ?? "MMOGame";
                        var baseOutDir = System.IO.Path.Combine(exportRoot, relPakDir);

                        if (!System.IO.Directory.Exists(baseOutDir))
                            System.IO.Directory.CreateDirectory(baseOutDir);

                        var files = manager.ListFiles ?? Array.Empty<MpqArchiveManagerFile>();
                        foreach (var f in files)
                        {
                            try
                            {
                                var internalPath = (f.Path ?? string.Empty).Replace('/', '\\');
                                var outFullPath = System.IO.Path.Combine(baseOutDir, internalPath);

                                var outDir = System.IO.Path.GetDirectoryName(outFullPath);
                                if (!string.IsNullOrEmpty(outDir) && !System.IO.Directory.Exists(outDir))
                                    System.IO.Directory.CreateDirectory(outDir);

                                using (var fileStream = manager.Archive.OpenFile(f.Path))
                                {
                                    var data = new byte[fileStream.Length];
                                    fileStream.Read(data, 0, data.Length);
                                    data = Mir3DCrypto.Crypto.Decrypt(data);
                                    System.IO.File.WriteAllBytes(outFullPath, data);
                                }
                                saved++;
                            }
                            catch
                            {
                                failed++;
                            }
                            finally
                            {
                                processed++;
                                progress?.Report(new ExportProgress
                                {
                                    Processed = processed,
                                    Total = total,
                                    CurrentPak = relPakDir,
                                    CurrentFile = f.Path
                                });
                            }
                        }
                    }
                });

                MessageBox.Show(
                    $"Export complete.\n\nSaved: {saved}\nFailed: {failed}",
                    "Export All Files",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            finally
            {
                _exportBusyAllFiles = false;
            }
        }

        public static string MakeRelativeFromMarker(string fullPath, string marker = "MMOGame")
        {
            if (string.IsNullOrEmpty(fullPath)) return fullPath ?? string.Empty;

            string norm = fullPath.Replace('/', '\\');
            string m = marker.Replace('/', '\\');

            int idx = norm.IndexOf("\\" + m + "\\", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) idx = norm.IndexOf(m, StringComparison.OrdinalIgnoreCase);

            if (idx >= 0)
                return norm.Substring(idx).TrimStart('\\');

            return System.IO.Path.GetFileName(norm);
        }
    }
}
