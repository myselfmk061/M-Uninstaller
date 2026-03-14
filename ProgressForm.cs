using System;
using System.Drawing;
using System.Windows.Forms;

namespace MunInstaller
{
    public partial class ProgressForm : Form
    {
        private ProgressBar progressBar;
        private Label lblStatus;
        private Label lblPercentage;
        private Label lblCurrentAction;

        public ProgressForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Force Uninstalling...";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;

            Label lblHeader = new Label
            {
                Text = "🔧 Force Uninstall in Progress",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(460, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue
            };

            progressBar = new ProgressBar
            {
                Location = new Point(10, 50),
                Size = new Size(360, 30),
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100
            };

            lblPercentage = new Label
            {
                Text = "0%",
                Location = new Point(380, 50),
                Size = new Size(90, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Green
            };

            lblCurrentAction = new Label
            {
                Text = "Starting...",
                Location = new Point(10, 90),
                Size = new Size(460, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            lblStatus = new Label
            {
                Text = "Initializing...",
                Location = new Point(10, 120),
                Size = new Size(460, 30),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Black
            };

            this.Controls.Add(lblHeader);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblPercentage);
            this.Controls.Add(lblCurrentAction);
            this.Controls.Add(lblStatus);
        }

        public void UpdateProgress(int percentage, string action, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(percentage, action, status)));
                return;
            }

            progressBar.Value = Math.Min(percentage, 100);
            lblPercentage.Text = $"{percentage}%";
            lblCurrentAction.Text = action;
            lblStatus.Text = status;
            Application.DoEvents();
        }
    }
}