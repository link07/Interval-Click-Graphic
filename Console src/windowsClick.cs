﻿/*---------------------------------------------------------------------------------------------------------------------------------------
 * Program:       Windows Click
 * Author:        Tyler aka Link aka Zafar
 *
 * Description:
 * Some windows click handling, primarely where the mouse is, and allowing it be moved and clicked elsewhere
 * 
 * 
 * Version History:
 * v1.0 on 5/19/2015 : Split from my "Open Divine Gifts" program, which was then renamed to "Interval Click"
 *                  
 * Compile Note: Requires adding "System.Drawing" and "System.Windows.Forms" to resources
 * --------------------------------------------------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace windowsClick
{
    class windowsClick
    {
        /// <summary>
        /// Reports current mouse location back to caller
        /// </summary>
        /// <param name="x">value of cursor's x at time of input</param>
        /// <param name="y">value of cursor's y at time of input</param>
        public static void currentMouseLocation(ref int x, ref int y)
        {
            bool exit = false;
            // Display current mouse location until control c
            while (!exit)
            {
                if (Regex.IsMatch(Convert.ToString(Console.ReadLine()), @"^[a-zA-Z0-9_]+$"))
                {
                    // Current Mouse Loc
                    Console.WriteLine("X: {0}, Y: {1}", Cursor.Position.X, Cursor.Position.Y);
                    x = Cursor.Position.X;
                    y = Cursor.Position.Y;

                    exit = true;
                }
            }
        }

        /// <summary>
        /// Clicks every x minutes until thread is killed
        /// </summary>
        /// <param name="xPos">Where to click</param>
        /// <param name="yPos">Where to click</param>
        /// <param name="button">Which button to click (0 for left, 1 for right)</param>
        /// <param name="interval">How often in minutes to click</param>
        public static void clickInterval(int xPos, int yPos, int button, double interval)
        {
            while (true)
            {
                // Attempt to open gift every 15 mins
                //Cursor.Position = new Point(xPos, yPos);
                click(xPos, yPos, button);

                // Sleep for 15 Mins
                Thread.Sleep((int)(interval * 60 * 1000)); // 900,000 ms = 900 sec = 15 mins
            }
        }


        #region Dll declaration and enums for mouse button usage
        /// <summary>
        ///  Imports required DLL and allows you to simulate mouse buttons
        /// </summary>
        /// <param name="dwFlags">Mouse button to press</param>
        /// <param name="dx">idk, possibly X to click on, but I just move there before hand</param>
        /// <param name="dy">idk, possibly Y to click on, but I just move there before hand</param>
        /// <param name="dwData">idk</param>
        /// <param name="dwExtraInfo">idk</param>
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy,
                              int dwData, int dwExtraInfo);

        /// <summary>
        ///  Values required to be sent to simulate mouse buttons
        /// </summary>
        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }
        #endregion


        /// <summary>
        /// Moves mouse to location, then left or right clicks twice (once to enter window, once to activate...maybe modify it to only click ones later)
        /// </summary>
        /// <param name="x">Position to move x part of cursor to</param>
        /// <param name="y">Position to move y part of cursor to</param>
        /// <param name="button">0 or 1, for left or right respectivly</param>
        public static void click(int x, int y, int button)
        {
            // Currently, as a test, it double clicks, as many applications seem to not accept a single click (and on a Windows 8 laptop I have, it doesn't even accept this double click)
            // using windows normal double click timeframe (500 ms), minus four ms (from http://ux.stackexchange.com/questions/40364/what-is-the-expected-timeframe-of-a-double-click)
            Cursor.Position = new Point(x, y);
            if (button == 0)
            {
                mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
                Thread.Sleep(248);
                mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
                mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
                Thread.Sleep(248);
                mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
            }
            else if (button == 1)
            {
                mouse_event((int)(MouseEventFlags.RIGHTDOWN), 0, 0, 0, 0);
                Thread.Sleep(248);
                mouse_event((int)(MouseEventFlags.RIGHTUP), 0, 0, 0, 0);
                mouse_event((int)(MouseEventFlags.RIGHTDOWN), 0, 0, 0, 0);
                Thread.Sleep(248);
                mouse_event((int)(MouseEventFlags.RIGHTUP), 0, 0, 0, 0);
            }
        }
    }
}