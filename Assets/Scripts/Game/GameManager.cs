using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public Piece SelectedPiece { get; set; } = null;
    public int HalfMoveClock { get; set; } // "The number of halfmoves since the last capture or pawn advance"
    public int FullMoveNumber { get; set; } = 1; // "The number of the full move. It starts at 1, and is incremented after Black's move."
    public int Difficulty { get; set; } = 1; // easy = 1, medium = 3, hard = 10
    public int PlayerColor { get; set; } = -1; // -1 as null, player chooses 0 (white) or 1 (black)
    public bool IsGameOver { get; set; } = false;
    public bool IsQuitting { get; set; } = false;


    [SerializeField] private GameObject uiParent;
    [SerializeField] private GameObject gameOverPrefab;
    [SerializeField] private Board boardObject;
    [SerializeField] private GameObject chooseColorParent;
    private Board board;
    private AudioManager audioManager;
    private static List<string> positionList = new List<string>();
    private HudComponents hudComponents;


    private void Awake()
    {
        audioManager = GameObject.Find("AudioSource").GetComponent<AudioManager>();
        board = boardObject.GetComponent<Board>();
        hudComponents = uiParent.GetComponent<HudComponents>();
    }

    public void SetDifficulty (int newDifficulty)
    {
        Difficulty = newDifficulty;
    }

    public void SetGameOver(int winner)
    {
        if (winner < 2) //index 2,3 = draws
        {
            if (winner == PlayerColor)
                winner = 0;
            else
                winner = 1;
        }
        IsGameOver = true;
        GameObject gameOverUI = Instantiate(gameOverPrefab, uiParent.transform);
        Button restartButton = gameOverUI.transform.GetChild(gameOverUI.transform.childCount - 1).GetComponent<Button>();
        Button quitButton = gameOverUI.transform.GetChild(gameOverUI.transform.childCount - 2).GetComponent<Button>();

        quitButton.onClick.AddListener(QuitGame);
        restartButton.onClick.AddListener(RestartGame);
        
        gameOverUI.transform.GetChild(winner).gameObject.SetActive(true);
        gameOverUI.transform.GetChild(winner).GetChild(0).GetComponent<Canvas>().overrideSorting = true;
        audioManager.PlayGameOverSFX();
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void RestartGame()
    {
        IsQuitting = true;
        positionList.Clear();
        SceneManager.LoadScene("MainScene");
    }
    private void OnApplicationQuit()
    {
        IsQuitting = true;
    }

    private void CheckForThreefoldRepetition()
    {
        foreach(string position in positionList)
        {
            if (positionList.Where(x => x == position).ToList().Count >= 3)
            {
                Debug.Log("Same position repeated three times.");
                SetGameOver(3);
                break;
            }
        }
    }

    public void AddPositionToList()
    {
        string[] words = FENgenerator.GenerateFen(board, this).Split(' ');
        string position = words[0] + $" {words[1]}";
        //Debug.Log(position);
        positionList.Add(position);

        CheckForThreefoldRepetition();
    }

    public void SetPlayerColor(int color)
    {
        PlayerColor = color;
        if (PlayerColor == (int)Enums.Color.Black)
        {
            board.RotateBoard();
            board.CurrentPlayer = 1;
            board.SwapPlayer();
        }
        Destroy(chooseColorParent);

        hudComponents.Create();
    }
}
