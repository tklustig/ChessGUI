using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.IO;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using DeCryptEnCrypt;

namespace ChessGUI {
    class chessServer {

        #region globale Instanzen
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient client = new HttpClient();
        private SchachGUI objekt = new SchachGUI();
        #endregion

        #region Verbinden
        public List<string> Connect(string url) {
            List<string> giveBack = new List<string>();
            try {

                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                WebResponse response = request.GetResponse();
                giveBack.Add(((HttpWebResponse)response).Server);
                giveBack.Add(((HttpWebResponse)response).StatusDescription);
                response.Close();
            }
            catch (WebException er) {
                giveBack.Add(er.Message);
            }
            return giveBack;
        }
        #endregion

        #region Request von PHP holen
        public string FetchRequestFromPHP(string url) {
            string response = String.Empty;
            response = SendRequest(url);
            if (!String.IsNullOrEmpty(response)) {
                return response;
            } else {
                response = "Der Schachserver hat nix zurueck geliefert";
                _logger.Warn("Keine der im PHP Script(index.php) vorhandenen Konditionen ergab ein TRUE/Der Schachserver hat nix zurueck geliefert");
            }
            return response;
        }
        #endregion

        #region Request senden
        private string SendRequest(string url) {
            try {
                using (WebClient client = new WebClient()) {
                    return client.DownloadString(new Uri(url));
                }
            }
            catch (WebException ex) {
                string errorAusgabe = "Error while receiving data from the server" + Environment.NewLine + ex.Message + " Something broke.. :(";
                return errorAusgabe;
            }
        }
        #endregion

        #region Dateien ueber SSH runterladen
        public void DownloadFiles(string url, string username, string password) {
            Encryption deCryptEnCrypt = new Encryption();
            string pfad = "/var/www/html/chessServer/pfad/", PutDownloadedFilesInto = Environment.CurrentDirectory + @"\Anmeldedaten\";
            List<string> Files = new List<string>();
            string dateNow = DateTime.Now.ToShortDateString(), dateFile;
            //alle (neuen) Dateien runterladen
            this.DowloadFiles(url, username, password, pfad, PutDownloadedFilesInto);
            //Passwort verschluesseln
            foreach (string file in Directory.GetFiles(PutDownloadedFilesInto)) {
                dateFile = File.GetCreationTime(file).ToShortDateString();
                if (String.Equals(dateNow, dateFile))
                    Files.Add(file);
            }
            foreach (string item in Files) {
                if (item.Contains("passwort")) {
                    StreamReader inputStreamReader = File.OpenText(item);
                    string fileContent = inputStreamReader.ReadToEnd();
                    inputStreamReader.Close();
                    fileContent = fileContent.Replace(fileContent, deCryptEnCrypt.EncryptString(fileContent, ""));
                    if (File.Exists(item)) {
                        StreamWriter outputStreamWriter = File.CreateText(item);
                        outputStreamWriter.Write(fileContent);
                        outputStreamWriter.Close();
                    }
                }
            }
        }
        #endregion

        #region Datenbestand synchronisieren
        public void ContactApache(string url, string username, string password) {
            Encryption deCryptEnCrypt = new Encryption();
            string pfad = "/var/www/html/chessServer/pfad/", PutDownloadedFilesInto = Environment.CurrentDirectory + @"\Anmeldedaten\";
            List<string> Files = new List<string>();
            int counter = 0, counter_ = 0;            
            DirectoryInfo ParentDirectory = new DirectoryInfo(PutDownloadedFilesInto);
            //pruefen, ob Datenbestand aktuell. Falls nicht, alles loeschen und erneut runterladen
            foreach (FileInfo f in ParentDirectory.GetFiles()) {
                counter++;
            }
            using (var sftp = new SftpClient(url, username, password)) {
                sftp.Connect();
                List<SftpFile> files = this.ListDirectory(url, username, password, pfad);
                foreach (var item in files) {
                    if (!item.Name.Equals(".") && !item.Name.Equals("..")) {
                        counter_++;
                    }
                }
                sftp.Disconnect();
            }
            if (counter != counter_) {
                foreach (FileInfo f in ParentDirectory.GetFiles()) {
                    f.Delete();
                }
                this.DowloadFiles(url, username, password, pfad, PutDownloadedFilesInto);
                //Passwort verschluesseln
                foreach (string file in Directory.GetFiles(PutDownloadedFilesInto)) {
                    Files.Add(file);
                }
                foreach (string item in Files) {
                    if (item.Contains("passwort")) {
                        StreamReader inputStreamReader = File.OpenText(item);
                        string fileContent = inputStreamReader.ReadToEnd();
                        inputStreamReader.Close();
                        fileContent = fileContent.Replace(fileContent, deCryptEnCrypt.EncryptString(fileContent, ""));
                        if (File.Exists(item)) {
                            StreamWriter outputStreamWriter = File.CreateText(item);
                            outputStreamWriter.Write(fileContent);
                            outputStreamWriter.Close();
                        }
                    }
                }
            }
        }

        #endregion

        #region Dateien aus SSH Ordner loeschen
        public void DeleteFiles(string url, string usernameForSSH, string passwordForSSH) {
            string pfad = "/var/www/html/chessServer/pfad/";
            using (var sftp = new SftpClient(url, usernameForSSH, passwordForSSH)) {
                sftp.Connect();
                sftp.Delete(pfad);
            }
        }
        #endregion

        #region SSH Ordner scannen
        private List<SftpFile> ListDirectory(string url, string username, string password, string remoteDirectory) {
            List<SftpFile> lstFiles = new List<SftpFile>();
            using (var sftp = new SftpClient(url, username, password)) {
                sftp.Connect();
                var files = sftp.ListDirectory(remoteDirectory);
                foreach (var file in files) {
                    lstFiles.Add(file);
                }
                sftp.Disconnect();
            }
            return lstFiles;
        }
        #endregion

        #region Dateien runterladen
        private void DowloadFiles(string url, string username, string password, string pfad, string PutDownloadedFilesInto) {
            using (var sftp = new SftpClient(url, username, password)) {
                sftp.Connect();
                List<SftpFile> files = this.ListDirectory(url, username, password, pfad);
                foreach (var item in files) {
                    if (!item.Name.Equals(".") && !item.Name.Equals("..") && !File.Exists(PutDownloadedFilesInto + item.Name)) {
                        using (Stream fileStream = File.Create(PutDownloadedFilesInto + item.Name)) {
                            sftp.DownloadFile(pfad + item.Name, fileStream);
                        }
                    }
                }
                sftp.Disconnect();
            }
        }
        #endregion
    }
}
