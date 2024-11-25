using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LogTextManager : MonoBehaviour
{
    private static string logFilePath;

    private void Awake()
    {
        Debug.Log($"Persistent Data Path: {Application.dataPath}");
        // 로그 파일 경로 설정
        logFilePath = Application.dataPath + "/GameLog.txt";

        // 기존 로그 파일 초기화 
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        // 경로가 유효한지 확인
        if (string.IsNullOrEmpty(logFilePath))
        {
            Debug.LogError("logFilePath is null or empty. Check your initialization.");
        }
        else
        {
            Debug.Log($"로그 파일 경로: {logFilePath}");
        }
    }
    public static void LogText(int turn, string messageType, string player = null, string geneData = null)
    {
        string logMessage = "";

        if (messageType == "Gene")
        {
            // 유전자 로그 형식
            logMessage = $"[Turn {turn}]\t[Player {player}]\t유전자:{geneData}";
        }
        else if (messageType == "Win")
        {
            // 승리 로그 형식
            logMessage = $"[Turn {turn}]\t[WIN]\t[Player {player}]";
        }
        else if(messageType == "Draw")
        {
            logMessage = $"[Turn {turn}]\t[DRAW]";
        }

        // 로그 출력
        Debug.Log(logMessage);

        // 로그를 파일에 기록
        AppendLogToFile(logMessage);
    }

    public static void geneText(string messageType, string player, string geneData = null, int crossindex = 0)
    {
        string logMessage = "";

        if (messageType == "Gene")
        {
            logMessage = $"[New Gene]\t[Player {player}]\t유전자:{geneData}";
        }
        else if(messageType == "Cross")
        {
            logMessage = $"[Cross Gene]\t[Player {player}]\t[교차지점 {crossindex+1}]\t유전자:{geneData}";
        }
        else if(messageType == "Mutation")
        {
            logMessage = $"[Mutation Gene]\t[Player {player}]\t[돌연변이지점 {crossindex+1}]\t{geneData}";
        }
        // 로그 출력
        Debug.Log(logMessage);

        // 로그를 파일에 기록
        AppendLogToFile(logMessage);
    }

    public static void Text(string message)
    {
        AppendLogToFile(message);
    }


    private static void AppendLogToFile(string logMessage)
    {
        if (string.IsNullOrEmpty(logFilePath))
        {
            Debug.LogError("Log file path is not initialized. Cannot write to log file.");
            return;
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(logMessage);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to write log to file: {ex.Message}");
        }
    }
}
