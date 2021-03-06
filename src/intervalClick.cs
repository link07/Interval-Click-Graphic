﻿/*---------------------------------------------------------------------------------------------------------------------------------------
 * Name:          Interval Click Graphic
 * Author:        Tyler aka Link aka Zafar
 *
 * Description:
 * A graphic version of Interval Click, to make navigating the menu easier
 * 
 * 
 * Version History:
 * v0.9 on 5/19/2015 : A lesser version of the console program (notably missing the auto find points function), but it works; Split from Console version 2.1
 * v0.9.1 on 6/8/2015 : Update TabIndexs, make the time NUD allow decimal places
 * v1.0 on 6/11/2015 : Add current XY display, add time since last click, basically feature equal to the console version now!
 * v1.1 on 6/11/2015 : Add option to enable / disable double click, cleanup form layout 
 * v1.2 on 6/11/2015 : Update to include new windowsClick.cs stuff
 * v1.3 on 7/23/2015 : Add hotkeys, add auto copy of lookup to the NUD's, start work on file saves
 * v1.3.1 on 7/23/2015 : Make x/y lookup consistent
 * 
 * Notes:
 * http://stackoverflow.com/a/2172484 Thread-safe label editing
 * --------------------------------------------------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace Interval_Click_Graphic
{

    public partial class intervalClick : Form
    {
        /// <summary>
        /// Create the thread handles
        /// </summary>
        private clickThreadHandler clickThread = new clickThreadHandler();
        private Thread sinceClickThread;
        private Thread xyThread;

        /// <summary>
        /// hotkey variables
        /// </summary>
        hotkey.KeyboardHook lookupHotkey = new hotkey.KeyboardHook();
        hotkey.KeyboardHook clickerHotkey = new hotkey.KeyboardHook();
        private bool xyThreadIsRunning = false, sinceClickThreadIsRunning = false;

        Point currentMouseLocation = new Point();

        /// <summary>
        /// Load form
        /// </summary>
        public intervalClick()
        {
            InitializeComponent();

            // register event keys
            lookupHotkey.KeyPressed += new EventHandler<hotkey.KeyPressedEventArgs>(lookupHotkey_KeyPressed);
            clickerHotkey.KeyPressed += new EventHandler<hotkey.KeyPressedEventArgs>(clickerHotkey_KeyPressed);

            // register control+alt+f12 combo as a hotkey
            try
            {
                lookupHotkey.RegisterHotKey(hotkey.ModifierKeys.Control, Keys.F);
                clickerHotkey.RegisterHotKey(hotkey.ModifierKeys.Control, Keys.L);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Hotkeys cannot be registered, close out of any other program that uses the global hotkeys Control+F or Control+L and then restart the programs" /*\n\nError Info:\n" + ex*/, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void lookupHotkey_KeyPressed(object sender, hotkey.KeyPressedEventArgs e)
        {
            lookupSwitch();
        }

        void lookupSwitch()
        {
            if (btnFindXY.Text == "Start XY Display")
            {
                // Create thread
                xyThread = new Thread(() => displayXY());

                // Start Thread
                xyThread.Start();
                xyThreadIsRunning = true;

                btnFindXY.Text = "Stop XY Display";

            }
            else if (btnFindXY.Text == "Stop XY Display")
            {
                if (xyThreadIsRunning)
                {
                    xyThread.Abort();
                    xyThreadIsRunning = false;
                    
                    if (cbCopyXY.Checked == true)
                    {
                        nudX.Value = currentMouseLocation.X;
                        nudY.Value = currentMouseLocation.Y;
                    }
                }

                btnFindXY.Text = "Start XY Display";
            }
        }

        void clickerHotkey_KeyPressed(object sender, hotkey.KeyPressedEventArgs e)
        {
            clickerSwitch();
        }

        void clickerSwitch()
        {
            int button = 0;
            Point p = new Point(Convert.ToInt32(nudX.Value), Convert.ToInt32(nudY.Value));

            if (btnOnOff.Text == "Start")
            {
                if (cbLeft.Checked == true && cbRight.Checked == true)
                    button = 2;
                else if (cbRight.Checked == true)
                    button = 1;
                else
                {
                    // default to left being checked
                    cbLeft.Checked = true;

                    button = 0;
                }


                // Start click and time since click threads
                clickThread.startThread(p, button, Convert.ToDouble(nudTime.Value), cbDouble.Checked);
                sinceClickThread = new Thread(() => updateTimeSinceClickLabel());

                // Start Click Thread
                sinceClickThread.Start();
                sinceClickThreadIsRunning = true;

                btnOnOff.Text = "Stop";
            }
            else if (btnOnOff.Text == "Stop")
            {
                // stop the thing
                clickThread.exitThread();

                if (sinceClickThreadIsRunning)
                {
                    sinceClickThread.Abort();
                    sinceClickThreadIsRunning = false;
                }


                // Cleanup Labels
                btnOnOff.Text = "Start";
                lblTimeSinceClick.Text = "Time Since Last Click: 0 Minutes 0 Seconds";
            }

        }

        /// <summary>
        /// Starts and stops clicking threads and associated things
        /// </summary>
        private void btnOnOff_Click(object sender, EventArgs e)
        {
           clickerSwitch();
        }

        /// <summary>
        /// Start / stop displaying current mouse x / y location info
        /// </summary>
        private void btnFindXY_Click(object sender, EventArgs e)
        {
            lookupSwitch();
        }

        /// <summary>
        /// cleanup when form closes
        /// </summary>
        private void intervalClick_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close all threads to cleanup if "stop" wasn't pressed
            clickThread.exitThread();


            if (sinceClickThreadIsRunning)
            {
                sinceClickThread.Abort();
                sinceClickThreadIsRunning = false;
            }


            if (xyThreadIsRunning)
            {
                xyThread.Abort();
                xyThreadIsRunning = false;
            }
        }


        /// <summary>
        /// Update x/y labels when threadHandle calls it
        /// </summary>
        public void displayXY()
        {
            while (true)
            {
                windowsClick.currentMouseLocation(ref currentMouseLocation);
                lblCurrX.Invoke((MethodInvoker)(() => lblCurrX.Text = "Current X: " + Convert.ToString(currentMouseLocation.X)));
                lblCurrY.Invoke((MethodInvoker)(() => lblCurrY.Text = "Current Y: " + Convert.ToString(currentMouseLocation.Y)));
                Thread.Sleep(100);
            }
        }


        /// <summary>
        /// Updates time since clicked label
        /// </summary>
        public void updateTimeSinceClickLabel()
        {
            // int intervalMinutes = Convert.ToInt32(Math.Floor(interval));
            // int intervalSeconds = Convert.ToInt32(interval * 60) - intervalMinutes * 60;
            int minutes = 0, seconds = 0;

            while (true)
            {
                minutes = Convert.ToInt32(Math.Floor(windowsClick.TimeSinceClick));
                seconds = Convert.ToInt32(windowsClick.TimeSinceClick * 60) - minutes * 60;

                lblTimeSinceClick.Invoke((MethodInvoker)(() => lblTimeSinceClick.Text = "Time Since Last Click: " + minutes + " Minutes " + seconds + " Seconds"));

                Thread.Sleep(100);
            }
        }

        private void btnDoubleClick_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Double Click is checked by default because most MMO's or other games require one click to enter the 3D window, and one click to interact with the game.\n\nMost non-3D programs, such as Chrome, and Windows, do not require this.", "Why is Double Click Checked by Default?");
        }

        private void tsmiHotkeys_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To Enable / Disable the XY Lookup, press Control + F.\n\n To Enable / Disbale the Clicking, press Control + L", "Hotkeys");
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            Form frm = new aboutForm.AboutForm();
            frm.Show();
        }
    }
}
