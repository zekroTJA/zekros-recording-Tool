﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recTimer.Properties;
using System.Windows.Forms;

namespace recTimer
{
    public partial class frmSettings : Form
    {

        public static string recHDD;
        public static string recKey;
        public static string recFolder;
        public static bool updates;
        public static bool showMessageCheckAlternateHookSetting = true;
        public static int timerMarkAfter;
        globalKeyboardHook hook = new globalKeyboardHook();

        public frmSettings()
        {

            InitializeComponent();
            if (Settings.Default["recHDD"].ToString() != "")
            {
                tbRecHDD.Text = Settings.Default["recHDD"].ToString();
            }

            if (Settings.Default["recKey"].ToString() != "")
            {
                cbRecKey.Text = Settings.Default["recKey"].ToString();
            }

            if (Settings.Default["markKey"].ToString() != "")
            {
                cbMarkKey.Text = Settings.Default["markKey"].ToString();
            }

            if (Settings.Default["programm1"].ToString() != "")
            {
                tbProgramm1.Text = Settings.Default["programm1"].ToString();
            }
            if (Settings.Default["programm2"].ToString() != "")
            {
                tbProgramm2.Text = Settings.Default["programm2"].ToString();
            }
            if (Settings.Default["programm3"].ToString() != "")
            {
                tbProgramm3.Text = Settings.Default["programm3"].ToString();
            }
            if (Settings.Default["programm4"].ToString() != "")
            {
                tbProgramm4.Text = Settings.Default["programm4"].ToString();
            }
            if (Settings.Default["programm5"].ToString() != "")
            {
                tbProgramm5.Text = Settings.Default["programm5"].ToString();
            }

            if (Convert.ToBoolean(Settings.Default["updates"]))
            {
                cbUpdates.Checked = true;
            } else
            {
                cbUpdates.Checked = false;
            }

            if (Convert.ToBoolean(Settings.Default["alternateHook"]))
            {
                cbAlternateHook.Checked = true;
            } else
            {
                cbAlternateHook.Checked = false;
            }

            if (Convert.ToBoolean(Settings.Default["alwaysOnTop"]))
            {
                cbAlwaysOnTop.Checked = true;
            }
            else
            {
                cbAlwaysOnTop.Checked = false;
            }

            tbTimer.Value = Convert.ToInt16(Settings.Default["timerMarker"]);
            lbTimerMarker.Text = Convert.ToInt16(Settings.Default["timerMarker"]).ToString() + " Minuten" ;
            timerMarkAfter = Convert.ToInt32(Settings.Default["timerMarker"]);
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }

        private void tbRecHDD_TextChanged(object sender, EventArgs e)
        {
            tbRecHDD.Text = clsConst.recHDD;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tbRecHDD_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveAll();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveAll();
            this.Close();
        }

        //SAVE ALL
        private void saveAll()
        {
            recHDD = tbRecHDD.Text;
            Settings.Default["markKey"] = cbMarkKey.Text;
            Settings.Default["recHDD"] = tbRecHDD.Text;
            Settings.Default["recKey"] = cbRecKey.Text;
            Settings.Default["recFolder"] = tbRecFolder.Text;

            Settings.Default["programm1"] = tbProgramm1.Text;
            Settings.Default["programm2"] = tbProgramm2.Text;
            Settings.Default["programm3"] = tbProgramm3.Text;
            Settings.Default["programm4"] = tbProgramm4.Text;
            Settings.Default["programm5"] = tbProgramm5.Text;

            Settings.Default["updates"] = cbUpdates.Checked;
            Settings.Default["alternateHook"] = cbAlternateHook.Checked;
            Settings.Default["alwaysOnTop"] = cbAlwaysOnTop.Checked;

            Settings.Default["timerMarker"] = Convert.ToInt32(tbTimer.Value);
            timerMarkAfter = Convert.ToInt32(Settings.Default["timerMarker"]);

            Settings.Default.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Start();
            folderBrowserDialog1.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tbRecFolder.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            folderBrowserDialog2.ShowDialog();
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            tbProgramm1.Text = "";
            tbProgramm2.Text = "";
            tbProgramm3.Text = "";
            tbProgramm4.Text = "";
            tbProgramm5.Text = "";
        }

        private void cbAlternateHook_CheckedChanged(object sender, EventArgs e)
        {
            Form currentform = Application.OpenForms["frmSettings"];
            if (currentform != null && showMessageCheckAlternateHookSetting)
            {
                string message = "Um die Änderung wirksam zu machen ist es erforderlich das Programm neu zu starten!";
                const string caption = "ACHTUNG!";
                var result = MessageBox.Show(message, caption,
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                showMessageCheckAlternateHookSetting = false;
            }
                              
        }

        private void tbTimer_Scroll(object sender, EventArgs e)
        {
            lbTimerMarker.Text = Convert.ToInt16(tbTimer.Value).ToString() + " Minuten";
        }
    }
}
