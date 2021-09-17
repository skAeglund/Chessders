using UnityEngine;

public class FENgenerator : MonoBehaviour
{
    public static string GenerateFen(Board board, GameManager gameManager)
    {
        // Generates a fen notation of the current position
        // where each piece is represented by a character (Q = white queen, q = black queen)
        // and empty cells are represented by numbers (how many in a row)

        string fenString = string.Empty;
        for (int y = 7; y >= 0; y--) // rows
        {
            int emptySquares = 0;
            for (int x = 0; x < 8; x++) // columns
            {
                if (board.Cells[x, y].transform.childCount <= 1)
                {
                    if (emptySquares < 7)
                        emptySquares++;
                    else
                        fenString += "8"; // 8 = empty row
                }
                else // contains piece
                {
                    if (emptySquares > 0)
                        fenString += emptySquares.ToString();

                    emptySquares = 0;
                    fenString += board.Cells[x, y].GetComponentInChildren<Piece>().FenId;

                }
                if (x == 7 && emptySquares > 0 && fenString[fenString.Length-1] != '8')
                    fenString += emptySquares.ToString();
            }
            if (y != 0)
                fenString += "/";
        }
        // get current player "w" or "b"
        fenString += board.CurrentPlayer == 0 ? " w " : " b ";

        fenString += MoveSystem.GetCastlingAvailability(board);

        fenString += " - "; // TO DO - IMPLEMENT EN PASSANT TARGET SQUARE

        fenString += gameManager.HalfMoveClock.ToString() + " ";

        fenString += gameManager.FullMoveNumber.ToString();
        return fenString;
    }
}
