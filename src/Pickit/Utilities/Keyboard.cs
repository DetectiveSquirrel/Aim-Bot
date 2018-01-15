#region Header

//-----------------------------------------------------------------
//   Class:          VirtualKeyboard
//   Description:    Keyboard control utils.
//   Author:         Stridemann, nymann        Date: 08.26.2017
//-----------------------------------------------------------------

#endregion

using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AimBot.Utilities
{
    public static class Keyboard
    {
        private const int KeyeventfExtendedkey = 0x0001;
        private const int KeyeventfKeyup       = 0x0002;

        private const int ActionDelay = 1;

        [DllImport("user32.dll")]
        private static extern uint Keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);


        public static void KeyDown(Keys key) { Keybd_event((byte) key, 0, KeyeventfExtendedkey | 0, 0); }

        public static void KeyUp(Keys key)
        {
            Keybd_event((byte) key, 0, KeyeventfExtendedkey | KeyeventfKeyup, 0); //0x7F
        }

        public static void KeyPress(Keys key)
        {
            KeyDown(key);
            Thread.Sleep(ActionDelay);
            KeyUp(key);
        }

        [DllImport("USER32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        public static bool IsKeyDown(int nVirtKey) => GetKeyState(nVirtKey) < 0;
    }
}