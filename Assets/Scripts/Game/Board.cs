using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject originalTransform;
    [SerializeField] private GameObject reversedTransform;
    [SerializeField] private Color whiteColor = Color.white;
    [SerializeField] private Color blackColor = Color.black;
    [SerializeField] private Color highlightColor;
    private Color whitePieceColor = new Color(0.9245283f, 0.8621327f, 0.7893378f);
    private Color blackPieceColor = new Color(0.5019608f, 0.1372549f, 0.04705883f);

    private readonly Cell[,] cells = new Cell[8, 8];
    private List<Piece> activePieces = new List<Piece>();

    private int currentPlayer = 0;
    private Vector2Int? currentEnPassantTarget = null;
    private Vector2Int highlightedCell;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private GameManager gameManager;
    private readonly List<GameObject> notationObjects = new List<GameObject>();
    private MoveByNotation pieceMover;

    #region properties
    public Color WhiteColor { get => whiteColor;  }
    public Color BlackColor { get => blackColor; }
    public Color HighlightColor { get => highlightColor; }
    public Cell[,] Cells { get => cells; }
    public Color WhitePieceColor { get => whitePieceColor; set => whitePieceColor = value; }
    public Color BlackPieceColor { get => blackPieceColor; set => blackPieceColor = value; }
    public int CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }
    public List<Piece> ActivePieces { get => activePieces; set => activePieces = value; }
    public Vector2Int? CurrentEnPassantTarget { get => currentEnPassantTarget; set => currentEnPassantTarget = value; }
    public List<Vector2Int> AvailableMoves { get => availableMoves; set => availableMoves = value; }
    #endregion

    public bool IsInsideBoard(int x, int y) => (x - 7) * x <= 0 && (y - 7) * y <= 0;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        pieceMover = GameObject.Find("PieceMover").GetComponent<MoveByNotation>();
        CreateBoard();
        CreatePieces();
        CreatePositionNotation();
        gameManager.AddPositionToList();
    }
    #region Board creation
    public void CreateBoard()
    {
        // Creates the cells of the board and stores them in a 2d-array
        for (int y = 0; y < 8; y++) // y-axis
        {
            for (int x = 0; x < 8; x++) // x-axis  
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                newCell.name = $"Cell({x},{y})";

                RectTransform rectTransform = newCell.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector3((x * 100) + 50, (y * 100) + 50); // cell size = 100x100

                Color cellColor;
                if (y % 2 == 0)
                    cellColor = x % 2 == 0 ? BlackColor : WhiteColor;
                else
                    cellColor = x % 2 == 0 ? WhiteColor : BlackColor;

                Cells[x, y] = newCell.GetComponent<Cell>();
                Cells[x, y].Create(new Vector2Int(x, y), cellColor, this);
            }
        }
    }

    public void CreatePieces()
    {
        for (int i = 0; i < 4; i++)
        {
            int color = i < 2 ? (int)Enums.Color.White : (int)Enums.Color.Black;
            int y = i < 2 ? i : i == 2 ? 6 : 7;
            for (int x = 0; x < 8; x++)
            {
                CreatePiece(x, y, color);
            }
        }
    }
    private void CreatePiece(int x, int row, int color)
    {
        // Creates a piece and saves it in a list of active pieces
        // Note: This could be improved by using a FEN string instead of if-statements
        GameObject newPiece = Instantiate(piecePrefab, Cells[x, row].transform);
        Piece pieceScript = newPiece.GetComponent<Piece>();
        ActivePieces.Add(newPiece.GetComponent<Piece>());
        pieceScript.Color = color;

        if (row == 0)
        {
            pieceScript.Create( x == 0 || x == 7 ? 'R' : x == 1 || x == 6 ? 'N' :
                                x == 2 || x == 5 ? 'B' : x == 3 ? 'Q' : 'K');
        }
        else if (row == 1)
        {
            pieceScript.Create('P');
        }
        else if (row == 6)
        {
            pieceScript.Create('p');
        }
        else if (row == 7)
        {
            pieceScript.Create(x == 0 || x == 7 ? 'r' : x == 1 || x == 6 ? 'n' :
                               x == 2 || x == 5 ? 'b' : x == 3 ? 'q' : 'k');
        }

        newPiece.name = newPiece.GetComponent<Image>().sprite.name;
    }

    private void CreatePositionNotation()
    {
        // Creates the notation at the side of the board

        // abcdefg
        for (int i = 0; i < 8; i++)
        {
            GameObject textObject = new GameObject();
            Text text = textObject.AddComponent<Text>();
            text.fontSize = 25;
            text.color = new Color(1, 1, 1, 0.5f);
            text.text = MoveByNotation.xPositions.FirstOrDefault(x => x.Value == i).Key.ToString();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textObject.name = text.text;
            textObject.transform.SetParent(this.transform.GetChild(0));
            textObject.transform.position = cells[i, 0].transform.position - new Vector3(-40, 115);
            notationObjects.Add(textObject);
        }

        // 12345678
        for (int i = 1; i < 9; i++)
        {
            GameObject textObject = new GameObject();
            Text text = textObject.AddComponent<Text>();
            text.fontSize = 23;
            text.color = new Color(1, 1, 1, 0.5f);
            text.text = i.ToString();
            textObject.name = text.text;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textObject.transform.SetParent(this.transform.GetChild(0));
            textObject.transform.position = cells[0, i-1].transform.position - new Vector3(40, 33 );
            notationObjects.Add(textObject);
        }
    }
    public void RotateBoard()
    {
        // Rotates the board (happens when the player chooses to play as black)
        // TODO: change anchor point to center before rotating (probably no need to adjust the position then)

        transform.position = reversedTransform.transform.position;
        transform.rotation = reversedTransform.transform.rotation;
        foreach (Piece piece in ActivePieces)
        {
            piece.GetComponent<RectTransform>().transform.Rotate(new Vector3(0, 0, -180));
        }
        foreach (GameObject notation in notationObjects)
        {
            notation.GetComponent<RectTransform>().transform.Rotate(new Vector3(0, 0, -180));
            if (char.IsDigit(notation.name[0]))
                notation.transform.position += new Vector3(-800, -50);
            else
                notation.transform.position += new Vector3(75, -950);
        }
    }

    #endregion

    #region Highlighting
    public void HighlightCell(Vector2Int boardPosition)
    {
        if (highlightedCell != null)
            Cells[highlightedCell.x, highlightedCell.y].Image.color = Cells[highlightedCell.x, highlightedCell.y].OriginalColor;

        Cells[boardPosition.x, boardPosition.y].Image.color = HighlightColor;

        highlightedCell = boardPosition;

    }

    public void UnHighlight()
    {
        if (highlightedCell != null)
            Cells[highlightedCell.x, highlightedCell.y].Image.color = Cells[highlightedCell.x, highlightedCell.y].OriginalColor;
        
        if (AvailableMoves.Count > 0)
        {
            foreach (Vector2Int move in AvailableMoves)
            {
                Cells[move.x, move.y].transform.GetChild(0).gameObject.SetActive(false);
            }
            AvailableMoves.Clear();
        }

    }
    public void HighlightMoves(List<Vector2Int> moveList)
    {
        if (AvailableMoves.Count >0)
        {
            foreach (Vector2Int move in AvailableMoves)
            {
                Cells[move.x, move.y].transform.GetChild(0).gameObject.SetActive(false);
            }
            AvailableMoves.Clear();
        }
        foreach (Vector2Int move in moveList)
        {
            
            AvailableMoves.Add(move);
        }
        foreach (Vector2Int move in moveList)
        {
            Cells[move.x, move.y].transform.GetChild(0).gameObject.SetActive(true);     
        }
    }
    #endregion

    #region Game events
    public void SwapPlayer()
    {
        CurrentPlayer = CurrentPlayer == 0 ? 1 : 0;

        gameManager.AddPositionToList(); // saves the position before making a move

        if (CurrentPlayer != gameManager.PlayerColor && !gameManager.IsGameOver && gameManager.Difficulty >= 0)
            StartCoroutine(MakeMoveByStockfish(1));
        else if (CurrentPlayer != gameManager.PlayerColor && !gameManager.IsGameOver && gameManager.Difficulty == -1)
            StartCoroutine(MoveRandomPieceRandomly(1));

        // checks if the game is over by checkmate/stalemate (could probably be moved somewhere else)
        if (ScanForCheck(CurrentPlayer, true))
        {
            if (ScanForMate(CurrentPlayer))
            {
                Debug.Log("CHECKMATE");
                gameManager.SetGameOver(CurrentPlayer == 0 ? 1 : 0);
            }
        }
        else if (ScanForMate(CurrentPlayer)) // no possible moves without check = stalemate/draw
        {
            Debug.Log("Stalemate");
            gameManager.SetGameOver(2);
        }
    }
    public IEnumerator MakeMoveByStockfish(float delay)
    {
        // Gets a move written in FEN notation from the application "stockfishExecutable.exe" and makes that move after a delay

        string move = MoveGenerator.GetStockfishMove(FENgenerator.GenerateFen(this, gameManager), gameManager.Difficulty);
        yield return new WaitForSeconds(delay);
        pieceMover.MakeThisMove(move);
    }
    public IEnumerator MoveRandomPieceRandomly(float delay)
    {
        // "Braindead" difficulty
        yield return new WaitForSeconds(delay);
        List<Piece> currentPlayerPieces = new List<Piece>();
        currentPlayerPieces.AddRange(ActivePieces.Where(n => n.Color == CurrentPlayer));
        bool moveWasFound = false;
        while (!moveWasFound && currentPlayerPieces.Count > 0)
        {
            Piece pieceToMove = currentPlayerPieces[Random.Range(0, currentPlayerPieces.Count - 1)];

            List<Vector2Int> availableMoves = MoveSystem.AvailableMoves(pieceToMove.gameObject, this);

            if (availableMoves.Count > 0)
            {
                Vector2Int randomMovePosition = availableMoves[Random.Range(0, availableMoves.Count - 1)];
                Transform newParent = cells[randomMovePosition.x, randomMovePosition.y].transform;
                pieceToMove.Move(this, newParent);
                moveWasFound = true;
            }
            else
                currentPlayerPieces.Remove(pieceToMove);
        }
    }
    public bool ScanForCheck(int color, bool updateColor = false )
    {
        // Checks if a king is under attack in the current position
        bool isChecked = false;
        List<Piece> opponentsPieces = new List<Piece>();
        opponentsPieces.AddRange(ActivePieces.Where(n => n.Color == color ));

        List<Piece> currentPlayerPieces = new List<Piece>();
        currentPlayerPieces.AddRange(ActivePieces.Where(n => n.Color != color));

        Piece king = opponentsPieces.FirstOrDefault(t => t.PieceType == (int)Enums.PieceType.King).GetComponent<Piece>();
        foreach (Piece piece in currentPlayerPieces)
        {    
            List<Vector2Int> moveList = MoveSystem.AvailableMoves(piece.gameObject, this, true);

            if (moveList.Contains(king.GetComponentInParent<Cell>().BoardPosition))
            {
                isChecked = true;
                    break;
            }
        }
        if (updateColor)
        {
            Image kingsImage = king.GetComponent<Image>();
            if (isChecked && kingsImage.color != Color.yellow)
                kingsImage.color = Color.yellow;
            else if (!isChecked /*&& kingsImage.color != Color.white*/)
                kingsImage.color = king.Color == (int)Enums.Color.White ? WhitePieceColor : BlackPieceColor;
        }
        return isChecked;
    }

    public bool ScanForMate(int color)
    {
        // Checks if the the current player has no available moves
        // needs to be used in conjunction with "scan for check" to find checkmate
        List<Piece> currentPlayerPieces = new List<Piece>();
        currentPlayerPieces.AddRange(ActivePieces.Where(n => n.Color == color));
        int possibleMoves = 0;

        for (int i = 0; i < currentPlayerPieces.Count /*&& possibleMoves == 0*/; i++)
        {
            possibleMoves += MoveSystem.AvailableMoves(currentPlayerPieces[i].gameObject, this).Count;
        }

        if (possibleMoves == 0)
        {
            currentPlayerPieces.FirstOrDefault(t => t.PieceType == (int)Enums.PieceType.King).GetComponent<Image>().color = Color.red;
        }

        return possibleMoves == 0;
    }
    #endregion
}
