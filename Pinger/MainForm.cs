using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using Pinger.Properties;
using System.Threading;

namespace Pinger
{
    public partial class MainForm : Form
    {
        const string dataFile = "data.txt";
        Bitmap emptyBitmap = new Bitmap(1, 1);

        public MainForm()
        {
            InitializeComponent();

            if (File.Exists(dataFile))
            {
                var data = File.ReadAllLines(dataFile);
                for (int i = 0; i < data.Length; i += 2)
                    AddRow(data[i], data[i + 1]);
            }
            RefreshStatus();
        }

        private void AddToolStripButton_Click(object sender, EventArgs e)
        {
            var form = new AddForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                AddRow(form.name, form.url);
                Save();
            }
        }

        private void AddRow(string name, string url)
        {
            dataGridView.Rows.Add();
            dataGridView["NameColumn", dataGridView.RowCount - 1].Value = name;
            dataGridView["URL", dataGridView.RowCount - 1].Value = url;
            dataGridView["Image", dataGridView.RowCount - 1].Value = emptyBitmap;
        }

        private void Save()
        {
            List<string> str = new List<string>();
            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                str.Add((string)dataGridView["NameColumn", i].Value);
                str.Add((string)dataGridView["URL", i].Value);
            }
            File.WriteAllLines(dataFile, str.ToArray());
        }

        private void RemoveToolStripButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
                dataGridView.Rows.Remove(row);
            Save();
        }

        private void normalTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView.RowCount; i++)
                if (dataGridView["Time", i].Value != null)
                    PingItem((string)dataGridView["URL", i].Value);            
        }

        private async void PingItem(string url)
        {
            using (Ping ping = new Ping())
            {
                PingReply reply = null;
                try
                {
                    reply = await ping.SendPingAsync(url);
                }
                catch { }

                int i;
                for (i = 0; i < dataGridView.RowCount; i++)
                    if (dataGridView["URL", i].Value.ToString() == url)
                        break;
                if (i == dataGridView.RowCount)
                    return;

                if (reply != null && reply.Status == IPStatus.Success)
                {
                    dataGridView["Time", i].Value = reply.RoundtripTime;
                    dataGridView["Image", i].Value = Resources.Sample_07;
                }
                else
                {
                    dataGridView["Time", i].Value = null;
                    dataGridView["Image", i].Value = Resources.Sample_02;
                }
            }
            RefreshStatus();
            GC.Collect();
        }

        private void errorTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView.RowCount; i++)
                if (dataGridView["Time", i].Value == null)
                    PingItem((string)dataGridView["URL", i].Value);
        }

        private void RefreshStatus()
        {
            bool foundError = false;
            bool foundSuccess = false;
            for (int i = 0; i < dataGridView.RowCount; i++)
                if (dataGridView["Time", i].Value == null)
                    foundError = true;
                else
                    foundSuccess = true;

            if (dataGridView.SortOrder == SortOrder.Ascending)
                dataGridView.Sort(dataGridView.SortedColumn, ListSortDirection.Ascending);
            else if (dataGridView.SortOrder == SortOrder.Descending)
                dataGridView.Sort(dataGridView.SortedColumn, ListSortDirection.Descending);

            if (foundSuccess && !foundError)
            {
                toolStripStatusLabel1.Text = "All Successful";
                toolStripStatusLabel1.Image = Resources.Sample_07;
                notifyIcon.Icon = Resources.Sample_071;
            }
            else if (foundSuccess && foundError)
            {
                toolStripStatusLabel1.Text = "Partial Successful";
                toolStripStatusLabel1.Image = Resources.info;
                notifyIcon.Icon = Resources.info1;
            }
            else
            {
                toolStripStatusLabel1.Text = "All Timedout";
                toolStripStatusLabel1.Image = Resources.Sample_02;
                notifyIcon.Icon = Resources.Sample_021;
            }

            notifyIcon.Text = toolStripStatusLabel1.Text;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
    }
}
