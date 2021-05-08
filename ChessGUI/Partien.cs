using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using pdf;
using PdfSharp.Drawing;
using System.Diagnostics;
using System.Threading;
using NLog;

namespace ChessGUI {
    public partial class Partien : Form {

        #region globale Variablen
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DataGridView DataGridView_Edit = new DataGridView();
        private SchachGUI form = new SchachGUI();
        private pdfMaker pdfDocument = new pdfMaker();
        private int countRows;
        #endregion

        #region Konstruktor
        public Partien() {
            InitializeComponent();
            DataGridView_Edit.CellClick += DataGridView_Edit_CellClick;
        }
        #endregion

        #region Initialisiere DataGrid
        private void SetupDataGridView_Edit() {
            this.Controls.Add(DataGridView_Edit);
            DataGridView_Edit.ColumnCount = 2;
            DataGridView_Edit.Name = "DataGridView_Edit";
            DataGridView_Edit.Location = new Point(100, 100);
            DataGridView_Edit.Size = new Size(500, 400);
            DataGridView_Edit.ReadOnly = true;
            DataGridView_Edit.AllowUserToDeleteRows = false;
            DataGridView_Edit.AllowUserToAddRows = false;
            DataGridView_Edit.Columns[0].Name = "Counter";
            DataGridView_Edit.Columns[1].Name = "Datum/Zuege";
            DataGridView_Edit.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }
        #endregion

        #region Lade Fomular

        private void Partien_Load(object sender, EventArgs e) {
            string username = Environment.UserName;
            string path = @"C:\Users\" + username + @"\ChessGUI\";
            string strPath = path + "moves.log";
            this.SetupDataGridView_Edit();
            ToolTip toolTipBtnExport = new ToolTip();
            toolTipBtnExport.SetToolTip(btnExport, "Erstellt aus der Zugliste eine PDF Datei");
            if (File.Exists(strPath)) {
                var zeilen = File.ReadAllLines(strPath);
                for (int i = 0; i < zeilen.Length; i++) {
                    if (i < zeilen.Length - 1)
                        DataGridView_Edit.Rows.Add(i + 1, zeilen[i], zeilen[i + 1]);
                    else
                        DataGridView_Edit.Rows.Add(i + 1, zeilen[i], zeilen[zeilen.Length - 1]);
                    countRows = i;
                }
            } else {
                string message = "Die Datei " + strPath + " existiert nicht";
                form.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                return;
            }
            this.ColourCells();
            DataGridView_Edit.ClearSelection();
            DataGridView_Edit.Columns.Cast<DataGridViewColumn>().ToList().ForEach(f => f.SortMode = DataGridViewColumnSortMode.NotSortable);
        }
        #endregion

        #region Reagiere auf einen Zellenclick
        private void DataGridView_Edit_CellClick(object sender, DataGridViewCellEventArgs e) {
            DataGridView_Edit.ClearSelection();
        }
        #endregion

        #region reagiert auf den Buttonclick

        private void BtnSearch_Click(object sender, EventArgs e) {
            int spalte = 1, counter = 0, trefferInt = 0;
            string message = String.Empty;
            bool trefferBool = false;
            this.ColourCells();
            //ueberpruefe, ob es einen Treffer gibt
            foreach (DataGridViewRow row in DataGridView_Edit.Rows) {
                if (row.Cells[spalte].Value.ToString().Contains(dtP.Value.ToShortDateString())) {
                    trefferBool = true;
                    break;
                } else if (!String.IsNullOrEmpty(txtBTerm.Text) && row.Cells[spalte].Value.ToString().Contains(txtBTerm.Text)) {
                    trefferBool = true;
                    break;
                } else
                    trefferBool = false;
                counter++;
            }
            counter = 0;
            if (trefferBool) {
                //ueberpruefe, wieviele Treffer es gibt
                foreach (DataGridViewRow row in DataGridView_Edit.Rows) {
                    if (row.Cells[spalte].Value.ToString().Contains(dtP.Value.ToShortDateString())) {
                        trefferInt++;
                    } else if (!String.IsNullOrEmpty(txtBTerm.Text) && row.Cells[spalte].Value.ToString().Contains(txtBTerm.Text)) {
                        trefferInt++;
                    }
                    counter++;
                }
                counter = 0;
                //hebe Treffer rot hervor
                foreach (DataGridViewRow row in DataGridView_Edit.Rows) {
                    if (row.Cells[spalte].Value.ToString().Contains(dtP.Value.ToShortDateString())) {
                        DataGridView_Edit.Rows[counter].DefaultCellStyle.BackColor = Color.Red;
                    } else if (!String.IsNullOrEmpty(txtBTerm.Text) && row.Cells[spalte].Value.ToString().Contains(txtBTerm.Text)) {
                        DataGridView_Edit.Rows[counter].DefaultCellStyle.BackColor = Color.Red;
                    }
                    counter++;
                }
            }

            if (!trefferBool) {
                if (!String.IsNullOrEmpty(txtBTerm.Text))
                    message = "Ihre Suche nach " + txtBTerm.Text + " und " + dtP.Value.ToShortDateString() + " ergab keinen Treffer.";
                else
                    message = "Ihre Suche nach " + dtP.Value.ToShortDateString() + " ergab keinen Treffer.";
                form.Ausgabe(message, "Info", MessageBoxIcon.Hand);
            } else {
                message = "Ihre Suche ergab " + trefferInt + " Treffer. Sie wurden rot hervorgehoben.";
                form.Ausgabe(message, "Info", MessageBoxIcon.Information);
            }
            counter++;
        }
        #endregion

        #region faerbt die Zellen dezent ein
        private void ColourCells() {
            int spalte = 1, counter = 0;
            if (DataGridView_Edit.Rows.Count > 1) {
                foreach (DataGridViewRow row in DataGridView_Edit.Rows) {
                    if (row.Cells[spalte].Value.ToString().Contains("White"))
                        DataGridView_Edit.Rows[counter].DefaultCellStyle.BackColor = Color.FromArgb(206, 246, 206);
                    else if (row.Cells[spalte].Value.ToString().Contains("Black"))
                        DataGridView_Edit.Rows[counter].DefaultCellStyle.BackColor = Color.FromArgb(206, 227, 246);
                    else if (row.Cells[spalte].Value.ToString().Contains("rochaded"))
                        DataGridView_Edit.Rows[counter].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    else
                        DataGridView_Edit.Rows[counter].DefaultCellStyle.BackColor = Color.FromArgb(242, 245, 169);
                    counter++;
                }
            }
        }
        #endregion

        #region Exportiert die GridView in ein PDF Dokument

        private void BtnExport_Click(object sender, EventArgs e) {
            try {
                string[] content = new string[countRows + 1];
                int indizie = 0;
                try {
                    foreach (DataGridViewRow row in DataGridView_Edit.Rows) {
                        content[indizie] = row.Cells[0].Value.ToString() + " " + row.Cells[1].Value.ToString();
                        indizie++;
                    }
                }
                catch (IndexOutOfRangeException er) {
                    string message = "Indizieverletzung bzgl. des Array content:" + Environment.NewLine + "Arraylaenge:" + content.Length + ", Indizie:" + indizie + Environment.NewLine + "Error wurde protokolliert!";
                    _logger.Error(er.Message + Environment.NewLine + er.ToString());
                    form.Ausgabe(message, "Error", MessageBoxIcon.Error);
                }
                string username = Environment.UserName;
                string pdfFilename = DateTime.Now.ToShortDateString() + "_Zugliste.pdf";
                string pathPDF = @"C:\Users\" + username + @"\ChessGUI\PDF\" + pdfFilename;
                byte schriftgroesse = 14;
                XFontStyle styl = XFontStyle.Regular;
                XStringFormat platzierung = XStringFormats.TopLeft;
                XBrush farbe = XBrushes.Black;
                string headline = "*** Zugliste der Applikation SchachGUI - written in C# ***";
                int length = content.Length;
                if (!pdfDocument.generatePDF(content, pathPDF, schriftgroesse, styl, platzierung, farbe, true, headline)) {
                    string message = "PDF Dokument konnte nicht erstellt werden";
                    form.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                }
                if (!pdfDocument.ShowPDF(pathPDF)) {
                    string message = "PDF Dokument konnte nicht erstellt werden";
                    form.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                }
            }
            catch (Exception er) {
                form.Ausgabe(er.Message, "Error", MessageBoxIcon.Error);
            }
        }
        #endregion

        #region loescht die PDF Datei nach Beendigung des Formulars

        private void Partien_FormClosing(object sender, FormClosingEventArgs e) {
            try {
                string username = Environment.UserName;
                string pathPDF = @"C:\Users\" + username + @"\ChessGUI\PDF\";
                FindAndKillProcess("AcroRd32");
                Thread.Sleep(500);
                foreach (string fileName in Directory.GetFiles(pathPDF)) {
                    File.Delete(fileName);
                }
            }
            catch (IOException er) {
                form.Ausgabe(er.Message, "Error", MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Killt den uebergebenen Task
        public void FindAndKillProcess(string name) {
            try {
                foreach (Process clsProcess in Process.GetProcesses()) {
                    if (clsProcess.ProcessName.Contains(name)) {
                        clsProcess.Kill();
                    }
                }
            }
            catch (Exception) { }
        }

        #endregion
    }
}

