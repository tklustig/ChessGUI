using System;
using System.Windows.Forms;

namespace ChessGUI {
    static class Program {
        /// <summary>
        /// Der Haupteinstiegspunkt fuer die Anwendung.
        /// </summary>
								/// STRG-M-O klappt ein,STRG-M-L kappt aus
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SchachGUI());
        }
    }
}
