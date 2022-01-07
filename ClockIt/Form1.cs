using ClockIt.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace ClockIt
{
    public partial class ClockIT : Form
    {



        public bool HasOpenBooking
        {
            get
            {
                if (_hasOpenBookingId == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }


            }
            set { _hasOpenBooking = value; }
        }

        private int _hasOpenBookingId = 0;
        private bool _hasOpenBooking;

        public ClockIT()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            this.CopyDatabase();

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
            var dataManager = new DataManager();
            var booking = await dataManager.GetOpenBooking();
            if (booking != null)
            {
                this._hasOpenBookingId = booking.Id;
            }
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipTitle = "ClockIt";
            notifyIcon1.BalloonTipText = "ClockIt has started and will now appear in your system tray";
            notifyIcon1.ShowBalloonTip(1000);
        }



        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.WindowState = FormWindowState.Minimized;
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.Show();
        }

        private async void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.HasOpenBooking)
            {
                MessageBox.Show($"Cannot create a new booking until you close existing booking", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string category = "";
                CategoryForm categoryForm = new CategoryForm();
                categoryForm.ShowDialog();
                category = categoryForm.CategoryValue;
                categoryForm.Close();
                categoryForm.Dispose();
               
                // test github
                // This needs moving to the Category form
                var dataManager = new DataManager();
                var startBooking = new StartClockModel
                {
                    Category = category,
                    Start = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };
                this._hasOpenBookingId = await dataManager.StartRecording(startBooking);
                MessageBox.Show($"Started booking with Id = {this._hasOpenBookingId} ");
            }
        }

        private async void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.HasOpenBooking)
            {
                MessageBox.Show($"There are no open bookings to close", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var dataManager = new DataManager();
                var stopBooking = new StopClockModel
                {
                    Id = this._hasOpenBookingId,
                    End = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };
                this._hasOpenBookingId = await dataManager.StopRecording(stopBooking);
                MessageBox.Show($"Booking {stopBooking.Id} has been closed");

            }
        }

        private void CopyDatabase()
        {
            string result = Assembly.GetExecutingAssembly().Location;
            int index = result.LastIndexOf("\\");
            string dbPath = $"{result.Substring(0, index)}\\clockIt.db";

            string destinationPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\ClockIt\\clockIt.db";
            string destinationFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\ClockIt\\";

            if (!File.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationFolder);
                File.Copy(dbPath, destinationPath, true);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.HasOpenBooking)
            {
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
                notifyIcon1.BalloonTipTitle = "Open Booking";
                notifyIcon1.BalloonTipText = "You still have an open booking.  Do you want to close it?";
                notifyIcon1.ShowBalloonTip(3000);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
