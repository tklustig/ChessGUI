using DeCryptEnCrypt;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace ChessGUI {

    #region Enumeration
    public enum Farbe {
        Weiss,
        Schwarz,
        Neutral
    }
    #endregion

    public partial class Zugtransfer : Form {

        #region globale Instanzen
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SchachGUI objektGUI = new SchachGUI();
        private readonly chessServer objektServer = new chessServer();
        private readonly string rootUrl = "http://tklustig.ddns.net/chessServer/";
        private string responsString2Compare, auswertungCopy = String.Empty;
        #endregion

        #region Getter&Setter      
        public static bool haveDeleted { get; private set; }
        public static bool haveLoggedIn { get; private set; }
        private static int countRegistrations { get; set; }
        #endregion

        #region Konstruktor
        public Zugtransfer() {
            InitializeComponent();
        }
        #endregion

        #region Tritt ein, sofern das Hauptformular geladen wird
        private void Zugtransfer_Load(object sender, EventArgs e) {
            timerData.Enabled = false;
            timerTime.Enabled = true;
            string now = DateTime.Now.ToLongTimeString();
            lblTime.Text = now;                    
            this.OfferSelection();
        }
        #endregion

        #region Zeigt die Zuege nach Gegnerauswahl an
        public void GetMove() {
            if (!this.Visible) this.Visible = !this.Visible;
            webBrowser1.Visible = false;
            this.DoActions(true, SchachGUI.moveForServer);
            this.Size = new Size(800, 340);
        }
        #endregion

        #region Zeigt die aktuelle Zeit im Label an
        private void ShowTimeWithSeconds() {
            string now = DateTime.Now.ToLongTimeString();
            lblTime.Text = now;
        }
        #endregion

        #region Ermoeglicht die Zuganzeige des Servers
        private void DoActions(bool status, string zug) {
            string url = String.Empty, auswertung, farbe;
            if (!status) {
                lblanDerReihe.Text = "Die Verbindung zum Schachserver ist beeintraechtigt - Zuguebermittlung derzeit nicht moeglich -\nMelden Sie sich ab und wieder an!";
            } else {
                if (!timerData.Enabled)
                    timerData.Enabled = true;
                timerTime.Enabled = true;
                //ToDo: Response auswerten
                farbe = Gegner.farbe;
                if (farbe.Equals("Weiss"))
                    url = "http://tklustig.ddns.net/chessServer/index.php?spielerWeiss=" + Gegner.nickname + "&&zugWeiss=" + zug;
                else if (farbe.Equals("Schwarz"))
                    url = "http://tklustig.ddns.net/chessServer/index.php?spielerSchwarz=" + Gegner.nickname + "&&zugSchwarz=" + zug;
                auswertung = objektServer.FetchRequestFromPHP(url);
                _logger.Debug("Der Webserver hat folgendes zurueck gegeben:" + Environment.NewLine + auswertung);
                if (responsString2Compare != auswertung)
                    lblanDerReihe.ForeColor = Color.Green;
                else
                    lblanDerReihe.ForeColor = Color.Brown;
                responsString2Compare = auswertung;
                if (auswertung.Contains("Error")) {
                    timerData.Enabled = false;
                    timerTime.Enabled = false;
                    lblStatus.Visible = false; lblStatusStand.Visible = false; lblHeader.Visible = false; lblMove.Visible = false;
                    this.DoActions(false, String.Empty);
                }
                try {
                    string now = DateTime.Now.ToLongTimeString();
                    List<string> response = objektServer.Connect(rootUrl);
                    lblTime.Text = now;
                    if (response.Count > 0) {
                        lblServer.Text = "Server: " + response[0];
                        if (response[1].Contains("OK")) {
                            lblStatusStand.ForeColor = Color.Green;
                            lblStatusStand.Text = "Online";
                        } else {
                            lblStatusStand.ForeColor = Color.Red;
                            lblStatusStand.Text = "Offline";
                        }
                    } else {
                        lblServer.Text = "- Unbekannt -";
                        lblStatusStand.Text = "- Unbekannt -";
                        return;
                    }
                    lblMove.Text = auswertung;
                    if (SchachGUI.whiteTurn) {
                        lblanDerReihe.Text = "Erwartet wird die Zugeingabe fuer den " + Farbe.Weiss.ToString() + "en" + " Spieler via Drag u.Drop";
                        farbe = Farbe.Schwarz.ToString() + "e";
                    } else if (SchachGUI.blackTurn) {
                        lblanDerReihe.Text = "Erwartet wird die Zugeingabe fuer den " + Farbe.Schwarz.ToString() + "en" + " Spieler via Drag u.Drop";
                    } else
                        lblanDerReihe.Text = "- Unbekannt -";
                    FontFamily family = new FontFamily("Comic Sans MS");
                    Font font = new Font(family, 16.0f, FontStyle.Underline);
                    lblHeader.Font = font;
                    lblHeader.Text = "Zugerwartungen und uebermittelte Zuege";
                    //verarbeite eventuelle Zugruecknahmen
                    url = "http://tklustig.ddns.net/chessServer/index.php?checkMoveBack=true";
                    auswertung = objektServer.FetchRequestFromPHP(url);
                    if (!auswertungCopy.Equals(auswertung) && !auswertung.Contains("nix")) {
                        timerData.Enabled = false;
                        _logger.Debug("Der Webserver hat folgendes zurueck gegeben:" + Environment.NewLine + auswertung);
                        DialogResult answer = MessageBox.Show("Bitte bestätigen Sie die folgende Mitteilung:" + Environment.NewLine + auswertung + Environment.NewLine + Environment.NewLine + "Nehmen sie die gegnerische Zugruecknahme in Ihrem Client nur dann zurück, wenn Sie ihn bereits ausgeführt hatten!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        if (answer == DialogResult.OK)
                            timerData.Enabled = true;
                    }
                    auswertungCopy = auswertung;
                }
                catch (WebException er) {
                    _logger.Error(er.Message + Environment.NewLine + er.ToString());
                    timerData.Enabled = false;
                    timerTime.Enabled = false;
                    this.DoActions(false, String.Empty);
                    objektGUI.Ausgabe("Bitte schicken Sie dem Programmierer ueber das Menu eine Mail", "Error", MessageBoxIcon.Error);
                }
                catch (ArgumentOutOfRangeException er) {
                    _logger.Error(er.Message + Environment.NewLine + er.ToString());
                    timerData.Enabled = false;
                    timerTime.Enabled = false;
                    this.DoActions(false, String.Empty);
                    objektGUI.Ausgabe("Bitte schicken Sie dem Programmierer ueber das Menu eine Mail", "Error", MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Timer-Data(Erneuerung der Zuganzeige alle 2 Sekunden
        private void TimerData_Tick(object sender, EventArgs e) {
            this.DoActions(true, SchachGUI.moveForServer);
        }
        #endregion

        #region Timer-Time(Erneuerung der Sekundenazeige im Label
        private void TimerTime_Tick(object sender, EventArgs e) {
            this.ShowTimeWithSeconds();
        }
        #endregion

        #region Tritt ein, wenn der User das Hauptformular schliesst
        private void Zugtransfer_FormClosed(object sender, FormClosedEventArgs e) {
            try {
                timerData.Enabled = false;
                timerTime.Enabled = false;
                //Das Loeschen aller Zuege veranlassen
                string url = "http://tklustig.ddns.net/chessServer/remove.php", response;
                HttpWebRequest webRequest = HttpWebRequest.CreateHttp(url);
                {
                    byte[] data = Encoding.UTF8.GetBytes("");
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.ContentLength = data.Length;
                    using (Stream stream = webRequest.GetRequestStream()) {
                        stream.Write(data, 0, data.Length);
                    }
                }
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse()) {
                    using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream())) {
                        response = streamReader.ReadToEnd();
                    }
                }
                if (!String.IsNullOrEmpty(response) && !response.Contains("0"))
                    _logger.Debug("Antwort vom Schachserver:{0}", response);
                Zugtransfer.haveDeleted = true;
                Zugtransfer.haveLoggedIn = false;
            }
            catch (Exception er) {
                Zugtransfer.haveDeleted = false;
                _logger.Error(er.Message + Environment.NewLine + er.ToString());
            }
        }
        #endregion

        #region Tritt ein, sofern die Website komplett geladen wurde
        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            Gegner fm = new Gegner();
            string url = e.Url.ToString(), message, contentOfLogIn;
            Encryption deCrypt = new Encryption();
            if (Zugtransfer.countRegistrations > 0 && url == rootUrl + "write_password.php") {
                message = "Sie haben sich bereits registriert. Pfui, wollen sie etwa cheaten?";
                objektGUI.Ausgabe(message, "Arghhh...", MessageBoxIcon.Hand);
                _logger.Debug("Cheatversuch während der Registrierung unterbunden.");
                return;
            }
            if (url == rootUrl + "write_password.php") {
                url = "tklustig.ddns.net";
                string username = "tklustig";
                string password = "v+KIVVybtv8MkKfATqSVsUF1o1+cfwAFZazKwfeX1O0=";
                password = deCrypt.DecryptString(password, "");
                contentOfLogIn = webBrowser1.DocumentText;
                if (!contentOfLogIn.Contains("existiert bereits")) {
                    objektServer.DownloadFiles(url, username, password);
                    Zugtransfer.countRegistrations++;
                }
            } else if (url == rootUrl + "decide.php") {
                contentOfLogIn = webBrowser1.DocumentText;
                if (contentOfLogIn.Contains("DENIED")) {
                    message = "Die Logindaten waren inkorrekt. Sind sie bereits registriert? Versuchen Sie es erneut!";
                    objektGUI.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                    webBrowser1.Visible = false;
                    this.Close();
                } else {
                    url = "tklustig.ddns.net";
                    string username = "tklustig";
                    string password = "v+KIVVybtv8MkKfATqSVsUF1o1+cfwAFZazKwfeX1O0=";
                    password = deCrypt.DecryptString(password, "");
                    message = "Der Login hat geklappt. Sie werden jetzt zur Nickwahl weitergeleitet...!";
                    objektGUI.Ausgabe(message, "Info", MessageBoxIcon.Information);
                    webBrowser1.Visible = false;
                    this.Visible = false;
                    objektServer.ContactApache(url, username, password);
                    Zugtransfer.haveLoggedIn = true;
                    fm.Show();
                }
            } else if (url == rootUrl + "anmelden.php" || url == rootUrl + "registrieren.php")
                this.Size = new Size(830, 640);
        }
        #endregion

        #region Bietet die Wahlmoeglichkeit im Websteuerlelement an
        private void OfferSelection() {
            WebBrowserHelper.FixBrowserVersion();
            webBrowser1.Url = new Uri(rootUrl + "auswahl.php");
            this.Size = new Size(830, 220);
        }
        #endregion

        #region Zeigt das PDF Dokument an

        private void BtnPDF_Click(object sender, EventArgs e) {
            try {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\Dokument\";
                Process.Start(path + "webServerTransfer.pdf");
                btnPDF.Visible = true;
            }
            catch (System.ComponentModel.Win32Exception er) {
                _logger.Error(er.Message + Environment.NewLine + er.ToString());
                objektGUI.Ausgabe(er.Message, "Error", MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
