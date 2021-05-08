namespace ChessGUI {

    #region Enumeration - Figur
    public enum PieceType {
        Bauer,
        Springer,
        Laeufer,
        Turm,
        Dame,
        Koenig
    }
    #endregion

    #region Enumeration - Farbe

    public enum PieceColor {
        Black,
        White,
        Dummy
    }
    #endregion
    public class Piece : IPiece {

        #region globale Variablen
        private readonly PieceColor _color;
        private readonly PieceType _type;
        #endregion

        #region Standardkonstruktor

        public Piece() { }
        #endregion

        #region Konstruktor - mit Argumenten
        public Piece(PieceType type, PieceColor color) {
            _type = type;
            _color = color;
        }
        #endregion

        #region Getter(ohne Setter)

        public PieceType Type {
            get { return _type; }
        }

        public PieceColor Color {
            get { return _color; }
        }
        #endregion

        #region Ueberschreibt die Methode aus der Klasse Object
        public override int GetHashCode() {
            unchecked {
                return ((int)_color * 397) ^ (int)_type;
            }
        }
        #endregion

        #region Ueberschreibt die Methode aus der Klasse Object

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return this.Equals((Piece)obj);
        }
        #endregion

        #region wird von obiger Methode benoetigt
        public bool Equals(Piece figure) {
            return _color == figure._color && _type == figure._type;
        }
        #endregion
    }
}
