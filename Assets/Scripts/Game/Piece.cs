using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Piece : MonoBehaviour
{
    public int Color { get; set; } // White = 0, Black = 1 - Enums.Color
    public int PieceType { get; set; } // Enums.PieceType
    public char FenId { get; set; } // Enums.FenId
    public Vector2Int BoardPosition { get; set; }
    public bool HasMovedOnce { get; set; } = false;

    [SerializeField] private Sprite[] Sprites = new Sprite[12];

    private AudioManager audioManager;
    private GameManager gameManager;
    private HudComponents hudComponents;


    private void Awake()
    {
        BoardPosition = GetComponentInParent<Cell>().BoardPosition;
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 2;
        audioManager = GameObject.Find("AudioSource").GetComponent<AudioManager>();
        gameManager = GameObject.Find("GameController").GetComponent<GameManager>();
        hudComponents = GameObject.Find("UI").GetComponent<HudComponents>();
    }

    private void Update() // Only for testing purposes (trying out colors)
    {
        if (PieceType != (int)Enums.PieceType.King)
            if (GetComponent<Image>().color != GetComponentInParent<Cell>().Board.WhitePieceColor || GetComponent<Image>().color != GetComponentInParent<Cell>().Board.BlackPieceColor)
                GetComponent<Image>().color = Color == (int)Enums.Color.White ? GetComponentInParent<Cell>().Board.WhitePieceColor : GetComponentInParent<Cell>().Board.BlackPieceColor;
    }

    public void Create(char fenChar)
    {
        FenId = fenChar;
        Enums.fenIds.TryGetValue(fenChar, out int fenInt);
        PieceType = fenInt > 5 ? fenInt - 6 : fenInt;
        GetComponent<Image>().sprite = Sprites[(int)(Enums.Piece)fenInt];
        GetComponent<Image>().color = Color == (int)Enums.Color.White ? GetComponentInParent<Cell>().Board.WhitePieceColor : GetComponentInParent<Cell>().Board.BlackPieceColor;
    }


    public void Move(Board board, Transform newParent)
    {
        Cell newParentCell = newParent.GetComponent<Cell>();
        bool pieceWasCaptured = false;

        // removes opponents piece
        if (newParent.childCount > 1)
        {
            pieceWasCaptured = true;
            GameObject objectToDestroy = newParent.GetChild(1).gameObject;
            hudComponents.CreateCapturedPiece(objectToDestroy.GetComponent<Image>().sprite);

            objectToDestroy.transform.SetParent(null, false);
            board.ActivePieces.Remove(objectToDestroy.GetComponent<Piece>());
            Destroy(objectToDestroy);
            gameManager.HalfMoveClock = 0;
        }
        // if the current move is trying to "En passant"
        else if (newParentCell.BoardPosition == board.CurrentEnPassantTarget && PieceType == (int)Enums.PieceType.Pawn)
        {
            pieceWasCaptured = true;
            GameObject objectToDestroy = Color == (int)Enums.Color.White ?
                board.Cells[newParentCell.BoardPosition.x, newParentCell.BoardPosition.y - 1].transform.GetChild(1).gameObject :
                board.Cells[newParentCell.BoardPosition.x, newParentCell.BoardPosition.y + 1].transform.GetChild(1).gameObject;

            hudComponents.CreateCapturedPiece(objectToDestroy.GetComponent<Image>().sprite);

            objectToDestroy.transform.SetParent(null, false);
            board.ActivePieces.Remove(objectToDestroy.GetComponent<Piece>());
            Destroy(objectToDestroy);
            gameManager.HalfMoveClock = 0;
        }

        else // no captures
            gameManager.HalfMoveClock = PieceType == (int)Enums.PieceType.Pawn ? 0 : gameManager.HalfMoveClock + 1;

        // castling - moves the rook over to the other side of the king
        if (PieceType == (int)Enums.PieceType.King && !HasMovedOnce)
        {
            // kingside
            if (newParentCell.BoardPosition.x - BoardPosition.x == 2)
            {
                board.Cells[newParentCell.BoardPosition.x + 1, newParentCell.BoardPosition.y].transform.GetChild(1).transform.SetParent(board.Cells[newParentCell.BoardPosition.x - 1, newParentCell.BoardPosition.y].transform, false);
            }
            // queenside
            if (newParentCell.BoardPosition.x - BoardPosition.x == -2)
            {
                board.Cells[newParentCell.BoardPosition.x - 2, newParentCell.BoardPosition.y].transform.GetChild(1).transform.SetParent(board.Cells[newParentCell.BoardPosition.x + 1, newParentCell.BoardPosition.y].transform, false);
            }
        }

        // sets en passant target
        if (PieceType == (int)(Enums.PieceType.Pawn))
        {
            if (Mathf.Abs(newParentCell.BoardPosition.y - BoardPosition.y) == 2)
            {
                board.CurrentEnPassantTarget = Color == (int)Enums.Color.White ?
                    new Vector2Int(BoardPosition.x, BoardPosition.y + 1) : new Vector2Int(BoardPosition.x, BoardPosition.y - 1);

                //Debug.Log("Current en passant target is: " + board.CurrentEnPassantTarget);
            }
            else if (board.CurrentEnPassantTarget != null)
            {
                board.CurrentEnPassantTarget = null;
            }
        }
        else if (board.CurrentEnPassantTarget != null)
        {
            board.CurrentEnPassantTarget = null;
        }

        // changes parent of the last clicked piece to this 
        transform.SetParent(newParent, true);

        // moves the piece smoothly to the new position
        StartCoroutine(SmoothJump(0.25f, newParent.transform.position));


        if (!HasMovedOnce)
            HasMovedOnce = true;

        if (Color == (int)Enums.Color.Black)
            gameManager.FullMoveNumber++;


        // Pawn promotion
        if (PieceType == (int)(Enums.PieceType.Pawn))
        {
            if (BoardPosition.y == 7 || BoardPosition.y == 0)
            {
                PieceType = (int)Enums.PieceType.Queen;
                FenId = Color == (int)Enums.Color.White ? 'Q' : 'q';
                GetComponent<Image>().sprite = Color == (int)Enums.Color.White ?
                    Sprites[(int)Enums.Piece.WhiteQueen] : Sprites[(int)Enums.Piece.BlackQueen];
            }
        }
        board.UnHighlight();

        gameManager.SelectedPiece = null;
        board.ScanForCheck(board.CurrentPlayer, true);

        if (board.ScanForCheck(board.CurrentPlayer == 0 ? 1 : 0, true) && !gameManager.IsGameOver)
            audioManager.PlayCheckSFX();
        else if (pieceWasCaptured && !gameManager.IsGameOver)
            audioManager.PlayPieceCapturedSFX(Color);
        else if (!pieceWasCaptured && !gameManager.IsGameOver)
            audioManager.PlayPieceMoveSFX(Color);

        if (pieceWasCaptured)
            hudComponents.UpdateScore();
        board.SwapPlayer();
    }

    public IEnumerator SmoothJump(float time, Vector3 destination)
    {
        // Moves a piece smoothly to a new position
        Vector3 startingPos = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(startingPos, destination, Mathf.SmoothStep(0.0f, 1.0f, elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTransformParentChanged()
    {
        Transform parent = transform.parent;
        Cell parentCell = null;

        if (parent != null)
        parent.TryGetComponent<Cell>(out parentCell);

        if (parentCell != null)
            BoardPosition = parentCell.BoardPosition;
    }
    public void OnDestroy()
    {
        if (!gameManager.IsQuitting)
        {
            Board board = GameObject.Find("Board").GetComponent<Board>();
            if (board.ActivePieces.Contains(this))
                board.ActivePieces.Remove(this);
        }
    }

}
