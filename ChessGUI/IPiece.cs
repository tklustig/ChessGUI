using System;

namespace ChessGUI {
    interface IPiece {
        PieceType Type { get; }
        PieceColor Color { get; }
        int GetHashCode();
        bool Equals(object o);
        bool Equals(Piece p);
    }
}
