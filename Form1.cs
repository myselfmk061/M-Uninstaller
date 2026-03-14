using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace MunInstaller
{
    public partial class Form1 : Form
    {
        private List<SoftwareInfo> allSoftware = new List<SoftwareInfo>();
        private List<SoftwareInfo> filteredSoftware = new List<SoftwareInfo>();
        private int sortColumn = 0;
        private bool sortAscending = true;

        // Dark Mode
        private bool isDarkMode = false;
        private ToolStripButton btnDarkMode;

        public Form1()
        {
            InitializeComponent();
        }

        // ============================================
        // FORM LOAD
        // ============================================
        private void Form1_Load(object sender, EventArgs e)
        {
            // Set Form Icon
            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            AddContextMenu();

            txtSearch.TextChanged -= TxtSearch_TextChanged;
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Enable multi-select for batch uninstall
            listViewPrograms.MultiSelect = true;

            // ADD: Dark Mode Button
            AddDarkModeButton();

            // UI Improvements
            ApplyModernTheme();

            LoadAllSoftware();
        }

        // ============================================
        // MODERN UI THEME
        // ============================================
        private void ApplyModernTheme()
        {
            // Form styling
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.Font = new Font("Segoe UI", 9);

            // ToolStrip styling
            toolStrip1.BackColor = Color.FromArgb(45, 45, 48);
            toolStrip1.ForeColor = Color.White;
            toolStrip1.Renderer = new ModernToolStripRenderer();

            // Buttons styling
            btnRefresh.ForeColor = Color.White;
            btnUninstall.ForeColor = Color.White;
            toolStripLabel1.ForeColor = Color.White;

            // Search box styling
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.BackColor = Color.White;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;

            // ListView styling
            listViewPrograms.BackColor = Color.White;
            listViewPrograms.Font = new Font("Segoe UI", 9);
            listViewPrograms.BorderStyle = BorderStyle.None;

            // Panel styling
            panelDetails.BackColor = Color.FromArgb(250, 250, 252);
            panelDetails.Font = new Font("Segoe UI", 9);

            // Detail labels styling
            lblDetailName.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblDetailName.ForeColor = Color.FromArgb(45, 45, 48);

            lblDetailVersion.ForeColor = Color.FromArgb(100, 100, 100);
            lblDetailPublisher.ForeColor = Color.FromArgb(100, 100, 100);

            // Status strip styling
            statusStrip1.BackColor = Color.FromArgb(0, 122, 204);
            toolStripStatusLabel.ForeColor = Color.White;
        }

        // ============================================
        // CONTEXT MENU (Right-Click)
        // ============================================
        private void AddContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem uninstallItem = new ToolStripMenuItem("Uninstall");
            uninstallItem.Click += BtnUninstall_Click;

            ToolStripMenuItem forceUninstallItem = new ToolStripMenuItem("Force Uninstall");
            forceUninstallItem.Click += ForceUninstall_Click;

            ToolStripMenuItem batchUninstallItem = new ToolStripMenuItem("🗑️ Batch Uninstall");
            batchUninstallItem.Click += BatchUninstall_Click;
            batchUninstallItem.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            ToolStripMenuItem openLocationItem = new ToolStripMenuItem("Open Install Location");
            openLocationItem.Click += OpenLocation_Click;

            contextMenu.Items.Add(uninstallItem);
            contextMenu.Items.Add(forceUninstallItem);
            contextMenu.Items.Add(batchUninstallItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(openLocationItem);

            listViewPrograms.ContextMenuStrip = contextMenu;
        }

        // ============================================
        // BATCH UNINSTALL (Multiple Programs)
        // ============================================
        private void BatchUninstall_Click(object sender, EventArgs e)
        {
            if (listViewPrograms.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one program!",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (listViewPrograms.SelectedItems.Count == 1)
            {
                MessageBox.Show("Please select multiple programs for batch uninstall!\n\n" +
                    "Tip: Hold Ctrl and click to select multiple programs.",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            List<SoftwareInfo> selectedPrograms = new List<SoftwareInfo>();
            foreach (ListViewItem item in listViewPrograms.SelectedItems)
            {
                selectedPrograms.Add(item.Tag as SoftwareInfo);
            }

            string programList = "";
            int count = 0;
            foreach (var prog in selectedPrograms)
            {
                count++;
                programList += $"{count}. {prog.Name}\n";
                if (count >= 10)
                {
                    programList += $"... and {selectedPrograms.Count - 10} more\n";
                    break;
                }
            }

            DialogResult result = MessageBox.Show(
                $"⚠️ BATCH UNINSTALL ⚠️\n\n" +
                $"You are about to uninstall {selectedPrograms.Count} programs:\n\n" +
                programList + "\n" +
                "This will launch each uninstaller one by one.\n\n" +
                "Continue?",
                "Batch Uninstall Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                PerformBatchUninstall(selectedPrograms);
            }
        }

        private void PerformBatchUninstall(List<SoftwareInfo> programs)
        {
            int successCount = 0;
            int failedCount = 0;
            int currentIndex = 0;
            int totalCount = programs.Count;

            this.Cursor = Cursors.WaitCursor;

            foreach (var software in programs)
            {
                currentIndex++;

                toolStripStatusLabel.Text =
                    $"Batch Uninstall: {currentIndex}/{totalCount} - {software.Name}";
                Application.DoEvents();

                try
                {
                    DialogResult proceed = MessageBox.Show(
                        $"Progress: {currentIndex}/{totalCount}\n\n" +
                        $"Uninstalling: {software.Name}\n" +
                        $"Publisher: {software.Publisher}\n\n" +
                        $"Success: {successCount} | Failed: {failedCount}\n\n" +
                        "Click OK to continue, Cancel to stop batch uninstall.",
                        $"Batch Uninstall [{currentIndex}/{totalCount}]",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Information);

                    if (proceed == DialogResult.Cancel)
                    {
                        MessageBox.Show(
                            $"Batch uninstall stopped!\n\n" +
                            $"Completed: {currentIndex - 1}/{totalCount}\n" +
                            $"Success: {successCount}\n" +
                            $"Failed: {failedCount}",
                            "Batch Uninstall Stopped",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        break;
                    }

                    // Check if Store app
                    if (software.RegistryKey == "Microsoft Store App")
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-Command \"Get-AppxPackage *{software.Name}* | Remove-AppxPackage\"",
                            UseShellExecute = true,
                            Verb = "runas",
                            CreateNoWindow = false
                        };
                        Process.Start(psi);
                    }
                    else
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c \"{software.UninstallString}\"",
                            UseShellExecute = true,
                            Verb = "runas",
                            CreateNoWindow = false
                        };
                        Process.Start(psi);
                    }

                    System.Threading.Thread.Sleep(2000);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;

                    DialogResult retry = MessageBox.Show(
                        $"Failed to uninstall: {software.Name}\n\n" +
                        $"Error: {ex.Message}\n\n" +
                        "Continue with remaining programs?",
                        "Uninstall Failed",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error);

                    if (retry == DialogResult.No)
                    {
                        break;
                    }
                }
            }

            this.Cursor = Cursors.Default;

            MessageBox.Show(
                $"🎯 BATCH UNINSTALL COMPLETED!\n\n" +
                $"Total Programs: {totalCount}\n" +
                $"✅ Successfully launched: {successCount}\n" +
                $"❌ Failed: {failedCount}\n\n" +
                "Refreshing program list...",
                "Batch Uninstall Summary",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            toolStripStatusLabel.Text = "Ready";
            LoadAllSoftware();
        }

        // ============================================
        // LOAD SOFTWARE (WITH SMART FILTERS)
        // ============================================
        private void LoadAllSoftware()
        {
            this.Cursor = Cursors.WaitCursor;

            toolStripProgressBar.Visible = true;
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
            toolStripStatusLabel.Text = "Loading installed programs...";
            Application.DoEvents();

            allSoftware.Clear();
            listViewPrograms.Items.Clear();

            try
            {
                string[] registryPaths = new string[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                };

                foreach (string path in registryPaths)
                {
                    LoadFromRegistry(Registry.LocalMachine, path);
                }

                LoadFromRegistry(Registry.CurrentUser,
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

                // Load Microsoft Store Apps
                toolStripStatusLabel.Text = "Loading Microsoft Store apps...";
                Application.DoEvents();
                LoadMicrosoftStoreApps();

                allSoftware = allSoftware.OrderBy(s => s.Name).ToList();
                filteredSoftware = new List<SoftwareInfo>(allSoftware);

                DisplaySoftware();
                UpdateStatusBar();

                toolStripStatusLabel.Text = $"Ready - {allSoftware.Count} programs loaded";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                toolStripProgressBar.Visible = false;
                this.Cursor = Cursors.Default;
                toolStripStatusLabel.Text = "Ready";
            }
        }

        private void LoadFromRegistry(RegistryKey rootKey, string path)
        {
            try
            {
                using (RegistryKey key = rootKey.OpenSubKey(path))
                {
                    if (key == null) return;

                    foreach (string subkeyName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                            {
                                if (subkey == null) continue;

                                string displayName = subkey.GetValue("DisplayName") as string;
                                string publisher = subkey.GetValue("Publisher") as string ?? "";
                                string uninstallString = subkey.GetValue("UninstallString") as string;

                                if (string.IsNullOrWhiteSpace(displayName)) continue;
                                if (string.IsNullOrWhiteSpace(uninstallString)) continue;
                                if (displayName.Length < 3) continue;
                                if (displayName.StartsWith("{") && displayName.EndsWith("}")) continue;

                                object systemComponent = subkey.GetValue("SystemComponent");
                                if (systemComponent != null && systemComponent.ToString() == "1")
                                    continue;

                                string parentKeyName = subkey.GetValue("ParentKeyName") as string;
                                if (!string.IsNullOrEmpty(parentKeyName))
                                    continue;

                                if (displayName.StartsWith("KB") ||
                                    displayName.StartsWith("Update for") ||
                                    displayName.Contains("Security Update") ||
                                    displayName.Contains("Hotfix") ||
                                    displayName.Contains("Definition Update") ||
                                    displayName.Contains("(KB"))
                                    continue;

                                string releaseType = subkey.GetValue("ReleaseType") as string;
                                if (!string.IsNullOrEmpty(releaseType))
                                {
                                    if (releaseType.Contains("Update") ||
                                        releaseType.Contains("Hotfix") ||
                                        releaseType.Contains("Security Update"))
                                        continue;
                                }

                                if (publisher.Contains("Microsoft Corporation"))
                                {
                                    if (displayName.Contains("Redistributable") && displayName.Length > 60)
                                        continue;
                                }

                                if (allSoftware.Any(s => s.Name == displayName))
                                    continue;

                                SoftwareInfo info = new SoftwareInfo
                                {
                                    Name = displayName,
                                    Version = subkey.GetValue("DisplayVersion") as string ?? "Unknown",
                                    Publisher = publisher == "" ? "Unknown" : publisher,
                                    InstallDate = FormatInstallDate(subkey.GetValue("InstallDate") as string),
                                    InstallLocation = subkey.GetValue("InstallLocation") as string ?? "Unknown",
                                    UninstallString = uninstallString,
                                    DisplayIcon = subkey.GetValue("DisplayIcon") as string,
                                    Size = GetSizeFromRegistry(subkey),
                                    RegistryKey = $"{rootKey.Name}\\{path}\\{subkeyName}"
                                };

                                allSoftware.Add(info);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        // ============================================
        // LOAD MICROSOFT STORE APPS
        // ============================================
        private void LoadMicrosoftStoreApps()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "Get-AppxPackage | Select-Object Name, Publisher, Version, InstallLocation | ConvertTo-Json",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (string.IsNullOrWhiteSpace(output)) return;

                    ParseStoreApps(output);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Store apps loading error: {ex.Message}");
            }
        }

        private void ParseStoreApps(string jsonOutput)
        {
            try
            {
                jsonOutput = jsonOutput.Trim('[', ']');
                string[] apps = jsonOutput.Split(new[] { "}," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string app in apps)
                {
                    try
                    {
                        string name = ExtractJsonValue(app, "Name");
                        if (string.IsNullOrWhiteSpace(name)) continue;

                        // Filter out system apps
                        if (name.StartsWith("Microsoft.Windows") ||
                            name.StartsWith("Microsoft.Xbox") ||
                            name.StartsWith("Microsoft.UI") ||
                            name.StartsWith("Microsoft.NET") ||
                            name.StartsWith("Microsoft.VCLibs") ||
                            name.StartsWith("Microsoft.Services") ||
                            name.StartsWith("Microsoft.ScreenSketch") ||
                            name.StartsWith("Microsoft.MicrosoftEdge") ||
                            name.StartsWith("Microsoft.DesktopAppInstaller") ||
                            name.StartsWith("Microsoft.StorePurchaseApp") ||
                            name.StartsWith("Microsoft.WindowsStore") ||
                            name.StartsWith("Microsoft.WindowsTerminal") ||
                            name.StartsWith("Microsoft.WindowsCalculator") ||
                            name.StartsWith("Microsoft.WindowsCamera") ||
                            name.StartsWith("Microsoft.WindowsAlarms") ||
                            name.StartsWith("Microsoft.WindowsMaps") ||
                            name.StartsWith("Microsoft.WindowsFeedbackHub") ||
                            name.StartsWith("Microsoft.WindowsSoundRecorder") ||
                            name.StartsWith("Microsoft.GetHelp") ||
                            name.StartsWith("Microsoft.Getstarted") ||
                            name.StartsWith("Microsoft.HEIFImageExtension") ||
                            name.StartsWith("Microsoft.HEVCVideoExtension") ||
                            name.StartsWith("Microsoft.WebMediaExtensions") ||
                            name.StartsWith("Microsoft.WebpImageExtension") ||
                            name.StartsWith("Microsoft.VP9VideoExtensions") ||
                            name.StartsWith("Microsoft.RawImageExtension") ||
                            name.Contains("InputApp") ||
                            name.Contains("ContentDeliveryManager") ||
                            name.Contains("SecHealthUI") ||
                            name.Contains("LockApp") ||
                            name.Contains("NarratorQuickStart") ||
                            name.Contains("ParentalControls") ||
                            name.Contains("Win32WebViewHost") ||
                            name.Contains("CloudExperienceHost") ||
                            name.Contains("AAD.BrokerPlugin") ||
                            name.Contains("AccountsControl") ||
                            name.Contains("AsyncTextService") ||
                            name.Contains("BioEnrollment") ||
                            name.Contains("CapturePicker") ||
                            name.Contains("CBSPreview") ||
                            name.Contains("CredDialogHost") ||
                            name.Contains("ECApp") ||
                            name.Contains("FileExplorer") ||
                            name.Contains("FilePicker") ||
                            name.Contains("MixedReality"))
                            continue;

                        // Make name readable
                        string displayName = name;
                        if (name.Contains("."))
                        {
                            string[] parts = name.Split('.');
                            if (parts.Length >= 2)
                            {
                                // Get last part as display name
                                displayName = parts[parts.Length - 1];

                                // Handle special cases
                                if (displayName == "App" && parts.Length >= 3)
                                    displayName = parts[parts.Length - 2];
                            }
                        }

                        // Check duplicate
                        if (allSoftware.Any(s => s.Name.ToLower() == displayName.ToLower()))
                            continue;

                        string publisher = ExtractJsonValue(app, "Publisher");
                        string version = ExtractJsonValue(app, "Version");
                        string installLocation = ExtractJsonValue(app, "InstallLocation");

                        // Clean publisher name
                        if (!string.IsNullOrEmpty(publisher))
                        {
                            if (publisher.Contains("CN="))
                            {
                                int cnIndex = publisher.IndexOf("CN=") + 3;
                                int commaIndex = publisher.IndexOf(",", cnIndex);
                                if (commaIndex > cnIndex)
                                    publisher = publisher.Substring(cnIndex, commaIndex - cnIndex);
                                else
                                    publisher = publisher.Substring(cnIndex);
                            }
                        }

                        SoftwareInfo info = new SoftwareInfo
                        {
                            Name = displayName + " (Store)",
                            Version = string.IsNullOrEmpty(version) ? "Unknown" : version,
                            Publisher = string.IsNullOrEmpty(publisher) ? "Microsoft Store" : publisher,
                            InstallDate = "Unknown",
                            InstallLocation = string.IsNullOrEmpty(installLocation) ? "Unknown" : installLocation,
                            UninstallString = $"powershell -Command \"Get-AppxPackage *{name}* | Remove-AppxPackage\"",
                            Size = 0,
                            RegistryKey = "Microsoft Store App"
                        };

                        allSoftware.Add(info);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JSON parsing error: {ex.Message}");
            }
        }

        private string ExtractJsonValue(string json, string key)
        {
            try
            {
                string searchKey = $"\"{key}\":";
                int startIndex = json.IndexOf(searchKey);
                if (startIndex == -1) return "";

                startIndex += searchKey.Length;

                while (startIndex < json.Length && (json[startIndex] == ' ' || json[startIndex] == '"'))
                    startIndex++;

                int endIndex = json.IndexOf('"', startIndex);
                if (endIndex == -1)
                {
                    endIndex = json.IndexOf(',', startIndex);
                    if (endIndex == -1)
                        endIndex = json.IndexOf('}', startIndex);
                }

                if (endIndex > startIndex)
                    return json.Substring(startIndex, endIndex - startIndex).Trim('"', ' ', ',');

                return "";
            }
            catch
            {
                return "";
            }
        }

        private long GetSizeFromRegistry(RegistryKey key)
        {
            try
            {
                object sizeObj = key.GetValue("EstimatedSize");
                if (sizeObj != null)
                {
                    return Convert.ToInt64(sizeObj) * 1024;
                }
            }
            catch { }
            return 0;
        }

        private string FormatInstallDate(string date)
        {
            if (string.IsNullOrEmpty(date) || date.Length != 8)
                return "Unknown";

            try
            {
                string year = date.Substring(0, 4);
                string month = date.Substring(4, 2);
                string day = date.Substring(6, 2);
                return $"{month}/{day}/{year}";
            }
            catch
            {
                return "Unknown";
            }
        }

        // ============================================
        // DISPLAY SOFTWARE
        // ============================================
        private void DisplaySoftware()
        {
            listViewPrograms.BeginUpdate();
            listViewPrograms.Items.Clear();

            int index = 0;
            foreach (var software in filteredSoftware)
            {
                ListViewItem item = new ListViewItem(software.Name);
                item.SubItems.Add(software.Publisher);
                item.SubItems.Add(software.Version);
                item.SubItems.Add(software.GetFormattedSize());
                item.SubItems.Add(software.InstallDate);
                item.Tag = software;

                // Theme-based colors
                if (isDarkMode)
                {
                    if (index % 2 == 0)
                        item.BackColor = Color.FromArgb(37, 37, 38);
                    else
                        item.BackColor = Color.FromArgb(45, 45, 48);

                    item.ForeColor = Color.White;

                    // Store apps - different color
                    if (software.RegistryKey == "Microsoft Store App")
                        item.ForeColor = Color.Cyan;
                }
                else
                {
                    if (index % 2 == 0)
                        item.BackColor = Color.White;
                    else
                        item.BackColor = Color.FromArgb(245, 245, 250);

                    item.ForeColor = Color.Black;

                    // Store apps - different color
                    if (software.RegistryKey == "Microsoft Store App")
                        item.ForeColor = Color.Blue;
                }

                listViewPrograms.Items.Add(item);
                index++;
            }

            listViewPrograms.EndUpdate();
        }

        // ============================================
        // SEARCH
        // ============================================
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                filteredSoftware = new List<SoftwareInfo>(allSoftware);
            }
            else
            {
                filteredSoftware = allSoftware.Where(s =>
                    s.Name.ToLower().Contains(searchText) ||
                    s.Publisher.ToLower().Contains(searchText)
                ).ToList();
            }

            DisplaySoftware();
            UpdateStatusBar();
        }

        // ============================================
        // LISTVIEW EVENTS
        // ============================================
        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewPrograms.SelectedItems.Count == 0)
            {
                ClearDetails();
                return;
            }

            SoftwareInfo software = listViewPrograms.SelectedItems[0].Tag as SoftwareInfo;

            if (software != null)
            {
                lblDetailName.Text = $"Name: {software.Name}";
                lblDetailVersion.Text = $"Version: {software.Version}";
                lblDetailPublisher.Text = $"Publisher: {software.Publisher}";
            }

            UpdateStatusBar();
        }

        private void ClearDetails()
        {
            lblDetailName.Text = "Name: -";
            lblDetailVersion.Text = "Version: -";
            lblDetailPublisher.Text = "Publisher: -";
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == sortColumn)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                sortColumn = e.Column;
                sortAscending = true;
            }

            SortList();
        }

        private void SortList()
        {
            switch (sortColumn)
            {
                case 0:
                    filteredSoftware = sortAscending ?
                        filteredSoftware.OrderBy(s => s.Name).ToList() :
                        filteredSoftware.OrderByDescending(s => s.Name).ToList();
                    break;
                case 1:
                    filteredSoftware = sortAscending ?
                        filteredSoftware.OrderBy(s => s.Publisher).ToList() :
                        filteredSoftware.OrderByDescending(s => s.Publisher).ToList();
                    break;
                case 2:
                    filteredSoftware = sortAscending ?
                        filteredSoftware.OrderBy(s => s.Version).ToList() :
                        filteredSoftware.OrderByDescending(s => s.Version).ToList();
                    break;
                case 3:
                    filteredSoftware = sortAscending ?
                        filteredSoftware.OrderBy(s => s.Size).ToList() :
                        filteredSoftware.OrderByDescending(s => s.Size).ToList();
                    break;
                case 4:
                    filteredSoftware = sortAscending ?
                        filteredSoftware.OrderBy(s => s.InstallDate).ToList() :
                        filteredSoftware.OrderByDescending(s => s.InstallDate).ToList();
                    break;
            }

            DisplaySoftware();
        }

        // ============================================
        // STATUS BAR
        // ============================================
        private void UpdateStatusBar()
        {
            long totalSize = filteredSoftware.Sum(s => s.Size);
            int selectedCount = listViewPrograms.SelectedItems.Count;
            int storeApps = filteredSoftware.Count(s => s.RegistryKey == "Microsoft Store App");

            toolStripStatusLabel.Text =
                $"{filteredSoftware.Count} Programs ({storeApps} Store Apps) | Selected: {selectedCount} | Total: {FormatBytes(totalSize)}";
        }

        private string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        // ============================================
        // REFRESH BUTTON
        // ============================================
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllSoftware();
        }

        // ============================================
        // UNINSTALL BUTTON
        // ============================================
        private void BtnUninstall_Click(object sender, EventArgs e)
        {
            if (listViewPrograms.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a program!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SoftwareInfo software = listViewPrograms.SelectedItems[0].Tag as SoftwareInfo;

            DialogResult result = MessageBox.Show(
                $"Uninstall: {software.Name}?\n\nPublisher: {software.Publisher}",
                "Confirm Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UninstallProgram(software);
            }
        }

        private void UninstallProgram(SoftwareInfo software)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                toolStripStatusLabel.Text = "Launching uninstaller...";
                Application.DoEvents();

                // Check if it's a Store app
                if (software.RegistryKey == "Microsoft Store App")
                {
                    DialogResult confirmStore = MessageBox.Show(
                        $"📦 Uninstall Microsoft Store App?\n\n{software.Name}\n\n" +
                        "This will use PowerShell to remove the app.",
                        "Confirm Store App Uninstall",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmStore == DialogResult.Yes)
                    {
                        // Extract original package name
                        string packageName = software.Name.Replace(" (Store)", "");

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-Command \"Get-AppxPackage *{packageName}* | Remove-AppxPackage\"",
                            UseShellExecute = true,
                            Verb = "runas",
                            CreateNoWindow = false
                        };

                        Process.Start(psi);

                        MessageBox.Show(
                            "✅ Store app uninstall command executed!\n\n" +
                            "Please wait a moment and then refresh the list (F5).",
                            "Info",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // Normal desktop app uninstall
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{software.UninstallString}\"",
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = false
                    };

                    Process.Start(psi);

                    MessageBox.Show(
                        "Uninstaller launched!\n\n" +
                        "If you see errors (like NSIS Error),\n" +
                        "use RIGHT-CLICK → 'Force Uninstall' instead.",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(
                    $"Normal uninstall failed!\n\n{ex.Message}\n\n" +
                    "Try Force Uninstall instead?",
                    "Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    ForceUninstall_Click(null, null);
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
                toolStripStatusLabel.Text = "Ready";
            }
        }

        // ============================================
        // FORCE UNINSTALL (For Corrupted Programs)
        // ============================================
        private void ForceUninstall_Click(object sender, EventArgs e)
        {
            if (listViewPrograms.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a program!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SoftwareInfo software = listViewPrograms.SelectedItems[0].Tag as SoftwareInfo;

            // Store apps can't be force uninstalled
            if (software.RegistryKey == "Microsoft Store App")
            {
                MessageBox.Show(
                    "⚠️ Force Uninstall is not available for Microsoft Store apps.\n\n" +
                    "Please use normal Uninstall for Store apps.",
                    "Not Available",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"⚠️ FORCE UNINSTALL ⚠️\n\n" +
                $"Program: {software.Name}\n\n" +
                "This will:\n" +
                "• Delete registry entry\n" +
                "• Remove install folder\n\n" +
                "Use only if normal uninstall fails!\n\n" +
                "Continue?",
                "Force Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                PerformForceUninstall(software);
            }
        }

        private void PerformForceUninstall(SoftwareInfo software)
        {
            try
            {
                PreviewForm previewForm = new PreviewForm(software);
                if (previewForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                ProgressForm progressForm = new ProgressForm();
                progressForm.Show();
                this.Enabled = false;

                int totalSteps = 3;
                int currentStep = 0;

                currentStep++;
                progressForm.UpdateProgress((currentStep * 100) / totalSteps,
                    "🗑️ Removing Registry Entry...",
                    software.RegistryKey);
                System.Threading.Thread.Sleep(500);

                try
                {
                    DeleteRegistryEntry(software.RegistryKey);
                }
                catch (Exception ex)
                {
                    progressForm.UpdateProgress((currentStep * 100) / totalSteps,
                        "⚠️ Registry deletion failed",
                        ex.Message);
                    System.Threading.Thread.Sleep(1000);
                }

                currentStep++;
                if (!string.IsNullOrEmpty(software.InstallLocation) &&
                    software.InstallLocation != "Unknown" &&
                    Directory.Exists(software.InstallLocation))
                {
                    progressForm.UpdateProgress((currentStep * 100) / totalSteps,
                        "📁 Deleting Installation Folder...",
                        software.InstallLocation);
                    System.Threading.Thread.Sleep(500);

                    try
                    {
                        Directory.Delete(software.InstallLocation, true);
                    }
                    catch (Exception ex)
                    {
                        progressForm.UpdateProgress((currentStep * 100) / totalSteps,
                            "⚠️ Folder deletion failed",
                            ex.Message);
                        System.Threading.Thread.Sleep(1000);
                    }
                }

                currentStep++;
                progressForm.UpdateProgress(100,
                    "✅ Force Uninstall Completed!",
                    "Refreshing list...");
                System.Threading.Thread.Sleep(1000);

                progressForm.Close();
                this.Enabled = true;

                MessageBox.Show(
                    "Force uninstall completed successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadAllSoftware();
            }
            catch (Exception ex)
            {
                this.Enabled = true;
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRegistryEntry(string registryKeyPath)
        {
            if (string.IsNullOrEmpty(registryKeyPath)) return;

            try
            {
                string[] parts = registryKeyPath.Split('\\');
                if (parts.Length < 3) return;

                RegistryKey rootKey = null;
                string rootName = parts[0];

                if (rootName.Contains("LOCAL_MACHINE") || rootName.Contains("LocalMachine"))
                {
                    rootKey = Registry.LocalMachine;
                }
                else if (rootName.Contains("CURRENT_USER") || rootName.Contains("CurrentUser"))
                {
                    rootKey = Registry.CurrentUser;
                }
                else
                {
                    return;
                }

                string subKeyPath = string.Join("\\", parts, 1, parts.Length - 2);
                string keyToDelete = parts[parts.Length - 1];

                using (RegistryKey key = rootKey.OpenSubKey(subKeyPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteSubKeyTree(keyToDelete, false);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Registry deletion failed: {ex.Message}");
            }
        }

        // ============================================
        // KEYBOARD SHORTCUTS (Enhanced)
        // ============================================
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.D:
                    ToggleDarkMode_Click(null, null);
                    return true;

                case Keys.F5:
                case Keys.Control | Keys.R:
                    BtnRefresh_Click(null, null);
                    return true;

                case Keys.Delete:
                case Keys.Control | Keys.U:
                    BtnUninstall_Click(null, null);
                    return true;

                case Keys.Control | Keys.Shift | Keys.U:
                    ForceUninstall_Click(null, null);
                    return true;

                case Keys.Control | Keys.Shift | Keys.B:
                    BatchUninstall_Click(null, null);
                    return true;

                case Keys.Control | Keys.L:
                    OpenLocation_Click(null, null);
                    return true;

                case Keys.Control | Keys.F:
                    txtSearch.Focus();
                    txtSearch.SelectAll();
                    return true;

                case Keys.Escape:
                    txtSearch.Clear();
                    txtSearch.Focus();
                    return true;

                case Keys.Control | Keys.A:
                    if (listViewPrograms.Focused)
                    {
                        foreach (ListViewItem item in listViewPrograms.Items)
                        {
                            item.Selected = true;
                        }
                        return true;
                    }
                    break;

                case Keys.F1:
                    ShowShortcutsHelp();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ============================================
        // SHOW KEYBOARD SHORTCUTS HELP
        // ============================================
        private void ShowShortcutsHelp()
        {
            string helpText = @"🎯 KEYBOARD SHORTCUTS

GENERAL:
  F5 / Ctrl+R         → Refresh program list
  Ctrl+F              → Focus search box
  Ctrl+D              → Toggle Dark Mode
  Esc                 → Clear search box
  F1                  → Show this help

PROGRAM ACTIONS:
  Delete / Ctrl+U     → Uninstall selected
  Ctrl+Shift+U        → Force Uninstall
  Ctrl+Shift+B        → Batch Uninstall
  Ctrl+L              → Open install location
  Ctrl+A              → Select all programs
  Double-Click        → Uninstall program

TIPS:
  • Hold Ctrl + Click to select multiple programs
  • Hold Shift + Click for range selection
  • Blue/Cyan text = Microsoft Store apps
";

            MessageBox.Show(helpText,
                "Keyboard Shortcuts - MunInstaller",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            BtnUninstall_Click(sender, e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Exit MunInstaller?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }

            base.OnFormClosing(e);
        }

        // ============================================
        // OPEN INSTALL LOCATION
        // ============================================
        private void OpenLocation_Click(object sender, EventArgs e)
        {
            if (listViewPrograms.SelectedItems.Count == 0) return;

            SoftwareInfo software = listViewPrograms.SelectedItems[0].Tag as SoftwareInfo;

            if (!string.IsNullOrEmpty(software.InstallLocation) &&
                software.InstallLocation != "Unknown" &&
                Directory.Exists(software.InstallLocation))
            {
                try
                {
                    Process.Start("explorer.exe", software.InstallLocation);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open folder:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Install location not found!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // DARK MODE
        // ============================================
        private void AddDarkModeButton()
        {
            btnDarkMode = new ToolStripButton
            {
                Text = "🌙 Dark Mode",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ForeColor = Color.White,
                Alignment = ToolStripItemAlignment.Right
            };
            btnDarkMode.Click += ToggleDarkMode_Click;

            toolStrip1.Items.Add(btnDarkMode);

            LoadThemePreference();
        }

        private void ToggleDarkMode_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();
            SaveThemePreference();
        }

        private void ApplyTheme()
        {
            if (isDarkMode)
            {
                ApplyDarkTheme();
                btnDarkMode.Text = "☀️ Light Mode";
            }
            else
            {
                ApplyLightTheme();
                btnDarkMode.Text = "🌙 Dark Mode";
            }
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);

            toolStrip1.BackColor = Color.FromArgb(20, 20, 20);
            toolStrip1.ForeColor = Color.White;
            btnRefresh.ForeColor = Color.White;
            btnUninstall.ForeColor = Color.White;
            toolStripLabel1.ForeColor = Color.White;
            btnDarkMode.ForeColor = Color.Yellow;

            txtSearch.BackColor = Color.FromArgb(45, 45, 48);
            txtSearch.ForeColor = Color.White;

            listViewPrograms.BackColor = Color.FromArgb(37, 37, 38);
            listViewPrograms.ForeColor = Color.White;

            panelDetails.BackColor = Color.FromArgb(45, 45, 48);
            lblDetailName.ForeColor = Color.White;
            lblDetailVersion.ForeColor = Color.LightGray;
            lblDetailPublisher.ForeColor = Color.LightGray;

            statusStrip1.BackColor = Color.FromArgb(0, 122, 204);
            toolStripStatusLabel.ForeColor = Color.White;

            RefreshListColors();
        }

        private void ApplyLightTheme()
        {
            this.BackColor = Color.FromArgb(240, 240, 245);

            toolStrip1.BackColor = Color.FromArgb(45, 45, 48);
            toolStrip1.ForeColor = Color.White;
            btnRefresh.ForeColor = Color.White;
            btnUninstall.ForeColor = Color.White;
            toolStripLabel1.ForeColor = Color.White;
            btnDarkMode.ForeColor = Color.White;

            txtSearch.BackColor = Color.White;
            txtSearch.ForeColor = Color.Black;

            listViewPrograms.BackColor = Color.White;
            listViewPrograms.ForeColor = Color.Black;

            panelDetails.BackColor = Color.FromArgb(250, 250, 252);
            lblDetailName.ForeColor = Color.FromArgb(45, 45, 48);
            lblDetailVersion.ForeColor = Color.FromArgb(100, 100, 100);
            lblDetailPublisher.ForeColor = Color.FromArgb(100, 100, 100);

            statusStrip1.BackColor = Color.FromArgb(0, 122, 204);
            toolStripStatusLabel.ForeColor = Color.White;

            RefreshListColors();
        }

        private void RefreshListColors()
        {
            int index = 0;
            foreach (ListViewItem item in listViewPrograms.Items)
            {
                SoftwareInfo software = item.Tag as SoftwareInfo;
                bool isStoreApp = software != null && software.RegistryKey == "Microsoft Store App";

                if (isDarkMode)
                {
                    if (index % 2 == 0)
                        item.BackColor = Color.FromArgb(37, 37, 38);
                    else
                        item.BackColor = Color.FromArgb(45, 45, 48);

                    item.ForeColor = isStoreApp ? Color.Cyan : Color.White;
                }
                else
                {
                    if (index % 2 == 0)
                        item.BackColor = Color.White;
                    else
                        item.BackColor = Color.FromArgb(245, 245, 250);

                    item.ForeColor = isStoreApp ? Color.Blue : Color.Black;
                }
                index++;
            }
        }

        private void SaveThemePreference()
        {
            try
            {
                string settingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MunInstaller");

                if (!Directory.Exists(settingsPath))
                    Directory.CreateDirectory(settingsPath);

                string settingsFile = Path.Combine(settingsPath, "settings.txt");
                File.WriteAllText(settingsFile, isDarkMode ? "dark" : "light");
            }
            catch { }
        }

        private void LoadThemePreference()
        {
            try
            {
                string settingsFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MunInstaller",
                    "settings.txt");

                if (File.Exists(settingsFile))
                {
                    string theme = File.ReadAllText(settingsFile).Trim().ToLower();
                    isDarkMode = (theme == "dark");
                    ApplyTheme();
                }
            }
            catch { }
        }
    }

    // ============================================
    // CUSTOM TOOLSTRIP RENDERER (Modern Look)
    // ============================================
    public class ModernToolStripRenderer : ToolStripProfessionalRenderer
    {
        public ModernToolStripRenderer() : base(new ModernColorTable()) { }
    }

    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(62, 62, 66);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(62, 62, 66);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(62, 62, 66);
        public override Color ButtonSelectedHighlight => Color.FromArgb(62, 62, 66);
        public override Color ButtonSelectedGradientBegin => Color.FromArgb(62, 62, 66);
        public override Color ButtonSelectedGradientEnd => Color.FromArgb(62, 62, 66);
        public override Color ToolStripGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color ToolStripGradientMiddle => Color.FromArgb(45, 45, 48);
        public override Color ToolStripGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
    }
}