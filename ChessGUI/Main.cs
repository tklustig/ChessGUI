using ChessGUI.Properties;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

/*Codekonventionen:
 (1) Variablen, Klasssen, Propertys und Methoden folgen der Camel Case Notation (2) Variablen und Propertys beginnen mit einem Kleinbuchstaben 
 (3) Methoden und Klassen beginnen mit einem Grossbuchstaben (4) in der Klasse Piece beginnen Getter mit einem Grossbuchstaben
 (5) Kommentare sind in Deutsch verfasst (6) einzeilige Konditionen haben keine Klammern (7) Schleifen haben prinzipiell immer Klammern
     */

namespace ChessGUI {
    public partial class SchachGUI : Form {

        #region Globale Variablen
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private bool whiteKingHasMoved, blackKingHasMoved, haveAlreadySwitched, bauerHaveMoved, kingIsMatt, forbidTakingBackMove;
        private int intMoveCounter = 0, intExPosX, intExPosY, fieldWith, fieldHeight;
        private string strPickNDrop;
        private Board boardObject = new Board();
        private Piece pieceObject = new Piece(), currentPiece;
        private List<Tuple<Piece, string>> listOfBauerMoved = new List<Tuple<Piece, string>>();
        private Piece lastFigureMoved;
        private PictureBox lastFigureIncluded = new PictureBox();
        private Dictionary<Piece, Bitmap> pieceObjectBitmapDict;
        #endregion

        #region Konstruktor
        public SchachGUI() {
            InitializeComponent();
        }
        #endregion

        #region Getter&Setter

        public static string moveForServer { get; private set; }
        public static bool kingHasRochaded { get; set; }
        public static bool rochadeShort { get; set; }
        public static bool rochadeLong { get; set; }
        public static bool kingIsChecked { get; private set; }
        public static bool playOnline { get; private set; }
        public static bool haveRestartedGame { get; set; }
        public static bool whiteTurn { get; private set; }
        public static bool blackTurn { get; private set; }
        public static bool haveTakeBackMove { get; private set; }
        public static int posKingWhiteX { get; set; }
        public static int posKingWhiteY { get; set; }
        public static int posKingBlackX { get; set; }
        public static int posKingBlackY { get; set; }
        #endregion

        #region wird ausgefuehrt, bevor das Forumlar geladen wird
        private void Form1_Load(object sender, EventArgs e) {
            Win.DisableCloseButton(this, false);
            this.InitializeGame();
            this.StelleBrettundFigurenDar();
            lstVMoves.Items.Add(PieceColor.White + " 's turn!");
            this.WriteToTextFile(true);
            //weise der Property die Koenigskooridaten zu
            SchachGUI.posKingWhiteX = 4; SchachGUI.posKingWhiteY = 7;
            SchachGUI.posKingBlackX = 4; SchachGUI.posKingBlackY = 0;
            _logger.Debug("WhiteKingX={0};WhiteKingY={1} / load formular", SchachGUI.posKingWhiteX, SchachGUI.posKingWhiteY);
            _logger.Debug("BlackKingX={0};BlackKingY={1} / load formular", SchachGUI.posKingBlackX, SchachGUI.posKingBlackY);
            SchachGUI.haveRestartedGame = false;
            SchachGUI.whiteTurn = true;
            this.btnPDF.Visible = false;
        }
        #endregion

        #region Menu - Restart

        private void RestartToolStripMenuItem_Click(object sender, EventArgs e) {
            this.RestartGame();
        }
        #endregion

        #region Menu - Exit

        private void BeendenToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                Partien frm = new Partien();
                frm.FindAndKillProcess("AcroRd32");
                Application.Exit();
            }
            catch (Exception er) {
                _logger.Error(er.Message + Environment.NewLine + er.ToString());
                Environment.Exit(-1);
            }
        }
        #endregion

        #region Menu - Zugliste leeren

        private void ZugboxLeerenToolStripMenuItem_Click(object sender, EventArgs e) {
            if (lstVMoves.Items.Count > 0) {
                lstVMoves.Items.Clear();
                string message = "Die Zuegebox wurde erfolgreich bereinigt";
                this.Ausgabe(message, "Info", MessageBoxIcon.Information);
            } else {
                string message = "Die Zuegebox enthaelt noch keinerlei Zuege";
                this.Ausgabe(message, "Info", MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Menu - Zeige Koordinaten

        private void BrettkoordinatenEinblendenToolStripMenuItem_Click_1(object sender, EventArgs e) {
            if (brettkoordinatenEinblendenToolStripMenuItem.Text == "Brettkoordinaten einblenden") {
                this.DrawCoordinates(true);
                brettkoordinatenEinblendenToolStripMenuItem.Text = "Brettkoordinaten ausblenden";
            } else if (brettkoordinatenEinblendenToolStripMenuItem.Text == "Brettkoordinaten ausblenden") {
                this.DrawCoordinates(false);
                brettkoordinatenEinblendenToolStripMenuItem.Text = "Brettkoordinaten einblenden";
            }
        }
        #endregion

        #region Menu - Zugliste in GridView anzeigen
        private void listeAnzeigenToolStripMenuItem_Click(object sender, EventArgs e) {
            Partien frm = new Partien();
            frm.Show();
        }
        #endregion

        #region  Menu- Zugliste loeschen
        private void listeLoeschenToolStripMenuItem_Click(object sender, EventArgs e) {
            string username = Environment.UserName, message;
            string path = @"C:\Users\" + username + @"\ChessGUI\";
            string strPath = path + "moves.log";
            if (File.Exists(strPath)) {
                DialogResult result = MessageBox.Show("Wollen sie die Datei " + strPath + " wirklich loeschen?", "Important Question", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) {
                    File.Delete(strPath);
                    message = "Die Datei " + strPath + " wurde soeben geloescht";
                    this.Ausgabe(message, "Info", MessageBoxIcon.Information);
                }
            } else {
                message = "Die Datei " + strPath + " exisitert nicht!";
                this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
            }
            cbMoves.Checked = false;
        }
        #endregion

        #region Menu - Help
        private void HelpToolStripMenuItem_Click(object sender, EventArgs e) {
            Help frm = new Help();
            frm.Show();
        }
        #endregion

        #region Menu - Zug zurueck nehmen
        private void ZugRueckgaengigMachenToolStripMenuItem_Click(object sender, EventArgs e) {
            this.TakeBackLastMove();
        }
        #endregion

        #region Menu - Schachserver kontaktieren
        private void SchachserverKontaktierenToolStripMenuItem_Click(object sender, EventArgs e) {
            if (schachserverKontaktierenToolStripMenuItem.Text.Contains("abmelden")) {
                string message = "Ihre Zuege werden an den Schachserver nicht mehr uebermittelt. Die Onlinepartie ist hiermit beendet!";
                if (SchachGUI.playOnline)
                    SchachGUI.playOnline = !SchachGUI.playOnline;
                schachserverKontaktierenToolStripMenuItem.Text = "Schachserver kontaktieren";
                this.Ausgabe(message, "Info", MessageBoxIcon.Exclamation);
            } else
                this.transmission();
        }
        #endregion

        #region Menu - Mail versenden
        private void MailVersendenToolStripMenuItem_Click(object sender, EventArgs e) {
            Mailformular fm = new Mailformular();
            fm.Show();
        }
        #endregion

        #region Figurdateien laden
        private void InitializeGame() {
            fieldWith = 64;
            this.fieldHeight = 64;
            string rootFolder = AppDomain.CurrentDomain.BaseDirectory + @"spielfiguren\";
            this.pieceObjectBitmapDict = new Dictionary<Piece, Bitmap>();
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Bauer, PieceColor.Black), new Bitmap(rootFolder + "Bauerblack.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Bauer, PieceColor.White), new Bitmap(rootFolder + "Bauerwhite.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Springer, PieceColor.White), new Bitmap(rootFolder + "Springerwhite.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Springer, PieceColor.Black), new Bitmap(rootFolder + "Springerblack.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Laeufer, PieceColor.White), new Bitmap(rootFolder + "Laeuferwhite.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Laeufer, PieceColor.Black), new Bitmap(rootFolder + "Laeuferblack.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Turm, PieceColor.White), new Bitmap(rootFolder + "Turmwhite.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Turm, PieceColor.Black), new Bitmap(rootFolder + "Turmblack.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Dame, PieceColor.White), new Bitmap(rootFolder + "Damewhite.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Dame, PieceColor.Black), new Bitmap(rootFolder + "Dameblack.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Koenig, PieceColor.White), new Bitmap(rootFolder + "Kingwhite.png"));
            this.pieceObjectBitmapDict.Add(new Piece(PieceType.Koenig, PieceColor.Black), new Bitmap(rootFolder + "Kingblack.png"));
        }
        #endregion

        #region Brett und Figuren darstellen
        private void StelleBrettundFigurenDar([Optional] bool restart) {
            Size tileSize = new Size(fieldWith, this.fieldHeight);
            Bitmap bitmap = CreateBoard(tileSize);
            if (!restart)
                ZeigeFigur(bitmap);
            else
                ZeigeFigur(bitmap, restart);
            picBrett.Image = bitmap;
        }
        #endregion

        #region Stelle alle geschlagenen Figuren dar
        private void ShowFiguresBeaten() {
            //Initialisierungen
            string rootFolder = AppDomain.CurrentDomain.BaseDirectory + @"spielfiguren\";
            string bauerBlack = rootFolder + "Bauerblack.png", springerBlack = rootFolder + "Springerblack.png", laeuferBlack = rootFolder + "LaeuferBlack.png", dameBlack = rootFolder + "Dameblack.png", turmBlack = rootFolder + "Turmblack.png";
            string bauerWhite = rootFolder + "Bauerwhite.png", springerWhite = rootFolder + "Springerwhite.png", laeuferWhite = rootFolder + "LaeuferWhite.png", dameWhite = rootFolder + "Damewhite.png", turmWhite = rootFolder + "Turmwhite.png";
            int counterWbauer = 0, counterWspringer = 0, counterWlaeufer = 0, counterWturm = 0, counterBbauer = 0, counterBspringer = 0, counterBlaeufer = 0, counterBturm = 0;
            Image bauerW = Image.FromFile(bauerWhite); Image springerW = Image.FromFile(springerWhite); Image laeuferW = Image.FromFile(laeuferWhite); Image turmW = Image.FromFile(turmWhite); Image dameW = Image.FromFile(dameWhite);
            Image bauerB = Image.FromFile(bauerBlack); Image springerB = Image.FromFile(springerBlack); Image laeuferB = Image.FromFile(laeuferBlack); Image turmB = Image.FromFile(turmBlack); Image dameB = Image.FromFile(dameBlack);
            bool queenW = false, queenB = false;
            //Setze alle Pictureboxen zurück
            foreach (Control control in this.Controls) {
                if (control is PictureBox && control.Name != "picBrett") {
                    ((PictureBox)control).Image = null;
                }
            }
            //Bildgroesse der weissen Figuren verkleinern =>definiere die Hoehe/Breite der geschlagenen Figuren
            byte figurenHoehe = 40, figurenBreite = 40;
            //instanziiere ein Objekt der Klasse und verkleiner das Bild mit der Methode fuer alle vorhandenen Bilder bzw. Figuren
            ImageFunctions objectResize = new ImageFunctions();
            MemoryStream convertTurmW = objectResize.ResizeImage(turmW, figurenHoehe, figurenBreite, ImageFormat.Png);
            //weise dem Bild bzw. der Figur den veraenderten Groessenstream zu. 
            turmW = Image.FromStream(convertTurmW);
            MemoryStream convertspringerW = objectResize.ResizeImage(springerW, figurenHoehe, figurenBreite, ImageFormat.Png);
            springerW = Image.FromStream(convertspringerW);
            MemoryStream convertlaeuferW = objectResize.ResizeImage(laeuferW, figurenHoehe, figurenBreite, ImageFormat.Png);
            laeuferW = Image.FromStream(convertlaeuferW);
            MemoryStream convertDameW = objectResize.ResizeImage(dameW, figurenHoehe, figurenBreite, ImageFormat.Png);
            dameW = Image.FromStream(convertDameW);
            MemoryStream convertBauerW = objectResize.ResizeImage(bauerW, figurenHoehe, figurenBreite, ImageFormat.Png);
            bauerW = Image.FromStream(convertBauerW);
            //dasselbe fuer die schwarzen Figuren
            MemoryStream convertTurmB = objectResize.ResizeImage(turmB, figurenHoehe, figurenBreite, ImageFormat.Png);
            turmB = Image.FromStream(convertTurmB);
            MemoryStream convertspringerB = objectResize.ResizeImage(springerB, figurenHoehe, figurenBreite, ImageFormat.Png);
            springerB = Image.FromStream(convertspringerB);
            MemoryStream convertlaeuferB = objectResize.ResizeImage(laeuferB, figurenHoehe, figurenBreite, ImageFormat.Png);
            laeuferB = Image.FromStream(convertlaeuferB);
            MemoryStream convertDameB = objectResize.ResizeImage(dameB, figurenHoehe, figurenBreite, ImageFormat.Png);
            dameB = Image.FromStream(convertDameB);
            MemoryStream convertBauerB = objectResize.ResizeImage(bauerB, figurenHoehe, figurenBreite, ImageFormat.Png);
            bauerB = Image.FromStream(convertBauerB);
            //Eruiere die Anzahl der jeweils geschlagenen figuren fuer Weiss und Schwarz
            if (Board.lstfiguresBeaten != null && Board.lstfiguresBeaten.Count > 0) {
                foreach (Piece item in Board.lstfiguresBeaten) {
                    if (item != null && item.Color == PieceColor.White && item.Type == PieceType.Bauer)
                        counterWbauer++;
                    else if (item != null && item.Color == PieceColor.White && item.Type == PieceType.Springer)
                        counterWspringer++;
                    else if (item != null && item.Color == PieceColor.White && item.Type == PieceType.Laeufer)
                        counterWlaeufer++;
                    else if (item != null && item.Color == PieceColor.White && item.Type == PieceType.Turm)
                        counterWturm++;
                    else if (item != null && item.Color == PieceColor.White && item.Type == PieceType.Dame)
                        queenW = true;
                    else if (item != null && item.Color == PieceColor.Black && item.Type == PieceType.Bauer)
                        counterBbauer++;
                    else if (item != null && item.Color == PieceColor.Black && item.Type == PieceType.Springer)
                        counterBspringer++;
                    else if (item != null && item.Color == PieceColor.Black && item.Type == PieceType.Laeufer)
                        counterBlaeufer++;
                    else if (item != null && item.Color == PieceColor.Black && item.Type == PieceType.Turm)
                        counterBturm++;
                    else if (item != null && item.Color == PieceColor.Black && item.Type == PieceType.Dame)
                        queenB = true;
                }
            }
            //die Dame als einzige nicht doppelt vorhandene geschlagene Figur ueber das Boolean darstellen
            if (queenW) {
                picWhiteDame.Image = dameW;
                lastFigureIncluded = picWhiteDame;
            }
            if (queenB) {
                picBlackDame.Image = dameB;
                lastFigureIncluded = picBlackDame;
            }
            //alle anderen Figuren abhaengig von den inkrementierten counter Variablen darstellen
            switch (counterWspringer) {
                case 1:
                    picWhiteSpringer1.Image = springerW;
                    lastFigureIncluded = picWhiteSpringer1;
                    break;
                case 2:
                    picWhiteSpringer1.Image = springerW;
                    picWhiteSpringer2.Image = springerW;
                    lastFigureIncluded = picWhiteSpringer2;
                    break;
                default:
                    break;
            }
            switch (counterWlaeufer) {
                case 1:
                    picWhiteLaeufer1.Image = laeuferW;
                    lastFigureIncluded = picWhiteLaeufer1;
                    break;
                case 2:
                    picWhiteLaeufer1.Image = laeuferW;
                    picWhiteLaeufer2.Image = laeuferW;
                    lastFigureIncluded = picWhiteLaeufer2;
                    break;
                default:
                    break;
            }
            switch (counterWturm) {
                case 1:
                    picWhiteTurm1.Image = turmW;
                    lastFigureIncluded = picWhiteTurm1;
                    break;
                case 2:
                    picWhiteTurm1.Image = turmW;
                    picWhiteTurm2.Image = turmW;
                    lastFigureIncluded = picWhiteTurm2;
                    break;
                default:
                    break;
            }
            switch (counterWbauer) {
                case 1:
                    picWhiteBauer1.Image = bauerW;
                    lastFigureIncluded = picWhiteBauer1;
                    break;
                case 2:
                    picWhiteBauer1.Image = bauerW;
                    picWhiteBauer2.Image = bauerW;
                    lastFigureIncluded = picWhiteBauer2;
                    break;
                case 3:
                    picWhiteBauer1.Image = bauerW;
                    picWhiteBauer2.Image = bauerW;
                    picWhiteBauer3.Image = bauerW;
                    lastFigureIncluded = picWhiteBauer3;
                    break;
                case 4:
                    picWhiteBauer1.Image = bauerW;
                    picWhiteBauer2.Image = bauerW;
                    picWhiteBauer3.Image = bauerW;
                    picWhiteBauer4.Image = bauerW;
                    lastFigureIncluded = picWhiteBauer4;
                    break;
                case 5:
                    picWhiteBauer1.Image = bauerW;
                    picWhiteBauer2.Image = bauerW;
                    picWhiteBauer3.Image = bauerW;
                    picWhiteBauer4.Image = bauerW;
                    picWhiteBauer5.Image = bauerW;
                    lastFigureIncluded = picWhiteBauer5;
                    break;
                case 6:
                    picWhiteBauer1.Image = bauerW;
                    picWhiteBauer2.Image = bauerW;
                    picWhiteBauer3.Image = bauerW;
                    picWhiteBauer4.Image = bauerW;
                    picWhiteBauer5.Image = bauerW;
                    picWhiteBauer6.Image = bauerW;
                    lastFigureIncluded = picWhiteBauer6;
                    break;
                case 7:
                    picWhiteBauer1.Image = bauerW;
                    picWhiteBauer2.Image = bauerW;
                    picWhiteBauer3.Image = bauerW;
                    picWhiteBauer4.Image = bauerW;
                    picWhiteBauer5.Image = bauerW;
                    picWhiteBauer6.Image = bauerW;
                    picWhiteBauer7.Image = bauerW;
                    lastFigureIncluded = picWhiteBauer7;
                    break;
                case 8:
                    picWhiteBauer1.Image = bauerW;
                    picWhiteBauer2.Image = bauerW;
                    picWhiteBauer3.Image = bauerW;
                    picWhiteBauer4.Image = bauerW;
                    picWhiteBauer5.Image = bauerW;
                    picWhiteBauer6.Image = bauerW;
                    picWhiteBauer7.Image = bauerW;
                    picWhiteBauer8.Image = bauerW;
                    lastFigureIncluded = picWhiteBauer8;
                    break;
                default:
                    break;
            }
            switch (counterBspringer) {
                case 1:
                    picBlackSpringer1.Image = springerB;
                    lastFigureIncluded = picBlackSpringer1;
                    break;
                case 2:
                    picBlackSpringer1.Image = springerB;
                    picBlackSpringer2.Image = springerB;
                    lastFigureIncluded = picBlackSpringer2;
                    break;
                default:
                    break;
            }
            switch (counterBlaeufer) {
                case 1:
                    picBlackLaeufer1.Image = laeuferB;
                    lastFigureIncluded = picBlackLaeufer1;
                    break;
                case 2:
                    picBlackLaeufer1.Image = laeuferB;
                    picBlackLaeufer2.Image = laeuferB;
                    lastFigureIncluded = picBlackLaeufer2;
                    break;
                default:
                    break;
            }
            switch (counterBturm) {
                case 1:
                    picBlackTurm1.Image = turmB;
                    lastFigureIncluded = picBlackTurm1;
                    break;
                case 2:
                    picBlackTurm1.Image = turmB;
                    picBlackTurm2.Image = turmB;
                    lastFigureIncluded = picBlackTurm2;
                    break;
                default:
                    break;
            }
            switch (counterBbauer) {
                case 1:
                    picBlackBauer1.Image = bauerB;
                    lastFigureIncluded = picBlackBauer1;
                    break;
                case 2:
                    picBlackBauer1.Image = bauerB;
                    picBlackBauer2.Image = bauerB;
                    lastFigureIncluded = picBlackBauer2;
                    break;
                case 3:
                    picBlackBauer1.Image = bauerB;
                    picBlackBauer2.Image = bauerB;
                    picBlackBauer3.Image = bauerB;
                    lastFigureIncluded = picBlackBauer3;
                    break;
                case 4:
                    picBlackBauer1.Image = bauerB;
                    picBlackBauer2.Image = bauerB;
                    picBlackBauer3.Image = bauerB;
                    picBlackBauer4.Image = bauerB;
                    lastFigureIncluded = picBlackBauer4;
                    break;
                case 5:
                    picBlackBauer1.Image = bauerB;
                    picBlackBauer2.Image = bauerB;
                    picBlackBauer3.Image = bauerB;
                    picBlackBauer4.Image = bauerB;
                    picBlackBauer5.Image = bauerB;
                    lastFigureIncluded = picBlackBauer5;
                    break;
                case 6:
                    picBlackBauer1.Image = bauerB;
                    picBlackBauer2.Image = bauerB;
                    picBlackBauer3.Image = bauerB;
                    picBlackBauer4.Image = bauerB;
                    picBlackBauer5.Image = bauerB;
                    picBlackBauer6.Image = bauerB;
                    lastFigureIncluded = picBlackBauer6;
                    break;
                case 7:
                    picBlackBauer1.Image = bauerB;
                    picBlackBauer2.Image = bauerB;
                    picBlackBauer3.Image = bauerB;
                    picBlackBauer4.Image = bauerB;
                    picBlackBauer5.Image = bauerB;
                    picBlackBauer6.Image = bauerB;
                    picBlackBauer7.Image = bauerB;
                    lastFigureIncluded = picBlackBauer7;
                    break;
                case 8:
                    picBlackBauer1.Image = bauerB;
                    picBlackBauer2.Image = bauerB;
                    picBlackBauer3.Image = bauerB;
                    picBlackBauer4.Image = bauerB;
                    picBlackBauer5.Image = bauerB;
                    picBlackBauer6.Image = bauerB;
                    picBlackBauer7.Image = bauerB;
                    picBlackBauer8.Image = bauerB;
                    lastFigureIncluded = picBlackBauer8;
                    break;
                default:
                    break;
            }
            if (!SchachGUI.haveTakeBackMove)
                lastFigureIncluded = null;
        }
        #endregion

        #region Brett zeichnen

        private Bitmap CreateBoard(Size tileSize) {
            Pen myBrush = new Pen(Color.Red);
            int fieldWith = tileSize.Width;
            int fieldHeight = tileSize.Height;
            Bitmap bitmap = new Bitmap(fieldWith * 8, fieldHeight * 8);
            using (Graphics graphics = Graphics.FromImage(bitmap)) {
                for (int x = 0; x < 8; x++) {
                    for (int y = 0; y < 8; y++) {
                        Brush brush = (x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0) ? Brushes.White : Brushes.Black;
                        graphics.FillRectangle(brush, new Rectangle(x * fieldWith, y * fieldHeight, fieldWith, fieldHeight));
                    }
                }
                this.ShowPossibleMoves(bitmap);
            }
            return bitmap;
        }
        #endregion

        #region Figuren zeichnen
        private void ZeigeFigur(Bitmap bitmap, [Optional] bool restart) {
            Board boardObject_ = new Board();
            using (Graphics graphics = Graphics.FromImage(bitmap)) {
                for (int x = 0; x < 8; x++) {
                    for (int y = 0; y < 8; y++) {
                        if (!restart)
                            pieceObject = boardObject.GetPiece(x, y);
                        else
                            pieceObject = boardObject_.GetPiece(x, y);
                        if (pieceObject != null) {
                            Bitmap bitmap1 = this.pieceObjectBitmapDict[pieceObject];
                            graphics.DrawImageUnscaled(bitmap1, new Point(x * this.fieldWith, y * this.fieldHeight));
                        }
                    }
                }
                if (restart)
                    boardObject = boardObject_;
            }
        }

        #endregion

        #region moegliche Zuege anzeigen(ToComplete)
        private Bitmap ShowPossibleMoves(Bitmap bitmap) {
            /*int SignX = 191 + 2 * 65, SignY = 320 + 2 * 65, SignLength = 65, dummySignX = SignX, dummyX, dummyY, counter = 0;
            Pen myBrush = new Pen(Color.Green, 5);
            dummyX = intExPosX; dummyY = intExPosY;
            using (Graphics graphics = Graphics.FromImage(bitmap)) {
                if (currentPiece != null && currentPiece.Type == PieceType.Laeufer && currentPiece.Color == PieceColor.White) {
                    while (dummyX > 0 && dummyX < 8 || dummyY > 0 && dummyY < 8) {
                        if (boardObject.GetPiece(dummyX - 1, dummyY - 1) != null) break;

                        dummyX--;
                        dummyY--;
                        counter++;
                    }
                    for (int i = 0; i <= counter; i++) {
                        graphics.DrawRectangle(myBrush, new Rectangle(SignX - (i * SignLength), SignY - (i * SignLength), SignLength, SignLength));
                    }
                }
            }
            */
            return bitmap;


        }
        #endregion

        #region Leere Brett
        private void LeereBrett() {
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    boardObject.SetPiece(x, y, null);
                }
            }
        }
        #endregion

        #region Figur anheben / Zuglistdatei fuellen
        private void PickPiece(MouseEventArgs e) {
            //setzte den Rochierstatus zurueck, da in der Klasse Board nur einmal geprueft werden soll
            if (SchachGUI.kingHasRochaded)
                SchachGUI.kingHasRochaded = !SchachGUI.kingHasRochaded;
            if (SchachGUI.rochadeShort)
                SchachGUI.rochadeShort = !SchachGUI.rochadeShort;
            else if (SchachGUI.rochadeLong)
                SchachGUI.rochadeLong = !SchachGUI.rochadeLong;
            if (Board.figureHasBeenBeaten) {
                Board.figureHasBeenBeaten = !Board.figureHasBeenBeaten;
                _logger.Debug("figureHasBeenBeaten wurde auf {0} gesetzt", Board.figureHasBeenBeaten);
            }
            string message;
            if (!kingIsMatt) {
                char bezAbisH = 'A';
                byte bez1bis8 = 0;
                Point location = e.Location;
                int x = location.X / this.fieldWith;
                int y = location.Y / fieldHeight;
                // Eine Figur anheben
                pieceObject = boardObject.GetPiece(x, y);
                if (pieceObject != null) {
                    if (intMoveCounter == 0 && pieceObject.Color == PieceColor.Black) {
                        message = "Im Schach wird der erste Zug mit Weiss gemacht!";
                        this.Ausgabe(message, "Warnung", MessageBoxIcon.Exclamation);
                        return;
                    }
                    if (SchachGUI.blackTurn && pieceObject.Color == PieceColor.White) {
                        message = "Weiss ist nicht an der Reihe!";
                        this.Ausgabe(message, "Warnung", MessageBoxIcon.Exclamation);
                        return;
                    } else if (SchachGUI.whiteTurn && pieceObject.Color == PieceColor.Black) {
                        message = "Schwarz ist nicht an der Reihe!";
                        this.Ausgabe(message, "Warnung", MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                // sorgt dafuer, dass die angehobene Figur aus dem Feld verschwindet
                boardObject.SetPiece(x, y, null);

                if (pieceObject != null) {
                    switch (x + 1) {
                        case 1:
                            bezAbisH = 'A';
                            break;
                        case 2:
                            bezAbisH = 'B';
                            break;
                        case 3:
                            bezAbisH = 'C';
                            break;
                        case 4:
                            bezAbisH = 'D';
                            break;
                        case 5:
                            bezAbisH = 'E';
                            break;
                        case 6:
                            bezAbisH = 'F';
                            break;
                        case 7:
                            bezAbisH = 'G';
                            break;
                        case 8:
                            bezAbisH = 'H';
                            break;
                    }
                    switch (y) {
                        case 0:
                            bez1bis8 = 8;
                            break;
                        case 1:
                            bez1bis8 = 7;
                            break;
                        case 2:
                            bez1bis8 = 6;
                            break;
                        case 3:
                            bez1bis8 = 5;
                            break;
                        case 4:
                            bez1bis8 = 4;
                            break;
                        case 5:
                            bez1bis8 = 3;
                            break;
                        case 6:
                            bez1bis8 = 2;
                            break;
                        case 7:
                            bez1bis8 = 1;
                            break;
                    }
                    lblZug.Text = string.Format("You picked a {0} {1} at {2}{3}", pieceObject.Color, pieceObject.Type, bezAbisH, bez1bis8);
                    strPickNDrop = pieceObject.Type + bezAbisH.ToString() + bez1bis8.ToString();
                } else {
                    lblZug.Text = "Nothing there !";
                    return;
                }
                currentPiece = pieceObject;
                intExPosX = x;
                intExPosY = y;
            } else {
                message = "Das Spiel wurde bereits beendet. Wollen Sie ein Neues starten?";
                DialogResult dialogResult = MessageBox.Show(message, "Game Over", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (dialogResult == DialogResult.Yes)
                    this.RestartGame();
                else
                    return;
            }
        }

        private void AuthorToolStripMenuItem_Click(object sender, EventArgs e) {
            About fm = new About();
            fm.Show();
        }
        #endregion

        #region Figur ablegen /Zuglistdatei fuellen
        private void DropPiece(MouseEventArgs e) {
            //bool askForRestart;
            if (currentPiece == null) {
                string m = "Error. Aktuelle Spielfigur wurde nicht geliefert!";
                _logger.Error(m);
                throw new Exception(m);
            }
            char bezAbisH = 'A';
            byte bez1bis8 = 0;
            bool printRochade = false;
            Point location = e.Location;
            int x = location.X / this.fieldWith;
            int y = location.Y / this.fieldHeight;
            string message = String.Empty;
            if (x == intExPosX && y == intExPosY) {
                message = "Bitte ziehen Sie mittels einer fluessigen Bewegung. Setzen Sie die Figur nur dort ab, wo Sie sie haben wollen!";
                this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                boardObject.SetPiece(x, y, currentPiece);
                return;
            }
            lblZug.ForeColor = Color.Black;
            switch (x + 1) {
                case 1:
                    bezAbisH = 'A';
                    break;
                case 2:
                    bezAbisH = 'B';
                    break;
                case 3:
                    bezAbisH = 'C';
                    break;
                case 4:
                    bezAbisH = 'D';
                    break;
                case 5:
                    bezAbisH = 'E';
                    break;
                case 6:
                    bezAbisH = 'F';
                    break;
                case 7:
                    bezAbisH = 'G';
                    break;
                case 8:
                    bezAbisH = 'H';
                    break;
            }
            switch (y) {
                case 0:
                    bez1bis8 = 8;
                    break;
                case 1:
                    bez1bis8 = 7;
                    break;
                case 2:
                    bez1bis8 = 6;
                    break;
                case 3:
                    bez1bis8 = 5;
                    break;
                case 4:
                    bez1bis8 = 4;
                    break;
                case 5:
                    bez1bis8 = 3;
                    break;
                case 6:
                    bez1bis8 = 2;
                    break;
                case 7:
                    bez1bis8 = 1;
                    break;
            }
            //Wurde ein Bauer bereits gezogen?
            var actualFigure = new Tuple<Piece, string>(currentPiece, intExPosX.ToString() + intExPosY.ToString());
            if (listOfBauerMoved.Contains(actualFigure))
                bauerHaveMoved = true;
            else
                bauerHaveMoved = false;
            //kurze Rochade
            if (currentPiece.Color == PieceColor.White && currentPiece.Type == PieceType.Koenig && bezAbisH == 'G' && bez1bis8 == 1 && !whiteKingHasMoved) {
                //Rochade untersagen, sofern der Koenig im Schach steht
                if (SchachGUI.kingIsChecked) {
                    boardObject.SetPiece(4, 7, currentPiece);
                    message = "Rochade nicht erlaubt. Koenig steht im Schach";
                    this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                    return;
                }
                Piece checkIfPieceIsEmpty1 = boardObject.GetPiece(5, 7);
                Piece checkIfPieceIsEmpty2 = boardObject.GetPiece(6, 7);
                if (checkIfPieceIsEmpty1 == null && checkIfPieceIsEmpty2 == null) {
                    boardObject.SetRochadeKurz("white");
                    printRochade = !printRochade;
                    lblZug.Text = "You rochaded short";
                    SchachGUI.kingHasRochaded = true;
                    SchachGUI.rochadeShort = true;
                    SchachGUI.posKingWhiteX = 6; SchachGUI.posKingWhiteY = 7;
                    _logger.Debug("WhiteKingX={0},WhiteKingY={1} / short rochaded", SchachGUI.posKingWhiteX, SchachGUI.posKingWhiteY);

                } else {
                    boardObject.SetPiece(4, 7, currentPiece);
                    SchachGUI.whiteTurn = true;
                    SchachGUI.blackTurn = false;
                    haveAlreadySwitched = true;
                }
            } else if (currentPiece.Color == PieceColor.Black && currentPiece.Type == PieceType.Koenig && bezAbisH == 'G' && bez1bis8 == 8 && !blackKingHasMoved) {
                //Rochade untersagen, sofern der koenig im Schach steht
                if (SchachGUI.kingIsChecked) {
                    boardObject.SetPiece(4, 0, currentPiece);
                    message = "Rochade nicht erlaubt. Koenig steht im Schach";
                    this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                    return;
                }
                Piece checkIfPieceIsEmpty1 = boardObject.GetPiece(5, 0);
                Piece checkIfPieceIsEmpty2 = boardObject.GetPiece(6, 0);
                if (checkIfPieceIsEmpty1 == null && checkIfPieceIsEmpty2 == null) {
                    boardObject.SetRochadeKurz("black");
                    printRochade = !printRochade;
                    lblZug.Text = "You rochaded short";
                    SchachGUI.kingHasRochaded = true;
                    SchachGUI.rochadeShort = true;
                    SchachGUI.posKingBlackX = 6; SchachGUI.posKingBlackY = 0;
                    _logger.Debug("BlackKingX={0},BlackKingY={1} / short rochaded", SchachGUI.posKingBlackX, SchachGUI.posKingBlackY);

                } else {
                    boardObject.SetPiece(4, 0, currentPiece);
                    SchachGUI.whiteTurn = false;
                    SchachGUI.blackTurn = true;
                    haveAlreadySwitched = true;
                }
                //lange Rochade  
            } else if (currentPiece.Color == PieceColor.White && currentPiece.Type == PieceType.Koenig && bezAbisH == 'C' && bez1bis8 == 1 && !whiteKingHasMoved) {
                //Rochade untersagen, sofern der koenig im Schach steht
                if (SchachGUI.kingIsChecked) {
                    boardObject.SetPiece(4, 0, currentPiece);
                    message = "Rochade nicht erlaubt. Koenig steht im Schach";
                    this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                    return;
                }
                Piece checkIfPieceIsEmpty1 = boardObject.GetPiece(1, 7);
                Piece checkIfPieceIsEmpty2 = boardObject.GetPiece(2, 7);
                Piece checkIfPieceIsEmpty3 = boardObject.GetPiece(3, 7);
                if (checkIfPieceIsEmpty1 == null && checkIfPieceIsEmpty2 == null && checkIfPieceIsEmpty3 == null) {
                    boardObject.SetRochadeLang("white");
                    printRochade = !printRochade;
                    lblZug.Text = "You rochaded long";
                    SchachGUI.kingHasRochaded = true;
                    SchachGUI.rochadeLong = true;
                    SchachGUI.posKingWhiteX = 2; SchachGUI.posKingWhiteY = 7;
                    _logger.Debug("WhiteKingX={0},WhiteKingY={1} / long rochaded", SchachGUI.posKingWhiteX, SchachGUI.posKingWhiteY);
                } else {
                    boardObject.SetPiece(4, 7, currentPiece);
                    SchachGUI.whiteTurn = true;
                    SchachGUI.blackTurn = false;
                    haveAlreadySwitched = true;
                }
            } else if (currentPiece.Color == PieceColor.Black && currentPiece.Type == PieceType.Koenig && bezAbisH == 'C' && bez1bis8 == 8 && !blackKingHasMoved) {
                //Rochade untersagen, sofern der Koenig im Schach steht
                if (SchachGUI.kingIsChecked) {
                    boardObject.SetPiece(4, 0, currentPiece);
                    message = "Rochade nicht erlaubt. Koenig steht im Schach";
                    this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                    return;
                }
                Piece checkIfPieceIsEmpty1 = boardObject.GetPiece(1, 0);
                Piece checkIfPieceIsEmpty2 = boardObject.GetPiece(2, 0);
                Piece checkIfPieceIsEmpty3 = boardObject.GetPiece(3, 0);
                if (checkIfPieceIsEmpty1 == null && checkIfPieceIsEmpty2 == null && checkIfPieceIsEmpty3 == null) {
                    boardObject.SetRochadeLang("black");
                    printRochade = !printRochade;
                    lblZug.Text = "You rochaded long";
                    SchachGUI.kingHasRochaded = true;
                    SchachGUI.rochadeLong = true;
                    SchachGUI.posKingBlackX = x; SchachGUI.posKingBlackY = y;
                    _logger.Debug("BlackKingX={0},BlackKingY={1} / long rochaded", SchachGUI.posKingBlackX, SchachGUI.posKingBlackY);
                } else {
                    boardObject.SetPiece(4, 0, currentPiece);
                    SchachGUI.whiteTurn = false;
                    SchachGUI.blackTurn = true;
                    haveAlreadySwitched = true;
                }
            }
            //Ist Zug legal?      
            if (!boardObject.IsMoveLegal(currentPiece, intExPosX, x, intExPosY, y, bauerHaveMoved)) {
                bool checkForCheck = boardObject.hasKingLeftCheck(x, y, currentPiece) && SchachGUI.kingIsChecked;
                if (checkForCheck)
                    message = "Koenig steht immer noch im Schach!";
                else
                    message = "Zug ist illegal";
                //wurde Figur losgelassen, ohne zu ziehen?
                if (x == intExPosX && y == intExPosY) {
                    //Lege Figur an alter Stelle ab
                    boardObject.SetPiece(intExPosX, intExPosY, currentPiece);
                    //unterbinde eine Zugrücknahme, sofern ungültiger Zug
                    forbidTakingBackMove = true;
                    return;
                }
                this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                boardObject.SetPiece(intExPosX, intExPosY, currentPiece);
                forbidTakingBackMove = true;
                return;
            }
            if (boardObject.hasKingLeftCheck(x, y, currentPiece) && SchachGUI.kingIsChecked) {
                message = "Koenig steht immer noch im Schach!";
                this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                if (Board.figureHasBeenPut) {
                    boardObject.SetPiece(intExPosX, intExPosY, currentPiece);
                    if (currentPiece.Color == PieceColor.White && currentPiece.Type == PieceType.Koenig) {
                        SchachGUI.posKingWhiteX = intExPosX;
                        SchachGUI.posKingWhiteY = intExPosY;
                        _logger.Debug("WhiteKingX={0},WhiteKingY={1} / king removed(still chessed)", SchachGUI.posKingWhiteX, SchachGUI.posKingWhiteY);
                    } else if (currentPiece.Color == PieceColor.Black && currentPiece.Type == PieceType.Koenig) {
                        SchachGUI.posKingBlackX = intExPosX;
                        SchachGUI.posKingBlackY = intExPosY;
                        _logger.Debug("BlackKingX={0},BlackKingY={1} / king removed(still chessed)", SchachGUI.posKingBlackX, SchachGUI.posKingBlackY);
                    }
                }
                forbidTakingBackMove = true;
                return;
            } else {
                if (!SchachGUI.kingHasRochaded) {
                    //wandle Bauer in Dame um, sofern er auf der letzten Reihe ankommt
                    string farbe = String.Empty;
                    if ((y == 0 || y == 7) && currentPiece.Type == PieceType.Bauer) {
                        if (currentPiece.Color == PieceColor.White)
                            farbe = "white";
                        else if (currentPiece.Color == PieceColor.Black)
                            farbe = "black";
                        boardObject.ChangeBauer2Dame(farbe, x, y);
                    } else
                        //Lege Figur an neuer Stelle ab
                        boardObject.SetPiece(x, y, currentPiece);
                }
            }
            //sichere die aktuellen Positionsdaten des Koenigs
            if (!SchachGUI.kingHasRochaded && currentPiece.Type == PieceType.Koenig && currentPiece.Color == PieceColor.White) {
                SchachGUI.posKingWhiteX = x;
                SchachGUI.posKingWhiteY = y;
                _logger.Debug("WhiteKingX={0},WhiteKingY={1} / white king moved", SchachGUI.posKingWhiteX, SchachGUI.posKingWhiteY);
            } else if (!SchachGUI.kingHasRochaded && currentPiece.Type == PieceType.Koenig && currentPiece.Color == PieceColor.Black) {
                SchachGUI.posKingBlackX = x;
                SchachGUI.posKingBlackY = y;
                _logger.Debug("BlackKingX={0},BlackKingY={1} / black king moved", SchachGUI.posKingBlackX, SchachGUI.posKingBlackY);
            }
            //Steht Koenig im Schach?
            string figureC = String.Empty;
            if (currentPiece.Color == PieceColor.White)
                figureC = "schwarze";
            else if (currentPiece.Color == PieceColor.Black)
                figureC = "weisse";
            if (boardObject.kingStandsChecked(currentPiece, x, y) && boardObject.IsKingMatt(currentPiece, x, y)) {
                this.ShowFiguresBeaten();
                message = "Glueckwunsch. Der " + figureC + " Koenig ist schachmatt!";
                lblZug.Text += string.Format(" - - You dropped a {0} {1} at {2}{3}", currentPiece.Color, currentPiece.Type, bezAbisH, bez1bis8);
                lblZug.Text += Environment.NewLine + message;
                string rootFolder = AppDomain.CurrentDomain.BaseDirectory + @"sound\";
                string soundFile = rootFolder + Resources.sound1;
                // Sounddatei threadsicher abspielen(GUI wird nicht blockiert)
                ThreadPool.QueueUserWorkItem(ignoredState => {
                    using (SoundPlayer player = new SoundPlayer(soundFile)) {
                        player.PlaySync();
                    }
                });
                this.Ausgabe(message, "Schach Matt !", MessageBoxIcon.Exclamation);
                kingIsMatt = true;
                if (cbMoves.Checked)
                    this.WriteToTextFile(false, lblZug.Text + Environment.NewLine);
                return;
            } else if (boardObject.kingStandsChecked(currentPiece, x, y)) {
                message = "Schach! Beseitigen Sie die Bedrohung.";
                this.Ausgabe(message, "Info", MessageBoxIcon.Asterisk);
                lblZug.ForeColor = Color.Red;
                lblZug.Text = "Der " + figureC + " Koenig steht im Schach!" + lblZug.Text;
                SchachGUI.kingIsChecked = true;
            } else
                SchachGUI.kingIsChecked = false;
            decimal zuegeGesamt = 0;
            if (intMoveCounter < 1)
                zuegeGesamt = intMoveCounter + 1;
            else
                zuegeGesamt = Math.Round(Convert.ToDecimal(intMoveCounter / 2), 0, MidpointRounding.AwayFromZero) + 1;
            string msgForList = String.Empty;
            if (bezAbisH.ToString() + bez1bis8.ToString() != strPickNDrop) {
                //sofern Zugzaehler restlos durch zwei teilbar...
                if (intMoveCounter % 2 == 0) {
                    if (!haveAlreadySwitched) {
                        msgForList = "(" + zuegeGesamt + ")" + PieceColor.Black + " 's turn!";
                        lstVMoves.Items.Add(msgForList);
                        SchachGUI.whiteTurn = false;
                        SchachGUI.blackTurn = true;
                    }
                } else {
                    if (!haveAlreadySwitched) {
                        msgForList = "(" + (zuegeGesamt + 1) + ")" + PieceColor.White + " 's turn!";
                        lstVMoves.Items.Add(msgForList);
                        SchachGUI.whiteTurn = true;
                        SchachGUI.blackTurn = false;
                    }
                }
                if (!haveAlreadySwitched)
                    intMoveCounter++;
            }
            //ermittle die vertikale Position des letzten ListView-Eintrages
            var r = Enumerable.Empty<ListViewItem>();
            if (lstVMoves.Items.Count > 0)
                r = lstVMoves.Items.OfType<ListViewItem>();
            ListViewItem last = r.LastOrDefault();
            ListViewItem lastItem = null;
            if (lstVMoves.Items.Count > 0) {
                lastItem = lstVMoves.Items[last.Index];
                /*  Sobald positionY den Wert 604(518) erreicht hat, ist die ListView voll; 
                    sofern die Checkbox akitivert ist und die ListView voll ist, wird sie geleert */
                if (cbClean.Checked && lastItem.Position.Y > 518) {
                    lstVMoves.Items.Clear();
                    lstVMoves.Items.Add(msgForList);
                }
            }
            if (!haveAlreadySwitched && !printRochade) {
                if (Board.figureHasBeenBeaten) {
                    string color = String.Empty;
                    if (currentPiece.Color == PieceColor.White)
                        color = "Black";
                    else if (currentPiece.Color == PieceColor.Black)
                        color = "White";
                    lblZug.Text += string.Format(" - - You beat with a {0} {1} a {2} {3} at {4}{5}", currentPiece.Color, currentPiece.Type, color, Board.figureBeaten, bezAbisH, bez1bis8);
                } else
                    lblZug.Text += string.Format(" - - You dropped a {0} {1} at {2}{3}", currentPiece.Color, currentPiece.Type, bezAbisH, bez1bis8);
            }
            if (cbMoves.Checked)
                this.WriteToTextFile(false, lblZug.Text + Environment.NewLine);
            if (haveAlreadySwitched)
                haveAlreadySwitched = false;
            //erstelle Notationsliste
            List<string> strLstBez = new List<string>() { "00", "10", "20", "30", "40", "50", "60", "70", "01", "11", "21", "31", "41", "51", "61", "71", "06", "16", "26", "36", "46", "56", "66", "76", "07", "17", "27", "37", "47", "57", "67", "77" };
            string actualCoordinates = intExPosX.ToString() + intExPosY.ToString();
            if (strLstBez.Contains(actualCoordinates) && currentPiece.Type == PieceType.Bauer)
                //verfrachte jeden erstgezogenen Bauern in eine generische Liste
                listOfBauerMoved.Add(new Tuple<Piece, string>(currentPiece, x.ToString() + y.ToString()));
            //sofern der Koenig gezogen hat, registriere das, um nicht mehr rochieren zu koennen
            if (currentPiece.Type == PieceType.Koenig && currentPiece.Color == PieceColor.White) {
                whiteKingHasMoved = true;
                _logger.Debug(" whiteKingHasMoved wurde auf {0} gesetzt", whiteKingHasMoved);
            } else if (currentPiece.Type == PieceType.Koenig && currentPiece.Color == PieceColor.Black) {
                blackKingHasMoved = true;
                _logger.Debug(" blackKingHasMoved wurde auf {0} gesetzt", blackKingHasMoved);
            }
            //registriere die gezogene Figuren und deren Positionsdaten
            if (currentPiece.Color == PieceColor.White) {
                Board.lastFigureMovedWhiteX = x;
                Board.lastFigureMovedWhiteY = y;
                Board.lastFigureMovedWhite = currentPiece;
                _logger.Debug("Last figure moved:{0}", Board.lastFigureMovedWhite.Type);
                _logger.Debug("Last figure moved: posX={0};posY={1}", Board.lastFigureMovedWhiteX, Board.lastFigureMovedWhiteY);
            } else if (currentPiece.Color == PieceColor.Black) {
                Board.lastFigureMovedBlackX = x;
                Board.lastFigureMovedBlackY = y;
                Board.lastFigureMovedBlack = currentPiece;
                _logger.Debug("Last figure moved:{0}", Board.lastFigureMovedBlack.Type);
                _logger.Debug("Last figure moved: posX={0};posY={1}", Board.lastFigureMovedBlackX, Board.lastFigureMovedBlackY);
            }
            lastFigureMoved = currentPiece;
            currentPiece = null;
            if (SchachGUI.rochadeShort)
                SchachGUI.moveForServer = "O-O (kurze Rochade)";
            else if (SchachGUI.rochadeLong)
                SchachGUI.moveForServer = "O-O-O (lange Rochade)";
            else
                SchachGUI.moveForServer = strPickNDrop + " - " + bezAbisH + bez1bis8;
            this.ShowFiguresBeaten();
            SchachGUI.haveTakeBackMove = false;
            forbidTakingBackMove = false;
            btnPDF.Visible = false;
        }
        #endregion

        #region Mausklick Event verarbeiten
        private void PictureBox1_MouseDown_1(object sender, MouseEventArgs e) {
            PickPiece(e);
            StelleBrettundFigurenDar();
        }
        #endregion

        #region Mausloslassen Event verarbeiten
        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) {
            DropPiece(e);
            StelleBrettundFigurenDar();
        }
        #endregion

        #region Verarbeitet das Schliessen des HauptFormulars
        private void SchachGUI_FormClosing(object sender, FormClosingEventArgs e) {
            //Das Loeschen aller Zuege veranlassen, sofern es nicht bereits in der Klasse Zugtransfer erwirkt wurde
            if (!Zugtransfer.haveDeleted) {
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
            }
        }
        #endregion

        #region Messagebox darstellen
        public void Ausgabe(string message, string art, MessageBoxIcon icon) {
            MessageBox.Show(message, art, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1);
        }

        #endregion

        #region Koordinaten zeichnen

        private void DrawCoordinates(bool onOff) {
            char[] notationVertical = new char[8] { '8', '7', '6', '5', '4', '3', '2', '1' };
            char[] notationHorizontal = new char[8] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
            Graphics formGraphics = CreateGraphics();
            Font drawFont = new Font("Arial", 14);
            var backgroundColor = SchachGUI.DefaultBackColor;
            SolidBrush drawBrush;
            if (onOff)
                drawBrush = new SolidBrush(Color.Brown);
            else
                drawBrush = new SolidBrush(backgroundColor);
            float xVertical = 175.0F;
            float yVertical = 105.0F;
            float xHorizontal = 220.0F;
            float yHorizontal = 600.0F;
            for (int i = 0; i < notationVertical.Length; i++) {
                formGraphics.DrawString(notationVertical[i].ToString(), drawFont, drawBrush, xVertical, yVertical + 65 * i); ;
                formGraphics.DrawString(notationHorizontal[i].ToString(), drawFont, drawBrush, xHorizontal + 65 * i, yHorizontal); ;
            }
            drawFont.Dispose();
            drawBrush.Dispose();
            formGraphics.Dispose();
        }
        #endregion

        #region Schreibt in eine Textdatei
        private void WriteToTextFile(bool justFolder, [Optional] string strText) {
            string username = Environment.UserName;
            string pathLog = @"C:\Users\" + username + @"\ChessGUI\";
            string pathPDF = @"C:\Users\" + username + @"\ChessGUI\PDF\";
            if (justFolder) {
                if (!Directory.Exists(pathLog))
                    Directory.CreateDirectory(pathLog);
                if (!Directory.Exists(pathPDF))
                    Directory.CreateDirectory(pathPDF);
            } else {
                string strPath = pathLog + "moves.log";
                using (StreamWriter myWriter = File.AppendText(strPath)) {
                    myWriter.Write(strText);
                    myWriter.Close();
                }
            }
        }
        #endregion

        #region verarbeitet einen Click auf die Checkbox 'Zuege in Textdatei schreiben'
        private void CbMoves_Click(object sender, EventArgs e) {
            string dat = DateTime.Now.ToString("dddd, dd.MM.yyyy HH:mm:ss");
            if (cbMoves.Checked)
                this.WriteToTextFile(false, dat + Environment.NewLine);
        }
        #endregion

        #region Spiel neu starten
        private void RestartGame() {
            foreach (Control control in base.Controls) {
                if (control is PictureBox)
                    ((PictureBox)control).Image = null;
            }
            this.LeereBrett();
            StelleBrettundFigurenDar(true);
            string strMessage = " You restarted game!";
            if (cbMoves.Checked)
                this.WriteToTextFile(false, strMessage + Environment.NewLine);
            lblZug.Text = "You restarted game!";
            lstVMoves.Clear();
            intMoveCounter = 0;
            SchachGUI.whiteTurn = true; SchachGUI.blackTurn = false; whiteKingHasMoved = false; blackKingHasMoved = false; haveAlreadySwitched = false; bauerHaveMoved = false; kingIsMatt = false;
            SchachGUI.kingHasRochaded = false; SchachGUI.kingIsChecked = false;
            SchachGUI.posKingWhiteX = 4; SchachGUI.posKingWhiteY = 7; SchachGUI.posKingBlackX = 4; SchachGUI.posKingBlackY = 0;
            _logger.Debug("WhiteKingX={0},WhiteKingY={1} / restart game", SchachGUI.posKingWhiteX, SchachGUI.posKingWhiteY);
            _logger.Debug("BlackKingX={0},BlackKingY={1} / restart game", SchachGUI.posKingBlackX, SchachGUI.posKingBlackY);
            SchachGUI.haveRestartedGame = true;
            if (Board.lstfiguresBeaten != null && Board.lstfiguresBeaten.Count > 0)
                Board.lstfiguresBeaten.Clear();
            if (SchachGUI.rochadeShort)
                SchachGUI.rochadeShort = !SchachGUI.rochadeShort;
            else if (SchachGUI.rochadeLong)
                SchachGUI.rochadeLong = !SchachGUI.rochadeLong;
            kingIsMatt = false;
            SchachGUI.moveForServer = String.Empty;
            currentPiece = null; lastFigureMoved = null;
        }
        #endregion

        #region Nimmt den letzten Zug zurueck
        private void TakeBackLastMove() {
            string message, spielfarbe, url, auswertung;
            chessServer objektServer = new chessServer();
            bool tookBack;
            if (lastFigureMoved == null) {
                message = "Diese Partie ist noch jungfraeulich. Taetigen Sie einen Zug, erst dann koennen Sie ihn zurueck nehmen!";
                this.Ausgabe(message, "Info", MessageBoxIcon.Warning);
                return;
            }
            if (kingIsMatt) {
                message = "Bereits beendete Partien erlauben keine Zugruecknahme mehr!";
                this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                return;
            }
            if (SchachGUI.haveTakeBackMove) {
                message = "Diese Applikation erlaubt nur jeweils einen Zug in Folge zurueck zu nehmen!";
                this.Ausgabe(message, "Info", MessageBoxIcon.Warning);
                return;
            }
            if (forbidTakingBackMove) {
                message = "Eine Zugrücknahme ist derzeit nicht statthaft!";
                this.Ausgabe(message, "Info", MessageBoxIcon.Warning);
                return;
            }
            Piece figure2TakeBack = lastFigureMoved;
            _logger.Debug("Der Uebergabeparameter figure2TakeBack:{0} wurde durch lastFigureMoved:{1} zugewiesen", figure2TakeBack.Type, lastFigureMoved.Type);
            int actualPosX, actualPosY;
            if (figure2TakeBack.Color == PieceColor.White) {
                actualPosX = Board.lastFigureMovedWhiteX;
                actualPosY = Board.lastFigureMovedWhiteY;
                spielfarbe = "weisse";
            } else {
                actualPosX = Board.lastFigureMovedBlackX;
                actualPosY = Board.lastFigureMovedBlackY;
                spielfarbe = "schwarze";
            }
            if (Board.figureHasBeenBeaten) {
                _logger.Debug("Da Board.figureHasBeenBeaten den Wert {0} hat, wird die geschlagene Figur wieder hergestellt", Board.figureHasBeenBeaten);
                int counter = 0;
                if (boardObject.TakeBackLastMove(actualPosX, actualPosY, intExPosX, intExPosY, figure2TakeBack, true)) {
                    message = "Der/Die " + spielfarbe + " " + figure2TakeBack.Type + " befindet sich wieder auf der alten Position.";
                    tookBack = true;
                    lblZug.Text = message;
                    if (cbMoves.Checked)
                        this.WriteToTextFile(false, message + Environment.NewLine);
                    this.Ausgabe(message, "Success", MessageBoxIcon.Information);
                    if (Board.lstfiguresBeaten.Count == 1)
                        Board.lstfiguresBeaten.RemoveAt(0);
                    else if (Board.lstfiguresBeaten.Count > 1) {
                        foreach (Piece item in Board.lstfiguresBeaten) {
                            if (item == Board.figureDeleted)
                                break;
                            else
                                counter++;
                        }
                        try {
                            Board.lstfiguresBeaten.RemoveAt(counter);
                            _logger.Debug("Aus der generischen Liste Board.lstfiguresBeaten wurde das {0}-te Element entfernt", counter);
                        }
                        catch (Exception er) {
                            _logger.Error(er.Message);
                            Board.lstfiguresBeaten.RemoveAt(counter - 1);
                            _logger.Debug("Exception ausgelöst(s.o.) Es wurde das {0}-te Element anstantt dem {1}-ten Element aus der generischen Liste Board.lstfiguresBeaten entfernt", counter - 1, counter);
                        }
                        this.ShowFiguresBeaten();
                        _logger.Debug("Removed figure {0} from Board.lstfiguresBeaten. Now ,this list contains {1} elements", Board.figureDeleted.Type, Board.lstfiguresBeaten.Count);
                    }
                } else {
                    message = "Error! Vorgang wurde geloggt. Die Spielfigur kann nicht zurueck genommen werden.";
                    this.Ausgabe(message, "Error", MessageBoxIcon.Error);
                    return;
                }
            } else {
                _logger.Debug("Da Board.figureHasBeenBeaten den Wert {0} hat, wird keine geschlagene Figur verarbeitet", Board.figureHasBeenBeaten);
                if (boardObject.TakeBackLastMove(actualPosX, actualPosY, intExPosX, intExPosY, figure2TakeBack, false)) {
                    message = "Der/Die " + spielfarbe + " " + figure2TakeBack.Type + " befindet sich wieder auf der alten Position.";
                    tookBack = true;
                    lblZug.Text = message;
                    if (cbMoves.Checked)
                        this.WriteToTextFile(false, message + Environment.NewLine);
                    this.Ausgabe(message, "Success", MessageBoxIcon.Information);
                } else {
                    message = "Error! Vorgang wurde geloggt. Die Spielfigur kann nicht zurueck genommen werden.";
                    this.Ausgabe(message, "Error", MessageBoxIcon.Error);
                    return;
                }
            }
            //zeichne das Brett neu
            this.StelleBrettundFigurenDar(false);
            // setze alle involvierten globale Variablen zurueck     
            SchachGUI.whiteTurn = !SchachGUI.whiteTurn; SchachGUI.blackTurn = !SchachGUI.blackTurn;
            intMoveCounter--;
            if (lastFigureIncluded != null && lastFigureIncluded.Image != null)
                lastFigureIncluded.Image = null;
            if (Board.figureHasBeenBeaten) {
                Board.figureHasBeenBeaten = !Board.figureHasBeenBeaten;
                _logger.Debug("figurehasBeenBeaten wurde auf {0} gesetzt", Board.figureHasBeenBeaten);
            }
            if (lastFigureMoved.Type == PieceType.Koenig && whiteKingHasMoved) {
                whiteKingHasMoved = !whiteKingHasMoved;
                _logger.Debug(" whiteKingHasMoved wurde auf {0} gesetzt", whiteKingHasMoved);
            } else if (lastFigureMoved.Type == PieceType.Koenig && blackKingHasMoved) {
                blackKingHasMoved = !blackKingHasMoved;
                _logger.Debug(" blackKingHasMoved wurde auf {0} gesetzt", blackKingHasMoved);
            }
            if (SchachGUI.kingIsChecked)
                SchachGUI.kingIsChecked = !SchachGUI.kingIsChecked;
            if (figure2TakeBack.Color == PieceColor.White && figure2TakeBack.Type == PieceType.Koenig) {
                SchachGUI.posKingWhiteX = intExPosX;
                SchachGUI.posKingWhiteY = intExPosY;
                _logger.Debug("Took back last Move:posKingWhiteX:{0}/posKingWhiteY:{1}", SchachGUI.posKingWhiteX, SchachGUI.posKingWhiteY);
            } else if (figure2TakeBack.Color == PieceColor.Black && figure2TakeBack.Type == PieceType.Koenig) {
                SchachGUI.posKingBlackX = intExPosX;
                SchachGUI.posKingBlackY = intExPosY;
                _logger.Debug("Took back last Move:posKingBlackX:{0}/posKingBlackY:{1}", SchachGUI.posKingBlackX, SchachGUI.posKingBlackY);
            }
            SchachGUI.haveTakeBackMove = true;
            if (tookBack && Zugtransfer.haveLoggedIn) {
                url = "http://tklustig.ddns.net/chessServer/index.php?takeback=true&&farbe=" + spielfarbe;
                auswertung = objektServer.FetchRequestFromPHP(url);
                if (auswertung.Contains("Failure")) {
                    message = "Die Zugruecknahme konnte vom Server nicht verarbeitet werden. Sie wird von Ihrem Gegner nicht zur Kenntnis genommen!";
                    this.Ausgabe(message, "Zugruecknahme", MessageBoxIcon.Hand);
                } else
                    _logger.Debug("Der Webserver meldet folgendes:" + Environment.NewLine + auswertung);
            }
        }
        #endregion

        #region Steuert die Zuguebermittlung an den Schachserver
        private void transmission() {
            if (!this.IsConnectionAvailable("tklustig.ddns.net")) {
                string message = "Entweder Sie oder der Schachserver sind momentan offline. Onlinepartien koennen derzeit folglich nicht gespielt werden!";
                this.Ausgabe(message, "Warnung", MessageBoxIcon.Warning);
                Mailformular objektMail = new Mailformular();
                objektMail.SendMail("Ping funkioniert nicht", "Der Schachserver konnte nicht angepingt werden. Vielleicht hilft die beigefuegte Logdatei weiter.");
                btnPDF.Visible = true;
                return;
            } else {
                Zugtransfer fm = new Zugtransfer();
                fm.Show();
            }
        }
        #endregion

        #region Prueft, ob der Schachserver erreichbar ist.Rekursiver Aufruf

        private bool IsConnectionAvailable(string url, [Optional] int timeout) {
            bool tryAgain = true;
            try {
                if (timeout > 0) {
                    tryAgain = false;
                    PingReply reply = new Ping().Send(url, timeout);
                    _logger.Debug("Der Ping auf den Server:{0} mit dem doppelten, jetzt Timeout von:{1} Milisekunden ergab {2}", url, timeout, reply.Status == IPStatus.Success);
                    return reply.Status == IPStatus.Success;
                } else {
                    PingReply reply = new Ping().Send(url, 1000);
                    if (reply.Status != IPStatus.Success) {
                        _logger.Debug("Der Ping auf den Schachserver:{0} scheint {1} zu sein. Versuche erneut, den Server mit doppeltem Timeout anzupingen", url, reply.Status == IPStatus.Success);
                        this.IsConnectionAvailable(url, 2000);
                    } else
                        return reply.Status == IPStatus.Success;
                }
            }
            catch (Exception er) {
                _logger.Error(er.Message + Environment.NewLine + er.ToString());
                _logger.Warn("Die Url:{0} konnte nicht angepingt werden. Versuche es mit dem doppelten TimeOut erneut");
                if (tryAgain)
                    IsConnectionAvailable(url, 2000);
                else
                    _logger.Debug("Da der Boolean auf {0} gesetzt wurde, wird kein erneuter Pingversuch gestartet. Der Kontakt ist gescheitert", tryAgain);
                return false;
            }
            return false;
        }
        #endregion

        #region Zeigt das PDF Dokument an

        private void BtnPDF_Click(object sender, EventArgs e) {
            try {
                string path = AppDomain.CurrentDomain.BaseDirectory + @"\Dokument\";
                Process.Start(path + "webServerTransfer.pdf");
            }
            catch (System.ComponentModel.Win32Exception er) {
                _logger.Error(er.Message + Environment.NewLine + er.ToString());
                this.Ausgabe(er.Message, "Error", MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
