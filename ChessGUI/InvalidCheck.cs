using System;
using System.Windows.Forms;

namespace ChessGUI {
    internal class InValidCheck : Exception {

        #region Konstruktor
        public InValidCheck(string message) : base(message) {
            MessageBox.Show("Error waehrend der Figureneruierung" + Environment.NewLine + message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }
        #endregion
    }
}
