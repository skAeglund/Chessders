using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoveByNotation : MonoBehaviour
{
    [SerializeField] Board board;
    public static Dictionary<char, int> xPositions = new Dictionary<char, int>() 
    {
        { 'a', 0},
        { 'b', 1},
        { 'c', 2},
        { 'd', 3},
        { 'e', 4},
        { 'f', 5},
        { 'g', 6},
        { 'h', 7}
    };
    public static Dictionary<char, int> yPositions = new Dictionary<char, int>()
    {
        { '1', 0},
        { '2', 1},
        { '3', 2},
        { '4', 3},
        { '5', 4},
        { '6', 5},
        { '7', 6},
        { '8', 7},
        { 'q', 7},
    };
    public void MakeThisMove(string move)
    {
        // Tries to make a move from long algebraic notation (f.e. e2e4)

        List<Piece> currentPlayerPieces = new List<Piece>();
        currentPlayerPieces.AddRange(board.ActivePieces.Where(n => n.Color == board.CurrentPlayer));

        if (move == "fail")
            MakeRandomMove(currentPlayerPieces);

        if (move[move.Length-1] == '+' || move[move.Length-1] == 'Q' || move[move.Length - 1] == 'q')
            move = move.Remove(move.Length - 1);

        if (char.IsLower(move[0]))
        {
            MakePawnMove(move, currentPlayerPieces);
        }
        else
        {
            MovePiece(move, currentPlayerPieces);
        }
    }

    private void MovePiece(string move, List<Piece> currentPlayerPieces)
    {
        if (!xPositions.TryGetValue(move[1], out int xStart) || !yPositions.TryGetValue(move[2], out int yStart))
            return;
        Vector2Int startPosition = new Vector2Int(xStart, yStart);
        Vector2Int destination = new Vector2Int(xPositions[move[move.Length - 2]], yPositions[move[move.Length - 1]]);
        Piece pieceToMove = board.Cells[startPosition.x, startPosition.y].GetComponentInChildren<Piece>();

        if (MoveSystem.AvailableMoves(pieceToMove.gameObject, board).Contains(destination))
        {
            pieceToMove.Move(board, board.Cells[destination.x, destination.y].transform);
        }
        else
        {
            Debug.Log($"{destination} = illegal move?");
            MakeRandomMove(currentPlayerPieces);
        }

    }

    private void MakePawnMove(string move, List<Piece> currentPlayerPieces)
    {
        bool correctString = true;
        if (!xPositions.TryGetValue(move[0], out int xStart)) correctString = false;
        if (!yPositions.TryGetValue(move[1], out int yStart)) correctString = false;
        if (!xPositions.TryGetValue(move[move.Length - 2], out int xDestination)) correctString = false;
        if (!yPositions.TryGetValue(move[move.Length - 1], out int yDestination)) correctString = false;

        Vector2Int startPosition = new Vector2Int(xStart, yStart);
        Vector2Int destination = new Vector2Int(xDestination, yDestination);

        Piece pawnToMove = board.Cells[startPosition.x, startPosition.y].GetComponentInChildren<Piece>();

        if (MoveSystem.AvailableMoves(pawnToMove.gameObject, board).Contains(destination) && correctString)
            pawnToMove.Move(board, board.Cells[destination.x, destination.y].transform );
        else
        {
            Debug.Log($"{destination} = illegal move?");
            MakeRandomMove(currentPlayerPieces);        
        }

    }

    private void MakeRandomMove(List<Piece> currentPlayerPieces)
    {
        List<Vector2Int> availableMoves = new List<Vector2Int>();
        foreach (Piece piece in currentPlayerPieces)
        {
            availableMoves.AddRange(MoveSystem.AvailableMoves(piece.gameObject, board));
            if (availableMoves.Count > 0)
            {
                piece.Move(board, board.Cells[availableMoves[0].x, availableMoves[0].y].transform);
                break;
            }

            else if (piece == currentPlayerPieces[currentPlayerPieces.Count - 1])
                Debug.Log("Stalemate or check mate?");
        }
        Debug.Log("A random move was made...");
    }

    #region OLD CODE

    // SHORT ALGEBRAIC NOTATION
    //...
    //else if (move[0] == 'N')
    //{
    //    MovePiece(move, (int)Enums.PieceType.Knight, currentPlayerPieces);
    //}
    //else if (move[0] == 'B')
    //{
    //    MovePiece(move, (int)Enums.PieceType.Bishop, currentPlayerPieces);
    //}
    //else if (move[0] == 'Q')
    //{
    //    MovePiece(move, (int)Enums.PieceType.Queen, currentPlayerPieces);
    //}
    //else if (move[0] == 'R')
    //{
    //    MovePiece(move, (int)Enums.PieceType.Rook, currentPlayerPieces);
    //}
    //else if (move[0] == 'K')
    //{
    //    MovePiece(move, (int)Enums.PieceType.King, currentPlayerPieces);
    //}


    //private void MakePawnMove(string move, List<Piece> currentPlayerPieces)
    //{
    //    List<Piece> pawns = new List<Piece>();
    //    pawns.AddRange(currentPlayerPieces.Where(x => (x.BoardPosition.x == xPositions[move[0]]) && (x.PieceType == (int)Enums.PieceType.Pawn)));

    //    Cell attackedPiece = null;
    //    if (move.Length > 2) // for example exd5 - e pawn attacks d5 piece
    //        attackedPiece = board.Cells[xPositions[move[2]], yPositions[move[3]]];

    //    if (pawns.Count == 1)
    //    {
    //        if (attackedPiece == null)
    //        {
    //            if (MoveSystem.AvailableMoves(pawns[0].gameObject, board).Contains(new Vector2Int(pawns[0].BoardPosition.x, yPositions[move[1]])))
    //                pawns[0].Move(board, board.Cells[pawns[0].BoardPosition.x, yPositions[move[1]]].transform);
    //        }

    //        else if (MoveSystem.AvailableMoves(pawns[0].gameObject, board).Contains(attackedPiece.boardPosition))
    //            pawns[0].Move(board, attackedPiece.transform);
    //    }
    //    else // multiple pawns on same row
    //    {
    //        foreach (Piece piece in pawns)
    //        {
    //            if (MoveSystem.AvailableMoves(piece.gameObject, board).Contains(new Vector2Int(xPositions[move[0]], yPositions[move[1]])))
    //            {
    //                if (attackedPiece == null)
    //                    piece.Move(board, board.Cells[piece.BoardPosition.x, yPositions[move[1]]].transform);
    //                else
    //                    pawns[0].Move(board, attackedPiece.transform);
    //                break;
    //            }
    //        }
    //    }
    //}

    //private void MovePiece(string move, int pieceType, List<Piece> currentPlayerPieces)
    //{
    //    List<Piece> matchingPieces = new List<Piece>();
    //    matchingPieces.AddRange(currentPlayerPieces.Where(x => x.PieceType == (int)(Enums.PieceType)pieceType));
    //    Vector2Int movePosition = new Vector2Int(xPositions[move[move.Length - 2]], yPositions[move[move.Length - 1]]);

    //    if (matchingPieces.Count == 1 && MoveSystem.AvailableMoves(matchingPieces[0].gameObject, board).Contains(movePosition))
    //    {
    //        matchingPieces[0].Move(board, board.Cells[xPositions[move[move.Length - 2]], yPositions[move[move.Length - 1]]].transform);
    //        return;
    //    }

    //    else if (move.Length > 3 && move[1] != 'x') // specified knight - Ngf3 or N5b3 for example
    //    {
    //        Piece piece = !char.IsDigit(move[1]) ? piece = matchingPieces.First(x => x.BoardPosition.x == xPositions[move[1]])  // abcdefgh
    //                                                       : matchingPieces.First(y => y.BoardPosition.y == yPositions[move[1]]); // 012345678

    //        piece.Move(board, board.Cells[xPositions[move[move.Length - 2]], yPositions[move[move.Length - 1]]].transform);
    //    }
    //    else // not specified - Nf3 for example
    //    {
    //        foreach (Piece piece in matchingPieces)
    //        {
    //            if (MoveSystem.AvailableMoves(piece.gameObject, board).Contains(movePosition))
    //            {
    //                piece.Move(board, board.Cells[xPositions[move[move.Length - 2]], yPositions[move[move.Length - 1]]].transform);
    //                break;
    //            }
    //        }
    //    }
    //}
    #endregion
}
