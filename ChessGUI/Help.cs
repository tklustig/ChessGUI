using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChessGUI {
    public partial class Help : Form {

        #region Konstruktor
        public Help() {
            InitializeComponent();
        }
        #endregion

        #region wird waehrend des Laden des Formulars ausgefuehrt
        private void Help_Load(object sender, EventArgs e) {
            txtHelp.Text = "Dieses Programm ermoeglicht das Spielen von Schachpartien gegen andere Mitspieler. Es bietet Onlinezuguebermittlung, aber keine Enginefunktionalitaet. Die Bedienung der Module erfolgt ueber ein Menu. Die Figuren werden ueber Drag&Drop bewegt. Bitte setzen Sie die Figur mittels einer fluessigen Bewegung,ohne Unterbrechung nur dort ab, wo Sie sie letztlich haben wollen. Unabhaengig dieser Module werden folgende Features bereitgestellt:" + Environment.NewLine;
            txtHelp.Text += "- Anzeige des ausgefuehrten Zuges / - Anzeige der Zuganzahl /- Anzeige der farblichen Zugerwartung" + Environment.NewLine;
            txtHelp.Text += "- ueber MessageBoxen: Schach, Schachmatt, ungueltiger Zug, inkorrekter farblicher Zug" + Environment.NewLine;
            txtHelp.Text += "- ueber CheckBoxen: Akualisierung der Zuganzahlausgabe, Speicherung (Aufzeichnung) der ausgefuehrten Zuege" + Environment.NewLine + Environment.NewLine;
            txtHelp.Text += "Folgende Module werden ueber das Menu bereitgestellt:" + Environment.NewLine;
            txtHelp.Text += "Aktionen-->Restart: Startet eine neue Partie ohne Rueckfrage" + Environment.NewLine;
            txtHelp.Text += "Aktionen-->Beenden: Beendet die Applikation ohne Rueckfrage" + Environment.NewLine;
            txtHelp.Text += "Aktionen-->Zugbox leeren: Loescht alle angezeigten farblichen Zugerwartungen und Zuganzahlausgaben" + Environment.NewLine;
            txtHelp.Text += "Aktionen-->Zugkkordinaten einblenden: Blendet die Brettkoordinaten ein bzw. aus" + Environment.NewLine;
            txtHelp.Text += "Aktionen-->Help: rendert dieses Formular" + Environment.NewLine;
            txtHelp.Text += "Aktionen-->Author: Blendet ein animiertes Formular ueber den Programmierer dieser Applikation ein. Alexa wird " + "                     " + "involviert." + Environment.NewLine + Environment.NewLine;
            txtHelp.Text += "Partien-->Liste anzeigen: Rendert ein Formular, welches Suchoptionen innerhalb einer GridView anbietet." + "                                                 " + " Die GridView beinhaltet die Zuege aller bisher aufgezeichneten Partien zzgl. des Datum." + "                                         " + "Auf Wunsch laesst sich daraus eine PDF generieren" + Environment.NewLine;
            txtHelp.Text += "Partien-->Liste loeschen: Loescht die fuer die GridView-Anzeige verantwortliche Textdatei mit Rueckfrage" + Environment.NewLine;
            txtHelp.Text += "Partien-->Zug rueckgaengig machen: Nimmt den zuletzt ausgefuehrten Zug zurueck. Es erfolgt eine Nachricht" + Environment.NewLine;
            txtHelp.Text += "Partien-->Schachserver kontaktieren: Uebermittelt online die farbliche Zugerwartung und den ausgefuehrten Zug." + "                     " +"Eine Speicherung der Zuege in eine MySQL Datenbank ist geplant!";
            txtHelp.Font = new Font(txtHelp.Font.FontFamily, 14);
            txtHelp.TextAlign = HorizontalAlignment.Left;
            txtHelp.ReadOnly = true;
            txtHelp.Enabled = false;
        }
        #endregion
    }
}
