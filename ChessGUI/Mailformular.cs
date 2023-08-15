using Mail;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mail;
using System.Runtime.InteropServices;

namespace ChessGUI {
    public partial class Mailformular : Form {

        #region Konstruktor
        public Mailformular() {
            InitializeComponent();
        }
        #endregion

        #region Formular laden
        private void Mailformular_Load(object sender, EventArgs e) {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(btnSend, "Versendet eine Mail");
        }
        #endregion

        #region Auf SendButton druecken
        private void BtnSend_Click(object sender, EventArgs e) {
            this.SendMail();
        }
        #endregion

        #region Mail verschicken
        public void SendMail() {
            SchachGUI objektMain = new SchachGUI();
            MailMaker mailObjekt;
            string feedback, message, PutDownloadedFilesInto = Environment.CurrentDirectory + @"\log\", log = String.Empty;
            List<string> lstFiles = new List<string>();
            foreach (string file in Directory.GetFiles(PutDownloadedFilesInto)) {
                log = file;
            }
            if (!String.IsNullOrEmpty(txtadress.Text)) {
                if (this.CheckMailAdress(txtadress.Text)) {
                    mailObjekt = new MailMaker(txtadress.Text) {
                        Absender = "kipp.thomas@gmx.net",
                        Empfaenger = new List<string>() { "tklustig.thomas@gmail.com" },
                        Betreff = "Schachserver - Error",
                        Nachricht = "Der Schachserver ist offline. Aktuell koennen keine Zuege uebertragen werden!",
                        Servername = "mail.gmx.net",
                        Port = 587,
                        Username = "kipp.thomas@gmx.net",
                        Passwort = "strengGeheim",
                        Anhaenge = new List<Attachment> { new Attachment(log) },
                        Ssl = true,
                    };
                    feedback = mailObjekt.Send();
                    if (feedback.Contains("OK")) {
                        message = "Die Mail wurde an den Programmierer verschickt. Er wird sich alsbald bei Ihnen melden";
                        objektMain.Ausgabe(message, "Info", MessageBoxIcon.Information);
                        this.Close();
                    } else {
                        message = "Die Mail konnte nicht verschickt werden:" + Environment.NewLine;
                        objektMain.Ausgabe(message + feedback, "Error", MessageBoxIcon.Error);
                    }
                } else {
                    message = "Bitte eine valide Mailadresse eingeben!";
                    objektMain.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                }
            } else {
                message = "Bitte geben Sie Ihre Mailadresse an!";
                objektMain.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
            }
        }

        public void SendMail(string betreff, string content) {
            SchachGUI objektMain = new SchachGUI();
            MailMaker mailObjekt;
            string feedback, message, PutDownloadedFilesInto = Environment.CurrentDirectory + @"\log\", log = String.Empty;
            List<string> lstFiles = new List<string>();
            foreach (string file in Directory.GetFiles(PutDownloadedFilesInto)) {
                log = file;
            }
            mailObjekt = new MailMaker(txtadress.Text) {
                Absender = "kipp.thomas@gmx.net",
                Empfaenger = new List<string>() { "tklustig.thomas@gmail.com" },
                Betreff = betreff,
                Nachricht = content,
                Servername = "mail.gmx.net",
                Port = 587,
                Username = "kipp.thomas@gmx.net",
                Passwort = "strengGeheim",
                Anhaenge = new List<Attachment> { new Attachment(log) },
                Ssl = true,
            };
            feedback = mailObjekt.Send();
            if (feedback.Contains("OK")) {
                message = "Die Mail wurde an den Programmierer verschickt. Er wird sich alsbald bei Ihnen melden";
                objektMain.Ausgabe(message, "Info", MessageBoxIcon.Information);
                this.Close();
            } else {
                message = "Die Mail konnte nicht verschickt werden:" + Environment.NewLine;
                objektMain.Ausgabe(message + feedback, "Error", MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Mailadresse auf Validitaet pruefen
        private bool CheckMailAdress(string email) {
            bool isValid;
            var valid = new EmailAddressAttribute();
            if (valid.IsValid(email))
                isValid = true;
            else
                isValid = false;
            return isValid;
        }
        #endregion
    }
}
