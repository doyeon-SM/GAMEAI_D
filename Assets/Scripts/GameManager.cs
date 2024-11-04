using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int turns = 10; // 플레이어가 입력한 턴 수
    private int currentTurn = 0;
    private List<AIPlayer> aiPlayers = new List<AIPlayer>();

    // Start is called before the first frame update
    void Start()
    {
        InitializePlayers();
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializePlayers()
    {
        for (int i = 0; i < 8; i++)
        {
            AIPlayer player = new AIPlayer();
            aiPlayers.Add(player);
        }
    }

    void StartGame()
    {
        while (turns > 0)
        {
            //RunSimulation();
            //if (aiPlayers.Count(x => x.IsAlive) <= 1) break;
            //ApplyGeneticAlgorithm();
            //turns--;
        }
    }
}
