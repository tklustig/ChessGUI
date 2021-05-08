using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChessGUI {
    static public class Win {
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIdEnableItem, uint uEnable);
        internal const UInt32 SC_CLOSE = 0xF060;
        internal const UInt32 MF_BYCOMMAND = 0x00000000;
        internal const UInt32 MF_ENABLED = 0x00000000;
        internal const UInt32 MF_GRAYED = 0x00000001;
        internal const UInt32 MF_DISABLED = 0x00000002;

        public static void DisableCloseButton(IWin32Window window, bool bEnabled) {
            IntPtr hSystemMenu = GetSystemMenu(window.Handle, false);

            EnableMenuItem(hSystemMenu, SC_CLOSE, (uint)(MF_BYCOMMAND | (bEnabled ? MF_ENABLED : MF_GRAYED | MF_DISABLED)));
        }
    }
}

