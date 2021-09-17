using System.Collections.Generic;

public class Enums 
{
    public enum Piece
    {
        WhitePawn = 0, WhiteKnight = 1, WhiteBishop = 2, WhiteRook = 3, WhiteQueen = 4, WhiteKing = 5,
        BlackPawn = 6, BlackKnight = 7, BlackBishop = 8, BlackRook = 9, BlackQueen = 10, BlackKing = 11
    }
    public enum FenId
    {
        P = 0, N = 1, B = 2, R = 3, Q = 4, K = 5,
        p = 6, n = 7, b = 8, r = 9, q = 10, k = 11
    }
    public enum Color
    {
        White = 0, Black = 1
    }
    public enum PieceType
    {
        Pawn = 0, Knight = 1, Bishop = 2, Rook = 3, Queen = 4, King = 5
    }

    public enum PieceValue
    {
        Pawn = 1, Knight = 3, Bishop = 3, Rook = 5, Queen = 9, King = 100
    }
    public static Dictionary<char, int> fenIds = new Dictionary<char, int>() // todo: find a better place for this
    {
        {'P' , 0},
        {'N' , 1},
        {'B' , 2},
        {'R' , 3},
        {'Q' , 4},
        {'K' , 5},
        {'p' , 6},
        {'n' , 7},
        {'b' , 8},
        {'r' , 9},
        {'q' , 10},
        {'k' , 11}
    };
}
