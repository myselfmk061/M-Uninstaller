namespace MunInstaller
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            menuStrip1 = new MenuStrip();
            toolStrip1 = new ToolStrip();
            btnRefresh = new ToolStripButton();
            btnUninstall = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripLabel1 = new ToolStripLabel();
            toolStripButton1 = new ToolStripButton();
            txtSearch = new TextBox();
            listViewPrograms = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            panelDetails = new Panel();
            lblDetailPublisher = new Label();
            lblDetailVersion = new Label();
            lblDetailName = new Label();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            toolStripProgressBar = new ToolStripProgressBar();
            toolStrip1.SuspendLayout();
            panelDetails.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 3, 0, 3);
            menuStrip1.Size = new Size(1143, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnRefresh, btnUninstall, toolStripSeparator1, toolStripLabel1, toolStripButton1 });
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1143, 27);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRefresh.ImageTransparentColor = Color.Magenta;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(87, 24);
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // btnUninstall
            // 
            btnUninstall.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnUninstall.ImageTransparentColor = Color.Magenta;
            btnUninstall.Name = "btnUninstall";
            btnUninstall.Size = new Size(95, 24);
            btnUninstall.Text = "🗑️ Uninstall";
            btnUninstall.Click += BtnUninstall_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 27);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(56, 24);
            toolStripLabel1.Text = "Search:";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.None;
            toolStripButton1.Enabled = false;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(29, 24);
            toolStripButton1.Text = "toolStripButton1";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(274, 24);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(184, 27);
            txtSearch.TabIndex = 5;
            // 
            // listViewPrograms
            // 
            listViewPrograms.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5 });
            listViewPrograms.Dock = DockStyle.Fill;
            listViewPrograms.FullRowSelect = true;
            listViewPrograms.GridLines = true;
            listViewPrograms.Location = new Point(0, 51);
            listViewPrograms.Margin = new Padding(3, 4, 3, 4);
            listViewPrograms.Name = "listViewPrograms";
            listViewPrograms.Size = new Size(832, 782);
            listViewPrograms.TabIndex = 2;
            listViewPrograms.UseCompatibleStateImageBehavior = false;
            listViewPrograms.View = View.Details;
            listViewPrograms.ColumnClick += ListView_ColumnClick;
            listViewPrograms.SelectedIndexChanged += ListView_SelectedIndexChanged;
            listViewPrograms.DoubleClick += ListView_DoubleClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Program Name";
            columnHeader1.Width = 300;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Publisher";
            columnHeader2.Width = 180;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Version";
            columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Size";
            columnHeader4.Width = 80;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Install Date";
            columnHeader5.Width = 100;
            // 
            // panelDetails
            // 
            panelDetails.BorderStyle = BorderStyle.FixedSingle;
            panelDetails.Controls.Add(lblDetailPublisher);
            panelDetails.Controls.Add(lblDetailVersion);
            panelDetails.Controls.Add(lblDetailName);
            panelDetails.Dock = DockStyle.Right;
            panelDetails.Location = new Point(832, 51);
            panelDetails.Margin = new Padding(3, 4, 3, 4);
            panelDetails.Name = "panelDetails";
            panelDetails.Padding = new Padding(11, 13, 11, 13);
            panelDetails.Size = new Size(311, 782);
            panelDetails.TabIndex = 3;
            // 
            // lblDetailPublisher
            // 
            lblDetailPublisher.AutoSize = true;
            lblDetailPublisher.Location = new Point(15, 93);
            lblDetailPublisher.Name = "lblDetailPublisher";
            lblDetailPublisher.Size = new Size(82, 20);
            lblDetailPublisher.TabIndex = 2;
            lblDetailPublisher.Text = "Publisher: -";
            // 
            // lblDetailVersion
            // 
            lblDetailVersion.AutoSize = true;
            lblDetailVersion.Location = new Point(15, 53);
            lblDetailVersion.Name = "lblDetailVersion";
            lblDetailVersion.Size = new Size(70, 20);
            lblDetailVersion.TabIndex = 1;
            lblDetailVersion.Text = "Version: -";
            // 
            // lblDetailName
            // 
            lblDetailName.AutoSize = true;
            lblDetailName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDetailName.Location = new Point(15, 13);
            lblDetailName.Name = "lblDetailName";
            lblDetailName.Size = new Size(65, 20);
            lblDetailName.TabIndex = 0;
            lblDetailName.Text = "Name: -";
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel, toolStripProgressBar });
            statusStrip1.Location = new Point(0, 833);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new Size(1143, 26);
            statusStrip1.TabIndex = 4;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(50, 20);
            toolStripStatusLabel.Text = "Ready";
            // 
            // toolStripProgressBar
            // 
            toolStripProgressBar.Name = "toolStripProgressBar";
            toolStripProgressBar.Size = new Size(171, 19);
            toolStripProgressBar.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1143, 859);
            Controls.Add(txtSearch);
            Controls.Add(listViewPrograms);
            Controls.Add(panelDetails);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "M_Uninstaller - Professional Uninstaller";
            Load += Form1_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panelDetails.ResumeLayout(false);
            panelDetails.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripButton btnUninstall;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ListView listViewPrograms;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Panel panelDetails;
        private System.Windows.Forms.Label lblDetailPublisher;
        private System.Windows.Forms.Label lblDetailVersion;
        private System.Windows.Forms.Label lblDetailName;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private ToolStripButton toolStripButton1;
    }
}