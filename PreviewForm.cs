using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MunInstaller
{
    public partial class PreviewForm : Form
    {
        private SoftwareInfo software;
        private ListView listViewItems;
        private Label lblTotalItems;
        private Label lblTotalSize;
        private Button btnProceed;
        private Button btnCancel;

        public PreviewForm(SoftwareInfo soft)
        {
            software = soft;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Preview - What will be removed?";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblHeader = new Label
            {
                Text = $"⚠️ Force Uninstall: {software.Name}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(660, 30),
                ForeColor = Color.DarkRed
            };

            listViewItems = new ListView
            {
                Location = new Point(10, 50),
                Size = new Size(660, 320),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                CheckBoxes = true
            };

            listViewItems.Columns.Add("Type", 100);
            listViewItems.Columns.Add("Item", 450);
            listViewItems.Columns.Add("Size", 100);

            lblTotalItems = new Label
            {
                Location = new Point(10, 380),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            lblTotalSize = new Label
            {
                Location = new Point(10, 405),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            btnProceed = new Button
            {
                Text = "✔️ Proceed with Uninstall",
                Location = new Point(450, 380),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button
            {
                Text = "✖️ Cancel",
                Location = new Point(450, 425),
                Size = new Size(220, 30),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblHeader);
            this.Controls.Add(listViewItems);
            this.Controls.Add(lblTotalItems);
            this.Controls.Add(lblTotalSize);
            this.Controls.Add(btnProceed);
            this.Controls.Add(btnCancel);

            LoadPreviewData();
        }

        private void LoadPreviewData()
        {
            listViewItems.Items.Clear();
            long totalSize = 0;
            int itemCount = 0;

            ListViewItem regItem = new ListViewItem("Registry");
            regItem.SubItems.Add(software.RegistryKey);
            regItem.SubItems.Add("-");
            regItem.Checked = true;
            listViewItems.Items.Add(regItem);
            itemCount++;

            if (!string.IsNullOrEmpty(software.InstallLocation) &&
                software.InstallLocation != "Unknown" &&
                Directory.Exists(software.InstallLocation))
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(software.InstallLocation);

                    FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);

                    foreach (FileInfo file in files)
                    {
                        ListViewItem fileItem = new ListViewItem("File");
                        fileItem.SubItems.Add(file.FullName);
                        fileItem.SubItems.Add(FormatBytes(file.Length));
                        fileItem.Checked = true;
                        listViewItems.Items.Add(fileItem);
                        totalSize += file.Length;
                        itemCount++;
                    }

                    DirectoryInfo[] dirs = dirInfo.GetDirectories("*", SearchOption.AllDirectories);
                    foreach (DirectoryInfo dir in dirs)
                    {
                        ListViewItem dirItem = new ListViewItem("Folder");
                        dirItem.SubItems.Add(dir.FullName);
                        dirItem.SubItems.Add("-");
                        dirItem.Checked = true;
                        listViewItems.Items.Add(dirItem);
                        itemCount++;
                    }
                }
                catch (Exception ex)
                {
                    ListViewItem errorItem = new ListViewItem("Error");
                    errorItem.SubItems.Add($"Could not scan folder: {ex.Message}");
                    errorItem.SubItems.Add("-");
                    errorItem.ForeColor = Color.Red;
                    listViewItems.Items.Add(errorItem);
                }
            }

            lblTotalItems.Text = $"📦 Total Items: {itemCount}";
            lblTotalSize.Text = $"💾 Total Size: {FormatBytes(totalSize)}";
        }

        private string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 B";
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public List<string> GetSelectedItems()
        {
            List<string> items = new List<string>();
            foreach (ListViewItem item in listViewItems.CheckedItems)
            {
                items.Add(item.SubItems[1].Text);
            }
            return items;
        }
    }
}