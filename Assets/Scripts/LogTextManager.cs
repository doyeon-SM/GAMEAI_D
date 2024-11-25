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
        // �α� ���� ��� ����
        logFilePath = Application.dataPath + "/GameLog.txt";

        // ���� �α� ���� �ʱ�ȭ 
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        // ��ΰ� ��ȿ���� Ȯ��
        if (string.IsNullOrEmpty(logFilePath))
        {
            Debug.LogError("logFilePath is null or empty. Check your initialization.");
        }
        else
        {
            Debug.Log($"�α� ���� ���: {logFilePath}");
        }
    }
    public static void LogText(int turn, string messageType, string player = null, string geneData = null)
    {
        string logMessage = "";

        if (messageType == "Gene")
        {
            // ������ �α� ����
            logMessage = $"[Turn {turn}]\t[Player {player}]\t������:{geneData}";
        }
        else if (messageType == "Win")
        {
            // �¸� �α� ����
            logMessage = $"[Turn {turn}]\t[WIN]\t[Player {player}]";
        }
        else if(messageType == "Draw")
        {
            logMessage = $"[Turn {turn}]\t[DRAW]";
        }

        // �α� ���
        Debug.Log(logMessage);

        // �α׸� ���Ͽ� ���
        AppendLogToFile(logMessage);
    }

    public static void geneText(string messageType, string player, string geneData = null, int crossindex = 0)
    {
        string logMessage = "";

        if (messageType == "Gene")
        {
            logMessage = $"[New Gene]\t[Player {player}]\t������:{geneData}";
        }
        else if(messageType == "Cross")
        {
            logMessage = $"[Cross Gene]\t[Player {player}]\t[�������� {crossindex+1}]\t������:{geneData}";
        }
        else if(messageType == "Mutation")
        {
            logMessage = $"[Mutation Gene]\t[Player {player}]\t[������������ {crossindex+1}]\t{geneData}";
        }
        // �α� ���
        Debug.Log(logMessage);

        // �α׸� ���Ͽ� ���
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
