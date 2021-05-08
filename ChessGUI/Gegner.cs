using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;

namespace ChessGUI {
    public partial class Gegner : Form {

        #region Globale Instzanzen
        private RadioButton rb;
        private SchachGUI objektGUI = new SchachGUI();
        private Logger _logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Konstruktor
        public Gegner() {
            InitializeComponent();
        }
        #endregion

        #region Getter&Setter
        public static string nickname { get; private set; }
        public static string farbe { get; private set; }

        #endregion

        #region Formular Laden Ereignis
        private void Gegner_Load(object sender, EventArgs e) {
            FontFamily family = new FontFamily("Comic Sans MS");
            Font font = new Font(family, 16.0f, FontStyle.Underline);
            this.EruateComponents();

        }
        #endregion

        #region Leitet die Ablaeufe ein
        private void EruateComponents() {
            Label dynamicLabel = new Label();
            dynamicLabel.AutoSize = true;
            dynamicLabel.Location = new Point(30, 60);
            FontFamily family = new FontFamily("Comic Sans MS");
            Font font = new Font(family, 16.0f, FontStyle.Underline);
            dynamicLabel.Font = font;
            dynamicLabel.Text = "Waehlen Sie Ihren eigenen Nickname aus:";
            this.Controls.Add(dynamicLabel);
            List<string> lstFiles = new List<string>();
            string GetDownloadedFilesFrom = Environment.CurrentDirectory + @"\Anmeldedaten\";
            foreach (string file in Directory.GetFiles(GetDownloadedFilesFrom)) {
                if (file.Contains("user"))
                    lstFiles.Add(file);
            }
            if (lstFiles.Count < 2) {
                string message = "Derzeit ist nur " + lstFiles.Count + " Person registriert. Das reicht offensichtlich nicht. Onlinepartien sind folglich nicht moeglich!";
                objektGUI.Ausgabe(message, "Warnung", MessageBoxIcon.Exclamation);
                this.Close();
            }
            this.SignBoxes(lstFiles);
        }
        #endregion

        #region Erstellt die RadioButtons
        private void SignBoxes(List<string> names) {
            char[] delimiters = new char[] { '\\', '/' };
            List<string> lstNameSeperated = new List<string>(), lstNameSeperated_ = new List<string>();
            for (int i = 0; i < names.Count; i++) {
                string[] parts = names[i].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                lstNameSeperated.Add(parts[parts.Length - 1]);
            }
            delimiters = new char[] { '_' };
            for (int i = 0; i < lstNameSeperated.Count; i++) {
                string[] parts = lstNameSeperated[i].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                lstNameSeperated_.Add(parts[0]);
            }
            for (int i = 0; i < lstNameSeperated_.Count; i++) {
                RadioButton rButton = new RadioButton();
                rButton.Name = String.Format("MYRADIOBUTTON_{0}", i);
                rButton.Text = lstNameSeperated_[i];
                rButton.Location = new Point(20, 100 + 20 * i);
                this.Controls.Add(rButton);
                rButton.CheckedChanged += new EventHandler(rButton_CheckedChanged);
            }
        }
        #endregion

        #region Verarbeitet den Submittbutton
        private void BtnSubmit_Click(object sender, EventArgs e) {
            string message;
            if (rbWeiss.Checked)
                Gegner.farbe = "Weiss";
            else if (rbSchwarz.Checked)
                Gegner.farbe = "Schwarz";
            else {
                message = "Bitte die Farbe waehlen, mit der sie spielen wollen!";
                objektGUI.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                return;
            }
            _logger.Debug("Der Spieler hat die {0} Farbe gewaehlt", Gegner.farbe);
            int counter = 0;
            foreach (Control t in this.Controls) {
                if (t.GetType() == typeof(RadioButton)) {
                    RadioButton rb = (RadioButton)t;
                    if (rb.Checked)
                        counter++;
                }
            }
            if (counter == 0) {
                message = "Waehlen Sie bitte genau einen Nickname";
                objektGUI.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                return;
            }
            if (rb != null) {
                DialogResult dr = MessageBox.Show("Wollen sie wirklich online spielen?", "Frage", MessageBoxButtons.YesNo);
                switch (dr) {
                    case DialogResult.Yes:
                        Gegner.nickname = rb.Text + "_user.txt";
                        _logger.Debug("Der Spieler hat folgenden Nickname gewaehlt:{0}", Gegner.nickname);
                        this.Close();
                        Zugtransfer fm = new Zugtransfer();
                        fm.GetMove();
                        break;
                    case DialogResult.No:
                        break;
                }
            } else if (rb != null) {
                this.Close();
            }
        }
        #endregion

        #region liefert den ausgewaehlten RadioButton in die globale Variable
        private void rButton_CheckedChanged(Object sender, EventArgs e) {
            if (((RadioButton)sender).Checked)
                rb = (RadioButton)sender;
        }
        #endregion

    }
}
