using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MetaDataStringEditor {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
            Text += " " + Application.ProductVersion;

            Logger.LogAction += delegate (string msg) {
                if (InvokeRequired) {
                    Invoke(new Action(delegate { toolStripStatusLabel1.Text = msg; }));
                } else {
                    toolStripStatusLabel1.Text = msg;
                }
            };

            ProgressBar.SetMaxAction += delegate (int max) {
                if (InvokeRequired) {
                    Invoke(new Action(delegate {
                        toolStripProgressBar1.Maximum = max;
                        toolStripProgressBar1.Value = 0;
                    }));
                } else {
                    toolStripProgressBar1.Maximum = max;
                    toolStripProgressBar1.Value = 0;
                }
            };

            ProgressBar.PlusOneAction += delegate {
                if (InvokeRequired) {
                    Invoke(new Action(delegate { toolStripProgressBar1.Value++; }));
                } else {
                    toolStripProgressBar1.Value++;
                }
            };

            ProgressBar.ReportAction += delegate (int val) {
                if (InvokeRequired) {
                    Invoke(new Action(delegate { toolStripProgressBar1.Value = val; }));
                } else {
                    toolStripProgressBar1.Value = val;
                }
            };
        }

        private FormStatus status = FormStatus.Waiting;
        private MetadataFile file;
        private EditForm editForm = new EditForm();

        // Menu Bar
        private void LoadToolStripMenuItem_Click(object sender, EventArgs e) {
            if (status == FormStatus.Loading || status == FormStatus.Saving) {
                Logger.E("Background operation in progress");
                return;
            }

            openFileDialog1.FileName = "global-metadata.dat";
            openFileDialog1.Filter = "global-metadata.dat|global-metadata.dat|all|*.*";
            if (openFileDialog1.ShowDialog(this) != DialogResult.OK) return;

            LoadFile(openFileDialog1.FileName);
        }

        private void LoadFile(string fullName) {
            status = FormStatus.Loading;
            ClearForm();
            ThreadPool.QueueUserWorkItem(delegate {
                try {
                    file = new MetadataFile(fullName);
                    Invoke(new Action(delegate { Text = fullName; }));
                    Invoke(new Action(RefreshListView));
                    status = FormStatus.Editing;
                    Logger.I("Download finished");
                } catch (Exception ex) {
                    Logger.E(ex.ToString());
                    file?.Dispose();
                    file = null;
                    status = FormStatus.Waiting;
                }
            });
        }

        private void RefreshListView() {
            Logger.I("Refresh the list");

            listView1.BeginUpdate();
            for (int i = 0; i < file.strBytes.Count; i++) {
                EditorListItem item = new EditorListItem(file.strBytes[i]) {
                    Tag = i,
                    Text = i.ToString(),
                };
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (status != FormStatus.Editing) {
                Logger.E("Status error");
                return;
            }

            saveFileDialog1.FileName = "global-metadata.dat";
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK) {
                status = FormStatus.Saving;
                
                ThreadPool.QueueUserWorkItem(delegate {
                    file.WriteToNewFile(saveFileDialog1.FileName);
                    status = FormStatus.Editing;
                });
            }
        }

        private void CloseFileToolStripMenuItem_Click(object sender, EventArgs e) {
            ClearForm();
            status = FormStatus.Waiting;
        }

        // Search
        private void button1_Click(object sender, EventArgs e) {
            if (textBox1.Text.Length > 0)
                SearchToNext();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r' && textBox1.Text.Length > 0)
                SearchToNext();
        }

        private void SearchToNext() {
            string keyWord = textBox1.Text;
            int start = listView1.SelectedIndices.Count > 0 ? listView1.SelectedIndices[0] : -1;
            for (int i = 0; i < listView1.Items.Count; i++) {
                var item = listView1.Items[(i + start + 1) % listView1.Items.Count] as EditorListItem;
                if (item.MatchKeyWord(keyWord)) {
                    item.Selected = true;
                    item.EnsureVisible();
                    return;
                }

            }
            Logger.I("No search string found");
        }

        // Modify

        private void ListView1_MouseClick(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                item.Selected = true;
                contextMenuStrip1.Show(listView1, e.Location);
            }
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e) {
            var item = listView1.SelectedItems[0] as EditorListItem;
            StartEditor(item);
        }


        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0] as EditorListItem;
            StartEditor(item);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) != true) return;

            e.Effect = DragDropEffects.Link;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            
            if (files == null || files.Length == 0) return;
            if (Path.GetFileName(files[0]) != "global-metadata.dat" && MessageBox.Show(string.Format("The file name({0}) is not \"global-metadata.dat\", do you still want to load it?", 
                Path.GetFileName(files[0])), "global-metadata.dat", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            LoadFile(files[0]);
        }

        private void ExportTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items == null || listView1.Items.Count == 0) return;

            saveFileDialog1.FileName = "global-metadata.dat.txt";
            if (saveFileDialog1.ShowDialog(this) != DialogResult.OK) return;

            using (StreamWriter writer = new StreamWriter(saveFileDialog1.FileName))
            {
                for (int idx = 0; idx < listView1.Items.Count; idx++)
                {
                    EditorListItem item = listView1.Items[idx] as EditorListItem;
                    writer.WriteLine("global-metadata:" + idx);
                    writer.WriteLine(item.SubItems[1].Text);
                }
            }
        }

        private void ImportTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items == null || listView1.Items.Count == 0) return;

            openFileDialog1.FileName = "global-metadata.dat.txt";
            openFileDialog1.Filter = "global-metadata.dat.txt|global-metadata.dat.txt|all|*.*";
            if (openFileDialog1.ShowDialog(this) != DialogResult.OK) return;

            string importTxt = File.ReadAllText(openFileDialog1.FileName);
            if (importTxt == null || importTxt.Length == 0) return;

            string[] globalMetadatas = importTxt.Split(new string[] {"global-metadata:"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string globalMetadata in globalMetadatas)
            {
                string[] texts = globalMetadata.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                if (texts == null || texts.Length < 2 || !int.TryParse(texts[0], out int idx)) continue;

                string newStr = texts[texts.Length - 1];
                EditorListItem item = listView1.Items[idx] as EditorListItem;

                if (item.SubItems[1].Text != newStr)
                {
                    if (newStr.Contains("\\n") || newStr.Contains("\\r")) newStr = newStr.Replace("\\r\\n", "\r\n").Replace("\\n", "\n").Replace("\\r", "\r");
                    item.SetNewStr(newStr);
                    file.strBytes[(int)item.Tag] = item.NewStrBytes;
                }
            }
        }

        private void StartEditor(EditorListItem item)
        {
            editForm.ShowDialog(this, item);
            if (item.IsEdit)
                file.strBytes[(int)item.Tag] = item.NewStrBytes;
            else
                file.strBytes[(int)item.Tag] = item.OriginStrBytes;
        }

        // Universal
        private void ClearForm() {
            listView1.Items.Clear();
            file?.Dispose();
            file = null;
            Text = "MetadataStringEditor " + Application.ProductVersion;
        }

        private enum FormStatus { Waiting, Loading, Saving, Editing }

    }

    public static class Logger {
        public static Action<string> LogAction;

        private static void Log(string level, string msg) {
            LogAction($"[{level}] {msg}");
        }

        public static void D(string msg) { Log("debug", msg); }
        public static void I(string msg) { Log("info", msg); }
        public static void E(string msg) { Log("error", msg); }
    }

    public static class ProgressBar {
        public static Action<int> SetMaxAction;
        public static Action PlusOneAction;
        public static Action<int> ReportAction;

        public static void SetMax(int max) => SetMaxAction(max);
        public static void Report(int val) => ReportAction(val);
        public static void Report() => PlusOneAction();
    }

}
