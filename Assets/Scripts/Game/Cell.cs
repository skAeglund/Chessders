using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerDownHandler
{   
    private GameManager gameManager;

    public Vector2Int BoardPosition { get; set; }
    public Color OriginalColor { get; set; }
    public Board Board { get; set; }
    public Image Image { get; set; }


    private void Awake()
    {
        Image = GetComponent<Image>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    public void Create(Vector2Int newBoardPosition, Color color, Board newBoard)
    {
        BoardPosition = newBoardPosition;
        OriginalColor = color;
        Image.color = OriginalColor;
        Board = newBoard;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameManager.IsGameOver || gameManager.PlayerColor == -1)
            return;
        Board.ScanForCheck(Board.CurrentPlayer, true);

        // clicking on a piece that belongs to the current player
        if (transform.childCount > 1 && transform.GetChild(1).GetComponent<Piece>().Color == Board.CurrentPlayer)
        {
            Board.HighlightCell(BoardPosition);

            gameManager.SelectedPiece = transform.GetChild(1).GetComponent<Piece>();

            Board.HighlightMoves(MoveSystem.AvailableMoves(transform.GetChild(1).gameObject, Board));
        }
        if (gameManager.SelectedPiece != null && Board.AvailableMoves.Contains(BoardPosition))
        {
            Piece selectedPiece = gameManager.SelectedPiece;

            selectedPiece.Move(Board, transform);
        }
    }
}
