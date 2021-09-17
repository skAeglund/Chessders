using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HudComponents : MonoBehaviour
{
    public GameObject[] CapturedSlotsWhite { get; set; } = new GameObject[16];
    public GameObject[] CapturedSlotsBlack { get; set; } = new GameObject[16];

    [SerializeField] private GameObject cellPrefab;

    private GameObject pointsObject;
    private GameManager gameManager;
    private Board board;

    public static Dictionary<string, int> pieceValues = new Dictionary<string, int>()
    {
        // used to calculate the evaluation score
        { "Pawn", 1},
        { "Knight", 3},
        { "Bishop", 3},
        { "Rook", 5},
        { "Queen", 9},
        { "King", 0},
    };
    private void Awake()
    {
        board = GameObject.Find("Board").GetComponent<Board>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    public void Create()
    {
        CreateCapturedPiecesCells();
        CreateScore();
    }
    private GameObject CreateCell(int x, int y, Transform parent, float xPosition, float yPosition, string color)
    {
        GameObject newCell = Instantiate(cellPrefab, parent);
        newCell.name = $"{color} Slot " + (x + (8 * y));
        RectTransform rectTransform = newCell.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector3((x * 40) + xPosition, (y * 40) + yPosition); // cell size = 100x100
        rectTransform.sizeDelta = new Vector2(40, 40);

        if (y == 0)
            newCell.GetComponent<Image>().color = x % 2 == 0 ? board.BlackColor : board.WhiteColor;
        else
            newCell.GetComponent<Image>().color = x % 2 == 1 ? board.BlackColor : board.WhiteColor;
        newCell.GetComponent<Image>().color -= new Color(0, 0, 0, 0.6f);
        newCell.SetActive(false);

        return newCell;
    }

    public void CreateCapturedPiecesCells()
    {

        float xPosition = 860;
        float yPosition = gameManager.PlayerColor == 0 ? 20 : 780; 
        Transform parent = GameObject.Find("CapturedPieces").transform; 
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                CapturedSlotsWhite[x+ (8*y)] = CreateCell(x, y, parent, xPosition, yPosition, "White");

            }
        }
        yPosition = gameManager.PlayerColor == 0 ? 780 : 20;
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                CapturedSlotsBlack[x + (8 * y)] = CreateCell(x, y, parent, xPosition, yPosition,"Black");
            }
        }
    }


    public void CreateCapturedPiece(Sprite sprite)
    {
        if (board.CurrentPlayer == (int)Enums.Color.White)
        {
            for (int i = 0; i < CapturedSlotsWhite.Length; i++)
                if (!CapturedSlotsWhite[i].activeSelf)
                {
                    CapturedSlotsWhite[i].transform.GetChild(0).GetComponent<Image>().sprite = sprite;
                    CapturedSlotsWhite[i].SetActive(true);
                    break;
                }
        }
        else
        {
            for (int i = 0; i < CapturedSlotsBlack.Length; i++)
                if (!CapturedSlotsBlack[i].activeSelf)
                {
                    CapturedSlotsBlack[i].transform.GetChild(0).GetComponent<Image>().sprite = sprite;
                    CapturedSlotsBlack[i].SetActive(true);
                    break;
                }
        }
    }
    
    private void CreateScore()
    {
        pointsObject = new GameObject("Points", typeof(Text));
        pointsObject.SetActive(false);
        Text text = pointsObject.GetComponent<Text>();
        pointsObject.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);
        text.fontSize = 25;
        text.color = new Color(1, 1, 1, 0.5f);
        text.text = "Evaluation: 0";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        pointsObject.transform.position = new Vector3(1483, -422);
        pointsObject.transform.SetParent(GameObject.Find("UI").transform, false);
    }
    public void UpdateScore()
    {
        if (!pointsObject.gameObject.activeSelf)
            pointsObject.SetActive(true);

        int blackPoints = 0;
        int whitePoints = 0;
        int evaluation;

        List<Piece> playerPieces = new List<Piece>();
        playerPieces.AddRange(board.ActivePieces.Where(x => x.Color == gameManager.PlayerColor));
        List<Piece> opponentPieces = new List<Piece>();
        opponentPieces.AddRange(board.ActivePieces.Where(x => x.Color != gameManager.PlayerColor));

        foreach(Piece piece in playerPieces)
        {
            pieceValues.TryGetValue(((Enums.PieceType)piece.PieceType).ToString(), out int value);
            whitePoints += value;
        }
        foreach (Piece piece in opponentPieces)
        {
            pieceValues.TryGetValue(((Enums.PieceType)piece.PieceType).ToString(), out int value);
            blackPoints += value;
        }
        evaluation = whitePoints - blackPoints;
        string sign = evaluation == 0? string.Empty : evaluation > 0 ? "+" : "-";
        pointsObject.GetComponent<Text>().text = $"Evaluation: {sign}{Mathf.Abs(evaluation)}";
    }
}
