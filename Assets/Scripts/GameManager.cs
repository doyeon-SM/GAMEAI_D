using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int turns = 1; // �÷��̾ �Է��� �� ��
    private int actionturns = 0;
    private int currentTurn = 0;
    private List<AIPlayer> aiPlayers = new List<AIPlayer>();
    public GameObject aiPlayerPrefab; // AI �÷��̾��� ������ ������Ʈ
    public Transform[] spawnPoints; // AI �÷��̾� ��ȯ ��ġ
    private char[] playerNames = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' }; // AI �÷��̾� �̸� �迭

    // Start is called before the first frame update
    void Start()
    {
        InitializePlayers();
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializePlayers()
    {
        for (int i = 0; i < 8; i++)
        {
            // ������ ��ġ�� AI �÷��̾� ������Ʈ ��ȯ
            GameObject aiObject = Instantiate(aiPlayerPrefab, spawnPoints[i].position, Quaternion.identity);
            AIPlayer player = aiObject.GetComponent<AIPlayer>();
            player.Initialize(aiPlayers);
            player.SetName(playerNames[i].ToString()); // AI �÷��̾ �̸� ����
            aiPlayers.Add(player);
        }
    }

    IEnumerator StartGame()
    {
        Debug.Log($"Trun {currentTurn+1} starts");
        while ((turns-currentTurn) > 0 && aiPlayers.FindAll(player => player.IsAlive).Count > 1)
        {
            Debug.Log($"Action {actionturns + 1} starts");

            foreach (AIPlayer player in aiPlayers)
            {
                if (player.IsAlive)
                {
                    int actionIndex = actionturns % player.genes.Length;
                    player.Act(actionIndex);
                }
            }

            actionturns++;

            yield return new WaitForSeconds(1.0f); // �� �� ���̿� 1�� ���

            if (aiPlayers.FindAll(player => player.IsAlive).Count <= 1)
            {
                if (aiPlayers.FindAll(player => player.IsAlive).Count == 1)
                {
                    Debug.Log("Game Over. Winner: " + aiPlayers.Find(player => player.IsAlive)?.playerName);
                    currentTurn++;
                    actionturns = 0;
                }
                else
                {
                    Debug.Log("Game Over. No clear winner.");
                    currentTurn++;
                    actionturns = 0;
                }
            }
        }

    }
}
