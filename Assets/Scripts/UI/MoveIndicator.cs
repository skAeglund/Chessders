using UnityEngine;
using UnityEngine.UI;

public class MoveIndicator : MonoBehaviour
{
    [SerializeField] private Sprite outlineNormal;
    [SerializeField] private Sprite outlinePiece;
    private GameManager gameManager;
    private Board board;

    private void Awake()
    {
        board = transform.parent.parent.GetComponent<Board>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    private void OnEnable()
    {
        Vector2Int boardPosition = GetComponentInParent<Cell>().BoardPosition;

        if (transform.parent.childCount > 1)
            transform.GetComponent<Image>().sprite = outlinePiece;
        else
            transform.GetComponent<Image>().sprite = outlineNormal;

        if (boardPosition == board.CurrentEnPassantTarget && gameManager.SelectedPiece.PieceType == (int)Enums.PieceType.Pawn)
            transform.GetComponent<Image>().sprite = outlinePiece;
    }
}
