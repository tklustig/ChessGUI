using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ChessGUI {

    public class Board {

        /*  Die globale, generische Tupelliste actualFigures enthaelt die aktuelle Figuren und Positionen
            Die globale, generische Liste figuresBeaten enthaelt alle geschlagenen Figuren
            Die lokale, generische Tupelliste GetXYofAllFigures enthaelt die X und Y Koordinaten samt Figuren aller Spielsteine auf dem Brett
        */

        #region globale Variablen
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Piece[] _pieces;
        private List<Piece> figuresBeaten = new List<Piece>();
        private List<Tuple<Piece, Byte>> actualFigures = new List<Tuple<Piece, byte>>();
        private List<Tuple<Piece, Byte>> actualFiguresCopy = new List<Tuple<Piece, byte>>();
        private List<int> positionX = new List<int>();
        private List<int> positionY = new List<int>();
        private bool compareTuples = false, CheckForMatt;
        #endregion

        #region Getter&Setter
        public static Piece lastFigureMovedWhite { get; set; }
        public static Piece lastFigureMovedBlack { get; set; }
        public static int lastFigureMovedWhiteX { get; set; }
        public static int lastFigureMovedWhiteY { get; set; }
        public static int lastFigureMovedBlackX { get; set; }
        public static int lastFigureMovedBlackY { get; set; }
        public static bool figureHasBeenPut { get; set; }
        public static bool figureHasBeenBeaten { get; set; }
        public static PieceType figureBeaten { get; set; }
        public static Piece figureDeleted { get; set; }
        public static List<Piece> lstfiguresBeaten { get; set; }
        #endregion

        #region Konstruktor
        public Board() {
            _pieces = new Piece[8 * 8];
            PopulatePieces();
        }
        #endregion

        #region Liefert die Spielfigur

        public Piece GetPiece(int x, int y) {
            //verhindere eine fehlerhafte Koordinatenuebergabe auf die Randbereiche
            //if (y == -1) y = 0; /*if (y == 8) y = 7;*/ if (x == -1) x = 0; if (x == 8) x = 7;
            int i = y * 8 + x;
            try {
                return _pieces[i];
            }
            catch (IndexOutOfRangeException er) {
                _logger.Error("Indizie:" + i + Environment.NewLine + er.Message + Environment.NewLine + er.ToString());
                throw new IndexOutOfRangeException("Error:Indize-Verletzung. Vorgang wurde protokolliert!");
            }
        }
        #endregion

        #region Setzt eine Spielfigur

        public void SetPiece(int x, int y, Piece piece) {
            //verhindere eine fehlerhafte Koordinatenuebergabe auf die Randbereiche
            //if (y == -1) y = 0; if (y == 8) y = 7; if (x == -1) x = 0; if (x == 8) x = 7;
            /*  mathematische lineare Gleichung zur Belegung aller Figuren auf dem Brett 
                (1)x=i-8y
                (2)y=[i-x]/8
            */
            int i = y * 8 + x;
            try {
                _pieces[i] = piece;
            }
            catch (IndexOutOfRangeException er) {
                _logger.Error("Indizie:" + i + Environment.NewLine + er.Message + Environment.NewLine + er.ToString());
                throw new IndexOutOfRangeException("Error:Indizie-Verletzung. Vorgang wurde protokolliert!");
            }
        }
        #endregion

        #region Initialisiere Spielfiguren

        private void PopulatePieces() {
            for (int i = 0; i < 8; i++) {
                this.SetPiece(i, 1, new Piece(PieceType.Bauer, PieceColor.Black));
                this.SetPiece(i, 6, new Piece(PieceType.Bauer, PieceColor.White));
            }
            this.SetPiece(1, 0, new Piece(PieceType.Springer, PieceColor.Black));
            this.SetPiece(6, 0, new Piece(PieceType.Springer, PieceColor.Black));
            this.SetPiece(1, 7, new Piece(PieceType.Springer, PieceColor.White));
            this.SetPiece(6, 7, new Piece(PieceType.Springer, PieceColor.White));
            this.SetPiece(2, 0, new Piece(PieceType.Laeufer, PieceColor.Black));
            this.SetPiece(5, 0, new Piece(PieceType.Laeufer, PieceColor.Black));
            this.SetPiece(2, 7, new Piece(PieceType.Laeufer, PieceColor.White));
            this.SetPiece(5, 7, new Piece(PieceType.Laeufer, PieceColor.White));
            this.SetPiece(0, 0, new Piece(PieceType.Turm, PieceColor.Black));
            this.SetPiece(7, 0, new Piece(PieceType.Turm, PieceColor.Black));
            this.SetPiece(0, 7, new Piece(PieceType.Turm, PieceColor.White));
            this.SetPiece(7, 7, new Piece(PieceType.Turm, PieceColor.White));
            this.SetPiece(3, 7, new Piece(PieceType.Dame, PieceColor.White));
            this.SetPiece(3, 0, new Piece(PieceType.Dame, PieceColor.Black));
            this.SetPiece(4, 7, new Piece(PieceType.Koenig, PieceColor.White));
            this.SetPiece(4, 0, new Piece(PieceType.Koenig, PieceColor.Black));
        }
        #endregion

        #region liefert die Koordinaten
        private int giveBackCoordinates(int x, int y) {
            int i = y * 8 + x;
            return i;
        }
        #endregion

        #region fuehre kurze Rochade aus
        public void SetRochadeKurz(string farbe) {
            if (farbe == "white") {
                Piece kingWhite = new Piece(PieceType.Koenig, PieceColor.White);
                Piece turmWhite = new Piece(PieceType.Turm, PieceColor.White);
                this.SetPiece(6, 7, kingWhite);
                this.SetPiece(5, 7, turmWhite);
                this.SetPiece(7, 7, null);
            } else if (farbe == "black") {
                Piece kingBlack = new Piece(PieceType.Koenig, PieceColor.Black);
                Piece turmBlack = new Piece(PieceType.Turm, PieceColor.Black);
                this.SetPiece(6, 0, kingBlack);
                this.SetPiece(5, 0, turmBlack);
                this.SetPiece(7, 0, null);
            }
        }
        #endregion

        #region fuehre lange Rochade aus
        public void SetRochadeLang(string farbe) {
            if (farbe == "white") {
                Piece kingWhite = new Piece(PieceType.Koenig, PieceColor.White);
                Piece turmWhite = new Piece(PieceType.Turm, PieceColor.White);
                this.SetPiece(2, 7, kingWhite);
                this.SetPiece(3, 7, turmWhite);
                this.SetPiece(0, 7, null);
            } else if (farbe == "black") {
                Piece kingBlack = new Piece(PieceType.Koenig, PieceColor.Black);
                Piece turmBlack = new Piece(PieceType.Turm, PieceColor.Black);
                this.SetPiece(2, 0, kingBlack);
                this.SetPiece(3, 0, turmBlack);
                this.SetPiece(0, 0, null);
            }
        }
        #endregion

        #region Nehme Zug zurueck
        public bool TakeBackLastMove(int x, int y, int exX, int exY, Piece figur, bool figureHasBeenBeaten) {
            _logger.Debug("Zugruecknahme gestartet....");
            _logger.Debug("Uebergebene Parameter: x={0}, y={1}, exX={2}, exY={3}, figur={4}, figureHasBeenBeaten={5}", x, y, exX, exY, figur.Type, figureHasBeenBeaten);
            try {
                if (SchachGUI.rochadeShort && figur.Color == PieceColor.White) {
                    Piece kingWhite = new Piece(PieceType.Koenig, PieceColor.White);
                    Piece turmWhite = new Piece(PieceType.Turm, PieceColor.White);
                    this.SetPiece(5, 7, null);
                    this.SetPiece(6, 7, null);
                    this.SetPiece(4, 7, kingWhite);
                    this.SetPiece(7, 7, turmWhite);
                    _logger.Debug("Kurze Rochade(weiss) wurde widerrufen.");
                    _logger.Debug("Folgende Figuren wurden veraendert:weisser Turm auf x=7/y=7, weisser Koenig auf x=4/y=7, NULL fuer x=5/y=7 und x=6/y=7");
                } else if (SchachGUI.rochadeLong && figur.Color == PieceColor.White) {
                    Piece kingWhite = new Piece(PieceType.Koenig, PieceColor.White);
                    Piece turmWhite = new Piece(PieceType.Turm, PieceColor.White);
                    this.SetPiece(2, 7, null);
                    this.SetPiece(3, 7, null);
                    this.SetPiece(4, 7, kingWhite);
                    this.SetPiece(0, 7, turmWhite);
                    _logger.Debug("Lange Rochade(weiss) wurde widerrufen.");
                    _logger.Debug("Folgende Figuren wurden veraendert:weisser Turm auf x=0/y=7, weisser Koenig auf x=4/y=7, NULL fuer x=2/y=7 und x=3/y=7");
                }
                if (SchachGUI.rochadeShort && figur.Color == PieceColor.Black) {
                    Piece kingBlack = new Piece(PieceType.Koenig, PieceColor.Black);
                    Piece turmBlack = new Piece(PieceType.Turm, PieceColor.Black);
                    this.SetPiece(5, 0, null);
                    this.SetPiece(6, 0, null);
                    this.SetPiece(4, 0, kingBlack);
                    this.SetPiece(7, 0, turmBlack);
                    _logger.Debug("Kurze Rochade(schwarz) wurde widerrufen.");
                    _logger.Debug("Folgende Figuren wurden veraendert:schwarzer Turm auf x=7/y=0, schwarzer Koenig auf x=4/y=0, NULL fuer x=5/y=0 und x=6/y=0");
                } else if (SchachGUI.rochadeLong && figur.Color == PieceColor.Black) {
                    Piece kingBlack = new Piece(PieceType.Koenig, PieceColor.Black);
                    Piece turmBlack = new Piece(PieceType.Turm, PieceColor.Black);
                    this.SetPiece(2, 0, null);
                    this.SetPiece(3, 0, null);
                    this.SetPiece(4, 0, kingBlack);
                    this.SetPiece(0, 0, turmBlack);
                    _logger.Debug("Lange Rochade(schwarz) wurde widerrufen.");
                    _logger.Debug("Folgende Figuren wurden veraendert:schwarzer Turm auf x=0/y=0, schwarzer Koenig auf x=4/y=0, NULL fuer x=2/y=0 und x=3/y=0");
                }
                if (!figureHasBeenBeaten) {
                    this.SetPiece(x, y, null);
                    this.SetPiece(exX, exY, figur);
                    _logger.Debug("Da figureHasBeenBeaten den Value {0} hat, wurde folgende Positionsaenderung durchgefuehrt:NULL fuer x={1}/y={2}", figureHasBeenBeaten, x, y);
                    _logger.Debug("Da figureHasBeenBeaten den Value {0} hat, wurde folgende Positionsaenderung durchgefuehrt:Figur:{1} wurde auf {2}/{3} gesetzt", figureHasBeenBeaten, figur.Type, exX, exY);
                } else {
                    this.SetPiece(x, y, Board.figureDeleted);
                    this.SetPiece(exX, exY, figur);
                    _logger.Debug("Da figureHasBeenBeaten den Value {0} hat, wurde folgende Positionsaenderung durchgefuehrt:Position {1}/{2} wurde mit {3} neu besetzt", figureHasBeenBeaten, x, y, Board.figureDeleted.Type);
                    _logger.Debug("Da figureHasBeenBeaten den Value {0} hat, wurde folgende Positionsaenderung durchgefuehrt:Figur {1} wurde auf {2}/{3} zurueck gesetzt", figureHasBeenBeaten, figur.Type, exX, exY);
                }
            }
            catch (Exception er) {
                _logger.Error(er.Message + Environment.NewLine + er.ToString());
                return false;
            }
            return true;
        }

        #endregion

        #region Pruefe Zug
        //Koordinate x entspricht A-H ; y entspricht 0-7, wobei kleine Ziffern-->Schwarz, grosse Ziffern -->Weiss
        public bool IsMoveLegal(Piece piece, int xAlt, int xNeu, int yAlt, int yNeu, bool bauerHasMoved) {
            //pruefe, ob Figur geschlagen wurde, damit es in der Zuganzeige der Klasse SchachGUI verwertet werden kann
            int xNeuKoenig = -1, yNeuKoenig = -1;
            if (piece.Color == PieceColor.White) {
                xNeuKoenig = SchachGUI.posKingBlackX;
                yNeuKoenig = SchachGUI.posKingBlackY;
            } else if (piece.Color == PieceColor.Black) {
                xNeuKoenig = SchachGUI.posKingWhiteX;
                yNeuKoenig = SchachGUI.posKingWhiteY;
            }
            if (!SchachGUI.kingHasRochaded && this.FigureCanDelete(piece, xAlt, xNeu, yAlt, yNeu)) {
                Board.figureHasBeenBeaten = true;
                _logger.Debug("figureHasBeenBeaten wurde auf {0} gesetzt", Board.figureHasBeenBeaten);
            }
            //befindet sich die Figur auf dem Brett?
            if (yNeu > 7 || xNeu > 7 || xNeu < 0 || yNeu < 0) return false;
            // ist das Feld bereits belegt?
            if (!FigureCanDelete(piece, xAlt, xNeu, yAlt, yNeu)) {
                if (this.GetPiece(xNeu, yNeu) != null)
                    return false;
            }
            if (piece.Color == PieceColor.White) {
                if (piece.Type == PieceType.Bauer) {
                    //hat der Bauer bereits gezogen..
                    if (bauerHasMoved)
                        //...dann darf er keine 2 Felder mehr vorgehen
                        if (yAlt - yNeu > 1) return false;
                    //sofern die Figur rueckwaerts zieht oder sie mehr als 2 Felder vor zieht
                    if (yAlt <= yNeu || yAlt - yNeu > 2) return false;
                    if (!this.FigureCanDelete(piece, xAlt, xNeu, yAlt, yNeu, true))
                        /*  sofern eine Figur nicht geschlagen werden kann, muss der Bauer auf derselben Horizontalen bleiben. Hier wird in FigureCanBeDeleted() 
                            ueberprueft, ob ein en passant moeglich ist */
                        if (xNeu != xAlt) return false;
                }
            } else if (piece.Color == PieceColor.Black) {
                if (piece.Type == PieceType.Bauer) {
                    if (bauerHasMoved)
                        if (yNeu - yAlt > 1) return false;
                    if (yAlt >= yNeu || yNeu - yAlt > 2) return false;
                    if (!FigureCanDelete(piece, xAlt, xNeu, yAlt, yNeu, true))
                        if (xNeu != xAlt) return false;
                }
            }
            if (piece.Type == PieceType.Dame) {
                /*  Eine Dame darf sowohl horizontal, als auch diaogonal ,als auch vertikal ziehen. Alle anderen  Zuege sind a priori verboten.
                    Das ergibt folglich eine Kombination aus Turm und Laeufer => aufwendig, da bzgl. der Ueberspringungspruefung sowohl die Diagonalitaet, als auch 
                    die Vertikaltitaet und Horizontalitaet beruecksichtig werden muessen. Dazu nehme man die Algorithmen aus Turm und Laeufer*/
                int dummyXAlt = xAlt, dummyXNeu = xNeu, dummyYAlt = yAlt, dummyYNeu = yNeu;
                /*  Ueberspringungspruefung
                    Pruefe, ob Damme eine Figur nach links vorne uebersprungen hat - Algorithmus I   */

                while (dummyXNeu < dummyXAlt && dummyYNeu < yAlt) {
                    dummyXNeu++;
                    dummyYNeu++;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Dame eine Figur nach links hinten uebersprungen hat - Algorithmus II  
                while (dummyXNeu < dummyXAlt && dummyYNeu > yAlt) {
                    dummyXNeu++;
                    dummyYNeu--;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Dame eine Figur nach rechts vorne uebersprungen hat - Algorithmus III  
                while (dummyXNeu > dummyXAlt && dummyYNeu < yAlt) {
                    dummyXNeu--;
                    dummyYNeu++;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Dame eine Figur nach rechts hinten uebersprungen hat - Algorithmus IV  
                while (dummyXNeu > dummyXAlt && dummyYNeu > yAlt) {
                    dummyXNeu--;
                    dummyYNeu--;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }

                //Pruefe, ob Dame eine Figur vertikal nach oben uebersprungen hat - Algorithmus V 
                while (dummyYNeu < dummyYAlt && dummyXAlt == dummyXNeu) {
                    dummyYNeu++;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Dame eine Figur vertikal nach unten uebersprungen hat - Algorithmus VI 
                while (dummyYNeu > dummyYAlt && dummyXAlt == dummyXNeu) {
                    dummyYNeu--;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Dame eine Figur horizontal nach rechts uebersprungen hat - Algorithmus VII 
                while (dummyYNeu == dummyYAlt && dummyXNeu > dummyXAlt) {
                    dummyXNeu--;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Dame eine Figur horizontal nach links uebersprungen hat - Algorithmus VIII
                while (dummyYNeu == dummyYAlt && dummyXNeu < dummyXAlt) {
                    dummyXNeu++;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //pruefe,ob Dame korrekt gezogen hat. Dazu nehme man die Algorithmen aus Laeufer und Turm
                bool diagonalMovement = Math.Abs(xAlt - xNeu) == Math.Abs(yAlt - yNeu);
                bool horizontalMovement = xAlt == xNeu && yAlt != yNeu;
                bool verticalMovement = yAlt == yNeu && xAlt != xNeu;
                if (!diagonalMovement && !horizontalMovement && !verticalMovement)
                    return false;
                /*if (Math.Abs(xAlt - xNeu) == Math.Abs(yAlt - yNeu)) //Dame hat sich diagonal bewegt
                    return true;
                if (xAlt == xNeu && yAlt != yNeu) return true; //Dame bewegt sich nur horizontal
                else if (yAlt == yNeu && xAlt != xNeu) return true; //Dame bewegt sich nur vertikal */

            } else if (piece.Type == PieceType.Koenig) {
                /*ToDo: Ein Koenig darf jeweils genau eine Einheit ziehen,sowohl horizontal, diaogonal, als auch vertikal. 
                Alle anderen  Zuege sind a priori verboten. Pruefe zunaechst auf Abzugsschach */
                if (this.IsDiscoveredCheck(xNeu, yNeu, xNeuKoenig, yNeuKoenig, piece, false))
                    return false;
                bool kingMove1 = Math.Abs(xNeu - xAlt) == 1 && yNeu == yAlt;
                bool kingMove2 = Math.Abs(yNeu - yAlt) == 1 && xNeu == xAlt;
                bool kingMove3 = Math.Abs(yNeu - yAlt) == 1 && (Math.Abs(xNeu - xAlt) == 1);
                bool kingmove4 = SchachGUI.kingHasRochaded && Math.Abs(xNeu - xAlt) == 2;
                if (!kingMove1 && !kingMove2 && !kingMove3 && !kingmove4) return false;
                /*
                if (Math.Abs(xNeu - xAlt) == 1 && yNeu == yAlt) return true;
                else if (Math.Abs(yNeu - yAlt) == 1 && xNeu == xAlt) return true;
                else if (Math.Abs(yNeu - yAlt) == 1 && (Math.Abs(xNeu - xAlt) == 1)) return true;
                else if (SchachGUI.kingHasRochaded && Math.Abs(xNeu - xAlt) == 2) return true;
                else return false;
                */

            } else if (piece.Type == PieceType.Laeufer) {
                int dummyXAlt = xAlt, dummyXNeu = xNeu, dummyYNeu = yNeu;
                if (Math.Abs(xAlt - xNeu) != Math.Abs(yAlt - yNeu)) //Laeufer hat sich nicht diagonal bewegt
                    return false;
                //Pruefe, ob Laeufer eine Figur nach links vorne uebersprungen hat - Algorithmus I       
                while (dummyXNeu < dummyXAlt && dummyYNeu < yAlt) {
                    dummyXNeu++;
                    dummyYNeu++;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Laeufer eine Figur nach links hinten uebersprungen hat - Algorithmus II  
                while (dummyXNeu < dummyXAlt && dummyYNeu > yAlt) {
                    dummyXNeu++;
                    dummyYNeu--;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Laeufer eine Figur nach rechts vorne uebersprungen hat - Algorithmus III  
                while (dummyXNeu > dummyXAlt && dummyYNeu < yAlt) {
                    dummyXNeu--;
                    dummyYNeu++;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Laeufer eine Figur nach rechts hinten uebersprungen hat - Algorithmus IV   
                while (dummyXNeu > dummyXAlt && dummyYNeu > yAlt) {
                    dummyXNeu--;
                    dummyYNeu--;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
            } else if (piece.Type == PieceType.Springer) {
                if (xAlt == xNeu || //Springer hat sich horizontal nicht bewegt
                    yAlt == yNeu || //Springer hat sich vertikal nicht bewegt
                    Math.Abs(xAlt - xNeu) > 2 || //Der Spinger hat sich horizontal mehr als 2 Einheiten bewegt  
                    Math.Abs(yAlt - yNeu) > 2 || //Der Spinger hat sich vertikal mehr als 2 Einheiten bewegt
                    (Math.Abs(xAlt - xNeu) == 2 && Math.Abs(yAlt - yNeu) == 2) || //Der Springer hat sich sowohl horizontal als auch vertikal genau 2 Einheiten bewegt
                    (Math.Abs(xAlt - xNeu) == 1 && Math.Abs(yAlt - yNeu) == 1)) //Der Springer hat sich sowohl horizontal als auch vertikal genau 1 Einheit bewegt
                    return false;

            } else if (piece.Type == PieceType.Turm) {
                int dummyXAlt = xAlt, dummyXNeu = xNeu, dummyYAlt = yAlt, dummyYNeu = yNeu;
                //Pruefe, ob Turm eine Figur vertikal nach oben uebersprungen hat - Algorithmus I 
                while (dummyYNeu < dummyYAlt && dummyXAlt == dummyXNeu) {
                    dummyYNeu++;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Turm eine Figur vertikal nach unten uebersprungen hat - Algorithmus II 
                while (dummyYNeu > dummyYAlt && dummyXAlt == dummyXNeu) {
                    dummyYNeu--;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Turm eine Figur horizontal nach rechts uebersprungen hat - Algorithmus III 
                while (dummyYNeu == dummyYAlt && dummyXNeu > dummyXAlt) {
                    dummyXNeu--;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                //Pruefe, ob Turm eine Figur horizontal nach links uebersprungen hat - Algorithmus IV 
                while (dummyYNeu == dummyYAlt && dummyXNeu < dummyXAlt) {
                    dummyXNeu++;
                    if (this.GetPiece(dummyXNeu, dummyYNeu) != null)
                        return false;
                }
                if (xAlt == xNeu && yAlt != yNeu) return true; //Turm bewegt sich nur horizontal
                else if (yAlt == yNeu && xAlt != xNeu) return true; //Turm bewegt sich nur vertikal
                else return false; //Turm bewegt sich diagonal
            }
            /*  Pruefe, ob durch das Abziehen einer Figur ein Schach entstand. 
                Falls ja=>ungueltiger Zug*/
            if (this.IsDiscoveredCheck(xNeu, yNeu, xNeuKoenig, yNeuKoenig, piece, true))
                return false;
            return true;
        }
        #endregion

        #region Pruefe, ob gezogene Figur schlagen kann
        private bool FigureCanDelete(Piece piece, int xAlt, int xNeu, int yAlt, int yNeu, [Optional] bool OnlyOnce) {
            Piece bauerBeatenByEnPassant = null;
            //Sonderfall::Sofern rochiert wurde, gebe TRUE zurueck, da in IsMoveLegal() nur auf FALSE geprueft wird
            if (SchachGUI.kingHasRochaded) return true;
            Piece p = this.GetPiece(xNeu, yNeu);
            //Koenig kann nie geschlagen werden
            if (p != null && p.Type == PieceType.Koenig) return false;

            if (piece.Type == PieceType.Bauer) {
                //wird versucht, den Bauer direkt davor zu schlagen?
                if (xAlt == xNeu) return false;
                if (p != null) {
                    //ist die gefundene Figur von derselben Farbe
                    if (p.Color == piece.Color) return false;
                    //wird versucht, eine nicht auf der naechsten Horizontalen vorhandene Figur zu schlagen?                   
                    if (Math.Abs(yNeu - yAlt) > 1 || Math.Abs(xAlt - xNeu) > 1) return false;
                    return true;
                } else {
                    if (OnlyOnce) {
                        if (this.CheckForEnPassant(piece, xAlt, xNeu, yAlt, yNeu)) {
                            /*  sofern ein Schlagen gemaess en passant moeglich war, verfrachte die Figur in die globale generische Liste und weise diese Liste der 
                                Property zu, damit die Klasse SchachGUI die Figur in der PictureBox anzeigt*/
                            if (piece.Color == PieceColor.White) {
                                bauerBeatenByEnPassant = this.GetPiece(xNeu, yNeu + 1);
                            } else if (piece.Color == PieceColor.Black)
                                bauerBeatenByEnPassant = this.GetPiece(xNeu, yNeu - 1);
                            figuresBeaten.Add(bauerBeatenByEnPassant);
                            Board.lstfiguresBeaten = figuresBeaten;
                            /*  sofern ein en passant moeglich ist, muss der gegnerische Bauer vom Brett entfernt werden. Damit das nur einmal vollzogen wird, muss 
                             auf OnlyOnce geprueft werden*/
                            if (piece.Color == PieceColor.White)
                                this.SetPiece(xNeu, yNeu + 1, null);
                            else if (piece.Color == PieceColor.Black)
                                this.SetPiece(xNeu, yNeu - 1, null);
                            return true;
                        }
                    }
                }

            } else if (piece.Type == PieceType.Koenig) {
                if (p != null) {
                    //ist die gefundene Figur von derselben Farbe
                    if (!SchachGUI.kingHasRochaded)
                        if (p.Color == piece.Color) return false;
                    //wird versucht, den gegnerischen Koenig zu schlagen?
                    PieceColor color = PieceColor.Dummy;
                    if (p.Color == PieceColor.White)
                        color = PieceColor.Black;
                    else if (p.Color == PieceColor.Black)
                        color = PieceColor.White;
                    if (p.Type == PieceType.Koenig && p.Color == color) return false;
                    return true;
                }

            } else if (piece.Type == PieceType.Dame || piece.Type == PieceType.Laeufer || piece.Type == PieceType.Springer || piece.Type == PieceType.Turm) {
                if (p != null) {
                    //ist die gefundene Figur von derselben Farbe
                    if (p.Color == piece.Color)
                        return false;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Pruefe, ob eine uebergebene Figur schlagen kann
        private bool FigureCanDeleteAttackFigure(Piece piece2Delete, int DeletesX, int DeletesY, int BeDeletedX, int BeDeletedY) {
            const int grenze = 7;
            int dummyDeletesX = DeletesX, dummyDeletesY = DeletesY;
            switch (piece2Delete.Type) {
                case PieceType.Bauer:
                    if (Math.Abs(BeDeletedX - DeletesX) == 1 && Math.Abs(BeDeletedY - DeletesY) == 1)
                        return true;
                    break;
                case PieceType.Laeufer:
                    do {
                        dummyDeletesX++;
                        dummyDeletesY++;
                        if (dummyDeletesX == BeDeletedX && dummyDeletesY == BeDeletedY)
                            return true;
                        if (dummyDeletesX > grenze || dummyDeletesY > grenze) break;
                        if (this.GetPiece(dummyDeletesX, dummyDeletesY) != null) break;
                    } while (dummyDeletesX < grenze);
                    dummyDeletesX = DeletesX; dummyDeletesY = DeletesY;
                    do {
                        dummyDeletesX--;
                        dummyDeletesY--;
                        if (dummyDeletesX == BeDeletedX && dummyDeletesY == BeDeletedY)
                            return true;
                        if (dummyDeletesX < 0 || dummyDeletesY < 0) break;
                        if (dummyDeletesX >= 0 && dummyDeletesY >= 0 && this.GetPiece(dummyDeletesX, dummyDeletesY) != null) break;
                    } while (dummyDeletesX < grenze);
                    dummyDeletesX = DeletesX; dummyDeletesY = DeletesY;
                    do {
                        dummyDeletesX++;
                        dummyDeletesY--;
                        if (dummyDeletesX == BeDeletedX && dummyDeletesY == BeDeletedY)
                            return true;
                        if (dummyDeletesX > grenze || dummyDeletesY < 0) break;
                        if (dummyDeletesX < 8 && dummyDeletesY >= 0 && this.GetPiece(dummyDeletesX, dummyDeletesY) != null) break;
                    } while (dummyDeletesX < grenze);
                    dummyDeletesX = DeletesX; dummyDeletesY = DeletesY;
                    do {
                        dummyDeletesX--;
                        dummyDeletesY++;
                        if (dummyDeletesX == BeDeletedX && dummyDeletesY == BeDeletedY)
                            return true;
                        if (dummyDeletesX < 0 || dummyDeletesY > grenze) break;
                        if (dummyDeletesX >= 0 && dummyDeletesY < 8 && this.GetPiece(dummyDeletesX, dummyDeletesY) != null) break;
                    } while (dummyDeletesX < grenze);
                    break;
                case PieceType.Springer:
                    if (DeletesX + 2 == BeDeletedX && DeletesY + 1 == BeDeletedY ||
                        DeletesX + 2 == BeDeletedX && DeletesY - 1 == BeDeletedY ||
                        DeletesX - 2 == BeDeletedX && DeletesY + 1 == BeDeletedY ||
                        DeletesX - 2 == BeDeletedX && DeletesY - 1 == BeDeletedY ||
                        DeletesX + 1 == BeDeletedX && DeletesY + 2 == BeDeletedY ||
                        DeletesX + 1 == BeDeletedX && DeletesY - 2 == BeDeletedY ||
                        DeletesX - 1 == BeDeletedX && DeletesY + 2 == BeDeletedY ||
                        DeletesX - 1 == BeDeletedX && DeletesY - 2 == BeDeletedY ||
                        DeletesX - 1 == BeDeletedX && DeletesY - 2 == BeDeletedY)
                        return true;
                    break;
                case PieceType.Turm:
                    dummyDeletesX = DeletesX; dummyDeletesY = DeletesY;
                    do {
                        dummyDeletesX++;
                        if (dummyDeletesX == BeDeletedX && dummyDeletesY == BeDeletedY)
                            return true;
                        if (dummyDeletesX > grenze) break;
                        if (this.GetPiece(dummyDeletesX, dummyDeletesY) != null) break;
                    } while (dummyDeletesX < grenze);
                    dummyDeletesX = DeletesX; dummyDeletesY = DeletesY;
                    do {
                        dummyDeletesX--;
                        if (dummyDeletesX == BeDeletedX && dummyDeletesY == BeDeletedY)
                            return true;
                        if (dummyDeletesX < 0) break;
                        if (this.GetPiece(dummyDeletesX, dummyDeletesY) != null) break;
                    } while (dummyDeletesX < grenze);
                    dummyDeletesX = DeletesX; dummyDeletesY = DeletesY;
                    do {
                        dummyDeletesY++;
                        if (dummyDeletesX == BeDeletedX && dummyDeletesY == BeDeletedY)
                            return true;
                        if (dummyDeletesY > grenze) break;
                        if (this.GetPiece(dummyDeletesX, dummyDeletesY) != null) break;
                    } while (dummyDeletesX < grenze);
                    dummyDeletesX = DeletesX; dummyDeletesY = DeletesY;
                    do {
                        dummyDeletesY--;
                        if (dummyDeletesX == BeDeletedX && dummyDeletesY == BeDeletedY)
                            return true;
                        if (dummyDeletesY < 0) break;
                        if (this.GetPiece(dummyDeletesX, dummyDeletesY) != null) break;
                    } while (dummyDeletesX < grenze);
                    break;
                case PieceType.Dame:
                    int dummyX = DeletesX, dummY = DeletesY;
                    do {
                        dummyX++;
                        dummY++;
                        if (dummyX == BeDeletedX && dummY == BeDeletedY)
                            return true;
                        if (dummyX > grenze || dummY > grenze) break;
                        if (this.GetPiece(dummyX, dummY) != null) break;
                    } while (dummyX < grenze);
                    dummyX = DeletesX; dummY = DeletesY;
                    do {
                        dummyX--;
                        dummY--;
                        if (dummyX == BeDeletedX && dummY == BeDeletedY)
                            return true;
                        if (dummyX < 0 || dummY < 0) break;
                        if (this.GetPiece(dummyX, dummY) != null) break;
                    } while (dummyX < grenze);
                    dummyX = DeletesX; dummY = DeletesY;
                    do {
                        dummyX++;
                        dummY--;
                        if (dummyX == BeDeletedX && dummY == BeDeletedY)
                            return true;
                        if (dummyX > grenze || dummY < 0) break;
                        if (this.GetPiece(dummyX, dummY) != null) break;
                    } while (dummyX < grenze);
                    dummyX = DeletesX; dummY = DeletesY;
                    do {
                        dummyX--;
                        dummY++;
                        if (dummyX == BeDeletedX && dummY == BeDeletedY)
                            return true;
                        if (dummyX < 0 || dummY > grenze) break;
                        if (this.GetPiece(dummyX, dummY) != null) break;
                    } while (dummyX < grenze);
                    dummyX = DeletesX; dummY = DeletesY;
                    do {
                        dummyX++;
                        if (dummyX == BeDeletedX && dummY == BeDeletedY)
                            return true;
                        if (dummyX > grenze) break;
                        if (this.GetPiece(dummyX, dummY) != null) break;
                    } while (dummyX < grenze);
                    dummyX = DeletesX; dummY = DeletesY;
                    do {
                        dummyX--;
                        if (dummyX == BeDeletedX && dummY == BeDeletedY)
                            return true;
                        if (dummyX < 0) break;
                        if (this.GetPiece(dummyX, dummY) != null) break;
                    } while (dummyX < grenze);
                    dummyX = DeletesX; dummY = DeletesY;
                    do {
                        dummY++;
                        if (dummyX == BeDeletedX && dummY == BeDeletedY)
                            return true;
                        if (dummY > grenze) break;
                        if (this.GetPiece(dummyX, dummY) != null) break;
                    } while (dummyX < grenze);
                    dummyX = DeletesX; dummY = DeletesY;
                    do {
                        dummY--;
                        if (dummyX == BeDeletedX && dummY == BeDeletedY)
                            return true;
                        if (dummY < 0) break;
                        if (this.GetPiece(dummyX, dummY) != null) break;
                    } while (dummyX < grenze);
                    break;
            }
            return false;
        }
        #endregion

        #region Pruefe, ob Figur Schach gibt
        /*  Folgende Figuren haben den Funktionstest bestanden:
                                Weiss:
            Turm:       Ja
            Springer:   Ja
            Laeufer:     Ja
            Dame:       Ja
            Bauer:      Ja
                                Schwarz:
            Turm:       Ja
            Springer:   Ja
            Laeufer:     Ja
            Dame:       Ja
            Bauer:      Ja
         */
        public bool kingStandsChecked(Piece figureMoved, int posFigureX, int posFigureY, int posKingX = -1, int posKingY = -1) {
            int xKingWhite = -1, yKingWhite = -1, xKingBlack = -1, yKingBlack = -1;
            this.EruateFiguresOnBoard(figureMoved, posFigureX, posFigureY);
            bool Checked = false;
            //die Positionsdaten des Koenigs sind in den statischen Propertys der Klasse SchachGUI enthalten
            if (posKingX == -1 && posKingY == -1) {
                xKingWhite = SchachGUI.posKingWhiteX; yKingWhite = SchachGUI.posKingWhiteY;
                xKingBlack = SchachGUI.posKingBlackX; yKingBlack = SchachGUI.posKingBlackY;
            } else if (posKingX != -1 && posKingY != -1) {
                xKingWhite = posKingX; yKingWhite = posKingY;
                xKingBlack = posKingX; yKingBlack = posKingY;
            }
            //(1)Pruefe, ob weisser Laeufer Schach gibt
            if (figureMoved.Color == PieceColor.White) {
                if (figureMoved.Type == PieceType.Laeufer) {
                    //Pruefe, ob weisser Laeufer dem schwarzen Koenig nach links vorne Schach gibt. Fall(1)      
                    while (xKingBlack < posFigureX && yKingBlack < posFigureY) {
                        xKingBlack++;
                        yKingBlack++;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Laeufer) stoesst, besteht Schach
                        if (this.GetPiece(xKingBlack, yKingBlack) != null && this.GetPiece(xKingBlack, yKingBlack) == figureMoved)
                            Checked = true;
                        else if (this.GetPiece(xKingBlack, yKingBlack) != null && this.GetPiece(xKingBlack, yKingBlack) != figureMoved)
                            break;
                    }
                    //Pruefe, ob weisser Laeufer dem schwarzen Koenig nach rechts vorne Schach gibt. Fall(2)      
                    while (xKingBlack > posFigureX && yKingBlack < posFigureY) {
                        xKingBlack--;
                        yKingBlack++;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Laeufer) stoesst, besteht Schach
                        if (this.GetPiece(xKingBlack, yKingBlack) != null && this.GetPiece(xKingBlack, yKingBlack) == figureMoved)
                            Checked = true;
                        else if (this.GetPiece(xKingBlack, yKingBlack) != null && this.GetPiece(xKingBlack, yKingBlack) != figureMoved)
                            break;
                    }
                    //ToDo:Pruefe, ob weisser Laeufer dem schwarzen Koenig nach links hinten Schach gibt. Fall(3) 
                    while (xKingBlack < posFigureX && yKingBlack > posFigureY) {
                        xKingBlack++;
                        yKingBlack--;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Laeufer) stoesst, besteht Schach
                        if (this.GetPiece(xKingBlack, yKingBlack) != null && this.GetPiece(xKingBlack, yKingBlack) == figureMoved)
                            Checked = true;
                        else if (this.GetPiece(xKingBlack, yKingBlack) != null && this.GetPiece(xKingBlack, yKingBlack) != figureMoved)
                            break;
                    }

                    //ToDo:Pruefe, ob weisser Laeufer dem schwarzen Koenig nach rechts hinten Schach gibt. Fall(4) 
                    while (xKingBlack > posFigureX && yKingBlack > posFigureY) {
                        xKingBlack--;
                        yKingBlack--;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Laeufer) stoesst, besteht Schach
                        if (this.GetPiece(xKingBlack, yKingBlack) != null && this.GetPiece(xKingBlack, yKingBlack) == figureMoved)
                            Checked = true;
                        else if (this.GetPiece(xKingBlack, yKingBlack) != null && this.GetPiece(xKingBlack, yKingBlack) != figureMoved)
                            break;
                    }

                    //(2)Pruefe, ob weisser Bauer Schach gibt
                } else if (figureMoved.Type == PieceType.Bauer) {
                    if ((Math.Abs(posFigureX - 1) == xKingBlack || Math.Abs(posFigureX + 1) == xKingBlack) && Math.Abs(posFigureY - 1) == yKingBlack)
                        Checked = true;
                    //(3)Pruefe, ob weisser Springer Schach gibt
                } else if (figureMoved.Type == PieceType.Springer) {
                    if (posFigureX - 2 == xKingBlack && posFigureY - 1 == yKingBlack
                        || posFigureX + 2 == xKingBlack && posFigureY - 1 == yKingBlack //2 nach rechts, 1 nach vorne
                        || posFigureX + 2 == xKingBlack && posFigureY + 1 == yKingBlack //2 nach rechts, 1 nach hinten
                        || posFigureX - 2 == xKingBlack && posFigureY - 1 == yKingBlack //2 nach links, 1 nach vorne 
                        || posFigureX - 2 == xKingBlack && posFigureY + 1 == yKingBlack //2 nach links, 1 nach hinten 
                        || posFigureX + 1 == xKingBlack && posFigureY - 2 == yKingBlack //1 nach rechts, 2 nach vorne
                        || posFigureX + 1 == xKingBlack && posFigureY + 2 == yKingBlack //1 nach rechts, 2 nach hinten
                        || posFigureX - 1 == xKingBlack && posFigureY - 2 == yKingBlack //1 nach links, 2 nach vorne
                        || posFigureX - 1 == xKingBlack && posFigureY + 2 == yKingBlack //1 nach links, 2 nach hinten
                        )
                        Checked = true;
                    //(4)Pruefe, ob weisser Turm Schach gibt
                } else if (figureMoved.Type == PieceType.Turm) {
                    //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus I 
                    while (posFigureY < yKingBlack && posFigureX == xKingBlack) {
                        posFigureY++;
                        if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                            Checked = false;
                            break;
                        } else
                            Checked = true;
                    }
                    //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus II
                    while (posFigureY > yKingBlack && posFigureX == xKingBlack) {
                        posFigureY--;
                        if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                            Checked = false;
                            break;
                        } else
                            Checked = true;
                    }
                    //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus III
                    while (posFigureY == yKingBlack && xKingBlack > posFigureX) {
                        posFigureX++;
                        if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                            Checked = false;
                            break;
                        } else
                            Checked = true;
                    }
                    //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus IV
                    while (posFigureY == yKingBlack && xKingBlack < posFigureX) {
                        posFigureX--;
                        if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                            Checked = false;
                            break;
                        } else
                            Checked = true;
                    }
                    //(5)Pruefe, ob weisse Dame Schach gibt
                } else if (figureMoved.Type == PieceType.Dame) {
                    bool checkHasGivenDiagonal = false;
                    //da sowohl diagonal als auch vertikal/horizontal gepuerft wird, muessen hier zusaetzliche Dummys initialisiert werden
                    int dummyXKingBlack = xKingBlack, dummyYKingBlack = yKingBlack;
                    /*besteht aus der Kombination der Konditionen aus Turm und Laeufer
                     (1)Laeufer */
                    //Pruefe, ob diagonal links vorne Schach gegeben wird. Fall(1)      
                    while (dummyXKingBlack < posFigureX && dummyYKingBlack < posFigureY) {
                        dummyXKingBlack++;
                        dummyYKingBlack++;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Dame) stoesst, besteht Schach
                        if ((this.GetPiece(dummyXKingBlack, dummyYKingBlack) != null && this.GetPiece(dummyXKingBlack, dummyYKingBlack) != figureMoved))
                            break;
                        else if (this.GetPiece(dummyXKingBlack, dummyYKingBlack) != null && this.GetPiece(dummyXKingBlack, dummyYKingBlack) == figureMoved)
                            Checked = true;
                    }
                    //Pruefe, ob diagonal rechts vorne Schach gegeben wird. Fall(2)    
                    while (dummyXKingBlack > posFigureX && dummyYKingBlack < posFigureY) {
                        dummyXKingBlack--;
                        dummyYKingBlack++;
                        if (this.GetPiece(dummyXKingBlack, dummyYKingBlack) != null && this.GetPiece(dummyXKingBlack, dummyYKingBlack) != figureMoved)
                            break;
                        if (this.GetPiece(dummyXKingBlack, dummyYKingBlack) != null && this.GetPiece(dummyXKingBlack, dummyYKingBlack) == figureMoved)
                            Checked = true;
                    }
                    //Pruefe, ob diagonal links hinten Schach gegeben wird. Fall(3)    
                    while (dummyXKingBlack < posFigureX && dummyYKingBlack > posFigureY) {
                        dummyXKingBlack++;
                        dummyYKingBlack--;
                        if (this.GetPiece(dummyXKingBlack, dummyYKingBlack) != null && this.GetPiece(dummyXKingBlack, dummyYKingBlack) != figureMoved)
                            break;
                        else if (this.GetPiece(dummyXKingBlack, dummyYKingBlack) != null && this.GetPiece(dummyXKingBlack, dummyYKingBlack) == figureMoved)
                            Checked = true;
                    }
                    //Pruefe, ob diagonal rechts hinten Schach gegeben wird. Fall(4)    
                    while (dummyXKingBlack > posFigureX && dummyYKingBlack > posFigureY) {
                        dummyXKingBlack--;
                        dummyYKingBlack--;
                        if (this.GetPiece(dummyXKingBlack, dummyYKingBlack) != null && this.GetPiece(dummyXKingBlack, dummyYKingBlack) != figureMoved)
                            break;
                        else if (this.GetPiece(dummyXKingBlack, dummyYKingBlack) != null && this.GetPiece(dummyXKingBlack, dummyYKingBlack) == figureMoved)
                            Checked = true;
                    }
                    if (Checked) checkHasGivenDiagonal = true;

                    /*  (2) Turm
                        Pruefe, ob zwischen Dame und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus I */
                    if (!checkHasGivenDiagonal) {
                        while (posFigureY < yKingBlack && posFigureX == xKingBlack) {
                            posFigureY++;
                            if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                                Checked = false;
                                break;
                            } else
                                Checked = true;
                        }
                        // Pruefe, ob zwischen Dame und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus II
                        while (posFigureY > yKingBlack && posFigureX == xKingBlack) {
                            posFigureY--;
                            if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                                Checked = false;
                                break;
                            } else
                                Checked = true;
                        }
                        // Pruefe, ob zwischen Dame und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus III
                        while (posFigureY == yKingBlack && xKingBlack > posFigureX) {
                            posFigureX++;
                            if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                                Checked = false;
                                break;
                            } else
                                Checked = true;
                        }
                        // Pruefe, ob zwischen Dame und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus IV
                        while (posFigureY == yKingBlack && xKingBlack < posFigureX) {
                            posFigureX--;
                            if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                                Checked = false;
                                break;
                            } else
                                Checked = true;
                        }
                    }
                }
            } else if (figureMoved.Color == PieceColor.Black) {
                //(1)Pruefe, ob schwarzer Laeufer Schach gibt
                if (figureMoved.Type == PieceType.Laeufer) {
                    //Pruefe, ob schwarzer Laeufer dem weissen Koenig nach links vorne Schach gibt. Fall(1)      
                    while (xKingWhite < posFigureX && yKingWhite < posFigureY) {
                        xKingWhite++;
                        yKingWhite++;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Laeufer) stoesst, besteht Schach
                        if (this.GetPiece(xKingWhite, yKingWhite) != null && this.GetPiece(xKingWhite, yKingWhite) == figureMoved)
                            Checked = true;
                        else if (this.GetPiece(xKingWhite, yKingWhite) != null && this.GetPiece(xKingWhite, yKingWhite) != figureMoved)
                            break;
                    }
                    //Pruefe, ob schwarzer Laeufer dem weissen Koenig nach rechts vorne Schach gibt. Fall(2)      
                    while (xKingWhite > posFigureX && yKingWhite < posFigureY) {
                        xKingWhite--;
                        yKingWhite++;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Laeufer) stoesst, besteht Schach
                        if (this.GetPiece(xKingWhite, yKingWhite) != null && this.GetPiece(xKingWhite, yKingWhite) == figureMoved)
                            Checked = true;
                        else if (this.GetPiece(xKingWhite, yKingWhite) != null && this.GetPiece(xKingWhite, yKingWhite) != figureMoved)
                            break;
                    }
                    //ToDo:Pruefe, ob schwarzer Laeufer dem weissen Koenig nach links hinten Schach gibt. Fall(3) 
                    while (xKingWhite < posFigureX && yKingWhite > posFigureY) {
                        xKingWhite++;
                        yKingWhite--;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Laeufer) stoesst, besteht Schach
                        if (this.GetPiece(xKingWhite, yKingWhite) != null && this.GetPiece(xKingWhite, yKingWhite) == figureMoved)
                            Checked = true;
                        else if (this.GetPiece(xKingWhite, yKingWhite) != null && this.GetPiece(xKingWhite, yKingWhite) != figureMoved)
                            break;
                    }

                    //ToDo:Pruefe, ob schwarzer Laeufer dem weissen Koenig nach rechts hinten Schach gibt. Fall(4) 
                    while (xKingWhite > posFigureX && yKingWhite > posFigureY) {
                        xKingWhite--;
                        yKingWhite--;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Laeufer) stoesst, besteht Schach
                        if (this.GetPiece(xKingWhite, yKingWhite) != null && this.GetPiece(xKingWhite, yKingWhite) == figureMoved)
                            Checked = true;
                        else if (this.GetPiece(xKingWhite, yKingWhite) != null && this.GetPiece(xKingWhite, yKingWhite) != figureMoved)
                            break;
                    }
                    //(2)Pruefe, ob schwarzer Bauer Schach gibt
                } else if (figureMoved.Type == PieceType.Bauer) {
                    if ((Math.Abs(posFigureX - 1) == xKingWhite || Math.Abs(posFigureX + 1) == xKingWhite) && Math.Abs(posFigureY + 1) == yKingWhite)
                        Checked = true;
                    //(3)Pruefe, ob schwarzer Springer Schach gibt
                } else if (figureMoved.Type == PieceType.Springer) {
                    if (posFigureX - 2 == xKingWhite && posFigureY - 1 == yKingWhite
                        || posFigureX + 2 == xKingWhite && posFigureY - 1 == yKingWhite //2 nach rechts, 1 nach vorne
                        || posFigureX + 2 == xKingWhite && posFigureY + 1 == yKingWhite //2 nach rechts, 1 nach hinten
                        || posFigureX - 2 == xKingWhite && posFigureY - 1 == yKingWhite //2 nach links, 1 nach vorne 
                        || posFigureX - 2 == xKingWhite && posFigureY + 1 == yKingWhite //2 nach links, 1 nach hinten 
                        || posFigureX + 1 == xKingWhite && posFigureY - 2 == yKingWhite //1 nach rechts, 2 nach vorne
                        || posFigureX + 1 == xKingWhite && posFigureY + 2 == yKingWhite //1 nach rechts, 2 nach hinten
                        || posFigureX - 1 == xKingWhite && posFigureY - 2 == yKingWhite //1 nach links, 2 nach vorne
                        || posFigureX - 1 == xKingWhite && posFigureY + 2 == yKingWhite //1 nach links, 2 nach hinten
                        )
                        Checked = true;
                    //(4)Pruefe, ob weisser Turm Schach gibt
                } else if (figureMoved.Type == PieceType.Turm) {
                    //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus I 
                    while (posFigureY < yKingWhite && posFigureX == xKingWhite) {
                        posFigureY++;
                        if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                            Checked = false;
                            break;
                        } else
                            Checked = true;
                    }
                    //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus II
                    while (posFigureY > yKingWhite && posFigureX == xKingWhite) {
                        posFigureY--;
                        if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                            Checked = false;
                            break;
                        } else
                            Checked = true;
                    }
                    //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus III
                    while (posFigureY == yKingWhite && xKingWhite > posFigureX) {
                        posFigureX++;
                        if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                            Checked = false;
                            break;
                        } else
                            Checked = true;
                    }
                    //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus IV
                    while (posFigureY == yKingWhite && xKingWhite < posFigureX) {
                        posFigureY--;
                        if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                            Checked = false;
                            break;
                        } else
                            Checked = true;
                    }
                    //(5)Pruefe, ob weisse Dame Schach gibt
                } else if (figureMoved.Type == PieceType.Dame) {
                    bool checkHasGivenDiagonal = false;
                    //da sowohl diagonal als auch vertikal/horizontal gepuerft wird, muessen hier zusaetzliche Dummys initialisiert werden
                    int dummyXKingWhite = xKingWhite, dummyYKingWhite = yKingWhite;
                    /*besteht aus der Kombination der Konditionen aus Turm und Laeufer
                     (1)Laeufer */
                    //Pruefe, ob diagonal links vorne Schach gegeben wird. Fall(1)      
                    while (dummyXKingWhite < posFigureX && dummyYKingWhite < posFigureY) {
                        dummyXKingWhite++;
                        dummyYKingWhite++;
                        //sofern die Schleife durchlaeuft und auf die schachgebende Figur(figureMoved=Dame) stoesst, besteht Schach
                        if (this.GetPiece(dummyXKingWhite, dummyYKingWhite) != null && this.GetPiece(dummyXKingWhite, dummyYKingWhite) != figureMoved)
                            break;
                        else if (this.GetPiece(dummyXKingWhite, dummyYKingWhite) != null && this.GetPiece(dummyXKingWhite, dummyYKingWhite) == figureMoved)
                            Checked = true;
                    }
                    //Pruefe, ob diagonal rechts vorne Schach gegeben wird. Fall(2)    
                    while (dummyXKingWhite > posFigureX && dummyYKingWhite < posFigureY) {
                        dummyXKingWhite--;
                        dummyYKingWhite++;
                        if ((this.GetPiece(dummyXKingWhite, dummyYKingWhite) != null && this.GetPiece(dummyXKingWhite, dummyYKingWhite) != figureMoved))
                            break;
                        if (this.GetPiece(dummyXKingWhite, dummyYKingWhite) != null && this.GetPiece(dummyXKingWhite, dummyYKingWhite) == figureMoved)
                            Checked = true;
                    }
                    //Pruefe, ob diagonal links hinten Schach gegeben wird. Fall(3)    
                    while (dummyXKingWhite < posFigureX && dummyYKingWhite > posFigureY) {
                        dummyXKingWhite++;
                        dummyYKingWhite--;
                        if (this.GetPiece(dummyXKingWhite, dummyYKingWhite) != null && this.GetPiece(dummyXKingWhite, dummyYKingWhite) != figureMoved)
                            break;
                        else if (this.GetPiece(dummyXKingWhite, dummyYKingWhite) != null && this.GetPiece(dummyXKingWhite, dummyYKingWhite) == figureMoved)
                            Checked = true;
                    }
                    //Pruefe, ob diagonal rechts hinten Schach gegeben wird. Fall(4)    
                    while (dummyXKingWhite > posFigureX && dummyYKingWhite > posFigureY) {
                        dummyXKingWhite--;
                        dummyYKingWhite--;
                        if (this.GetPiece(dummyXKingWhite, dummyYKingWhite) != null && this.GetPiece(dummyXKingWhite, dummyYKingWhite) != figureMoved)
                            break;
                        else if (this.GetPiece(dummyXKingWhite, dummyYKingWhite) != null && this.GetPiece(dummyXKingWhite, dummyYKingWhite) == figureMoved)
                            Checked = true;
                    }
                    if (Checked) checkHasGivenDiagonal = true;

                    /*  (2) Turm
                        Pruefe, ob zwischen Dame und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus I */
                    if (!checkHasGivenDiagonal) {
                        while (posFigureY < yKingWhite && posFigureX == xKingWhite) {
                            posFigureY++;
                            if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                                Checked = false;
                                break;
                            } else
                                Checked = true;
                        }
                        // Pruefe, ob zwischen Dame und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus II
                        while (posFigureY > yKingWhite && posFigureX == xKingWhite) {
                            posFigureY--;
                            if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                                Checked = false;
                                break;
                            } else
                                Checked = true;
                        }
                        // Pruefe, ob zwischen Dame und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus III
                        while (posFigureY == yKingWhite && xKingWhite > posFigureX) {
                            posFigureX++;
                            if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                                Checked = false;
                                break;
                            } else
                                Checked = true;
                        }
                        // Pruefe, ob zwischen Dame und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus IV
                        while (posFigureY == yKingWhite && xKingWhite < posFigureX) {
                            posFigureX--;
                            if (this.GetPiece(posFigureX, posFigureY) != null && this.GetPiece(posFigureX, posFigureY) != figureMoved && this.GetPiece(posFigureX, posFigureY).Type != PieceType.Koenig) {
                                Checked = false;
                                break;
                            } else
                                Checked = true;
                        }
                    }
                }
            }
            //weise den Inhalt des lokalen Boolean Checked dem globalen Boolean CheckForMatt zu, damit die Methode IsKingMatt() prueft, ob matt ist
            CheckForMatt = Checked;
            return Checked;
        }
        #endregion

        #region Pruefe, ob Koenig aus dem Schach gegangen ist. Benoetigt IsDiscoveredCheck
        public bool hasKingLeftCheck(int xNeu, int yNeu, Piece piece) {
            int xNeuKoenig = -1, yNeuKoenig = -1;
            if (piece.Type == PieceType.Koenig && piece.Color == PieceColor.White) {
                SchachGUI.posKingWhiteX = xNeu;
                SchachGUI.posKingWhiteY = yNeu;
                _logger.Debug("WhiteKingX={0},WhiteKingY={1} / king moved after chess", SchachGUI.posKingWhiteX, SchachGUI.posKingWhiteY);
            } else if (piece.Type == PieceType.Koenig && piece.Color == PieceColor.Black) {
                SchachGUI.posKingBlackX = xNeu;
                SchachGUI.posKingBlackY = yNeu;
                _logger.Debug("BlackKingX={0},BlackKingY={1} / king moved after chess", SchachGUI.posKingBlackX, SchachGUI.posKingBlackY);
            }
            if (piece.Color == PieceColor.White) {
                xNeuKoenig = SchachGUI.posKingWhiteX;
                yNeuKoenig = SchachGUI.posKingWhiteY;
            } else if (piece.Color == PieceColor.Black) {
                xNeuKoenig = SchachGUI.posKingBlackX;
                yNeuKoenig = SchachGUI.posKingBlackY;
            }
            //ToDo:Es muss in allen Varianten ueberprueft werden, ob der Koenig von einem Schach ins andere gezogen hat.
            bool stillChecked = true, figureHasAlreadyBeenSet = false;
            int dummyKingBlackX = SchachGUI.posKingBlackX, dummyKingBlackY = SchachGUI.posKingBlackY, dummyKingWhiteX = SchachGUI.posKingWhiteX, dummyKingWhiteY = SchachGUI.posKingWhiteY;
            int dummyLastFigureMovedWhiteX = Board.lastFigureMovedWhiteX, dummyLastFigureMovedWhiteY = Board.lastFigureMovedWhiteY, dummyLastFigureMovedBlackX = Board.lastFigureMovedBlackX, dummyLastFigureMovedBlackY = Board.lastFigureMovedBlackY;
            Piece figure, figureHasBeenRepressed = null;
            if (SchachGUI.kingIsChecked) {
                //sofern auf eine Figur derselben Farbe gezogen wird, darf die Figur nicht gesetzt werden
                if (this.GetPiece(xNeu, yNeu) != null && this.GetPiece(xNeu, yNeu).Color == piece.Color)
                    figureHasAlreadyBeenSet = false;
                //setze die gezogene Figur, sofern der Koenig im Schach steht. Muss nach Ablauf aller Algorithmen rueckgaengig gemacht werden(04.08.2019 in Zeile 1388)
                else {
                    figureHasBeenRepressed = this.GetPiece(xNeu, yNeu);
                    this.SetPiece(xNeu, yNeu, piece);
                    figureHasAlreadyBeenSet = true;
                }
            }

            //befindet sich der schwarze Koenig im Schach?
            if (piece.Color == PieceColor.Black && SchachGUI.kingIsChecked) {
                //pruefe Schach auf Diagonalitaet.Fuer den ersten Fall wird die Logik kommentiert, alle anderen Faelle sind identisch
                if (lastFigureMovedWhite.Type == PieceType.Laeufer || lastFigureMovedWhite.Type == PieceType.Dame) {
                    //sofern  die schagebende Figur geschlagen wurde, besteht kein Schach
                    if (Board.lastFigureMovedWhiteX == xNeu && Board.lastFigureMovedWhiteY == yNeu)
                        stillChecked = false;
                    //Fall (1)
                    while (dummyLastFigureMovedWhiteX < dummyKingBlackX && dummyLastFigureMovedWhiteY > dummyKingBlackY) {
                        if (xNeu == Board.lastFigureMovedWhiteX && yNeu == Board.lastFigureMovedWhiteY) {
                            stillChecked = false;
                            break;
                        }
                        dummyLastFigureMovedWhiteX++;
                        dummyLastFigureMovedWhiteY--;
                        figure = this.GetPiece(dummyLastFigureMovedWhiteX, dummyLastFigureMovedWhiteY);
                        //sofern eine Figur blockiert, besteht kein Schach mehr
                        if (figure != null && figure.Type != PieceType.Koenig) {
                            stillChecked = false;
                            break;
                            //sofern der Koenig ausgewichen ist, besteht kein Schach
                        } else if (piece.Type == PieceType.Koenig) {
                            if (Math.Abs(Board.lastFigureMovedWhiteX - dummyKingBlackX) != Math.Abs(Board.lastFigureMovedWhiteY - dummyKingBlackY)) {
                                stillChecked = false;
                                break;
                            }
                        }
                    }
                    //Fall (2)
                    while (stillChecked && dummyLastFigureMovedWhiteX > dummyKingBlackX && dummyLastFigureMovedWhiteY > dummyKingBlackY) {
                        if (xNeu == Board.lastFigureMovedWhiteX && yNeu == Board.lastFigureMovedWhiteY) {
                            stillChecked = false;
                            break;
                        }
                        dummyLastFigureMovedWhiteX--;
                        dummyLastFigureMovedWhiteY--;
                        figure = this.GetPiece(dummyLastFigureMovedWhiteX, dummyLastFigureMovedWhiteY);
                        if (figure != null && figure.Type != PieceType.Koenig) {
                            stillChecked = false;
                            break;
                            //sofern der Koenig ausgewichen ist, besteht kein Schach
                        } else if (piece.Type == PieceType.Koenig) {
                            if (Math.Abs(Board.lastFigureMovedWhiteX - dummyKingBlackX) != Math.Abs(Board.lastFigureMovedWhiteY - dummyKingBlackY)) {
                                stillChecked = false;
                                break;
                            }
                        }
                    }
                    //Fall (3)
                    while (stillChecked && dummyLastFigureMovedWhiteX < dummyKingBlackX && dummyLastFigureMovedWhiteY < dummyKingBlackY) {
                        if (xNeu == Board.lastFigureMovedWhiteX && yNeu == Board.lastFigureMovedWhiteY) {
                            stillChecked = false;
                            break;
                        }
                        dummyLastFigureMovedWhiteX++;
                        dummyLastFigureMovedWhiteY++;
                        figure = this.GetPiece(dummyLastFigureMovedWhiteX, dummyLastFigureMovedWhiteY);
                        if (figure != null && figure.Type != PieceType.Koenig) {
                            stillChecked = false;
                            break;
                            //sofern der Koenig ausgewichen ist, besteht kein Schach
                        } else if (piece.Type == PieceType.Koenig) {
                            if (Math.Abs(Board.lastFigureMovedWhiteX - dummyKingBlackX) != Math.Abs(Board.lastFigureMovedWhiteY - dummyKingBlackY)) {
                                stillChecked = false;
                                break;
                            }
                        }
                    }
                    //Fall (4)
                    while (stillChecked && dummyLastFigureMovedWhiteX > dummyKingBlackX && dummyLastFigureMovedWhiteY < dummyKingBlackY) {
                        if (xNeu == Board.lastFigureMovedWhiteX && yNeu == Board.lastFigureMovedWhiteY) {
                            stillChecked = false;
                            break;
                        }
                        dummyLastFigureMovedWhiteX--;
                        dummyLastFigureMovedWhiteY++;
                        figure = this.GetPiece(dummyLastFigureMovedWhiteX, dummyLastFigureMovedWhiteY);
                        if (figure != null && figure.Type != PieceType.Koenig) {
                            stillChecked = false;
                            break;
                            //sofern der Koenig ausgewichen ist, besteht kein Schach
                        } else if (piece.Type == PieceType.Koenig) {
                            if (Math.Abs(Board.lastFigureMovedWhiteX - dummyKingBlackX) != Math.Abs(Board.lastFigureMovedWhiteY - dummyKingBlackY)) {
                                stillChecked = false;
                                break;
                            }
                        }
                    }
                } else if (lastFigureMovedWhite.Type == PieceType.Springer) {
                    /*  sofern der Koenig gezogen hat oder die schachgebende Figur geschlagen wurde besteht kein Schach. Die zweite Pruefung entfaellt dann.                                                                */
                    if (piece.Type == PieceType.Koenig || Board.lastFigureMovedWhiteX == xNeu && Board.lastFigureMovedWhiteY == yNeu)
                        stillChecked = false;
                    if (stillChecked) {
                        if (Board.lastFigureMovedWhiteX + 2 == SchachGUI.posKingBlackX && Board.lastFigureMovedWhiteY - 1 == SchachGUI.posKingBlackY //2 nach rechts, 1 nach vorne
                            || Board.lastFigureMovedWhiteX + 2 == SchachGUI.posKingBlackX && Board.lastFigureMovedWhiteY + 1 == SchachGUI.posKingBlackY //2 nach rechts, 1 nach hinten
                            || Board.lastFigureMovedWhiteX - 2 == SchachGUI.posKingBlackX && Board.lastFigureMovedWhiteY - 1 == SchachGUI.posKingBlackY //2 nach links, 1 nach vorne 
                            || Board.lastFigureMovedWhiteX - 2 == SchachGUI.posKingBlackX && Board.lastFigureMovedWhiteY + 1 == SchachGUI.posKingBlackY //2 nach links, 1 nach hinten 
                            || Board.lastFigureMovedWhiteX + 1 == SchachGUI.posKingBlackX && Board.lastFigureMovedWhiteY - 2 == SchachGUI.posKingBlackY //1 nach rechts, 2 nach vorne
                            || Board.lastFigureMovedWhiteX + 1 == SchachGUI.posKingBlackX && Board.lastFigureMovedWhiteY + 2 == SchachGUI.posKingBlackY //1 nach rechts, 2 nach hinten
                            || Board.lastFigureMovedWhiteX - 1 == SchachGUI.posKingBlackX && Board.lastFigureMovedWhiteY - 2 == SchachGUI.posKingBlackY //1 nach links, 2 nach vorne
                            || Board.lastFigureMovedWhiteX - 1 == SchachGUI.posKingBlackX && Board.lastFigureMovedWhiteY + 2 == SchachGUI.posKingBlackY //1 nach links, 2 nach hinten
                         )
                            stillChecked = true;
                        else
                            stillChecked = false;
                    }
                } else if (lastFigureMovedWhite.Type == PieceType.Bauer) {
                    //sofern der Koenig gezogen hat oder die schachgebende Figur geschlagen wurde, besteht kein Schach
                    if (piece.Type == PieceType.Koenig || (Board.lastFigureMovedWhiteX == xNeu && Board.lastFigureMovedWhiteY == yNeu))
                        stillChecked = false;
                }
                if (stillChecked && (lastFigureMovedWhite.Type == PieceType.Turm || lastFigureMovedWhite.Type == PieceType.Dame)) {
                    //sofern  die schagebende Figur geschlagen wurde, besteht kein Schach
                    if (Board.lastFigureMovedWhiteX == xNeu && Board.lastFigureMovedWhiteY == yNeu)
                        stillChecked = false;
                    /*  sofern sich eine Figur dazwischen gestellt hat, besteht kein Schach
                        Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus I
                        Da der obige Algorithmus die dummyVariablen bereits veraendert hat, werden die Originalvariablen veraendert.*/
                    if (stillChecked) {
                        //um ueberlange Variablennamen zu vermeiden, wird hier CamelCase ausnahmsweise gross angefangen
                        int DummyLastFigureMovedWhiteX = Board.lastFigureMovedWhiteX, DummyLastFigureMovedWhiteY = Board.lastFigureMovedWhiteY;
                        while (DummyLastFigureMovedWhiteY < SchachGUI.posKingBlackY && DummyLastFigureMovedWhiteX == SchachGUI.posKingBlackX) {
                            DummyLastFigureMovedWhiteY++;
                            if (this.GetPiece(DummyLastFigureMovedWhiteX, DummyLastFigureMovedWhiteY) != null && this.GetPiece(DummyLastFigureMovedWhiteX, DummyLastFigureMovedWhiteY).Type != PieceType.Koenig) {
                                stillChecked = false;
                                break;
                            }
                        }
                        //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus II
                        while (DummyLastFigureMovedWhiteY > SchachGUI.posKingBlackY && DummyLastFigureMovedWhiteX == SchachGUI.posKingBlackX) {
                            DummyLastFigureMovedWhiteY--;
                            if (this.GetPiece(DummyLastFigureMovedWhiteX, DummyLastFigureMovedWhiteY) != null && this.GetPiece(DummyLastFigureMovedWhiteX, DummyLastFigureMovedWhiteY).Type != PieceType.Koenig) {
                                stillChecked = false;
                                break;
                            }
                        }
                        //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus III
                        while (DummyLastFigureMovedWhiteY == SchachGUI.posKingBlackY && SchachGUI.posKingBlackX > DummyLastFigureMovedWhiteX) {
                            DummyLastFigureMovedWhiteX++;
                            if (this.GetPiece(DummyLastFigureMovedWhiteX, DummyLastFigureMovedWhiteY) != null && this.GetPiece(DummyLastFigureMovedWhiteX, DummyLastFigureMovedWhiteY).Type != PieceType.Koenig) {
                                stillChecked = false;
                                break;
                            }
                        }
                        //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus IV
                        while (DummyLastFigureMovedWhiteY == SchachGUI.posKingBlackY && SchachGUI.posKingBlackX < DummyLastFigureMovedWhiteX) {
                            DummyLastFigureMovedWhiteX--;
                            if (this.GetPiece(DummyLastFigureMovedWhiteX, DummyLastFigureMovedWhiteY) != null && this.GetPiece(DummyLastFigureMovedWhiteX, DummyLastFigureMovedWhiteY).Type != PieceType.Koenig) {
                                stillChecked = false;
                                break;
                            }
                        }
                        //ueberpruefe, ob der Koenig die Horizontale oder Vertikale Angriffslinie verlassen hat. Falls ja, kein Schach. 
                        if (lastFigureMovedWhite.Type == PieceType.Turm && stillChecked && piece.Type == PieceType.Koenig && (Math.Abs(Board.lastFigureMovedWhiteX - dummyKingBlackX) != 0 && Math.Abs(Board.lastFigureMovedWhiteY - dummyKingBlackY) != 0))
                            stillChecked = false;
                    }
                }
                //befindet sich der weisse Koenig im Schach?
            } else if (piece.Color == PieceColor.White && SchachGUI.kingIsChecked) {
                if (lastFigureMovedBlack.Type == PieceType.Laeufer || lastFigureMovedBlack.Type == PieceType.Dame) {
                    //sofern  die schagebende Figur geschlagen wurde, besteht kein Schach
                    if (Board.lastFigureMovedBlackX == xNeu && Board.lastFigureMovedBlackY == yNeu)
                        stillChecked = false;
                    //Fall (1)
                    while (dummyLastFigureMovedBlackX < dummyKingWhiteX && dummyLastFigureMovedBlackY > dummyKingWhiteY) {
                        if (xNeu == Board.lastFigureMovedBlackX && yNeu == Board.lastFigureMovedBlackY) {
                            stillChecked = false;
                            break;
                        }
                        dummyLastFigureMovedBlackX++;
                        dummyLastFigureMovedBlackY--;
                        figure = this.GetPiece(dummyLastFigureMovedBlackX, dummyLastFigureMovedBlackY);
                        //sofern eine Figur blockiert, besteht kein Schach mehr
                        if (figure != null && figure.Type != PieceType.Koenig) {
                            stillChecked = false;
                            break;
                            //sofern der Koenig ausgewichen ist, besteht kein Schach
                        } else if (piece.Type == PieceType.Koenig) {
                            if (Math.Abs(Board.lastFigureMovedBlackX - dummyKingWhiteX) != Math.Abs(Board.lastFigureMovedBlackY - dummyKingWhiteY)) {
                                stillChecked = false;
                                break;
                            }
                        }
                    }
                    //Fall (2)
                    while (dummyLastFigureMovedBlackX > dummyKingWhiteX && dummyLastFigureMovedBlackY > dummyKingWhiteY) {
                        if (xNeu == Board.lastFigureMovedBlackX && yNeu == Board.lastFigureMovedBlackY) {
                            stillChecked = false;
                            break;
                        }
                        dummyLastFigureMovedBlackX--;
                        dummyLastFigureMovedBlackY--;
                        figure = this.GetPiece(dummyLastFigureMovedBlackX, dummyLastFigureMovedBlackY);
                        if (figure != null && figure.Type != PieceType.Koenig) {
                            stillChecked = false;
                            break;
                            //sofern der Koenig ausgewichen ist, besteht kein Schach
                        } else if (piece.Type == PieceType.Koenig) {
                            if (Math.Abs(Board.lastFigureMovedBlackX - dummyKingWhiteX) != Math.Abs(Board.lastFigureMovedBlackY - dummyKingWhiteY)) {
                                stillChecked = false;
                                break;
                            }
                        }
                    }
                    //Fall (3)
                    while (dummyLastFigureMovedBlackX < dummyKingWhiteX && dummyLastFigureMovedBlackY < dummyKingWhiteY) {
                        if (xNeu == Board.lastFigureMovedBlackX && yNeu == Board.lastFigureMovedBlackY) {
                            stillChecked = false;
                            break;
                        }
                        dummyLastFigureMovedBlackX++;
                        dummyLastFigureMovedBlackY++;
                        figure = this.GetPiece(dummyLastFigureMovedBlackX, dummyLastFigureMovedBlackY);
                        if (figure != null && figure.Type != PieceType.Koenig) {
                            stillChecked = false;
                            break;
                            //sofern der Koenig ausgewichen ist, besteht kein Schach
                        } else if (piece.Type == PieceType.Koenig) {
                            if (Math.Abs(Board.lastFigureMovedBlackX - dummyKingWhiteX) != Math.Abs(Board.lastFigureMovedBlackY - dummyKingWhiteY)) {
                                stillChecked = false;
                                break;
                            }
                        }
                    }
                    //Fall (4)
                    while (dummyLastFigureMovedBlackX > dummyKingWhiteX && dummyLastFigureMovedBlackY < dummyKingWhiteY) {
                        if (xNeu == Board.lastFigureMovedBlackX && yNeu == Board.lastFigureMovedBlackY) {
                            stillChecked = false;
                            break;
                        }
                        dummyLastFigureMovedBlackX--;
                        dummyLastFigureMovedBlackY++;
                        figure = this.GetPiece(dummyLastFigureMovedBlackX, dummyLastFigureMovedBlackY);
                        if (figure != null && figure.Type != PieceType.Koenig) {
                            stillChecked = false;
                            break;
                            //sofern der Koenig ausgewichen ist, besteht kein Schach
                        } else if (piece.Type == PieceType.Koenig) {
                            if (Math.Abs(Board.lastFigureMovedBlackX - dummyKingWhiteX) != Math.Abs(Board.lastFigureMovedBlackY - dummyKingWhiteY)) {
                                stillChecked = false;
                                break;
                            }
                        }
                    }
                } else if (lastFigureMovedBlack.Type == PieceType.Bauer) {
                    //sofern der Koenig gezogen hat oder die schachgebende Figur geschlagen wurde, besteht kein Schach
                    if (piece.Type == PieceType.Koenig || (Board.lastFigureMovedBlackX == xNeu && Board.lastFigureMovedBlackY == yNeu))
                        stillChecked = false;
                } else if (lastFigureMovedBlack.Type == PieceType.Springer) {
                    //Sofern der Koenig gezogen hat oder die schachgebende Figur geschlagen wurde besteht kein Schach. Die zweite Pruefung entfaellt dann.
                    if (piece.Type == PieceType.Koenig || Board.lastFigureMovedBlackX == xNeu && Board.lastFigureMovedBlackY == yNeu)
                        stillChecked = false;
                    if (stillChecked) {
                        if (Board.lastFigureMovedBlackX + 2 == SchachGUI.posKingWhiteX && Board.lastFigureMovedBlackY - 1 == SchachGUI.posKingWhiteY //2 nach rechts, 1 nach vorne
                            || Board.lastFigureMovedBlackX + 2 == SchachGUI.posKingWhiteX && Board.lastFigureMovedBlackY + 1 == SchachGUI.posKingWhiteY //2 nach rechts, 1 nach hinten
                            || Board.lastFigureMovedBlackX - 2 == SchachGUI.posKingWhiteX && Board.lastFigureMovedBlackY - 1 == SchachGUI.posKingWhiteY //2 nach links, 1 nach vorne 
                            || Board.lastFigureMovedBlackX - 2 == SchachGUI.posKingWhiteX && Board.lastFigureMovedBlackY + 1 == SchachGUI.posKingWhiteY //2 nach links, 1 nach hinten 
                            || Board.lastFigureMovedBlackX + 1 == SchachGUI.posKingWhiteX && Board.lastFigureMovedBlackY - 2 == SchachGUI.posKingWhiteY //1 nach rechts, 2 nach vorne
                            || Board.lastFigureMovedBlackX + 1 == SchachGUI.posKingWhiteX && Board.lastFigureMovedBlackY + 2 == SchachGUI.posKingWhiteY //1 nach rechts, 2 nach hinten
                            || Board.lastFigureMovedBlackX - 1 == SchachGUI.posKingWhiteX && Board.lastFigureMovedBlackY - 2 == SchachGUI.posKingWhiteY //1 nach links, 2 nach vorne
                            || Board.lastFigureMovedBlackX - 1 == SchachGUI.posKingWhiteX && Board.lastFigureMovedBlackY + 2 == SchachGUI.posKingWhiteY //1 nach links, 2 nach hinten
                         )
                            stillChecked = true;
                        else
                            stillChecked = false;
                    }
                }
                if (lastFigureMovedBlack.Type == PieceType.Turm || lastFigureMovedBlack.Type == PieceType.Dame) {
                    //sofern  die schagebende Figur geschlagen wurde, besteht kein Schach
                    if (Board.lastFigureMovedBlackX == xNeu && Board.lastFigureMovedBlackY == yNeu) {
                        stillChecked = false;
                    }
                    /*  sofern sich eine Figur dazwischen gestellt hat, besteht kein Schach
                        Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus I
                        Da der obige Algorithmus die dummyVariablen bereits veraendert hat, werden neue Dummyavariablen verwendet.*/
                    if (stillChecked) {
                        //um ueberlange Variablennamen zu vermeiden, wird hier CamelCase ausnahmsweise gross angefangen
                        int DummyLastFigureMovedBlackX = Board.lastFigureMovedBlackX, DummyLastFigureMovedBlackY = Board.lastFigureMovedBlackY;
                        while (DummyLastFigureMovedBlackY < SchachGUI.posKingWhiteY && DummyLastFigureMovedBlackX == SchachGUI.posKingWhiteX) {
                            DummyLastFigureMovedBlackY++;
                            if (this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY) != null && this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY).Type != PieceType.Koenig) {
                                stillChecked = false;
                                break;
                            }
                        }
                        //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus II
                        while (DummyLastFigureMovedBlackY > SchachGUI.posKingWhiteY && DummyLastFigureMovedBlackX == SchachGUI.posKingWhiteX) {
                            DummyLastFigureMovedBlackY--;
                            if (this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY) != null && this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY).Type != PieceType.Koenig) {
                                stillChecked = false;
                                break;
                            }
                        }
                        //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus III
                        while (DummyLastFigureMovedBlackY == SchachGUI.posKingWhiteY && SchachGUI.posKingWhiteX > DummyLastFigureMovedBlackX) {
                            DummyLastFigureMovedBlackX++;
                            Piece x = this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY);
                            if (this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY) != null && this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY).Type != PieceType.Koenig) {
                                stillChecked = false;
                                break;
                            }
                        }
                        //Pruefe, ob zwischen Turm und Koenig eine Figur steht. Falls ja, kein Schach - Algorithmus IV
                        while (DummyLastFigureMovedBlackY == SchachGUI.posKingWhiteY && SchachGUI.posKingWhiteX < DummyLastFigureMovedBlackX) {
                            DummyLastFigureMovedBlackX--;
                            if (this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY) != null && this.GetPiece(DummyLastFigureMovedBlackX, DummyLastFigureMovedBlackY).Type != PieceType.Koenig) {
                                stillChecked = false;
                                break;
                            }
                        }
                        //ueberpruefe, ob der Koenig die Horizontale oder Vertikale Angriffslinie verlassen hat. Falls ja, kein Schach.
                        if (lastFigureMovedBlack.Type == PieceType.Turm && stillChecked && piece.Type == PieceType.Koenig && (Math.Abs(Board.lastFigureMovedBlackX - dummyKingWhiteX) != 0 && Math.Abs(Board.lastFigureMovedBlackY - dummyKingWhiteY) != 0))
                            stillChecked = false;
                    }
                }
            }
            //Ueberpruefe zuletzt noch, ob der Koenig in ein Abzugsschach gelaufen ist. Falls ja, invertiere das BOOLEAN
            if (!stillChecked && this.IsDiscoveredCheck(xNeu, yNeu, xNeuKoenig, yNeuKoenig, piece, false))
                stillChecked = !stillChecked;
            //sofern aufgrund obiger Pruefungen stillChecked nicht auf false gesetzt wurde, muss das Setzen der Figur rueckgaengig gemacht werden
            Board.figureHasBeenPut = false;
            if (stillChecked && SchachGUI.kingIsChecked && figureHasAlreadyBeenSet) {
                //Setzen der Figur rueckgaengig machen
                this.SetPiece(xNeu, yNeu, figureHasBeenRepressed);
                Board.figureHasBeenPut = true;
            }
            return stillChecked;
        }
        #endregion

        #region Eruiere nach jedem Zug die Figuren auf dem Brett
        private void EruateFiguresOnBoard(Piece currentFigure, int figureMovedX, int figureMovedY) {
            if (SchachGUI.haveRestartedGame) {
                figuresBeaten.Clear();
                actualFigures.Clear();
                if (Board.lstfiguresBeaten != null && Board.lstfiguresBeaten.Count > 0)
                    lstfiguresBeaten.Clear();
                SchachGUI.haveRestartedGame = !SchachGUI.haveRestartedGame;
            }
            int posFigureX, posAllFiguresY = 0, counter = 0, coordinate;
            //leere und fuelle die jeweiligen Tupel
            int countMoreThanZero = actualFigures.Where(t => t.Item2 > 0).Count();
            if (countMoreThanZero > 0) {
                actualFiguresCopy.AddRange(actualFigures);
                actualFigures.Clear();
            }
            //iteriere ueber das gesamte Feld und packe alle Figuren in eine Tupelliste
            foreach (var item in this._pieces) {
                if (item != null) {
                    //ergibt sich durch mathematische Umformung der Gleichung aus SetPiece()
                    posFigureX = Math.Abs(counter - 8 * posAllFiguresY);
                    //posFigureY ist immer 0. Verbleibt aus Debugging-bzw. Ueberpruefungsgruenden im Code!
                    posAllFiguresY = Math.Abs((counter - posFigureX) / 8);
                    Piece figure = this.GetPiece(posFigureX, posAllFiguresY);
                    actualFigures.Add(new Tuple<Piece, Byte>(figure, Convert.ToByte(posFigureX)));
                }
                counter++;
            }
            bool sameCount = actualFigures.Count == actualFiguresCopy.Count;
            bool doLeaveLoop = false, sameFigure;
            //vergleiche die beiden Tupellisten nur dann, sofern Sie ungleiche Elemente haben
            if (compareTuples && !sameCount) {
                for (int i = 0; i < actualFigures.Count; i++) {
                    //steige aus, sofern unten ein Element gefunden wurde
                    if (doLeaveLoop) break;
                    for (int j = 0; j < actualFiguresCopy.Count; j++) {
                        if (i == j) {
                            //pruefe, ob die Figuren identisch sind..
                            if (actualFigures[i].Item1 != null && actualFiguresCopy[j].Item1 != null) {
                                sameFigure = actualFigures[i].Item1.Equals(actualFiguresCopy[j].Item1);
                                //..falls nein,verfrachte die gefundene Figur in die Liste der geschlagenen Figuren
                                if (!sameFigure) {
                                    coordinate = this.giveBackCoordinates(figureMovedX, figureMovedY);
                                    foreach (var item in actualFiguresCopy) {
                                        if (item.Item2 == coordinate) {
                                            figuresBeaten.Add(item.Item1);
                                            //verfrachte den Inhalt in den Getter&Setter, damit SchachGUI darauf zugreifen kann
                                            Board.lstfiguresBeaten = figuresBeaten;
                                            Board.figureBeaten = item.Item1.Type;
                                            Board.figureDeleted = item.Item1;
                                            break;
                                        }
                                    }
                                    doLeaveLoop = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            actualFiguresCopy.Clear();
            //setze die globale Variable auf TRUE, damit mit dem Tupelvergleich begonnen wird
            compareTuples = true;
        }
        #endregion

        #region Wandle Bauer in Dame um
        public void ChangeBauer2Dame(string farbe, int x, int y) {
            Piece dameWhite = new Piece(PieceType.Dame, PieceColor.White);
            Piece dameBlack = new Piece(PieceType.Dame, PieceColor.Black);
            if (farbe.Equals("white"))
                this.SetPiece(x, y, dameWhite);
            else if (farbe.Equals("black"))
                this.SetPiece(x, y, dameBlack);
        }
        #endregion

        #region Pruefe, ob en passant moeglich ist
        private bool CheckForEnPassant(Piece piece, int xAlt, int xNeu, int yAlt, int yNeu) {
            bool enPassant = false;
            Piece figureLeft = this.GetPiece(xAlt - 1, yAlt);
            Piece figureRight = this.GetPiece(xAlt + 1, yAlt);
            if (piece.Color == PieceColor.White) {
                if (figureLeft != null) {
                    if (piece.Type == PieceType.Bauer && figureLeft.Type == PieceType.Bauer && Board.lastFigureMovedBlackY == 3 && Board.lastFigureMovedBlackX == xNeu && Board.lastFigureMovedBlackY - yNeu == 1)
                        enPassant = true;
                } else if (figureRight != null) {
                    if (piece.Type == PieceType.Bauer && figureRight.Type == PieceType.Bauer && Board.lastFigureMovedBlackY == 3 && Board.lastFigureMovedBlackX == xNeu && Board.lastFigureMovedBlackY - yNeu == 1)
                        enPassant = true;
                }
            } else if (piece.Color == PieceColor.Black) {
                if (figureLeft != null) {
                    if (piece.Type == PieceType.Bauer && figureLeft.Type == PieceType.Bauer && Board.lastFigureMovedWhiteY == 4 && Board.lastFigureMovedWhiteX == xNeu && Math.Abs(Board.lastFigureMovedWhiteY - yNeu) == 1)
                        enPassant = true;
                } else if (figureRight != null) {
                    if (piece.Type == PieceType.Bauer && figureRight.Type == PieceType.Bauer && Board.lastFigureMovedWhiteY == 4 && Board.lastFigureMovedWhiteX == xNeu && Math.Abs(Board.lastFigureMovedWhiteY - yNeu) == 1)
                        enPassant = true;
                }
            }
            return enPassant;
        }
        #endregion

        #region Pruefe,ob ein Abzugsschach entstanden ist, oder ob der Koenig in ein Schach gezogen hat. Benoetigt GetXYofAllFigures
        private bool IsDiscoveredCheck(int xNeu, int yNeu, int xNeuKoenig, int yNeuKoenig, Piece piece, [Optional]bool checkAbzugsSchach) {
            List<Tuple<Piece, int, int>> figuresGiveCheck;
            Piece queen = null, laeufer = null, turm = null;
            PieceColor color = PieceColor.Dummy;
            bool discoveredCheck = false;
            if (piece.Color == PieceColor.White) {
                color = PieceColor.Black;
            } else if (piece.Color == PieceColor.Black) {
                color = PieceColor.White;
            }
            //Setze die Figur, damit die folgenden Algorithmen korrekt arbeiten. Muss am Ende (in Zeile 1546) wieder rueckgaengig gemacht werden
            this.SetPiece(xNeu, yNeu, piece);
            figuresGiveCheck = this.GetXYofAllFigures(color);
            if (!checkAbzugsSchach) {
                xNeuKoenig = xNeu; yNeuKoenig = yNeu;
                if (piece.Type == PieceType.Koenig) {
                    discoveredCheck = this.CalculateDiscoveredCheck(figuresGiveCheck, xNeuKoenig, yNeuKoenig, queen, laeufer, turm);
                }
            } else {
                if (piece.Color == PieceColor.White) {
                    //wenn mit Weiss gezogen wird, ist von Interesse, ob eine schwarze Figur dem weissen Koenig Schach gibt
                    color = PieceColor.Black;
                    xNeuKoenig = SchachGUI.posKingWhiteX;
                    yNeuKoenig = SchachGUI.posKingWhiteY;
                } else if (piece.Color == PieceColor.Black) {
                    //wenn mit Schwarz gezogen wird, ist von Interesse, ob eine weisse Figur dem schwarzen Koenig Schach gibt
                    color = PieceColor.White;
                    xNeuKoenig = SchachGUI.posKingBlackX;
                    yNeuKoenig = SchachGUI.posKingBlackY;
                }
                //Sicher gehen, dass die generische Tupelliste wirklich leer ist
                if (figuresGiveCheck.Count > 0)
                    figuresGiveCheck.Clear();
                figuresGiveCheck = this.GetXYofAllFigures(color);
                discoveredCheck = this.CalculateDiscoveredCheck(figuresGiveCheck, xNeuKoenig, yNeuKoenig, queen, laeufer, turm);
            }
            //Mache das Setzen der Figur rueckgaengig
            if (!SchachGUI.kingHasRochaded)
                this.SetPiece(xNeu, yNeu, null);
            return discoveredCheck;
        }

        #endregion

        #region Pruefe, ob Koenig schachmatt ist
        public bool IsKingMatt(Piece attackFigure, int x, int y) {
            int posKingX = 0, posKingY = 0, iterationen = 0;
            const int grenze = 7;
            bool matt = true, rightOfKing = false, leftOfKing = false, figureAttacksHorizontal = false, figureAttacksVertical = false, figureInFrontOfKing = false, figureBehindOfKing = false;
            List<Tuple<Piece, int, int>> Figures2AvoidMatt;
            PieceColor color = PieceColor.Dummy;
            //Loesche die globale generische Liste, sofern Sie Inhalt hat
            if (positionX.Count > 0 && positionY.Count > 0) {
                positionX.Clear();
                positionY.Clear();
            }
            if (attackFigure.Color == PieceColor.White) {
                posKingX = SchachGUI.posKingBlackX;
                posKingY = SchachGUI.posKingBlackY;
                color = PieceColor.Black;
            } else if (attackFigure.Color == PieceColor.Black) {
                posKingX = SchachGUI.posKingWhiteX;
                posKingY = SchachGUI.posKingWhiteY;
                color = PieceColor.White;
            }
            //sofern der globale Boolean TRUE, pruefe auf matt. Setze wichtige Booleans
            if (CheckForMatt) {
                if (x > posKingX)
                    rightOfKing = true;
                else if (x < posKingX)
                    leftOfKing = true;
                if (posKingX == x)
                    figureAttacksVertical = true;
                else if (posKingY == y)
                    figureAttacksHorizontal = true;
                if (posKingY > y)
                    figureBehindOfKing = true;
                else if (posKingY < y)
                    figureInFrontOfKing = true;
                //pruefe, ob der schwarze Koenig matt ist
                if (attackFigure.Color == PieceColor.White) {
                    /*                  +++++++++++++++++++++++++++++ Ausweichen +++++++++++++++++++++++++++++
                        sofern der Koenig ausweichen kann, ohne dass er in ein Abzugsschach laeuft besteht generell 
                        kein Matt.Dazu muss fuer alle moeglichen Ausweichfelder ueberprueft werden, ob es leer ist. Die potentiellen Schachbereiche der schachgebenden
                        Figur duerfen dabei allerdings nicht geprueft werden, da sie keine Ausweichoption darstellen
                                                                       
                        Sofern sich der Laeufer bzw. die Dame aus weisser Sicht links vom Koenig befindet, darf das diagonale Feld links vom schwarzen Koenig nicht als Ausweichfeld
                        in Betracht gezogen werden. Analoges gilt fuer rechts vom Koenig. Maximal gibt es 8 Ausweichfelder fuer einen Koenig. Davon muessen 8-2 
                        auf Belegtheit ueberprueft werden */
                    if (attackFigure.Type == PieceType.Laeufer || attackFigure.Type == PieceType.Dame) {
                        matt = this.CheckMattForDameOrLaeufer(figureAttacksHorizontal, figureAttacksVertical, leftOfKing, rightOfKing, figureInFrontOfKing, figureBehindOfKing, posKingX, posKingY, x, y, attackFigure);
                        /*                              +++++++++++++++++++++++++++++ Ausweichen +++++++++++++++++++++++++++++  */
                    } else if (attackFigure.Type == PieceType.Springer || attackFigure.Type == PieceType.Bauer) {
                        //kapsle auch hier aus denselben Gruenden
                        matt = this.CheckMattForSpringerOrBauer(posKingY, posKingX, attackFigure);
                        // Ist die Angriffsfigur ein Turm werden prinzipiell 8-2 Ausweichfelder ueberprueft, wobei eruiert werden musste, ob der Turm horizontal oder vertikal angreift

                        /*    +++++++++++++++++++++++++++++ Ausweichen +++++++++++++++++++++++++++++ */
                    } else if (attackFigure.Type == PieceType.Turm) {
                        //kapsle auch hier aus denselben Gruenden
                        matt = this.CheckMattForTurm(figureAttacksVertical, posKingY, posKingX, figureAttacksHorizontal, attackFigure);
                    }
                    //eruiere die X und Y Koordinaten aller Figuren. Dazu bediene man sich der Methode GetXYofAllFigures                
                    Figures2AvoidMatt = this.GetXYofAllFigures(color);
                    /*                          +++++++++++++++++++++++++++++ Blockieren +++++++++++++++++++++++++++++
                        jetzt muss mittels geeigneter Algorithmen ueberprueft werden, ob die Figuren in der Tupelliste Figures2AvoidMatt ein Matt verhindern. Dazu
                        muss ueber die generische Tupelliste iteriert werden, um zu pruefen, ob potentielle Figuren die Angriffsfigur blockieren koennen */
                    if (matt) {
                        int dummyX = x; int dummyY = y; bool doLeaveLoop = false;
                        foreach (var item in Figures2AvoidMatt) {
                            if (doLeaveLoop) break;
                            if (item.Item1 != null) {
                                if (attackFigure.Type == PieceType.Laeufer || attackFigure.Type == PieceType.Dame) {
                                    //fuelle 2 generische Listen mit den Koordinaten der Dame bzw. Laeufer(Diagonaler Angriff)
                                    if (positionX.Count == 0 && positionY.Count == 0) {
                                        this.CreateDiagonalAttackCoordinates(dummyX, posKingX, dummyY, posKingY, attackFigure);
                                    }
                                    /*  Pruefe, ob eine beliebige Figur die Angriffsfigur blockieren kann. Falls ja,kein Matt=>Pruefe Bauer
                                        item.Item2=posXFigure ; item.Item3=posYFigure */
                                    if (item.Item1.Type == PieceType.Bauer) {
                                        //  Da ein Bauer nicht rueckwaerts ziehen kann entfallen die Pruefungen auf dummy<item.Item3. Diagonaler Angriff
                                        if (!figureAttacksVertical && !figureAttacksHorizontal && leftOfKing && item.Item2 > x) {
                                            while (dummyX <= item.Item2 && dummyY >= item.Item3) {
                                                dummyX++;
                                                dummyY--;
                                                if (dummyX == item.Item2 && dummyY == item.Item3 + 1 || dummyX == item.Item2 && dummyY == item.Item3 + 2) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                            }
                                            dummyX = x; dummyY = y;
                                        } else if (!figureAttacksVertical && !figureAttacksHorizontal && rightOfKing && item.Item2 < x) {
                                            while (dummyX >= item.Item2 && dummyY >= item.Item3) {
                                                dummyX--;
                                                dummyY--;
                                                if (dummyX == item.Item2 && dummyY == item.Item3 + 1 || dummyX == item.Item2 && dummyY == item.Item3 + 2) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                            }

                                        } else if (figureAttacksVertical) {
                                            //Tue nix, da sich ein Bauer einem vertikalen Angriff niemals blockierend dazwischen stellen kann
                                        } else if (figureAttacksHorizontal) {
                                            if (positionX.Contains(item.Item2) && positionY.Contains(item.Item3 + 1) || positionX.Contains(item.Item2) && positionY.Contains(item.Item3 + 2)) {
                                                matt = false;
                                                break;
                                            }
                                        }
                                        dummyX = x; dummyY = y;
                                    } else if (item.Item1.Type == PieceType.Springer) {
                                        /*  Pruefe, ob die Angriffsfigur durch den Springer blockiert werden kann. Falls ja,kein Matt=>Pruefe Springer
                                            item.Item2=posXFigure ; item.Item3=posYFigure */
                                        if (positionX.Contains(item.Item2 - 2) && positionY.Contains(item.Item3 - 1) ||
                                            positionX.Contains(item.Item2 - 2) && positionY.Contains(item.Item3 + 1) ||
                                            positionX.Contains(item.Item2 + 2) && positionY.Contains(item.Item3 - 1) ||
                                            positionX.Contains(item.Item2 + 2) && positionY.Contains(item.Item3 + 1) ||
                                            positionX.Contains(item.Item2 - 1) && positionY.Contains(item.Item3 - 2) ||
                                            positionX.Contains(item.Item2 - 1) && positionY.Contains(item.Item3 + 2) ||
                                            positionX.Contains(item.Item2 + 1) && positionY.Contains(item.Item3 - 2) ||
                                            positionX.Contains(item.Item2 + 1) && positionY.Contains(item.Item3 + 2)
                                        ) {
                                            matt = false;
                                            doLeaveLoop = true;
                                            break;
                                        }
                                        /*  Pruefe, ob die Angriffsfigur durch den Turm oder die Dame blockiert werden kann. Falls ja,kein Matt
                                            item.Item2=posXFigure ; item.Item3=posYFigure */
                                    } else if (item.Item1.Type == PieceType.Turm || item.Item1.Type == PieceType.Dame) {
                                        int defenseFigureChangedX = item.Item2, defenseFigureChangedY = item.Item3;
                                        //Ueberpruefe Horizontalitaet
                                        while (iterationen <= grenze) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                positionX.Clear();
                                                positionY.Clear();
                                                doLeaveLoop = true;
                                                break;
                                            } else
                                                defenseFigureChangedX--;
                                            if (defenseFigureChangedX < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedX = item.Item2;
                                        while (iterationen <= grenze) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                positionX.Clear();
                                                positionY.Clear();
                                                doLeaveLoop = true;
                                                break;
                                            } else
                                                defenseFigureChangedX++;
                                            if (defenseFigureChangedX > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        defenseFigureChangedX = item.Item2;
                                        //Ueberpruefe Vertikalitaet
                                        iterationen = 0; defenseFigureChangedX = item.Item2;
                                        while (iterationen <= grenze) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                positionX.Clear();
                                                positionY.Clear();
                                                doLeaveLoop = true;
                                                break;
                                            } else
                                                defenseFigureChangedY--;
                                            if (defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedY = item.Item3;
                                        while (iterationen <= grenze) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                positionX.Clear();
                                                positionY.Clear();
                                                doLeaveLoop = true;
                                                break;
                                            } else
                                                defenseFigureChangedY++;
                                            if (defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                    }
                                    /*  ToDo:ueberpruefe noch die diagonale Blockadefaehigkeit der Dame. Die horizontale und vertikale wurde oben durch den Turm bereits 
                                        ueberprueft=>Pruefe Dame und Laeufer */
                                    if ((item.Item1.Type == PieceType.Dame || item.Item1.Type == PieceType.Laeufer) && matt) {
                                        int defenseFigureChangedX = item.Item2;
                                        int defenseFigureChangedY = item.Item3;
                                        bool leaveLoop = false;
                                        iterationen = 0;
                                        while (iterationen < grenze && !leaveLoop) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                leaveLoop = true;
                                                doLeaveLoop = true;
                                                positionX.Clear();
                                                positionY.Clear();
                                                break;
                                            } else {
                                                defenseFigureChangedX++;
                                                defenseFigureChangedY++;
                                            }
                                            if (defenseFigureChangedX > 7 || defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3; iterationen = 0;
                                        while (iterationen < grenze && !leaveLoop) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                leaveLoop = true;
                                                doLeaveLoop = true;
                                                positionX.Clear();
                                                positionY.Clear();
                                                break;
                                            } else {
                                                defenseFigureChangedX--;
                                                defenseFigureChangedY--;
                                            }
                                            if (defenseFigureChangedX < 0 || defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3; iterationen = 0;
                                        while (iterationen < grenze && !leaveLoop) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                leaveLoop = true;
                                                doLeaveLoop = true;
                                                positionX.Clear();
                                                positionY.Clear();
                                                break;
                                            } else {
                                                defenseFigureChangedX++;
                                                defenseFigureChangedY--;
                                            }
                                            if (defenseFigureChangedX > 7 || defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3; iterationen = 0;
                                        while (iterationen < grenze && !leaveLoop) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                leaveLoop = true;
                                                doLeaveLoop = true;
                                                positionX.Clear();
                                                positionY.Clear();
                                                break;
                                            } else {
                                                defenseFigureChangedX--;
                                                defenseFigureChangedY++;
                                            }
                                            if (defenseFigureChangedX < 0 || defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }

                                    }
                                    //Es gibt keine Figuren, die sich zwischen einen Springer-oder Bauernangriff stellen koennen
                                } else if (attackFigure.Type == PieceType.Springer || attackFigure.Type == PieceType.Bauer) {

                                } else if (attackFigure.Type == PieceType.Turm) {
                                    //erstelle die beiden generischen Listen durch eine gekapselte Methode, diesesmal mit den Koordinaten des Tums
                                    dummyX = x; dummyY = y;
                                    if (positionX.Count == 0 && positionY.Count == 0) {
                                        this.FillListTurm(figureAttacksHorizontal, figureAttacksVertical, dummyX, posKingX, dummyY, x, y, posKingY);
                                    }
                                    int defenseFigureChangedX = item.Item2;
                                    int defenseFigureChangedY = item.Item3;
                                    //Ein Bauer kann nur dann blockieren, sofern horizontal angegriffen wird
                                    if (item.Item1.Type == PieceType.Bauer && figureAttacksHorizontal) {
                                        if (defenseFigureChangedX == x && defenseFigureChangedY + 1 == y || (defenseFigureChangedX == x && defenseFigureChangedY + 2 == y)) {
                                            matt = false;
                                            break;
                                        }
                                    } else if (item.Item1.Type == PieceType.Springer) {
                                        if (positionX.Contains(item.Item2 - 2) && positionY.Contains(item.Item3 - 1) ||
                                               positionX.Contains(item.Item2 - 2) && positionY.Contains(item.Item3 + 1) ||
                                               positionX.Contains(item.Item2 + 2) && positionY.Contains(item.Item3 - 1) ||
                                               positionX.Contains(item.Item2 + 2) && positionY.Contains(item.Item3 + 1) ||
                                               positionX.Contains(item.Item2 - 1) && positionY.Contains(item.Item3 - 2) ||
                                               positionX.Contains(item.Item2 - 1) && positionY.Contains(item.Item3 + 2) ||
                                               positionX.Contains(item.Item2 + 1) && positionY.Contains(item.Item3 - 2) ||
                                               positionX.Contains(item.Item2 + 1) && positionY.Contains(item.Item3 + 2)
                                           ) {
                                            matt = false;
                                            doLeaveLoop = true;
                                            break;
                                        }
                                    } else if (item.Item1.Type == PieceType.Laeufer || item.Item1.Type == PieceType.Dame) {
                                        iterationen = 0;
                                        while (iterationen < grenze) {
                                            defenseFigureChangedX++;
                                            defenseFigureChangedY++;
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                doLeaveLoop = true;
                                                break;
                                            }
                                            if (defenseFigureChangedX > 7 || defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                        while (iterationen < grenze) {
                                            defenseFigureChangedX--;
                                            defenseFigureChangedY--;
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                doLeaveLoop = true;
                                                break;
                                            }
                                            if (defenseFigureChangedX < 0 || defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                        while (iterationen < grenze) {
                                            defenseFigureChangedX++;
                                            defenseFigureChangedY--;
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                doLeaveLoop = true;
                                                break;
                                            }
                                            if (defenseFigureChangedX > 7 || defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                        while (iterationen < grenze) {
                                            defenseFigureChangedX--;
                                            defenseFigureChangedY++;
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                doLeaveLoop = true;
                                                break;
                                            }
                                            if (defenseFigureChangedX < 0 || defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                    }
                                    //Pruefe abschliessend noch auf Blockadefaehigkeit von Turm und horizontalen/vertikalen Damenzuegen
                                    if (item.Item1.Type == PieceType.Turm || item.Item1.Type == PieceType.Dame) {
                                        iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                        if (figureAttacksHorizontal && !figureAttacksVertical) {
                                            while (iterationen < grenze) {
                                                defenseFigureChangedX++;
                                                if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                                if (defenseFigureChangedX > 7) break;
                                                if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                                iterationen++;
                                            }
                                            iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                            while (iterationen < grenze) {
                                                defenseFigureChangedX--;
                                                if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                                if (defenseFigureChangedX < 0) break;
                                                if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                                iterationen++;
                                            }
                                        } else if (!figureAttacksHorizontal && figureAttacksVertical) {
                                            while (iterationen < grenze) {
                                                defenseFigureChangedY--;
                                                if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                                if (defenseFigureChangedY < 0) break;
                                                if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                                iterationen++;
                                            }
                                            iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                            while (iterationen < grenze) {
                                                defenseFigureChangedY++;
                                                if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                                if (defenseFigureChangedY > 7) break;
                                                if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                                iterationen++;
                                            }
                                        }
                                    }
                                }
                                /*          +++++++++++++++++++++++++++++ Schlagen+++++++++++++++++++++++++++++ */
                                if (this.FigureCanDeleteAttackFigure(item.Item1, item.Item2, item.Item3, x, y)) {
                                    matt = false;
                                    doLeaveLoop = true;
                                    break;
                                }
                            }
                        }
                        //pruefe, ob die schachgebende Figur vom Koenig geschlagen werden kann
                        if (Math.Abs(posKingX - x) == 1 && Math.Abs(posKingY - y) == 1)
                            matt = false;
                    }
                    //pruefe, ob der weisse Koenig matt ist
                } else if (attackFigure.Color == PieceColor.Black) {
                    /*          +++++++++++++++++++++++++++++Ausweichen+++++++++++++++++++++++++++++ */
                    if (attackFigure.Type == PieceType.Laeufer || attackFigure.Type == PieceType.Dame) {
                        matt = this.CheckMattForDameOrLaeufer(figureAttacksHorizontal, figureAttacksVertical, leftOfKing, rightOfKing, figureInFrontOfKing, figureBehindOfKing, posKingX, posKingY, x, y, attackFigure);
                        /*      +++++++++++++++++++++++++++++ Ausweichen +++++++++++++++++++++++++++++  */
                    } else if (attackFigure.Type == PieceType.Springer || attackFigure.Type == PieceType.Bauer) {
                        //kapsle auch hier aus denselben Gruenden
                        matt = this.CheckMattForSpringerOrBauer(posKingY, posKingX, attackFigure);
                        // Ist die Angriffsfigur ein Turm werden prinzipiell 8-2 Ausweichfelder ueberprueft, wobei eruiert werden musste, ob der Turm horizontal oder vertikal angreift

                        /*    +++++++++++++++++++++++++++++ Ausweichen +++++++++++++++++++++++++++++ */
                    } else if (attackFigure.Type == PieceType.Turm) {
                        //kapsle auch hier aus denselben Gruenden
                        matt = this.CheckMattForTurm(figureAttacksVertical, posKingY, posKingX, figureAttacksHorizontal, attackFigure);
                    }
                    //Da hier alles spiegelverkehrt ist, kann der bisherige Algorithmus nicht mehr verwendet werden
                    Figures2AvoidMatt = this.GetXYofAllFigures(color);
                    if (matt) {
                        int dummyX = x; int dummyY = y; bool doLeaveLoop = false;
                        foreach (var item in Figures2AvoidMatt) {
                            if (doLeaveLoop) break;
                            if (item.Item1 != null) {
                                if (attackFigure.Type == PieceType.Laeufer || attackFigure.Type == PieceType.Dame) {
                                    //fuelle 2 generische Listen mit den Koordinaten der Dame bzw. Laeufer(Diagonaler Angriff)
                                    if (positionX.Count == 0 && positionY.Count == 0) {
                                        this.CreateDiagonalAttackCoordinates(dummyX, posKingX, dummyY, posKingY, attackFigure);
                                    }
                                    /*  ++++++++++++++++++Blockieren++++++++++++++++++
                                        Da hier alles spiegelverkehrt ist, muss der bisherige Algorithmus geringfuegig abgeaendert werden
                                    */
                                    if (item.Item1.Type == PieceType.Bauer) {
                                        //  Da ein Bauer nicht rueckwaerts ziehen kann entfallen die Pruefungen auf dummy<item.Item3. Diagonaler Angriff
                                        if (!figureAttacksHorizontal && !figureAttacksVertical && leftOfKing && item.Item2 > x) {
                                            if (!figureAttacksHorizontal && !figureAttacksVertical) {
                                                while (dummyX <= item.Item2 && dummyY <= item.Item3) {
                                                    dummyX++;
                                                    dummyY++;
                                                    if (dummyX == item.Item2 && dummyY == item.Item3 - 1 || dummyX == item.Item2 && dummyY == item.Item3 - 2) {
                                                        matt = false;
                                                        doLeaveLoop = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            dummyX = x; dummyY = y;
                                        } else if (!figureAttacksVertical && !figureAttacksHorizontal && rightOfKing && item.Item2 < x) {
                                            while (dummyX >= item.Item2 && dummyY <= item.Item3) {
                                                dummyX--;
                                                dummyY++;
                                                if (dummyX == item.Item2 && dummyY == item.Item3 - 1 || dummyX == item.Item2 && dummyY == item.Item3 - 2) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                            }

                                        } else if (figureAttacksVertical) {
                                            //Tue nix, da sich ein Bauer einem vertikalen Angriff niemals blockierend dazwischen stellen kann
                                        } else if (figureAttacksHorizontal) {
                                            if (positionX.Contains(item.Item2) && positionY.Contains(item.Item3 - 1) || positionX.Contains(item.Item2) && positionY.Contains(item.Item3 - 2)) {
                                                matt = false;
                                                break;
                                            }
                                        }
                                        dummyX = x; dummyY = y;
                                    } else if (item.Item1.Type == PieceType.Springer) {
                                        /*  Derselbe Code wie fuer das weisse Pendant. Da sich die Komponenten innerhalb einer Schleife befinden, kann nix gekapselt werden.
                                            Dieser Code ist also redundant*/
                                        if (positionX.Contains(item.Item2 - 2) && positionY.Contains(item.Item3 - 1) ||
                                            positionX.Contains(item.Item2 - 2) && positionY.Contains(item.Item3 + 1) ||
                                            positionX.Contains(item.Item2 + 2) && positionY.Contains(item.Item3 - 1) ||
                                            positionX.Contains(item.Item2 + 2) && positionY.Contains(item.Item3 + 1) ||
                                            positionX.Contains(item.Item2 - 1) && positionY.Contains(item.Item3 - 2) ||
                                            positionX.Contains(item.Item2 - 1) && positionY.Contains(item.Item3 + 2) ||
                                            positionX.Contains(item.Item2 + 1) && positionY.Contains(item.Item3 - 2) ||
                                            positionX.Contains(item.Item2 + 1) && positionY.Contains(item.Item3 + 2)
                                            ) {
                                            matt = false;
                                            break;
                                        }
                                    } else if (item.Item1.Type == PieceType.Turm || item.Item1.Type == PieceType.Dame) {
                                        /*  Derselbe Code wie fuer das weisse Pendant. Da sich die Komponenten innerhalb einer Schleife befinden, kann nix gekapselt werden.
                                            Dieser Code ist also redundant */
                                        int defenseFigureChangedX = item.Item2, defenseFigureChangedY = item.Item3;
                                        //Ueberpruefe Horizontalitaet
                                        while (iterationen <= grenze) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                positionX.Clear();
                                                positionY.Clear();
                                                doLeaveLoop = true;
                                                break;
                                            } else
                                                defenseFigureChangedX--;
                                            if (defenseFigureChangedX < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedX = item.Item2;
                                        while (iterationen <= grenze) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                positionX.Clear();
                                                positionY.Clear();
                                                doLeaveLoop = true;
                                                break;
                                            } else
                                                defenseFigureChangedX++;
                                            if (defenseFigureChangedX > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        defenseFigureChangedX = item.Item2;
                                        //Ueberpruefe Vertikalitaet
                                        iterationen = 0; defenseFigureChangedX = item.Item2;
                                        while (iterationen <= grenze) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                positionX.Clear();
                                                positionY.Clear();
                                                doLeaveLoop = true;
                                                break;
                                            } else
                                                defenseFigureChangedY--;
                                            if (defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedY = item.Item3;
                                        while (iterationen <= grenze) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                positionX.Clear();
                                                positionY.Clear();
                                                doLeaveLoop = true;
                                                break;
                                            } else
                                                defenseFigureChangedY++;
                                            if (defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                    }
                                    /*  ToDo:ueberpruefe noch die diagonale Blockadefaehigkeit der Dame. Die horizontale und vertikale wurde oben durch den Turm bereits 
                                        ueberprueft=>Pruefe Dame und Laeufer */
                                    if ((item.Item1.Type == PieceType.Dame || item.Item1.Type == PieceType.Laeufer) && matt) {
                                        /*  Derselbe Code wie fuer das weisse Pendant. Da sich die Komponenten innerhalb einer Schleife befinden, kann nix gekapselt werden.
                                            Dieser Code ist also redundant */
                                        int defenseFigureChangedX = item.Item2;
                                        int defenseFigureChangedY = item.Item3;
                                        bool leaveLoop = false;
                                        iterationen = 0;
                                        while (iterationen < grenze && !leaveLoop) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                leaveLoop = true;
                                                doLeaveLoop = true;
                                                positionX.Clear();
                                                positionY.Clear();
                                                break;
                                            } else {
                                                defenseFigureChangedX++;
                                                defenseFigureChangedY++;
                                            }
                                            if (defenseFigureChangedX > 7 || defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3; iterationen = 0;
                                        while (iterationen < grenze && !leaveLoop) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                leaveLoop = true;
                                                doLeaveLoop = true;
                                                positionX.Clear();
                                                positionY.Clear();
                                                break;
                                            } else {
                                                defenseFigureChangedX--;
                                                defenseFigureChangedY--;
                                            }
                                            if (defenseFigureChangedX < 0 || defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3; iterationen = 0;
                                        while (iterationen < grenze && !leaveLoop) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                leaveLoop = true;
                                                doLeaveLoop = true;
                                                positionX.Clear();
                                                positionY.Clear();
                                                break;
                                            } else {
                                                defenseFigureChangedX++;
                                                defenseFigureChangedY--;
                                            }
                                            if (defenseFigureChangedX > 7 || defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3; iterationen = 0;
                                        while (iterationen < grenze && !leaveLoop) {
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                leaveLoop = true;
                                                doLeaveLoop = true;
                                                positionX.Clear();
                                                positionY.Clear();
                                                break;
                                            } else {
                                                defenseFigureChangedX--;
                                                defenseFigureChangedY++;
                                            }
                                            if (defenseFigureChangedX < 0 || defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }

                                    }
                                    /*  Derselbe Code wie fuer das weisse Pendant. Da sich die Komponenten innerhalb einer Schleife befinden, kann nix gekapselt werden. 
                                        Dieser Code ist also redundant. Es gibt keine Figuren, die sich zwischen einen Springer-oder Bauernangriff stellen koennen */
                                } else if (attackFigure.Type == PieceType.Springer || attackFigure.Type == PieceType.Bauer) {

                                } else if (attackFigure.Type == PieceType.Turm) {
                                    //erstelle die beiden generische Listen durch eine gekapselte Methode, diesesmal mit den Koordinaten des Tums
                                    dummyX = x; dummyY = y;
                                    if (positionX.Count == 0 && positionY.Count == 0) {
                                        this.FillListTurm(figureAttacksHorizontal, figureAttacksVertical, dummyX, posKingX, dummyY, x, y, posKingY);
                                    }
                                    int defenseFigureChangedX = item.Item2;
                                    int defenseFigureChangedY = item.Item3;
                                    /*  ToDo: Je nach geographischer Angriffsart muss mittels geeigneter Algorithmen ueberprueft werden, ob sich eine Figur zwischen Turm 
                                        und Koenig stellen kann. Falls ja, kein Matt. Ein Bauer kann nur dann blockieren, sofern horizontal angegriffen wird
                                    */
                                    if (item.Item1.Type == PieceType.Bauer && figureAttacksHorizontal) {
                                        if (defenseFigureChangedX == x && defenseFigureChangedY + 1 == y || (defenseFigureChangedX == x && defenseFigureChangedY + 2 == y)) {
                                            matt = false;
                                            break;
                                        }
                                    } else if (item.Item1.Type == PieceType.Springer) {
                                        if (positionX.Contains(item.Item2 - 2) && positionY.Contains(item.Item3 - 1) ||
                                          positionX.Contains(item.Item2 - 2) && positionY.Contains(item.Item3 + 1) ||
                                          positionX.Contains(item.Item2 + 2) && positionY.Contains(item.Item3 - 1) ||
                                          positionX.Contains(item.Item2 + 2) && positionY.Contains(item.Item3 + 1) ||
                                          positionX.Contains(item.Item2 - 1) && positionY.Contains(item.Item3 - 2) ||
                                          positionX.Contains(item.Item2 - 1) && positionY.Contains(item.Item3 + 2) ||
                                          positionX.Contains(item.Item2 + 1) && positionY.Contains(item.Item3 - 2) ||
                                          positionX.Contains(item.Item2 + 1) && positionY.Contains(item.Item3 + 2)
                                      ) {
                                            matt = false;
                                            doLeaveLoop = true;
                                            break;
                                        }
                                    } else if (item.Item1.Type == PieceType.Laeufer || item.Item1.Type == PieceType.Dame) {
                                        iterationen = 0;
                                        while (iterationen < grenze) {
                                            defenseFigureChangedX++;
                                            defenseFigureChangedY++;
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                doLeaveLoop = true;
                                                break;
                                            }
                                            if (defenseFigureChangedX > 7 || defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                        while (iterationen < grenze) {
                                            defenseFigureChangedX--;
                                            defenseFigureChangedY--;
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                doLeaveLoop = true;
                                                break;
                                            }
                                            if (defenseFigureChangedX < 0 || defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                        while (iterationen < grenze) {
                                            defenseFigureChangedX++;
                                            defenseFigureChangedY--;
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                doLeaveLoop = true;
                                                break;
                                            }
                                            if (defenseFigureChangedX > 7 || defenseFigureChangedY < 0) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                        iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                        while (iterationen < grenze) {
                                            defenseFigureChangedX--;
                                            defenseFigureChangedY++;
                                            if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                matt = false;
                                                doLeaveLoop = true;
                                                break;
                                            }
                                            if (defenseFigureChangedX < 0 || defenseFigureChangedY > 7) break;
                                            if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                            iterationen++;
                                        }
                                    }
                                    //Pruefe abschliessend noch auf Blockadefaehigkeit von Turm und horizontalen/vertikalen Damenzuegen
                                    if (item.Item1.Type == PieceType.Turm || item.Item1.Type == PieceType.Dame) {
                                        iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                        if (figureAttacksHorizontal && !figureAttacksVertical) {
                                            while (iterationen < grenze) {
                                                defenseFigureChangedX++;
                                                if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                                if (defenseFigureChangedX > 7) break;
                                                if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                                iterationen++;
                                            }
                                            iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                            while (iterationen < grenze) {
                                                defenseFigureChangedX--;
                                                if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                                if (defenseFigureChangedX < 0) break;
                                                if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                                iterationen++;
                                            }
                                        } else if (!figureAttacksHorizontal && figureAttacksVertical) {
                                            while (iterationen < grenze) {
                                                defenseFigureChangedY--;
                                                if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                                if (defenseFigureChangedY < 0) break;
                                                if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                                iterationen++;
                                            }
                                            iterationen = 0; defenseFigureChangedX = item.Item2; defenseFigureChangedY = item.Item3;
                                            while (iterationen < grenze) {
                                                defenseFigureChangedY++;
                                                if (positionX.Contains(defenseFigureChangedX) && positionY.Contains(defenseFigureChangedY)) {
                                                    matt = false;
                                                    doLeaveLoop = true;
                                                    break;
                                                }
                                                if (defenseFigureChangedY > 7) break;
                                                if (this.GetPiece(defenseFigureChangedX, defenseFigureChangedY) != null) break;
                                                iterationen++;
                                            }
                                        }
                                    }
                                    /*          +++++++++++++++++++++++++++++ Schlagen+++++++++++++++++++++++++++++ */
                                    if (this.FigureCanDeleteAttackFigure(item.Item1, item.Item2, item.Item3, x, y)) {
                                        matt = false;
                                        doLeaveLoop = true;
                                        break;
                                    }
                                }
                            }
                            //pruefe, ob die schachgebende Figur vom Koenig geschlagen werden kann
                            if (Math.Abs(posKingX - x) == 1 && Math.Abs(posKingY - y) == 1)
                                matt = false;
                        }
                    }
                }
            }
            return matt;
        }
        #endregion

        #region GetXYofAllFigures:Eruiere die X und Y Koordinaten aller Figuren, sofern Schach gegeben wurde
        private List<Tuple<Piece, int, int>> GetXYofAllFigures(PieceColor color) {
            List<Tuple<Piece, int, int>> Figures = new List<Tuple<Piece, int, int>>();
            int counterS = 0, counterL = 0, counterT = 0, counterB = 0;
            //da eine weisse Figur angreift, werden alle schwarzen Figuren benoetigt.
            if (color == PieceColor.Black) {
                //B1 bedeutet: erste schwarze Figur, B2 bedeutet: zweite schwarze Figur...B8 bedeutet achte schwarze Figur
                int posDameB = 0;
                int posSpringerB1 = 0, posSpringerB2 = 0, posLaeuferB1 = 0, posLaeuferB2 = 0, posTurmB1 = 0, posTurmB2 = 0;
                int posBauerB1 = 0, posBauerB2 = 0, posBauerB3 = 0, posBauerB4 = 0, posBauerB5 = 0, posBauerB6 = 0, posBauerB7 = 0, posBauerB8 = 0;
                Piece figureSpringerB1 = null, figureSpringerB2 = null, figureLaeuferB1 = null, figureLaeuferB2 = null, figureTurmB1 = null, figureTurmB2 = null;
                Piece figureDameB = null;
                Piece figureBauerB1 = null, figureBauerB2 = null, figureBauerB3 = null, figureBauerB4 = null, figureBauerB5 = null, figureBauerB6 = null, figureBauerB7 = null, figureBauerB8 = null;
                //iteriere ueber die globale generische Tupelliste und weise die gefundene Koordinate und Figur den jeweiligen Variablen zu
                foreach (var item in actualFigures) {
                    if (counterS == 0 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Springer) {
                        counterS++;
                        posSpringerB1 = item.Item2;
                        figureSpringerB1 = item.Item1;
                    } else if (counterS == 1 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Springer) {
                        posSpringerB2 = item.Item2;
                        figureSpringerB2 = item.Item1;
                    } else if (counterL == 0 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Laeufer) {
                        counterL++;
                        posLaeuferB1 = item.Item2;
                        figureLaeuferB1 = item.Item1;
                    } else if (counterS == 1 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Laeufer) {
                        posLaeuferB2 = item.Item2;
                        figureLaeuferB2 = item.Item1;
                    } else if (counterT == 0 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Turm) {
                        counterT++;
                        posTurmB1 = item.Item2;
                        figureTurmB1 = item.Item1;
                    } else if (counterT == 1 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Turm) {
                        posTurmB2 = item.Item2;
                        figureTurmB2 = item.Item1;
                    } else if (item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Dame) {
                        posDameB = item.Item2;
                        figureDameB = item.Item1;
                    } else if (counterB == 0 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerB1 = item.Item2;
                        figureBauerB1 = item.Item1;
                    } else if (counterB == 1 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerB2 = item.Item2;
                        figureBauerB2 = item.Item1;
                    } else if (counterB == 2 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerB3 = item.Item2;
                        figureBauerB3 = item.Item1;
                    } else if (counterB == 3 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerB4 = item.Item2;
                        figureBauerB4 = item.Item1;
                    } else if (counterB == 4 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerB5 = item.Item2;
                        figureBauerB5 = item.Item1;
                    } else if (counterB == 5 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerB6 = item.Item2;
                        figureBauerB6 = item.Item1;
                    } else if (counterB == 6 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerB7 = item.Item2;
                        figureBauerB7 = item.Item1;
                    } else if (counterB == 7 && item.Item1 != null && item.Item1.Color == PieceColor.Black && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerB8 = item.Item2;
                        figureBauerB8 = item.Item1;
                    }
                }
                //Fuelle die lokale generische Tupelliste mit der jeweiligen Figur und deren beiden Koordinaten. Erster schwarzer Bauer 
                Figures = this.FillListAllFigures(posBauerB1, figureBauerB1, Figures);
                //zweiter schwarzer Bauer
                Figures = this.FillListAllFigures(posBauerB2, figureBauerB2, Figures);
                //dritter schwarzer Bauer
                Figures = this.FillListAllFigures(posBauerB3, figureBauerB3, Figures);
                //vierter schwarzer Bauer
                Figures = this.FillListAllFigures(posBauerB4, figureBauerB4, Figures);
                //fuenfter schwarzer Bauer
                Figures = this.FillListAllFigures(posBauerB5, figureBauerB5, Figures);
                //sechster schwarzer Bauer
                Figures = this.FillListAllFigures(posBauerB6, figureBauerB6, Figures);
                //siebter schwarzer Bauer
                Figures = this.FillListAllFigures(posBauerB7, figureBauerB7, Figures);
                //achter schwarzer Bauer
                Figures = this.FillListAllFigures(posBauerB8, figureBauerB8, Figures);
                //erster schwarzer Turm
                Figures = this.FillListAllFigures(posTurmB1, figureTurmB1, Figures);
                //erster schwarzer Springer
                Figures = this.FillListAllFigures(posSpringerB1, figureSpringerB1, Figures);
                //erster schwarzer Laeufer
                Figures = this.FillListAllFigures(posLaeuferB1, figureLaeuferB1, Figures);
                //schwarze Dame
                Figures = this.FillListAllFigures(posDameB, figureDameB, Figures);
                //zweiter schwarzer Laeufer
                Figures = this.FillListAllFigures(posLaeuferB2, figureLaeuferB2, Figures);
                //zweiter schwarzer Springer
                Figures = this.FillListAllFigures(posSpringerB2, figureSpringerB2, Figures);
                //zweiter schwarzer Turm
                Figures = this.FillListAllFigures(posTurmB2, figureTurmB2, Figures);
            } else if (color == PieceColor.White) {
                //W1 bedeutet: erste weisse Figur, W2 bedeutet: zweite weisse Figur...
                int posDameW = 0;
                int posSpringerW1 = 0, posSpringerW2 = 0, posLaeuferW1 = 0, posLaeuferW2 = 0, posTurmW1 = 0, posTurmW2 = 0;
                int posBauerW1 = 0, posBauerW2 = 0, posBauerW3 = 0, posBauerW4 = 0, posBauerW5 = 0, posBauerW6 = 0, posBauerW7 = 0, posBauerW8 = 0;
                Piece figureSpringerW1 = null, figureSpringerW2 = null, figureLaeuferW1 = null, figureLaeuferW2 = null, figureTurmW1 = null, figureTurmW2 = null;
                Piece figureDameW = null;
                Piece figureBauerW1 = null, figureBauerW2 = null, figureBauerW3 = null, figureBauerW4 = null, figureBauerW5 = null, figureBauerW6 = null, figureBauerW7 = null, figureBauerW8 = null;
                //da eine schwarze Figur angreift, werden alle weissen Figuren benoetigt.
                foreach (var item in actualFigures) {
                    if (counterS == 0 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Springer) {
                        counterS++;
                        posSpringerW1 = item.Item2;
                        figureSpringerW1 = item.Item1;
                    } else if (counterS == 1 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Springer) {
                        posSpringerW2 = item.Item2;
                        figureSpringerW2 = item.Item1;
                    } else if (counterL == 0 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Laeufer) {
                        counterL++;
                        posLaeuferW1 = item.Item2;
                        figureLaeuferW1 = item.Item1;
                    } else if (counterL == 1 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Laeufer) {
                        posLaeuferW2 = item.Item2;
                        figureLaeuferW2 = item.Item1;
                    } else if (counterT == 0 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Turm) {
                        counterT++;
                        posTurmW1 = item.Item2;
                        figureTurmW1 = item.Item1;
                    } else if (counterT == 1 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Turm) {
                        posTurmW2 = item.Item2;
                        figureTurmW2 = item.Item1;
                    } else if (item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Dame) {
                        posDameW = item.Item2;
                        figureDameW = item.Item1;
                    } else if (counterB == 0 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerW1 = item.Item2;
                        figureBauerW1 = item.Item1;
                    } else if (counterB == 1 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerW2 = item.Item2;
                        figureBauerW2 = item.Item1;
                    } else if (counterB == 2 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerW3 = item.Item2;
                        figureBauerW3 = item.Item1;
                    } else if (counterB == 3 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerW4 = item.Item2;
                        figureBauerW4 = item.Item1;
                    } else if (counterB == 4 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerW5 = item.Item2;
                        figureBauerW5 = item.Item1;
                    } else if (counterB == 5 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerW6 = item.Item2;
                        figureBauerW6 = item.Item1;
                    } else if (counterB == 6 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerW7 = item.Item2;
                        figureBauerW7 = item.Item1;
                    } else if (counterB == 7 && item.Item1 != null && item.Item1.Color == PieceColor.White && item.Item1.Type == PieceType.Bauer) {
                        counterB++;
                        posBauerW8 = item.Item2;
                        figureBauerW8 = item.Item1;
                    }
                }
                //Fuelle die lokale generische Tupelliste mit der jeweiligen Figur und deren beiden Koordinaten. Erster weisser Bauer 
                Figures = this.FillListAllFigures(posBauerW1, figureBauerW1, Figures);
                //zweiter weisser Bauer
                Figures = this.FillListAllFigures(posBauerW2, figureBauerW2, Figures);
                //dritter weisser Bauer
                Figures = this.FillListAllFigures(posBauerW3, figureBauerW3, Figures);
                //vierter weisser Bauer
                Figures = this.FillListAllFigures(posBauerW4, figureBauerW4, Figures);
                //fuenfter weisser Bauer
                Figures = this.FillListAllFigures(posBauerW5, figureBauerW5, Figures);
                //sechster weisser Bauer
                Figures = this.FillListAllFigures(posBauerW6, figureBauerW6, Figures);
                //siebter weisser Bauer
                Figures = this.FillListAllFigures(posBauerW7, figureBauerW7, Figures);
                //achter weisser Bauer
                Figures = this.FillListAllFigures(posBauerW8, figureBauerW8, Figures);
                //erster weisser Turm
                Figures = this.FillListAllFigures(posTurmW1, figureTurmW1, Figures);
                //erster weisser Springer
                Figures = this.FillListAllFigures(posSpringerW1, figureSpringerW1, Figures);
                //erster weisser Laeufer
                Figures = this.FillListAllFigures(posLaeuferW1, figureLaeuferW1, Figures);
                //schwarze Dame
                Figures = this.FillListAllFigures(posDameW, figureDameW, Figures);
                //zweiter weisser Laeufer
                Figures = this.FillListAllFigures(posLaeuferW2, figureLaeuferW2, Figures);
                //zweiter weisser Springer
                Figures = this.FillListAllFigures(posSpringerW2, figureSpringerW2, Figures);
                //zweiter weisser Turm
                Figures = this.FillListAllFigures(posTurmW2, figureTurmW2, Figures);
            }
            return Figures;
        }
        #endregion

        #region generische Figurenliste beider Farben fuellen
        private List<Tuple<Piece, int, int>> FillListAllFigures(int posFigure, Piece Figure, List<Tuple<Piece, int, int>> Figures) {
            const int divisor = 8;
            string figurenFarbe = String.Empty, FigurenFarbe = String.Empty;
            int counter = 1, result, yCoordinateFigure, xCoordinateFigure;
            /*  fuer posFigure:X=59
                59=y*8+x =>Pruefe, wieviel mal die Konstante divisor multipliziert werden kann, bis das Ergebnis nicht groesser als posFigure ist. Das Ergebnis
                liefert die Y - Koordinate der Figur. Von posFigure subtrahiere man die Y - Koordinate, multipliziert mit divisor und man 
                erhaelt die X - Koordinate. Diese mathematische Konstelation verwende man in einer fussgesteuerten Schleife
            */
            do {
                result = divisor * counter;
                counter++;
            } while (result <= posFigure);
            yCoordinateFigure = counter - 2;
            xCoordinateFigure = Math.Abs(posFigure - divisor * yCoordinateFigure);
            //ueberpruefe, ob Berechnungen stimmig sind
            Piece figure = this.GetPiece(xCoordinateFigure, yCoordinateFigure);
            if (figure != null && figure.Color == PieceColor.White)
                figurenFarbe = "weissen";
            else if (figure != null && figure.Color == PieceColor.Black)
                figurenFarbe = "schwarzen";
            if (Figure != null && Figure.Color == PieceColor.White)
                FigurenFarbe = "weissen";
            else if (Figure != null && Figure.Color == PieceColor.Black)
                FigurenFarbe = "schwarzen";
            if (figure != Figure && figure != null && Figure != null) {
                string m = "Errechnete Koordinaten X=" + xCoordinateFigure + ";Y=" + yCoordinateFigure + " entsprechen nicht der/dem " + FigurenFarbe + " " + Figure.Type + ", sondern der/dem " + figurenFarbe + " " + figure.Type + " auf dem Brett";
                _logger.Warn(m);
                _logger.Info("Fuege NULL anstatt {0} {1} der Liste bei", FigurenFarbe, Figure.Type);
                /*SchachGUI objectAusgabe = new SchachGUI();
                objectAusgabe.Ausgabe("Fuege NULL anstatt " + FigurenFarbe + " " + Figure.Type + " der Liste bei", "Warnung", System.Windows.Forms.MessageBoxIcon.Exclamation); */
                Figures.Add(new Tuple<Piece, int, int>(null, xCoordinateFigure, yCoordinateFigure));
            } else
                Figures.Add(new Tuple<Piece, int, int>(Figure, xCoordinateFigure, yCoordinateFigure));
            return Figures;
        }
        #endregion

        #region generische Figurenliste fuer Turmangriff beider Farben fuellen
        private void FillListTurm(bool figureAttacksHorizontal, bool figureAttacksVertical, int dummyX, int posKingX, int dummyY, int x, int y, int posKingY) {
            if (positionX.Count > 0) positionX.Clear();
            if (positionY.Count > 0) positionY.Clear();
            if (figureAttacksHorizontal && !figureAttacksVertical) {
                while (dummyX < posKingX && dummyY == posKingY) {
                    dummyX++;
                    if (dummyX == posKingX) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = x; dummyY = y;
                while (dummyX > posKingX && dummyY == posKingY) {
                    dummyX--;
                    if (dummyX == posKingX) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                //Vertikaler Angriff
            } else if (!figureAttacksHorizontal && figureAttacksVertical) {
                while (dummyX == posKingX && dummyY < posKingY) {
                    dummyY++;
                    if (dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = x; dummyY = y;
                while (dummyX == posKingX && dummyY > posKingY) {
                    dummyY--;
                    if (dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
            }

        }

        #endregion

        #region Pruefe Ausweichmoeglichkeiten des weissen und schwarzen Koenigs auf Damen-oder Laeuferangriff
        private bool CheckMattForDameOrLaeufer(bool figureAttacksHorizontal, bool figureAttacksVertical, bool leftOfKing, bool rightOfKing, bool frontOfKing, bool behindOfKing, int posKingX, int posKingY, int figureX, int figureY, Piece figure) {
            bool matt = true;
            //die Methode AdditionalFigureCreatesCheck prueft, ob das Ausweichfeld des Koenigs unter Schach steht. Falls ja, ist dieses Feld keine Option
            if (figureAttacksHorizontal && Math.Abs(posKingX - figureX) > 2) {
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null
                    /* Diese beiden Felder duerfen nicht ueberprueft werden, da sie im horizontalen Schachbereich der angreifenden Figur liegen
                    posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                    posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null */
                    )
                    matt = false;
            } else if (figureAttacksHorizontal && Math.Abs(posKingX - figureX) <= 2 && leftOfKing) {
                //if (posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                   //posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null
                   //posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                   //posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
                   )
                    matt = false;
            } else if (figureAttacksHorizontal && Math.Abs(posKingX - figureX) <= 2 && rightOfKing) {
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                    //posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                    //posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null
                   //posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                   //posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
                   )
                    matt = false;
            } else if (figureAttacksVertical && Math.Abs(posKingY - figureY) > 2) {
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                   /* Diese beiden Felder duerfen nicht ueberprueft werden, da sie im vertikalen Schachbereich der angreifenden Figur liegen
                   posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                   posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null || */
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
                    )
                    matt = false;
            } else if (figureAttacksVertical && Math.Abs(posKingY - figureY) <= 2 && frontOfKing) {
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                  //posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                  !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                  //posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                  //posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                  //posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null ||
                  !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                  !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
                   )
                    matt = false;
            } else if (figureAttacksVertical && Math.Abs(posKingY - figureY) <= 2 && behindOfKing) {
                //if (posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                //posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                //posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                //posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null ||
                !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
               )
                    matt = false;
            } else if (leftOfKing && !figureAttacksVertical && !figureAttacksHorizontal && figure.Color == PieceColor.White) {
                //die folgenden beiden Konditionen koennen nicht mehr gekapselt werden, da sie sich vom Angriff auf den weissen Koenig unterscheiden
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                   /* Diese beiden Felder duerfen nicht ueberprueft werden, da sie im diagonalen Schachbereich der angreifenden Figur liegen
                       posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                       posKingX - 1 >= 0 && posKingY+1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null || */
                   !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
              )
                    matt = false;
            } else if (rightOfKing && !figureAttacksVertical && !figureAttacksHorizontal && figure.Color == PieceColor.White) {
                /* Diese beiden Felder duerfen nicht ueberprueft werden, da sie im diagonalen Schachbereich der angreifenden Figur liegen
                if (posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                    posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null || */
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
                 )
                    matt = false;
            } else if (leftOfKing && !figureAttacksVertical && !figureAttacksHorizontal && figure.Color == PieceColor.Black) {
                //die folgenden beiden Konditionen koennen nicht mehr gekapselt werden, da sie sich vom Angriff auf den weissen Koenig unterscheiden
                /*if (posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                    posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||  */
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null ||
                !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
           )
                    matt = false;
            } else if (rightOfKing && !figureAttacksVertical && !figureAttacksHorizontal && figure.Color == PieceColor.Black) {
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null ||
                   //posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                   //posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
                 )
                    matt = false;
            }
            return matt;
        }
        #endregion

        #region  Pruefe Ausweichmoeglichkeiten des weissen und schwarzen Koenigs auf Springer-oder Bauernangriff
        /*   Ist die Angriffsfigur ein Springer, gibt es keinerlei Einschraenkungen bzgl. der Ausweichmoeglichkeit, da der Koenig von jedem Standpunkt unmoeglich in einen 
             anderen Schachbereich des Springers ziehen kann. Es werden folglich alle 8 Moeglichkeiten geprueft. Die Methode AdditionalFigureCreatesCheck ueberprueft, ob
             der Koenig in ein anderes Schach laeuft. Falls ja, ist dieser Zug keine Option*/
        private bool CheckMattForSpringerOrBauer(int posKingY, int posKingX, Piece figure) {
            bool matt = true;
            if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
               !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
               !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, Math.Abs(posKingY - 1)) == null ||
               !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(Math.Abs(posKingX - 1), posKingY + 1) == null ||
               !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
               !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null ||
               !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
               !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
                )
                matt = false;
            return matt;
        }

        #endregion

        #region Pruefe Ausweichmoeglichkeiten des weissen und schwarzen Koenigs auf Turmangriff
        private bool CheckMattForTurm(bool figureAttacksVertical, int posKingY, int posKingX, bool figureAttacksHorizontal, Piece figure) {
            bool matt = true;
            if (figureAttacksVertical) {
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingX + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                   //posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                   //posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY) && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                   !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY) && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null
                    )
                    matt = false;
            } else if (figureAttacksHorizontal) {
                if (!this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY - 1) && posKingY - 1 >= 0 && posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY + 1) && posKingY + 1 < 8 && posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX + 1, posKingY - 1) && posKingX + 1 < 8 && posKingY - 1 >= 0 && this.GetPiece(posKingX + 1, posKingY - 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX - 1, posKingY + 1) && posKingX - 1 >= 0 && posKingY + 1 < 8 && this.GetPiece(posKingX - 1, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY + 1) && posKingY + 1 < 8 && this.GetPiece(posKingX, posKingY + 1) == null ||
                    !this.AdditionalFigureCreatesCheck(figure, posKingX, posKingY - 1) && posKingY - 1 >= 0 && this.GetPiece(posKingX, posKingY - 1) == null
                   /*  Diese beiden Felder duerfen nicht ueberprueft werden, da sie im horizontalen Schachbereich des weissen Turms liegen
                       posKingX + 1 < 8 && this.GetPiece(posKingX + 1, posKingY) == null ||
                       posKingX - 1 >= 0 && this.GetPiece(posKingX - 1, posKingY) == null */
                   )
                    matt = false;
            }
            return matt;
        }
        #endregion

        #region Erstelle generische Liste: Diagonale Positionsdaten der angreifenden Figur
        private void CreateDiagonalAttackCoordinates(int dummyX, int posKingX, int dummyY, int posKingY, Piece piece) {
            int originX = dummyX, originY = dummyY;
            //Diagonaler Angriff
            if (piece.Type == PieceType.Dame) {
                while (dummyX < posKingX && dummyY > posKingY) {
                    dummyX++;
                    dummyY--;
                    if (dummyX == posKingX || dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = originX; dummyY = originY;
                while (dummyX < posKingX && dummyY < posKingY) {
                    dummyX++;
                    dummyY++;
                    if (dummyX == posKingX || dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = originX; dummyY = originY;
                while (dummyX > posKingX && dummyY < posKingY) {
                    dummyX--;
                    dummyY++;
                    if (dummyX == posKingX || dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = originX; dummyY = originY;
                while (dummyX > posKingX && dummyY > posKingY) {
                    dummyX--;
                    dummyY--;
                    if (dummyX == posKingX || dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                //Vertikaler Angriff
                dummyX = originX; dummyY = originY;
                while (dummyX == posKingX && dummyY < posKingY) {
                    dummyY++;
                    if (dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = originX; dummyY = originY;
                while (dummyX == posKingX && dummyY > posKingY) {
                    dummyY--;
                    if (dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                //Horizontaler Angriff Angriff
                dummyX = originX; dummyY = originY;
                while (dummyX < posKingX && dummyY == posKingY) {
                    dummyX++;
                    if (dummyX == posKingX) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = originX; dummyY = originY;
                while (dummyX > posKingX && dummyY == posKingY) {
                    dummyX--;
                    if (dummyX == posKingX) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
            } else if (piece.Type == PieceType.Laeufer) {
                //Nur Diagonaler Angriff
                while (dummyX < posKingX && dummyY > posKingY) {
                    dummyX++;
                    dummyY--;
                    if (dummyX == posKingX || dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = originX; dummyY = originY;
                while (dummyX < posKingX && dummyY < posKingY) {
                    dummyX++;
                    dummyY++;
                    if (dummyX == posKingX || dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = originX; dummyY = originY;
                while (dummyX > posKingX && dummyY < posKingY) {
                    dummyX--;
                    dummyY++;
                    if (dummyX == posKingX || dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
                dummyX = originX; dummyY = originY;
                while (dummyX > posKingX && dummyY > posKingY) {
                    dummyX--;
                    dummyY--;
                    if (dummyX == posKingX || dummyY == posKingY) break;
                    positionX.Add(dummyX);
                    positionY.Add(dummyY);
                }
            }
        }
        #endregion

        #region Erstelle Tuppelliste gemaess der uebergebenen Parameter
        private Tuple<List<int>, List<int>> CreatePositionList(int x, int y, int posKingX, int posKingY, List<int> posX, List<int> posY, Piece piece) {
            int originX = x, originY = y;
            if (posX.Count > 0 && posY.Count > 0) {
                posX.Clear();
                posY.Clear();
            }
            //sofern illegitime Koordianten uebergeben werden, gebe leere Listen zurueck
            if (x < 0 || y < 0 || x > 7 || y > 7)
                return Tuple.Create(posX, posY);

            /*  Erstelle alle Koordinaten zwischen der Figur und dem Koenig anhand der uebergebenen Parameter. Unterscheide zwischen diagonalen, horizontalen und vertikalen 
                Angriff
            */
            //Diagonaler Angriff
            if (piece != null && piece.Type == PieceType.Dame) {
                while (x < posKingX && y > posKingY) {
                    x++;
                    y--;
                    //if(x == posKingX || y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x < posKingX && y < posKingY) {
                    x++;
                    y++;
                    //if(x == posKingX || y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x > posKingX && y < posKingY) {
                    x--;
                    y++;
                    //if(x == posKingX || y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x > posKingX && y > posKingY) {
                    x--;
                    y--;
                    //if(x == posKingX || y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                //Vertikaler Angriff
                x = originX; y = originY;
                while (x == posKingX && y < posKingY) {
                    y++;
                    //if(y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x == posKingX && y > posKingY) {
                    y--;
                    //if(y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                //Horizontaler Angriff Angriff
                x = originX; y = originY;
                while (x < posKingX && y == posKingY) {
                    x++;
                    //if(x == posKingX) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x > posKingX && y == posKingY) {
                    x--;
                    if (x == posKingX) break;
                    posX.Add(x);
                    posY.Add(y);
                }
            } else if (piece != null && piece.Type == PieceType.Laeufer) {
                //Nur diagonaler Angriff
                while (x < posKingX && y > posKingY) {
                    x++;
                    y--;
                    //if(x == posKingX || y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x < posKingX && y < posKingY) {
                    x++;
                    y++;
                    //if(x == posKingX || y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x > posKingX && y < posKingY) {
                    x--;
                    y++;
                    //if(x == posKingX || y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x > posKingX && y > posKingY) {
                    x--;
                    y--;
                    //if(x == posKingX || y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
            } else if (piece != null && piece.Type == PieceType.Turm) {
                while (x < posKingX && y == posKingY) {
                    x++;
                    //if (x == posKingX) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x > posKingX && y == posKingY) {
                    x--;
                    //if(x == posKingX) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                //Vertikaler Angriff
                while (x == posKingX && y < posKingY) {
                    y++;
                    //if(y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
                x = originX; y = originY;
                while (x == posKingX && y > posKingY) {
                    y--;
                    //if(y == posKingY) break;
                    posX.Add(x);
                    posY.Add(y);
                }
            }
            return Tuple.Create(posX, posY);
        }
        #endregion

        #region Pruefe, ob zusaetzliche Figuren durch Schachgebote das Ziehen des Koenigs verhindern
        private bool AdditionalFigureCreatesCheck(Piece piece, int posKingX, int posKingY) {
            bool matt = false;
            if (posKingX + 1 > 7 || posKingY - 1 < 0)
                return true;
            List<Tuple<Piece, int, int>> AdditionalChess = this.GetXYofAllFigures(piece.Color);
            foreach (var item in AdditionalChess) {
                if (item.Item1 != null && item.Item1 != piece && this.kingStandsChecked(item.Item1, item.Item2, item.Item3, posKingX, posKingY))
                    matt = true;
            }
            return matt;
        }
        #endregion

        #region Pruefe auf Abzugsschach - Hauptalgorithmus
        private bool CalculateDiscoveredCheck(List<Tuple<Piece, int, int>> figuresGiveCheck, int xNeuKoenig, int yNeuKoenig, Piece queen, Piece laeufer, Piece turm) {
            List<int> lstDameX = new List<int>(); List<int> lstDameY = new List<int>(); List<int> lstLaeuferX = new List<int>(); List<int> lstLaeuferY = new List<int>(); List<int> lstTurmX = new List<int>(); List<int> lstTurmY = new List<int>();
            bool discoveredCheck = false, doLeaveLoop = false;
            int posDameX = -1, posDameY = -1, posLauefer1X = -1, posLaeufer1Y = -1, posLauefer2X = -1, posLaeufer2Y = -1, posTurm1X = -1, posTurm1Y = -1, posTurm2X = -1, posTurm2Y = -1;
            Tuple<List<int>, List<int>> liste;
            foreach (var item in figuresGiveCheck) {
                if (item.Item1 != null && item.Item1.Type == PieceType.Dame) {
                    queen = item.Item1; posDameX = item.Item2; posDameY = item.Item3;
                } else if (item.Item1 != null && item.Item1.Type == PieceType.Laeufer && posLauefer1X < 0 && posLaeufer1Y < 0) {
                    laeufer = item.Item1; posLauefer1X = item.Item2; posLaeufer1Y = item.Item3;
                } else if (item.Item1 != null && item.Item1.Type == PieceType.Laeufer && posLauefer2X < 0 && posLaeufer2Y < 0) {
                    laeufer = item.Item1; posLauefer2X = item.Item2; posLaeufer2Y = item.Item3;
                } else if (item.Item1 != null && item.Item1.Type == PieceType.Turm && posTurm1X < 0 && posTurm1Y < 0) {
                    turm = item.Item1; posTurm1X = item.Item2; posTurm1Y = item.Item3;
                } else if (item.Item1 != null && item.Item1.Type == PieceType.Turm && posTurm2X < 0 && posTurm2Y < 0) {
                    turm = item.Item1; posTurm2X = item.Item2; posTurm2Y = item.Item3;
                }
            }
            foreach (var item in figuresGiveCheck) {
                if (doLeaveLoop) break;
                if (item.Item1 != null && item.Item1.Type == PieceType.Bauer) {
                    if (item.Item2 - 1 == xNeuKoenig && item.Item3 - 1 == yNeuKoenig ||
                        item.Item2 - 1 == xNeuKoenig && item.Item3 + 1 == yNeuKoenig ||
                        item.Item2 + 1 == xNeuKoenig && item.Item3 - 1 == yNeuKoenig ||
                        item.Item2 + 1 == xNeuKoenig && item.Item3 + 1 == yNeuKoenig) {
                        discoveredCheck = true;
                        break;
                    }
                } else if (item.Item1 != null && item.Item1.Type == PieceType.Dame) {
                    //fuelle 2 generische Listen mit den Koordinaten der Dame
                    liste = this.CreatePositionList(posDameX, posDameY, xNeuKoenig, yNeuKoenig, lstDameX, lstDameY, queen);
                    lstDameX = liste.Item1; lstDameY = liste.Item2;
                    if (lstDameX.Contains(xNeuKoenig) && lstDameY.Contains(yNeuKoenig)) {
                        for (int i = 0; i < lstDameX.Count; i++) {
                            //sofern auf eine Figur getroffen wird, die nicht der Koenig ist, besteht kein Abzugsschach...
                            if (this.GetPiece(lstDameX[i], lstDameY[i]) != null && this.GetPiece(lstDameX[i], lstDameY[i]).Type != PieceType.Koenig) {
                                break;
                                //..andernfalls besteht ein Abzugschach, sofern auf den Koenig getroffen wird
                            } else if (this.GetPiece(lstDameX[i], lstDameY[i]) != null && this.GetPiece(lstDameX[i], lstDameY[i]).Type == PieceType.Koenig) {
                                discoveredCheck = true;
                                doLeaveLoop = true;
                            }
                        }
                    }
                } else if (item.Item1 != null && item.Item1.Type == PieceType.Laeufer) {
                    //fuelle 2 generische Listen mit den Koordinaten des Laeufers
                    liste = this.CreatePositionList(posLauefer1X, posLaeufer1Y, xNeuKoenig, yNeuKoenig, lstLaeuferX, lstLaeuferY, laeufer);
                    lstLaeuferX = liste.Item1; lstLaeuferY = liste.Item2;
                    if (lstLaeuferX.Contains(xNeuKoenig) && lstLaeuferY.Contains(yNeuKoenig)) {
                        for (int i = 0; i < lstLaeuferX.Count; i++) {
                            //sofern auf eine Figur getroffen wird, die nicht der Koenig ist, besteht kein Abzugsschach...
                            if (this.GetPiece(lstLaeuferX[i], lstLaeuferY[i]) != null && this.GetPiece(lstLaeuferX[i], lstLaeuferY[i]).Type != PieceType.Koenig)
                                break;
                            //..andernfalls besteht ein Abzugschach, sofern auf den Koenig getroffen wird
                            else if (this.GetPiece(lstLaeuferX[i], lstLaeuferY[i]) != null && this.GetPiece(lstLaeuferX[i], lstLaeuferY[i]).Type == PieceType.Koenig) {
                                discoveredCheck = true;
                                doLeaveLoop = true;
                            }
                        }
                    }
                    lstLaeuferX.Clear(); lstLaeuferY.Clear();
                    liste = this.CreatePositionList(posLauefer2X, posLaeufer2Y, xNeuKoenig, yNeuKoenig, lstLaeuferX, lstLaeuferY, laeufer);
                    lstLaeuferX = liste.Item1; lstLaeuferY = liste.Item2;
                    if (lstLaeuferX.Contains(xNeuKoenig) && lstLaeuferY.Contains(yNeuKoenig)) {
                        for (int i = 0; i < lstLaeuferX.Count; i++) {
                            if (this.GetPiece(lstLaeuferX[i], lstLaeuferY[i]) != null && this.GetPiece(lstLaeuferX[i], lstLaeuferY[i]).Type != PieceType.Koenig)
                                break;
                            else if (this.GetPiece(lstLaeuferX[i], lstLaeuferY[i]) != null && this.GetPiece(lstLaeuferX[i], lstLaeuferY[i]).Type == PieceType.Koenig) {
                                discoveredCheck = true;
                                doLeaveLoop = true;
                            }
                        }
                    }
                } else if (item.Item1 != null && item.Item1.Type == PieceType.Springer) {
                    if (item.Item2 - 1 == xNeuKoenig && item.Item3 - 2 == yNeuKoenig ||
                        item.Item2 - 1 == xNeuKoenig && item.Item3 + 2 == yNeuKoenig ||
                        item.Item2 + 1 == xNeuKoenig && item.Item3 - 2 == yNeuKoenig ||
                        item.Item2 + 1 == xNeuKoenig && item.Item3 + 2 == yNeuKoenig ||
                        item.Item2 - 2 == xNeuKoenig && item.Item3 - 1 == yNeuKoenig ||
                        item.Item2 - 2 == xNeuKoenig && item.Item3 + 1 == yNeuKoenig ||
                        item.Item2 + 2 == xNeuKoenig && item.Item3 - 1 == yNeuKoenig ||
                        item.Item2 + 2 == xNeuKoenig && item.Item3 + 1 == yNeuKoenig) {
                        discoveredCheck = true;
                        break;
                    }
                } else if (item.Item1 != null && item.Item1.Type == PieceType.Turm) {
                    liste = this.CreatePositionList(posTurm1X, posTurm1Y, xNeuKoenig, yNeuKoenig, lstTurmX, lstTurmY, turm);
                    lstTurmX = liste.Item1; lstTurmY = liste.Item2;
                    for (int i = 0; i < lstTurmX.Count; i++) {
                        if (this.GetPiece(lstTurmX[i], lstTurmY[i]) != null && this.GetPiece(lstTurmX[i], lstTurmY[i]).Type != PieceType.Koenig) {
                            break;
                        } else if (this.GetPiece(lstTurmX[i], lstTurmY[i]) != null && this.GetPiece(lstTurmX[i], lstTurmY[i]).Type == PieceType.Koenig) {
                            discoveredCheck = true;
                            doLeaveLoop = true;
                        }
                    }
                    lstTurmX.Clear(); lstTurmY.Clear();
                    liste = this.CreatePositionList(posTurm2X, posTurm2Y, xNeuKoenig, yNeuKoenig, lstTurmX, lstTurmY, turm);
                    lstTurmX = liste.Item1; lstTurmY = liste.Item2;
                    for (int i = 0; i < lstTurmX.Count; i++) {
                        if (this.GetPiece(lstTurmX[i], lstTurmY[i]) != null && this.GetPiece(lstTurmX[i], lstTurmY[i]).Type != PieceType.Koenig) {
                            break;
                        } else if (this.GetPiece(lstTurmX[i], lstTurmY[i]) != null && this.GetPiece(lstTurmX[i], lstTurmY[i]).Type == PieceType.Koenig) {
                            discoveredCheck = true;
                            doLeaveLoop = true;
                        }
                    }
                }
            }
            return discoveredCheck;
        }
        #endregion
    }
}
