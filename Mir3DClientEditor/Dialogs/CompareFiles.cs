using Mir3DClientEditor.FormValueEditors;
using StormLibSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir3DClientEditor.Dialogs
{
    public partial class CompareFiles : Form
    {
        public CompareFiles()
        {
            InitializeComponent();

            //CompareListView.ItemActivate += CompareListView_ItemActivate;

            this.KeyPreview = true;                  // form gets keys first
            //this.KeyDown += CompareFiles_KeyDown;    // handle Backspace to go back
        }

        private void ComparePaksButton_Click(object sender, EventArgs e)
        {
            try
            {
                var pickA = PickFolderOrFile(
                    "Select folder A that contains .pak files (e.g., MMOGame\\CookedPC). Cancel to choose a single .pak instead.",
                    "Select .pak file A",
                    "Mir3D Pak (*.pak)|*.pak|All files (*.*)|*.*");
                if (pickA == null) { SetStatus("Pak compare cancelled."); return; }

                var pickB = PickFolderOrFile(
                    "Select folder B that contains .pak files (e.g., MMOGame\\CookedPC). Cancel to choose a single .pak instead.",
                    "Select .pak file B",
                    "Mir3D Pak (*.pak)|*.pak|All files (*.*)|*.*");
                if (pickB == null) { SetStatus("Pak compare cancelled."); return; }

                Cursor = Cursors.WaitCursor;

                if (!pickA.IsFile && !pickB.IsFile)
                {
                    // Folder ↔ Folder: compare .pak
                    SetStatus("Scanning .pak folders...");
                    var mapA = ScanFiles(pickA.Path!, "*.pak", allDirectories: true);
                    var mapB = ScanFiles(pickB.Path!, "*.pak", allDirectories: true);

                    // annotate internal file counts
                    foreach (var m in mapA.Values)
                        if (m.FullPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                            m.Count = TryGetPakFileCount(m.FullPath);
                    foreach (var m in mapB.Values)
                        if (m.FullPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                            m.Count = TryGetPakFileCount(m.FullPath);

                    var keys = new HashSet<string>(mapA.Keys, StringComparer.OrdinalIgnoreCase);
                    keys.UnionWith(mapB.Keys);

                    var rows = keys.Select(k =>
                    {
                        mapA.TryGetValue(k, out var a);
                        mapB.TryGetValue(k, out var b);
                        var status = CompareEntry(a, b);

                        return new CompareRow
                        {
                            Key = k,
                            PathA = a?.FullPath,
                            PathB = b?.FullPath,
                            FileCountA = a?.Count,
                            FileCountB = b?.Count,
                            SizeA = a?.Size,
                            SizeB = b?.Size,
                            DateAUtc = a?.Utc,
                            DateBUtc = b?.Utc,
                            Status = status
                        };
                    });

                    PopulateListViewForCompare(Path.GetFileName(pickA.Path), Path.GetFileName(pickB.Path), rows, includePakCounts: true);
                }
                else if (pickA.IsFile && pickB.IsFile)
                {
                    // .pak ↔ .pak
                    var infoA = ProbeSingleFile(pickA.Path!);
                    var infoB = ProbeSingleFile(pickB.Path!);

                    if (infoA != null && infoA.FullPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                        infoA.Count = TryGetPakFileCount(infoA.FullPath);
                    if (infoB != null && infoB.FullPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                        infoB.Count = TryGetPakFileCount(infoB.FullPath);

                    var status = CompareEntry(infoA, infoB);

                    var rows = new[]
                    {
                        new CompareRow
                        {
                            Key = Path.GetFileName(pickA.Path!) + " ↔ " + Path.GetFileName(pickB.Path!),
                            PathA = infoA?.FullPath,
                            PathB = infoB?.FullPath,
                            FileCountA = infoA?.Count,
                            FileCountB = infoB?.Count,
                            SizeA = infoA?.Size,
                            SizeB = infoB?.Size,
                            DateAUtc = infoA?.Utc,
                            DateBUtc = infoB?.Utc,
                            Status = status
                        }
                    };

                    PopulateListViewForCompare(Path.GetFileName(pickA.Path), Path.GetFileName(pickB.Path), rows, includePakCounts: true);
                }
                else
                {
                    // Mixed: .pak file ↔ folder (match by pak filename)
                    var filePick = pickA.IsFile ? pickA : pickB;
                    var folderPick = pickA.IsFile ? pickB : pickA;

                    SetStatus("Scanning .pak folder and matching file...");
                    var mapFolder = ScanFiles(folderPick.Path!, "*.pak", allDirectories: true);

                    // add counts for folder-side paks
                    foreach (var m in mapFolder.Values)
                        if (m.FullPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                            m.Count = TryGetPakFileCount(m.FullPath);

                    var targetName = Path.GetFileName(filePick.Path!);
                    var matchKey = mapFolder.Keys.FirstOrDefault(k =>
                        Path.GetFileName(k).Equals(targetName, StringComparison.OrdinalIgnoreCase));

                    var infoFile = ProbeSingleFile(filePick.Path!);
                    if (infoFile != null && infoFile.FullPath.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))
                        infoFile.Count = TryGetPakFileCount(infoFile.FullPath);

                    FileMeta? infoFolder = (matchKey != null && mapFolder.TryGetValue(matchKey, out var meta)) ? meta : null;

                    bool fileIsA = pickA.IsFile;
                    var status = CompareEntry(fileIsA ? infoFile : infoFolder,
                                              fileIsA ? infoFolder : infoFile);

                    var row = new CompareRow
                    {
                        Key = targetName,
                        PathA = fileIsA ? infoFile?.FullPath : infoFolder?.FullPath,
                        PathB = fileIsA ? infoFolder?.FullPath : infoFile?.FullPath,
                        FileCountA = fileIsA ? infoFile?.Count : infoFolder?.Count,
                        FileCountB = fileIsA ? infoFolder?.Count : infoFile?.Count,
                        SizeA = fileIsA ? infoFile?.Size : infoFolder?.Size,
                        SizeB = fileIsA ? infoFolder?.Size : infoFile?.Size,
                        DateAUtc = fileIsA ? infoFile?.Utc : infoFolder?.Utc,
                        DateBUtc = fileIsA ? infoFolder?.Utc : infoFile?.Utc,
                        Status = status
                    };

                    PopulateListViewForCompare(Path.GetFileName(pickA.Path), Path.GetFileName(pickB.Path), new[] { row }, includePakCounts: true);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Pak compare failed: {ex.Message}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CompareExportsButton_Click(object sender, EventArgs e)
        {
            try
            {
                var pickA = PickFolderOrFile(
                    "Select export folder A (contains extracted pak files). Cancel to choose a single file instead.",
                    "Select exported file A",
                    "*.*");
                if (pickA == null) { SetStatus("Export compare cancelled."); return; }

                var pickB = PickFolderOrFile(
                    "Select export folder B (contains extracted pak files). Cancel to choose a single file instead.",
                    "Select exported file B",
                    "*.*");
                if (pickB == null) { SetStatus("Export compare cancelled."); return; }

                Cursor = Cursors.WaitCursor;

                if (!pickA.IsFile && !pickB.IsFile)
                {
                    // Folder ↔ Folder
                    SetStatus("Scanning export folders...");
                    var mapA = ScanFiles(pickA.Path!, "*", allDirectories: true);
                    var mapB = ScanFiles(pickB.Path!, "*", allDirectories: true);

                    var keys = new HashSet<string>(mapA.Keys, StringComparer.OrdinalIgnoreCase);
                    keys.UnionWith(mapB.Keys);

                    var rows = keys.Select(k =>
                    {
                        mapA.TryGetValue(k, out var a);
                        mapB.TryGetValue(k, out var b);
                        var status = CompareEntry(a, b);

                        return new CompareRow
                        {
                            Key = k,
                            PathA = a?.FullPath,
                            PathB = b?.FullPath,
                            SizeA = a?.Size,
                            SizeB = b?.Size,
                            DateAUtc = a?.Utc,
                            DateBUtc = b?.Utc,
                            Status = status
                        };
                    });

                    PopulateListViewForCompare(Path.GetFileName(pickA.Path), Path.GetFileName(pickB.Path), rows);
                }
                else if (pickA.IsFile && pickB.IsFile)
                {
                    // File ↔ File
                    var infoA = ProbeSingleFile(pickA.Path!);
                    var infoB = ProbeSingleFile(pickB.Path!);
                    var status = CompareEntry(infoA, infoB);

                    var rows = new[]
                    {
                        new CompareRow
                        {
                            Key = Path.GetFileName(pickA.Path!) + " ↔ " + Path.GetFileName(pickB.Path!),
                            PathA = infoA?.FullPath,
                            PathB = infoB?.FullPath,
                            SizeA = infoA?.Size,
                            SizeB = infoB?.Size,
                            DateAUtc = infoA?.Utc,
                            DateBUtc = infoB?.Utc,
                            Status = status
                        }
                    };

                    PopulateListViewForCompare(Path.GetFileName(pickA.Path), Path.GetFileName(pickB.Path), rows);
                }
                else
                {
                    // Mixed: File ↔ Folder — match by filename anywhere under the folder
                    var filePick = pickA.IsFile ? pickA : pickB;
                    var folderPick = pickA.IsFile ? pickB : pickA;

                    SetStatus("Scanning export folder and matching file...");
                    var mapFolder = ScanFiles(folderPick.Path!, "*", allDirectories: true);

                    var targetName = Path.GetFileName(filePick.Path!);
                    var matchKey = mapFolder.Keys.FirstOrDefault(k =>
                        Path.GetFileName(k).Equals(targetName, StringComparison.OrdinalIgnoreCase));

                    var infoFile = ProbeSingleFile(filePick.Path!);
                    FileMeta? infoFolder = null;
                    string? folderFullPath = null;

                    if (matchKey != null && mapFolder.TryGetValue(matchKey, out var meta))
                    {
                        infoFolder = meta;
                        folderFullPath = meta.FullPath;
                    }

                    bool fileIsA = pickA.IsFile;
                    var status = CompareEntry(fileIsA ? infoFile : infoFolder,
                                              fileIsA ? infoFolder : infoFile);

                    var row = new CompareRow
                    {
                        Key = targetName,
                        PathA = fileIsA ? infoFile?.FullPath : folderFullPath,
                        PathB = fileIsA ? folderFullPath : infoFile?.FullPath,
                        SizeA = fileIsA ? infoFile?.Size : infoFolder?.Size,
                        SizeB = fileIsA ? infoFolder?.Size : infoFile?.Size,
                        DateAUtc = fileIsA ? infoFile?.Utc : infoFolder?.Utc,
                        DateBUtc = fileIsA ? infoFolder?.Utc : infoFile?.Utc,
                        Status = status
                    };

                    PopulateListViewForCompare(Path.GetFileName(pickA.Path), Path.GetFileName(pickB.Path), new[] { row });
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Export compare failed: {ex.Message}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void PopulateListViewForCompare(string titleLeft, string titleRight, IEnumerable<CompareRow> rows, bool includePakCounts = false)
        {
            _compareHeaderLeft = titleLeft;
            _compareHeaderRight = titleRight;
            _compareRows = rows.ToList();
            _currentIsPakCompare = includePakCounts;

            CompareListView.BeginUpdate();
            try
            {
                CompareListView.Items.Clear();
                CompareListView.Columns.Clear();

                CompareListView.Columns.Add("Item", 150);
                CompareListView.Columns.Add($"{_compareHeaderLeft} (Path) A", 380);
                CompareListView.Columns.Add($"{_compareHeaderRight} (Path) B", 380);
                if (includePakCounts)
                {
                    CompareListView.Columns.Add($"{_compareHeaderLeft} (Files) A", 120, HorizontalAlignment.Right);
                    CompareListView.Columns.Add($"{_compareHeaderRight} (Files) B", 120, HorizontalAlignment.Right);
                }
                CompareListView.Columns.Add($"{_compareHeaderLeft} (Size) A", 120, HorizontalAlignment.Right);
                CompareListView.Columns.Add($"{_compareHeaderRight} (Size) B", 120, HorizontalAlignment.Right);
                CompareListView.Columns.Add($"{_compareHeaderLeft} (UTC) A", 150);
                CompareListView.Columns.Add($"{_compareHeaderRight} (UTC) B", 150);
                CompareListView.Columns.Add("Status", 120);

                foreach (var r in _compareRows
            .OrderBy(r => r.Status)
            .ThenBy(r => r.Key, StringComparer.OrdinalIgnoreCase))
                {
                    var lvi = new ListViewItem(r.Key) { Tag = r };
                    lvi.SubItems.Add(r.PathA ?? "");
                    lvi.SubItems.Add(r.PathB ?? "");
                    if (includePakCounts)
                    {
                        lvi.SubItems.Add(r.FileCountA.HasValue ? r.FileCountA.Value.ToString("N0") : "");
                        lvi.SubItems.Add(r.FileCountB.HasValue ? r.FileCountB.Value.ToString("N0") : "");
                    }
                    lvi.SubItems.Add(r.SizeA.HasValue ? r.SizeA.Value.ToString("N0") : "");
                    lvi.SubItems.Add(r.SizeB.HasValue ? r.SizeB.Value.ToString("N0") : "");
                    lvi.SubItems.Add(r.DateAUtc.HasValue ? r.DateAUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "");
                    lvi.SubItems.Add(r.DateBUtc.HasValue ? r.DateBUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "");
                    lvi.SubItems.Add(r.Status.ToString());
                    CompareListView.Items.Add(lvi);
                }
            }
            finally { CompareListView.EndUpdate(); }

            SetStatus($"Compared {_compareRows.Count:N0} item(s).");
        }

        private void SetStatus(string text)
        {
            if (CompareLabel == null) return;
            if (CompareLabel.IsHandleCreated)
                CompareLabel.BeginInvoke(new Action(() => CompareLabel.Text = text));
        }

        private sealed class PickResult
        {
            public string? Path;
            public bool IsFile;
        }

        private sealed class FileMeta
        {
            public long Size;
            public DateTime Utc;
            public string FullPath = "";
            public int? Count; // only for .pak (internal file count)
        }

        private PickResult? PickFolderOrFile(string titleFolder, string titleFile, string? filter = "*.*")
        {
            using (var fbd = new FolderBrowserDialog { Description = titleFolder })
            {
                if (fbd.ShowDialog(this) == DialogResult.OK)
                    return new PickResult { Path = fbd.SelectedPath, IsFile = false };
            }
            using (var ofd = new OpenFileDialog
            {
                Title = titleFile,
                Filter = filter ?? "*.*",
                Multiselect = false,
                CheckFileExists = true
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                    return new PickResult { Path = ofd.FileName, IsFile = true };
            }
            return null;
        }

        private static int? TryGetPakFileCount(string pakPath)
        {
            try
            {
                using var arc = new MpqArchive(pakPath, FileAccess.Read);
                using var s = arc.OpenFile("(listfile)");
                using var sr = new StreamReader(s, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                int count = 0;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line)) count++;
                }
                return count;
            }
            catch
            {
                // Some paks may not expose (listfile); return null when unknown
                return null;
            }
        }

        private static string ToRelPath(string root, string fullPath)
        {
            var rel = fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                ? fullPath.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                : Path.GetFileName(fullPath);
            return rel.Replace('/', '\\');
        }

        private static Dictionary<string, FileMeta> ScanFiles(string root, string searchPattern, bool allDirectories)
        {
            var dict = new Dictionary<string, FileMeta>(StringComparer.OrdinalIgnoreCase);
            var option = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var file in Directory.EnumerateFiles(root, searchPattern, option))
            {
                try
                {
                    var fi = new FileInfo(file);
                    var rel = ToRelPath(root, file);
                    dict[rel] = new FileMeta
                    {
                        Size = fi.Length,
                        Utc = fi.LastWriteTimeUtc,
                        FullPath = fi.FullName
                    };
                }
                catch { /* ignore */ }
            }
            return dict;
        }

        private static FileMeta? ProbeSingleFile(string path)
        {
            try
            {
                var fi = new FileInfo(path);
                if (!fi.Exists) return null;
                return new FileMeta { Size = fi.Length, Utc = fi.LastWriteTimeUtc, FullPath = fi.FullName };
            }
            catch { return null; }
        }

        private enum CompareStatus
        {
            OnlyInA,
            OnlyInB,
            Same,
            SizeDiffers,
            DateDiffers,
            SizeAndDateDiffers
        }

        private sealed class CompareRow
        {
            public string Key = "";          // relative item key (or filename label)
            public string? PathA;            // full path (A)
            public string? PathB;            // full path (B)
            public long? SizeA;
            public long? SizeB;
            public DateTime? DateAUtc;
            public DateTime? DateBUtc;
            public int? FileCountA;          // only used for .pak compare
            public int? FileCountB;          // only used for .pak compare
            public CompareStatus Status;
        }

        private List<CompareRow> _compareRows = new();
        private string _compareHeaderLeft = "A";
        private string _compareHeaderRight = "B";

        private static CompareStatus CompareEntry(FileMeta? A, FileMeta? B)
        {
            if (A != null && B == null) return CompareStatus.OnlyInA;
            if (A == null && B != null) return CompareStatus.OnlyInB;
            if (A == null && B == null) return CompareStatus.Same;

            bool sizeEq = A!.Size == B!.Size;
            bool dateEq = A.Utc == B.Utc;

            if (sizeEq && dateEq) return CompareStatus.Same;
            if (!sizeEq && dateEq) return CompareStatus.SizeDiffers;
            if (sizeEq && !dateEq) return CompareStatus.DateDiffers;
            return CompareStatus.SizeAndDateDiffers;
        }

        private sealed class ViewState
        {
            public string TitleLeft = "A";
            public string TitleRight = "B";
            public List<CompareRow> Rows = new();
            public bool IncludePakCounts;
        }

        private readonly Stack<ViewState> _navStack = new();
        private bool _currentIsPakCompare;

        private void CompareListView_ItemActivate(object sender, EventArgs e)
        {
            // Only drill when the top view is Pak↔Pak list
            if (!_currentIsPakCompare) return;
            if (CompareListView.SelectedItems.Count == 0) return;

            var row = CompareListView.SelectedItems[0].Tag as CompareRow;
            if (row == null) return;

            bool isPakRow =
                (row.PathA?.EndsWith(".pak", StringComparison.OrdinalIgnoreCase) ?? false) ||
                (row.PathB?.EndsWith(".pak", StringComparison.OrdinalIgnoreCase) ?? false);

            if (!isPakRow) return;

            DrillIntoPakRow(row);
        }

        // Backspace = Go Back
        private void CompareListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && _navStack.Count > 0)
            {
                var prev = _navStack.Pop();
                PopulateListViewForCompare(prev.TitleLeft, prev.TitleRight, prev.Rows, prev.IncludePakCounts);
                e.Handled = true;  // don’t send Backspace to the ListView
            }
        }

        private void DrillIntoPakRow(CompareRow pakRow)
        {
            // Save current view for Backspace
            _navStack.Push(new ViewState
            {
                TitleLeft = _compareHeaderLeft,
                TitleRight = _compareHeaderRight,
                Rows = _compareRows.ToList(),
                IncludePakCounts = _currentIsPakCompare
            });

            string? pakA = pakRow.PathA;
            string? pakB = pakRow.PathB;

            var rows = new List<CompareRow>();

            MpqArchive? arcA = null, arcB = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(pakA) && File.Exists(pakA)) arcA = new MpqArchive(pakA, FileAccess.Read);
                if (!string.IsNullOrWhiteSpace(pakB) && File.Exists(pakB)) arcB = new MpqArchive(pakB, FileAccess.Read);

                // Read (listfile) from each side
                var filesA = arcA != null ? ReadListfile(arcA) : Enumerable.Empty<string>();
                var filesB = arcB != null ? ReadListfile(arcB) : Enumerable.Empty<string>();

                var allInternal = new HashSet<string>(filesA, StringComparer.OrdinalIgnoreCase);
                foreach (var f in filesB) allInternal.Add(f);

                foreach (var internalPath in allInternal.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
                {
                    (long? size, DateTime? utc) metaA = (null, null);
                    (long? size, DateTime? utc) metaB = (null, null);

                    if (arcA != null) metaA = TryGetPakEntryMeta(arcA, internalPath);
                    if (arcB != null) metaB = TryGetPakEntryMeta(arcB, internalPath);

                    var status = CompareEntry(
                        metaA.size.HasValue ? new FileMeta { Size = metaA.size.Value, Utc = metaA.utc ?? DateTime.MinValue, FullPath = internalPath } : null,
                        metaB.size.HasValue ? new FileMeta { Size = metaB.size.Value, Utc = metaB.utc ?? DateTime.MinValue, FullPath = internalPath } : null
                    );

                    rows.Add(new CompareRow
                    {
                        Key = internalPath,                 // show internal path as the item
                        PathA = metaA.size.HasValue ? internalPath : null,
                        PathB = metaB.size.HasValue ? internalPath : null,
                        SizeA = metaA.size,
                        SizeB = metaB.size,
                        DateAUtc = metaA.utc,
                        DateBUtc = metaB.utc,
                        Status = status
                    });
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to read pak contents: {ex.Message}");
            }
            finally
            {
                try { arcA?.Dispose(); } catch { }
                try { arcB?.Dispose(); } catch { }
            }

            // Now show file-level compare (no pak counts)
            var leftTitle = string.IsNullOrEmpty(pakA) ? "-" : Path.GetFileName(pakA);
            var rightTitle = string.IsNullOrEmpty(pakB) ? "-" : Path.GetFileName(pakB);
            PopulateListViewForCompare(leftTitle, rightTitle, rows, includePakCounts: false);

            SetStatus($"Viewing {rows.Count:N0} files inside {leftTitle} ↔ {rightTitle}. (Backspace to go back)");
        }

        // enumerate lines from (listfile)
        private static IEnumerable<string> ReadListfile(MpqArchive arc)
        {
            try
            {
                using var s = arc.OpenFile("(listfile)");
                using var sr = new StreamReader(s, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        yield return line.Trim().Replace('/', '\\');
                }
            }
            finally { }
        }

        // try to fetch per-file size + time from an opened archive
        private static (long? size, DateTime? utc) TryGetPakEntryMeta(MpqArchive arc, string internalPath)
        {
            try
            {
                var size = (long)arc.GetFileSize(internalPath);
                var time = arc.GetFileTime(internalPath); // DateTime? from your wrapper
                return (size, time);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}
