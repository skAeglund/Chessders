using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : MonoBehaviour
{
    private static int currentPlayer;

    private static readonly List<Vector2Int> knightMoves = new List<Vector2Int>()
    {
        new Vector2Int(1,2), // one step right, two steps up
        new Vector2Int(2,1), // two steps right, one step up etc...
        new Vector2Int(2,-1),
        new Vector2Int(1,-2),
        new Vector2Int(-1,-2),
        new Vector2Int(-2,-1),
        new Vector2Int(-2,1),
        new Vector2Int(-1,2)
    };

    private static readonly List<Vector2Int> directions = new List<Vector2Int>()
    {
        new Vector2Int(1, 0),   // right
        new Vector2Int(-1, 0),  // left
        new Vector2Int(0, -1),  // up
        new Vector2Int(0, 1),   // down
        new Vector2Int(1,-1),   // up right    /
        new Vector2Int(-1, 1),  // up left     \
        new Vector2Int(1, 1),   // down right  \
        new Vector2Int(-1, -1)  // down left   /
    };

    static bool ContainsOpponentsPiece(Cell cell)
    {
        if (cell.transform.childCount <= 1)
            return false;
        else if (cell.transform.GetChild(1).GetComponent<Piece>().Color != currentPlayer)
            return true;

        return false;
    }

    public static List<Vector2Int> AvailableMoves(GameObject piece, Board board, bool isScanningForCheck = false)
    {
        // Returns a list of all the available moves of a piece of any type

        List<Vector2Int> availableMoves = new List<Vector2Int>();
        Piece pieceScript = piece.GetComponent<Piece>();
        if (piece.GetComponentInParent<Cell>() == null)
            return availableMoves;
        Vector2Int position = piece.GetComponentInParent<Cell>().BoardPosition;
        currentPlayer = piece.GetComponent<Piece>().Color;



        currentPlayer = piece.GetComponent<Piece>().Color;
        switch (piece.GetComponent<Piece>().PieceType)
        {
            case (int)Enums.PieceType.Pawn:
                if (pieceScript.Color == (int)Enums.Color.White)
                {
                    // 1 step up
                    if (board.Cells[position.x, position.y + 1].transform.childCount == 1)
                    {
                        availableMoves.Add(new Vector2Int(position.x, position.y + 1)); // 1 step up
                        
                        if (position.y == 1 && board.Cells[position.x, position.y + 2].transform.childCount == 1)
                        {
                            availableMoves.Add(new Vector2Int(position.x, position.y + 2)); // 2 steps up
                        }
                    }
                    if (position.x > 0)
                    if (ContainsOpponentsPiece(board.Cells[position.x-1, position.y+1]) ||
                            board.CurrentEnPassantTarget == new Vector2Int(position.x - 1, position.y + 1))
                    {
                        availableMoves.Add(new Vector2Int(position.x - 1, position.y + 1)); // diagonally left
                    }
                    if (position.x < 7)
                    if (ContainsOpponentsPiece(board.Cells[position.x + 1, position.y + 1]) ||
                            board.CurrentEnPassantTarget == new Vector2Int(position.x + 1, position.y + 1))
                    {
                        availableMoves.Add(new Vector2Int(position.x + 1, position.y + 1)); // diagonally right
                    }
                }
                else if(pieceScript.Color == (int)Enums.Color.Black && position.y > 0)
                {
                    if (board.Cells[position.x, position.y - 1].transform.childCount == 1)
                    {
                        availableMoves.Add(new Vector2Int(position.x, position.y - 1)); // 1 step down

                        if (position.y == 6 && board.Cells[position.x, position.y - 2].transform.childCount == 1)
                        {
                            availableMoves.Add(new Vector2Int(position.x, position.y - 2)); // two step up if at original position
                        }
                    }
                    if (position.x > 0)
                    if (ContainsOpponentsPiece(board.Cells[position.x - 1, position.y - 1]) ||
                            board.CurrentEnPassantTarget == new Vector2Int(position.x - 1, position.y - 1))
                    {
                        availableMoves.Add(new Vector2Int(position.x - 1, position.y - 1));
                    }
                    if (position.x < 7)
                    if (ContainsOpponentsPiece(board.Cells[position.x +1, position.y -1]) ||
                            board.CurrentEnPassantTarget == new Vector2Int(position.x + 1, position.y - 1))
                    {
                        availableMoves.Add(new Vector2Int(position.x + 1, position.y - 1));
                    }
                }

                break;

            case (int)Enums.PieceType.Knight:
                for (int i = 0; i < knightMoves.Count; i++)
                {
                    Vector2Int newPosition = position + knightMoves[i];
                    if (!board.IsInsideBoard(newPosition.x, newPosition.y))
                        continue;

                    Cell cell = board.Cells[newPosition.x, newPosition.y];
                    if (cell.transform.childCount == 1)
                        availableMoves.Add(newPosition);
                    else if (cell.transform.childCount > 1 && cell.transform.GetChild(1).GetComponent<Piece>().Color != currentPlayer)
                    {
                        availableMoves.Add(newPosition);
                    }
                }

                break;

            case (int)Enums.PieceType.Bishop:

                for (int i = 4; i < directions.Count; i++)
                {
                    Vector2Int newPosition = position;
                    for (int j = 1; j <= 8; j++)
                    {
                        newPosition += directions[i];

                        if (!board.IsInsideBoard(newPosition.x, newPosition.y))
                            break;

                        Cell cell = board.Cells[newPosition.x, newPosition.y];
                        if (cell.transform.childCount == 1)
                            availableMoves.Add(newPosition);
                        else if (cell.transform.childCount > 1)
                        {
                            if (cell.transform.GetChild(1).GetComponent<Piece>().Color != currentPlayer)
                            {
                                availableMoves.Add(newPosition);
                            }
                            break;
                        }
                    }
                }

                break;

            case (int)Enums.PieceType.Rook:

                for (int i = 0; i < 4; i++)
                {
                    Vector2Int newPosition = position;
                    for (int j = 0; j < directions.Count; j++)
                    {
                        newPosition += directions[i];

                        if (!board.IsInsideBoard(newPosition.x, newPosition.y))
                            break;

                        Cell cell = board.Cells[newPosition.x, newPosition.y];
                        if (cell.transform.childCount == 1)
                            availableMoves.Add(newPosition);
                        else if (cell.transform.childCount > 1)
                        {
                            if (cell.transform.GetChild(1).GetComponent<Piece>().Color != currentPlayer)
                            {
                                availableMoves.Add(newPosition);
                            }
                            break;
                        }
                    }
                }
                break;

            case (int)Enums.PieceType.Queen:
                for (int i = 0; i < 8; i++)
                {
                    Vector2Int newPosition = position;
                    for (int j = 0; j < 8; j++)
                    {
                        newPosition += directions[i];

                        if (!board.IsInsideBoard(newPosition.x, newPosition.y))
                            break;

                        Cell cell = board.Cells[newPosition.x, newPosition.y];
                        if (cell.transform.childCount == 1)
                            availableMoves.Add(newPosition);
                        else if (cell.transform.childCount > 1)
                        {
                            if (cell.transform.GetChild(1).GetComponent<Piece>().Color != currentPlayer)
                            {
                                availableMoves.Add(newPosition);
                            }
                            break;
                        }
                    }
                }
                break;

            case (int)Enums.PieceType.King:
                for (int i = 0; i < 8; i++)
                {
                    Vector2Int newPosition = position + directions[i];
                    if (!board.IsInsideBoard(newPosition.x, newPosition.y))
                        continue;

                    Cell cell = board.Cells[newPosition.x, newPosition.y];
                    if (cell.transform.childCount == 1)
                        availableMoves.Add(newPosition);
                    else if (cell.transform.childCount > 1)
                    {
                        if (cell.transform.GetChild(1).GetComponent<Piece>().Color != currentPlayer)
                        {
                            availableMoves.Add(newPosition);
                        }
                    }
                }
                break;
        }
        if (!isScanningForCheck)
        {
            RemoveMovesLeadingToCheck(piece, availableMoves, board, currentPlayer, position);

            // castling
            if (pieceScript.PieceType == (int)Enums.PieceType.King && !pieceScript.HasMovedOnce && !board.ScanForCheck(currentPlayer == 0 ? 1 : 0))
            {
                // castling kingside
                if (!HasThisPieceMoved(board.Cells[position.x + 3, position.y]))
                if (!board.Cells[position.x + 3, position.y].GetComponentInChildren<Piece>().HasMovedOnce
                    && availableMoves.Contains(new Vector2Int(position.x + 1, position.y)) && board.Cells[position.x + 2, position.y].transform.childCount <= 1)
                {
                    availableMoves.Add(new Vector2Int(position.x + 2, position.y));
                    RemoveMovesLeadingToCheck(piece, availableMoves, board, currentPlayer == 0 ? 1 : 0, position);
                }

                // castling queenside
                if (!HasThisPieceMoved(board.Cells[position.x - 4, position.y]))
                    if (!board.Cells[position.x - 4, position.y].GetComponentInChildren<Piece>().HasMovedOnce
                    && availableMoves.Contains(new Vector2Int(position.x - 1, position.y)) && board.Cells[position.x - 2, position.y].transform.childCount <= 1
                    && board.Cells[position.x - 3, position.y].transform.childCount <= 1)
                {
                    availableMoves.Add(new Vector2Int(position.x - 2, position.y));
                    RemoveMovesLeadingToCheck(piece, availableMoves, board, currentPlayer == 0 ? 1 : 0, position);
                }

            }
        }
        return availableMoves;
    }

    static void RemoveMovesLeadingToCheck(GameObject piece, List<Vector2Int> availableMoves, Board board, int currentPlayer, Vector2Int boardPosition)
    {
        // Removes moves leading to check by "trying them out" and scanning for a check

        List<Vector2Int> movesToRemove = new List<Vector2Int>();
        GameObject checkTestingParent = GameObject.Find("CheckTestingParent");
        foreach (Vector2Int move in availableMoves)
        {

            piece.transform.SetParent(board.Cells[move.x, move.y].transform, false);
            if (board.Cells[move.x, move.y].transform.childCount > 2)
            {
                GameObject pieceToRemove = board.Cells[move.x, move.y].transform.GetChild(1).gameObject;

                    pieceToRemove.transform.SetParent(checkTestingParent.transform, false);
                    if (board.ScanForCheck(currentPlayer))
                    {
                        movesToRemove.Add(move);
                        // Debug.Log("Move " + move + " was removed");
                    }
                pieceToRemove.transform.SetParent(board.Cells[move.x, move.y].transform, false);

            }
            else if (board.ScanForCheck(currentPlayer))
            {
                movesToRemove.Add(move);
                //Debug.Log("Move " + move + " was removed");
            }

            piece.transform.SetParent(board.Cells[boardPosition.x, boardPosition.y].transform, false);
        }
        foreach (Vector2Int move in movesToRemove)
        {
            availableMoves.Remove(move);
        }
    }

    public static string GetCastlingAvailability(Board board)
    {
        // Checks if any kings or rooks has moved once, which determines if the king can castle in that direction or not

        string fenCastlingString = string.Empty;

        if (!HasThisPieceMoved(board.Cells[4, 0]) && !HasThisPieceMoved(board.Cells[7, 0]))
            fenCastlingString += "K"; // white kingside castling

        if (!HasThisPieceMoved(board.Cells[4, 0]) && !HasThisPieceMoved(board.Cells[0, 0]))
            fenCastlingString += "Q"; // white queenside castling

        if (!HasThisPieceMoved(board.Cells[4, 7]) && !HasThisPieceMoved(board.Cells[7, 7]))
            fenCastlingString += "k"; 

        if (!HasThisPieceMoved(board.Cells[4, 7]) && !HasThisPieceMoved(board.Cells[0, 7]))
            fenCastlingString += "q";

        if (fenCastlingString.Length == 0)
            fenCastlingString = "-"; // noone can castle

        return fenCastlingString;
    }

    public static bool HasThisPieceMoved(Cell cell)
    {
        if (cell.transform.childCount > 1)
        {
            if (!cell.GetComponentInChildren<Piece>().HasMovedOnce)
            {
                return false;
            }
        }
        return true;
    }
}
