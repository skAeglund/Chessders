using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator : MonoBehaviour
{
    public static string GetStockfishMove(string FENstring, int difficulty = 1)
    {
        var process = new System.Diagnostics.Process();
        //p.StartInfo.FileName = "C:\\Users\\AndersHägglund\\Unity projects\\Chessders\\Assets\\Applications\\stockfishExecutable.exe";

        //process.StartInfo.FileName = Application.dataPath + "/Applications/stockfishExecutable.exe";

        process.StartInfo.FileName = Application.streamingAssetsPath + "/stockfishExecutable.exe";

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        process.StandardInput.WriteLine($"setoption name Skill Level value {difficulty}");
        string setupString = "position fen " + FENstring;
        process.StandardInput.WriteLine(setupString);

        string processString = $"go depth {difficulty}";

        process.StandardInput.WriteLine(processString);

        string stockfishMove = null;
        while (!process.StandardOutput.EndOfStream)
        {
            
            stockfishMove = process.StandardOutput.ReadLine();
            if (stockfishMove.Contains("bestmove"))
                break;
        }
        if (!stockfishMove.Contains("bestmove"))
        {
            process.CloseMainWindow();
            return "fail";
        }
        //Debug.Log(stockfishMove);
        string[] words = stockfishMove.Split(' ');
        stockfishMove = words[1];

        //p.Close();
        process.CloseMainWindow();

        return stockfishMove;
    }
}
