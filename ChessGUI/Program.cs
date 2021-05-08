using System;
using System.Windows.Forms;

namespace ChessGUI {
    static class Program {
        /// <summary>
        /// Der Haupteinstiegspunkt fuer die Anwendung.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SchachGUI());
        }
    }
}
