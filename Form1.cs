﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using recTimer.Properties;
using System.Threading;
using System.Runtime.InteropServices;


namespace recTimer
{
    public partial class Form1 : Form
    {

        #region vars
        int tSS = 0;
        int tMM = 0;
        int tHH = 0;
        int numbMarks = 1;
        bool keyWasPressed = false;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        globalKeyboardHook hook = new globalKeyboardHook();

        const int MOD_CONTROL = 0x0002;
        const int MOD_SHIFT = 0x0004;
        const int WM_HOTKEY = 0x0312;
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Je nach Einstellung wird die globalkeyhook oder alternative Hook aktiviert.
            if (Convert.ToBoolean(Settings.Default["alternateHook"]))
            {
                recAltKeyHook();
                markAltKeyHook();
            }
            else {
                timerKeyboardHook.Start();
            }

            timerCPU.Start();
            lbVersion.Text = "preAlpha v." + clsConst.buildVersion + "a";

            //Wenn Updatenotification aktiviert ist wird der Update testausgeführt, sonst Warnung.
            if (Convert.ToBoolean(Settings.Default["updates"]))
            {
                clsUpdate.testForUpdate();
            } else
            {
                lbUpdateWarn.Text = "WARNUNG!";
            }
        }

        //Alternative Hotkey Hook
        protected override void WndProc(ref Message m)
        {
            
            if (Convert.ToBoolean(Settings.Default["alternateHook"]))
            {
            
                if (m.Msg == WM_HOTKEY && (int)m.WParam == 1)
                {
                    hookAlt_keyUp();
                }
                if (m.Msg == WM_HOTKEY && (int)m.WParam == 2)
                {
                    hookAlt_keyUpMark();
                }
                base.WndProc(ref m);
            
            } else
            {
                base.WndProc(ref m);
            }
            
        }

        private void cmdEinstellungen_Click(object sender, EventArgs e)
        {
            Form settings = new frmSettings();
            settings.ShowDialog();
        }

        //Timer Reset bei Aktivieren des Buttons
        private void btReset_Click(object sender, EventArgs e)
        {
            tSS = 0;
            tMM = 0;
            tHH = 0;
            lbTimerSS.Text = "00";
            lbTimerMM.Text = "00";
            lbTimerHH.Text = "00";

            timerColorBlack();
        }

        //Offen der Infopage bei aktivieren des "Info"-Linklables
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form info = new frmInfo();
            info.ShowDialog();
        }

        #region timer
        //Timer für Timer Sekunden
        private void timerSS_Tick(object sender, EventArgs e)
        {
            tSS = tSS + 1;
            if (tSS == 60)
            {
                tSS = 0;
            }
            
            if (tSS < 10)
            {
                lbTimerSS.Text = "0" + tSS.ToString();
            } else
            {
                lbTimerSS.Text = tSS.ToString();
            }
            
        }
        //Timer für Timer Minuten
        private void timerMM_Tick(object sender, EventArgs e)
        {
            tMM = tMM + 1;
            if (tMM == 60)
            {
                tMM = 0;
            }

            if (tMM < 10)
            {
                lbTimerMM.Text = "0" + tMM.ToString();
            }
            else
            {
                lbTimerMM.Text = tMM.ToString();
            }

            if (   tMM     == frmSettings.timerMarkAfter 
                || tMM / 2 == frmSettings.timerMarkAfter 
                || tMM / 3 == frmSettings.timerMarkAfter 
                || tMM / 4 == frmSettings.timerMarkAfter 
                || tMM / 5 == frmSettings.timerMarkAfter 
                || tMM / 6 == frmSettings.timerMarkAfter)
            {
                lbTimerHH.ForeColor = Color.Red;
                lbTimerMM.ForeColor = Color.Red;
                lbTimerSS.ForeColor = Color.Red;
            }

        }
        //Timer für Timer Stunden
        private void timerHH_Tick(object sender, EventArgs e)
        {
            tHH = tHH + 1;

            if (tHH < 10)
            {
                lbTimerHH.Text = "0" + tHH.ToString();
            }
            else
            {
                lbTimerHH.Text = tHH.ToString();
            }
            
        }

        /// <summary>
        /// Timer für Aktualisierug und Anzeige der PC-Stats.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private void timerCPU_Tick(object sender, EventArgs e)
        {
            int CPUload = Convert.ToInt16(pcCPU.NextValue());
            lbCpu.Text = CPUload.ToString() + " %";
            pbCPUload.Value = CPUload;

            int RAMLoad = Convert.ToInt16(pcRAM.NextValue());
            lbRam.Text = RAMLoad.ToString() + " %";
            pbRAMload.Value = RAMLoad;

            string recHDDload = Settings.Default["recHDD"].ToString();

            lbDatenträger1.ForeColor = Color.Black;
            lbDatenträger1.Text = "Datenträger " + recHDDload + ":";
            DriveInfo[] Drives = DriveInfo.GetDrives();
            foreach (DriveInfo d in Drives)
            {
                if (d.Name == recHDDload + ":\\")
                {
                    var totalUsedSpace = (d.TotalSize - d.TotalFreeSpace) / (1024F * 1024F * 1024F);
                    var totalFreeSpace = d.TotalFreeSpace / (1024F * 1024F * 1024F);
                    var totalSize = d.TotalSize / (1024F * 1024F * 1024F);
                    var partOfSpace = (totalUsedSpace / totalSize) * 100;
                    lbSpace.Text = totalFreeSpace.ToString() + " GB";
                    pbDisks.Value = Convert.ToInt16((totalUsedSpace / totalSize) * 100);
                }
                else
                {
                    lbDatenträger1.ForeColor = Color.Black;
                    lbDatenträger1.Text = "Datenträger " + recHDDload + ":";
                }
            }

            
            if (Convert.ToBoolean(Settings.Default["alwaysOnTop"]))
            {
                if (TopMost == false)
                {
                    TopMost = true;
                }
            }
            else
            {
                if (TopMost == true)
                {
                    TopMost = false;
                }
            }
        }
        /// <summary>
        /// Timer und aktualisierung der Regestrierung des eingestellten Keyboard-Hotkeys und der Keyboard Hook.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerKeyboardHook_Tick(object sender, EventArgs e)
        {
            recKeyHook();
            markKeyHook();
        }
        #endregion

        /// <summary>
        /// Bei Aktivierung des Hotkeys für Timer.
        /// [Für die standart Keyboard Hook]
        /// </summary>
        private void recAltKeyHook()
        {
            string recKeyload = Settings.Default["recKey"].ToString();

            #region IfHellOfDOOM

                if (recKeyload == "F1")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F1);
                }
                else if (recKeyload == "F2")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F2);
                }
                else if (recKeyload == "F3")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F3);
                }
                else if (recKeyload == "F4")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F4);
                }
                else if (recKeyload == "F5")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F5);
                }
                else if (recKeyload == "F6")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F6);
                }
                else if (recKeyload == "F7")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F7);
                }
                else if (recKeyload == "F8")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F8);
                }
                else if (recKeyload == "F9")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F9);
                }
                else if (recKeyload == "F10")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F10);
                }
                else if (recKeyload == "F11")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F11);
                }
                else if (recKeyload == "F12")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F12);
                }

                else if (recKeyload == "A")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.A);
                }
                else if (recKeyload == "B")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.B);
                }
                else if (recKeyload == "C")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.Z);
                }
                else if (recKeyload == "D")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.D);
                }
                else if (recKeyload == "E")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.E);
                }
                else if (recKeyload == "F")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F);
                }
                else if (recKeyload == "G")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.G);
                }
                else if (recKeyload == "H")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.H);
                }
                else if (recKeyload == "I")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.I);
                }
                else if (recKeyload == "J")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.J);
                }
                else if (recKeyload == "K")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.K);
                }
                else if (recKeyload == "L")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.L);
                }
                else if (recKeyload == "M")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.M);
                }
                else if (recKeyload == "N")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.N);
                }
                else if (recKeyload == "O")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.O);
                }
                else if (recKeyload == "P")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.P);
                }
                else if (recKeyload == "Q")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.Q);
                }
                else if (recKeyload == "R")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.R);
                }
                else if (recKeyload == "S")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.S);
                }
                else if (recKeyload == "T")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.T);
                }
                else if (recKeyload == "U")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.U);
                }
                else if (recKeyload == "V")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.V);
                }
                else if (recKeyload == "W")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.W);
                }
                else if (recKeyload == "X")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.X);
                }
                else if (recKeyload == "Y")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.Y);
                }
                else if (recKeyload == "Z")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.C);
                }

                else if (recKeyload == "NumPad1")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad1);
                }
                else if (recKeyload == "NumPad2")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad2);
                }
                else if (recKeyload == "NumPad3")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad3);
                }
                else if (recKeyload == "NumPad4")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad4);
                }
                else if (recKeyload == "NumPad5")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad5);
                }
                else if (recKeyload == "NumPad6")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad6);
                }
                else if (recKeyload == "NumPad7")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad7);
                }
                else if (recKeyload == "NumPad18")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad8);
                }
                else if (recKeyload == "NumPad9")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad9);
                }
                else if (recKeyload == "NumPad0")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.NumPad0);
                }

                else if (recKeyload == "Add")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.Add);
                }
                else if (recKeyload == "Substract")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.Subtract);
                }
                else if (recKeyload == "Multiply")
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.Multiply);
                }

                else
                {
                    RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.F4);
                }
            #endregion
        }
        /// <summary>
        /// Bei Aktivierung des Hotkeys für Timer.
        /// [Für die alternative Keyboard Hook]
        /// </summary>
        private void recKeyHook()
        {
            globalKeyboardHook hook = new globalKeyboardHook();
            string recKeyload = Settings.Default["recKey"].ToString();

            #region IfHellOfDOOM
            if (recKeyload == "F1")
            {
                hook.HookedKeys.Add(Keys.F1);
            }
            else if (recKeyload == "F2")
            {
                hook.HookedKeys.Add(Keys.F2);
            }
            else if (recKeyload == "F3")
            {
                hook.HookedKeys.Add(Keys.F3);
            }
            else if (recKeyload == "F4")
            {
                hook.HookedKeys.Add(Keys.F4);
            }
            else if (recKeyload == "F5")
            {
                hook.HookedKeys.Add(Keys.F5);
            }
            else if (recKeyload == "F6")
            {
                hook.HookedKeys.Add(Keys.F6);
            }
            else if (recKeyload == "F7")
            {
                hook.HookedKeys.Add(Keys.F7);
            }
            else if (recKeyload == "F8")
            {
                hook.HookedKeys.Add(Keys.F8);
            }
            else if (recKeyload == "F9")
            {
                hook.HookedKeys.Add(Keys.F9);
            }
            else if (recKeyload == "F10")
            {
                hook.HookedKeys.Add(Keys.F10);
            }
            else if (recKeyload == "F11")
            {
                hook.HookedKeys.Add(Keys.F11);
            }
            else if (recKeyload == "F12")
            {
                hook.HookedKeys.Add(Keys.F12);
            }

            else if (recKeyload == "A")
            {
                hook.HookedKeys.Add(Keys.A);
            }
            else if (recKeyload == "B")
            {
                hook.HookedKeys.Add(Keys.B);
            }
            else if (recKeyload == "C")
            {
                hook.HookedKeys.Add(Keys.Z);
            }
            else if (recKeyload == "D")
            {
                hook.HookedKeys.Add(Keys.D);
            }
            else if (recKeyload == "E")
            {
                hook.HookedKeys.Add(Keys.E);
            }
            else if (recKeyload == "F")
            {
                hook.HookedKeys.Add(Keys.F);
            }
            else if (recKeyload == "G")
            {
                hook.HookedKeys.Add(Keys.G);
            }
            else if (recKeyload == "H")
            {
                hook.HookedKeys.Add(Keys.H);
            }
            else if (recKeyload == "I")
            {
                hook.HookedKeys.Add(Keys.I);
            }
            else if (recKeyload == "J")
            {
                hook.HookedKeys.Add(Keys.J);
            }
            else if (recKeyload == "K")
            {
                hook.HookedKeys.Add(Keys.K);
            }
            else if (recKeyload == "L")
            {
                hook.HookedKeys.Add(Keys.L);
            }
            else if (recKeyload == "M")
            {
                hook.HookedKeys.Add(Keys.M);
            }
            else if (recKeyload == "N")
            {
                hook.HookedKeys.Add(Keys.N);
            }
            else if (recKeyload == "O")
            {
                hook.HookedKeys.Add(Keys.O);
            }
            else if (recKeyload == "P")
            {
                hook.HookedKeys.Add(Keys.P);
            }
            else if (recKeyload == "Q")
            {
                hook.HookedKeys.Add(Keys.Q);
            }
            else if (recKeyload == "R")
            {
                hook.HookedKeys.Add(Keys.R);
            }
            else if (recKeyload == "S")
            {
                hook.HookedKeys.Add(Keys.S);
            }
            else if (recKeyload == "T")
            {
                hook.HookedKeys.Add(Keys.T);
            }
            else if (recKeyload == "U")
            {
                hook.HookedKeys.Add(Keys.U);
            }
            else if (recKeyload == "V")
            {
                hook.HookedKeys.Add(Keys.V);
            }
            else if (recKeyload == "W")
            {
                hook.HookedKeys.Add(Keys.W);
            }
            else if (recKeyload == "X")
            {
                hook.HookedKeys.Add(Keys.X);
            }
            else if (recKeyload == "Y")
            {
                hook.HookedKeys.Add(Keys.Y);
            }
            else if (recKeyload == "Z")
            {
                hook.HookedKeys.Add(Keys.C);
            }

            else if (recKeyload == "NumPad1")
            {
                hook.HookedKeys.Add(Keys.NumPad1);
            }
            else if (recKeyload == "NumPad2")
            {
                hook.HookedKeys.Add(Keys.NumPad2);
            }
            else if (recKeyload == "NumPad3")
            {
                hook.HookedKeys.Add(Keys.NumPad3);
            }
            else if (recKeyload == "NumPad4")
            {
                hook.HookedKeys.Add(Keys.NumPad4);
            }
            else if (recKeyload == "NumPad5")
            {
                hook.HookedKeys.Add(Keys.NumPad5);
            }
            else if (recKeyload == "NumPad6")
            {
                hook.HookedKeys.Add(Keys.NumPad6);
            }
            else if (recKeyload == "NumPad7")
            {
                hook.HookedKeys.Add(Keys.NumPad7);
            }
            else if (recKeyload == "NumPad18")
            {
                hook.HookedKeys.Add(Keys.NumPad8);
            }
            else if (recKeyload == "NumPad9")
            {
                hook.HookedKeys.Add(Keys.NumPad9);
            }
            else if (recKeyload == "NumPad0")
            {
                hook.HookedKeys.Add(Keys.NumPad0);
            }

            else if (recKeyload == "Add")
            {
                hook.HookedKeys.Add(Keys.Add);
            }
            else if (recKeyload == "Substract")
            {
                hook.HookedKeys.Add(Keys.Subtract);
            }
            else if (recKeyload == "Multiply")
            {
                hook.HookedKeys.Add(Keys.Multiply);
            }

            else
            {
                hook.HookedKeys.Add(Keys.F4);
            }
            #endregion

            hook.KeyUp += new KeyEventHandler(hook_keyUp);
        }

        /// <summary>
        /// Bei Aktivierung des Hotkeys für Marker setzten.
        /// [Für die standart Keyboard Hook]
        /// </summary>
        private void markAltKeyHook()
        {
            string recKeyload = Settings.Default["markKey"].ToString();

            #region IfHellOfDOOM_2

                if (recKeyload == "F1")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F1);
                }
                else if (recKeyload == "F2")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F2);
                }
                else if (recKeyload == "F3")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F3);
                }
                else if (recKeyload == "F4")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F4);
                }
                else if (recKeyload == "F5")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F5);
                }
                else if (recKeyload == "F6")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F6);
                }
                else if (recKeyload == "F7")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F7);
                }
                else if (recKeyload == "F8")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F8);
                }
                else if (recKeyload == "F9")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F9);
                }
                else if (recKeyload == "F10")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F10);
                }
                else if (recKeyload == "F11")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F11);
                }
                else if (recKeyload == "F12")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F12);
                }

                else if (recKeyload == "A")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.A);
                }
                else if (recKeyload == "B")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.B);
                }
                else if (recKeyload == "C")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.Z);
                }
                else if (recKeyload == "D")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.D);
                }
                else if (recKeyload == "E")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.E);
                }
                else if (recKeyload == "F")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F);
                }
                else if (recKeyload == "G")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.G);
                }
                else if (recKeyload == "H")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.H);
                }
                else if (recKeyload == "I")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.I);
                }
                else if (recKeyload == "J")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.J);
                }
                else if (recKeyload == "K")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.K);
                }
                else if (recKeyload == "L")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.L);
                }
                else if (recKeyload == "M")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.M);
                }
                else if (recKeyload == "N")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.N);
                }
                else if (recKeyload == "O")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.O);
                }
                else if (recKeyload == "P")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.P);
                }
                else if (recKeyload == "Q")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.Q);
                }
                else if (recKeyload == "R")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.R);
                }
                else if (recKeyload == "S")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.S);
                }
                else if (recKeyload == "T")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.T);
                }
                else if (recKeyload == "U")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.U);
                }
                else if (recKeyload == "V")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.V);
                }
                else if (recKeyload == "W")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.W);
                }
                else if (recKeyload == "X")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.X);
                }
                else if (recKeyload == "Y")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.Y);
                }
                else if (recKeyload == "Z")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.C);
                }

                else if (recKeyload == "NumPad1")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad1);
                }
                else if (recKeyload == "NumPad2")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad2);
                }
                else if (recKeyload == "NumPad3")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad3);
                }
                else if (recKeyload == "NumPad4")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad4);
                }
                else if (recKeyload == "NumPad5")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad5);
                }
                else if (recKeyload == "NumPad6")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad6);
                }
                else if (recKeyload == "NumPad7")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad7);
                }
                else if (recKeyload == "NumPad18")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad8);
                }
                else if (recKeyload == "NumPad9")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad9);
                }
                else if (recKeyload == "NumPad0")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.NumPad0);
                }

                else if (recKeyload == "Add")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.Add);
                }
                else if (recKeyload == "Substract")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.Subtract);
                }
                else if (recKeyload == "Multiply")
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.Multiply);
                }

                else
                {
                    RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.F4);
                }
            #endregion
        }
        /// <summary>
        /// Bei Aktivierung des Hotkeys für Marker setzten.
        /// [Für die alternative Keyboard Hook]
        /// </summary>
        private void markKeyHook()
        {
            globalKeyboardHook hook = new globalKeyboardHook();
            string recKeyload = Settings.Default["markKey"].ToString();

            #region IfHellOfDOOM
            if (recKeyload == "F1")
            {
                hook.HookedKeys.Add(Keys.F1);
            }
            else if (recKeyload == "F2")
            {
                hook.HookedKeys.Add(Keys.F2);
            }
            else if (recKeyload == "F3")
            {
                hook.HookedKeys.Add(Keys.F3);
            }
            else if (recKeyload == "F4")
            {
                hook.HookedKeys.Add(Keys.F4);
            }
            else if (recKeyload == "F5")
            {
                hook.HookedKeys.Add(Keys.F5);
            }
            else if (recKeyload == "F6")
            {
                hook.HookedKeys.Add(Keys.F6);
            }
            else if (recKeyload == "F7")
            {
                hook.HookedKeys.Add(Keys.F7);
            }
            else if (recKeyload == "F8")
            {
                hook.HookedKeys.Add(Keys.F8);
            }
            else if (recKeyload == "F9")
            {
                hook.HookedKeys.Add(Keys.F9);
            }
            else if (recKeyload == "F10")
            {
                hook.HookedKeys.Add(Keys.F10);
            }
            else if (recKeyload == "F11")
            {
                hook.HookedKeys.Add(Keys.F11);
            }
            else if (recKeyload == "F12")
            {
                hook.HookedKeys.Add(Keys.F12);
            }

            else if (recKeyload == "A")
            {
                hook.HookedKeys.Add(Keys.A);
            }
            else if (recKeyload == "B")
            {
                hook.HookedKeys.Add(Keys.B);
            }
            else if (recKeyload == "C")
            {
                hook.HookedKeys.Add(Keys.Z);
            }
            else if (recKeyload == "D")
            {
                hook.HookedKeys.Add(Keys.D);
            }
            else if (recKeyload == "E")
            {
                hook.HookedKeys.Add(Keys.E);
            }
            else if (recKeyload == "F")
            {
                hook.HookedKeys.Add(Keys.F);
            }
            else if (recKeyload == "G")
            {
                hook.HookedKeys.Add(Keys.G);
            }
            else if (recKeyload == "H")
            {
                hook.HookedKeys.Add(Keys.H);
            }
            else if (recKeyload == "I")
            {
                hook.HookedKeys.Add(Keys.I);
            }
            else if (recKeyload == "J")
            {
                hook.HookedKeys.Add(Keys.J);
            }
            else if (recKeyload == "K")
            {
                hook.HookedKeys.Add(Keys.K);
            }
            else if (recKeyload == "L")
            {
                hook.HookedKeys.Add(Keys.L);
            }
            else if (recKeyload == "M")
            {
                hook.HookedKeys.Add(Keys.M);
            }
            else if (recKeyload == "N")
            {
                hook.HookedKeys.Add(Keys.N);
            }
            else if (recKeyload == "O")
            {
                hook.HookedKeys.Add(Keys.O);
            }
            else if (recKeyload == "P")
            {
                hook.HookedKeys.Add(Keys.P);
            }
            else if (recKeyload == "Q")
            {
                hook.HookedKeys.Add(Keys.Q);
            }
            else if (recKeyload == "R")
            {
                hook.HookedKeys.Add(Keys.R);
            }
            else if (recKeyload == "S")
            {
                hook.HookedKeys.Add(Keys.S);
            }
            else if (recKeyload == "T")
            {
                hook.HookedKeys.Add(Keys.T);
            }
            else if (recKeyload == "U")
            {
                hook.HookedKeys.Add(Keys.U);
            }
            else if (recKeyload == "V")
            {
                hook.HookedKeys.Add(Keys.V);
            }
            else if (recKeyload == "W")
            {
                hook.HookedKeys.Add(Keys.W);
            }
            else if (recKeyload == "X")
            {
                hook.HookedKeys.Add(Keys.X);
            }
            else if (recKeyload == "Y")
            {
                hook.HookedKeys.Add(Keys.Y);
            }
            else if (recKeyload == "Z")
            {
                hook.HookedKeys.Add(Keys.C);
            }

            else if (recKeyload == "NumPad1")
            {
                hook.HookedKeys.Add(Keys.NumPad1);
            }
            else if (recKeyload == "NumPad2")
            {
                hook.HookedKeys.Add(Keys.NumPad2);
            }
            else if (recKeyload == "NumPad3")
            {
                hook.HookedKeys.Add(Keys.NumPad3);
            }
            else if (recKeyload == "NumPad4")
            {
                hook.HookedKeys.Add(Keys.NumPad4);
            }
            else if (recKeyload == "NumPad5")
            {
                hook.HookedKeys.Add(Keys.NumPad5);
            }
            else if (recKeyload == "NumPad6")
            {
                hook.HookedKeys.Add(Keys.NumPad6);
            }
            else if (recKeyload == "NumPad7")
            {
                hook.HookedKeys.Add(Keys.NumPad7);
            }
            else if (recKeyload == "NumPad18")
            {
                hook.HookedKeys.Add(Keys.NumPad8);
            }
            else if (recKeyload == "NumPad9")
            {
                hook.HookedKeys.Add(Keys.NumPad9);
            }
            else if (recKeyload == "NumPad0")
            {
                hook.HookedKeys.Add(Keys.NumPad0);
            }

            else if (recKeyload == "Add")
            {
                hook.HookedKeys.Add(Keys.Add);
            }
            else if (recKeyload == "Substract")
            {
                hook.HookedKeys.Add(Keys.Subtract);
            }
            else if (recKeyload == "Multiply")
            {
                hook.HookedKeys.Add(Keys.Multiply);
            }

            else
            {
                hook.HookedKeys.Add(Keys.F4);
            }
            #endregion

            hook.KeyUp += new KeyEventHandler(hook_keyUpMark);
        }

        /// <summary>
        /// HOOK KEYDOWN EVENT RECORDING
        /// [STANDART KEY HOOK]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hook_keyUp(object sender, KeyEventArgs e)
        {
            if (keyWasPressed)
            {
                keyWasPressed = false;
                lbRecInfo.Text = "Aufnahme gestoppt.";
                lbRecInfo.ForeColor = Color.Red;

                timerHH.Stop();
                timerMM.Stop();
                timerSS.Stop();
            }
            else
            {
                timerHH.Start();
                timerMM.Start();
                timerSS.Start();

                lbRecInfo.Text = "Aufnahme läuft.";
                lbRecInfo.ForeColor = Color.Green;
                keyWasPressed = true;
            }

            e.Handled = true;
        }
        /// <summary>
        /// HOOK KEYDOWN EVENT RECORDING
        /// [ALTERNATIVE KEY HOOK]
        /// </summary>
        private void hookAlt_keyUp()
        {
            if (keyWasPressed)
            {
                keyWasPressed = false;
                lbRecInfo.Text = "Aufnahme gestoppt.";
                lbRecInfo.ForeColor = Color.Red;

                timerHH.Stop();
                timerMM.Stop();
                timerSS.Stop();
            }
            else
            {
                timerHH.Start();
                timerMM.Start();
                timerSS.Start();

                lbRecInfo.Text = "Aufnahme läuft.";
                lbRecInfo.ForeColor = Color.Green;
                keyWasPressed = true;
            }
        }

        /// <summary>
        /// HOOK KEYDOWN EVENT MARKING
        /// [STANDART KEY HOOK]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hook_keyUpMark(object sender, KeyEventArgs e)
        {
            listMarker.Items.Add(numbMarks + " - " + lbTimerHH.Text + ":" + lbTimerMM.Text + ":" + lbTimerSS.Text);
            numbMarks++;

            timerColorBlack();

            e.Handled = true;
        }
        /// <summary>
        /// HOOK KEYDOWN EVENT MARKING
        /// [ALTERNATE KEY HOOK]
        /// </summary>
        private void hookAlt_keyUpMark()
        {
            listMarker.Items.Add(numbMarks + " - " + lbTimerHH.Text + ":" + lbTimerMM.Text + ":" + lbTimerSS.Text);
            numbMarks++;

            timerColorBlack();
        }

        private void btResetMarks_Click(object sender, EventArgs e)
        {
            listMarker.Items.Clear();
            numbMarks = 1;
        }

        private void btSaveMarks_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();

            StreamWriter Writer = new StreamWriter(folderBrowserDialog1.SelectedPath + @"\saved_marks.txt");

            foreach(var item in listMarker.Items)
            {
                Writer.WriteLine(item.ToString());
            }
            Writer.Close();

            MessageBox.Show("Datei gespeichert!");
        }

        private void cmdSoftwareStarten_Click(object sender, EventArgs e)
        {
            if (Settings.Default["programm1"].ToString() != "")
            {
                Process.Start(Settings.Default["programm1"].ToString());
            }
            if (Settings.Default["programm2"].ToString() != "")
            {
                Process.Start(Settings.Default["programm2"].ToString());
            }
            if (Settings.Default["programm3"].ToString() != "")
            {
                Process.Start(Settings.Default["programm3"].ToString());
            }
            if (Settings.Default["programm4"].ToString() != "")
            {
                Process.Start(Settings.Default["programm4"].ToString());
            }
            if (Settings.Default["programm5"].ToString() != "")
            {
                Process.Start(Settings.Default["programm5"].ToString());
            }
        }

        private void lbUpdateWarn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("ACHTUNG! Die Benachichtigung für Updates ist deaktiviert! Wenn sie dies ändern wollen gehen sie in die Einstellungen und aktivieren sie die Update-Benachichtigung!");
            lbUpdateWarn.Text = "";
        }

        private void timerColorBlack()
        {
            lbTimerHH.ForeColor = Color.Black;
            lbTimerMM.ForeColor = Color.Black;
            lbTimerSS.ForeColor = Color.Black;
        }
        #region EMPTY METHODS

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void pbDisks_Click(object sender, EventArgs e)
        {
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {
        }
        #endregion
    }
}
