using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int turns = 2; // 플레이어가 입력한 턴 수
    private int actionturns = 0;
    private int currentTurn = 0;
    private List<AIPlayer> aiPlayers = new List<AIPlayer>();
    public GameObject aiPlayerPrefab; // AI 플레이어의 프리팹 오브젝트
    public GameObject TrenchPrefab; //참호 프리팹 오브젝트
    public Transform[] spawnPoints; // AI 플레이어 소환 위치

    public Button startButton; // 시작 버튼
    private char[] playerNames = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' }; // AI 플레이어 이름 배열

    private List<string> ranklist = new List<string>(); // 순위와 이름을 저장할 리스트
    private int rank = 1;

    // Start is called before the first frame update
    void Start()
    {
        // Null 체크 추가
        if (startButton == null)
        {
            Debug.LogError("Start Button is not assigned in the inspector.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn Points are not assigned or empty in the inspector.");
            return;
        }

        InitializePlayers();
        startButton.onClick.AddListener(StartGame); // 스타트 버튼 클릭 시 게임 시작
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializePlayers()
    {
        for (int i = 0; i < 8; i++)
        {
            // 지정된 위치에 AI 플레이어 오브젝트 소환
            GameObject aiObject = Instantiate(aiPlayerPrefab, spawnPoints[i].position, Quaternion.identity);
            AIPlayer player = aiObject.GetComponent<AIPlayer>();
            player.Initialize(aiPlayers);
            player.SetName(playerNames[i].ToString()); // AI 플레이어에 이름 설정
            aiPlayers.Add(player);
        }
    }
    void StartGame()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while ((turns - currentTurn) > 0)
        {
            Debug.Log($"Trun {currentTurn + 1} starts");
            while (aiPlayers.FindAll(player => player.IsAlive).Count > 1)
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
                foreach (string deadPlayerName in AIPlayer.deathList)
                {
                    AIPlayer deadPlayer = aiPlayers.Find(player => player.playerName == deadPlayerName);
                    if (deadPlayer != null)
                    {
                        deadPlayer.IsAlive = false;
                        ranklist.Add($"{deadPlayer.playerName}: {rank}위"); // 이름과 순위를 함께 추가
                        Debug.Log($"{deadPlayer.playerName} has been ranked {rank}");
                    }
                }

                // Clear deathList after updating IsAlive status
                rank = rank + AIPlayer.deathList.Count;
                AIPlayer.deathList.Clear();

                actionturns++;

                yield return new WaitForSeconds(1.0f); // 각 턴 사이에 1초 대기

                if (aiPlayers.FindAll(player => player.IsAlive).Count <= 1)
                {
                    if (aiPlayers.FindAll(player => player.IsAlive).Count == 1)
                    {
                        Debug.Log("Game Over. Winner: " + aiPlayers.Find(player => player.IsAlive)?.playerName);

                        ranklist.Add($"{aiPlayers.Find(player => player.IsAlive)?.playerName}: {8}위");
                    }
                    else
                    {
                        Debug.Log("Game Over. No clear winner.");
                    }
                    Debug.Log("Final Rank List: " + string.Join(" ", ranklist));
                    ExtractGenesFromRank(); // 게임 종료 후 유전자 추출 실행

                    
                }
            }
            // 모든 AI 플레이어 다시 살리기
            foreach (AIPlayer player in aiPlayers)
            {
                player.IsAlive = true;
                player.IsTakeCover = false;
            }

            // 다음 턴 진행을 위해 초기화
            currentTurn++;
            actionturns = 0;
            rank = 1;
            ranklist.Clear();

            // 다음 턴 진행
            //Debug.Log($"All AI players revived. Proceeding to next turn {currentTurn + 1}");

            yield return new WaitForSeconds(1.0f); // 다음 턴 시작 전 대기
        }
        Debug.Log("All turns completed.");
    }

    void ExtractGenesFromRank()
    {
        List<AIPlayer.ActionType> newGenes = new List<AIPlayer.ActionType>();

        // 모든 순위의 우선도 합 계산
        int totalPriority = 0;
        foreach (string rankedPlayer in ranklist)
        {
            string[] parts = rankedPlayer.Split(':');
            int playerRank = int.Parse(parts[1].Replace("위", "").Trim());
            totalPriority += playerRank;
        }

        // 룰렛휠 방식을 사용하여 유전자 추출
        for (int i = 0; i < 8; i++)
        {
            int randomValue = Random.Range(0, totalPriority);
            int cumulativePriority = 0;

            foreach (string rankedPlayer in ranklist)
            {
                string[] parts = rankedPlayer.Split(':');
                char playerName = parts[0][0];
                int playerRank = int.Parse(parts[1].Replace("위", "").Trim());
                cumulativePriority += playerRank;

                if (randomValue < cumulativePriority)
                {
                    AIPlayer player = aiPlayers.Find(p => p.playerName == playerName.ToString());
                    if (player != null)
                    {
                        newGenes.AddRange(player.genes);
                    }
                    break;
                }
            }
        }

        // 새로운 유전자를 AI 플레이어들에게 8개씩 나누어 할당하고 로그 출력
        for (int i = 0; i < aiPlayers.Count; i++)
        {
            AIPlayer player = aiPlayers[i];
            player.genes = newGenes.GetRange(i * 8, 8).ToArray();
            Debug.Log($"{player.playerName}: genes: " + string.Join(", ", player.genes));
        }

        // 3쌍의 AI 플레이어 추출 및 교차 수행
        List<(AIPlayer, AIPlayer)> pairs = new List<(AIPlayer, AIPlayer)>();
        while (pairs.Count < 3)
        {
            AIPlayer parent1 = aiPlayers[Random.Range(0, aiPlayers.Count)];
            AIPlayer parent2 = aiPlayers[Random.Range(0, aiPlayers.Count)];

            if (parent1 != parent2 && !pairs.Contains((parent1, parent2)) && !pairs.Contains((parent2, parent1)))
            {
                pairs.Add((parent1, parent2));
            }
        }

        foreach (var (parent1, parent2) in pairs)
        {
            // 교차: 부모들의 유전자 배열 중 절반을 교환
            int crossoverPoint = Random.Range(1, parent1.genes.Length - 1);
            for (int i = crossoverPoint; i < parent1.genes.Length; i++)
            {
                AIPlayer.ActionType tempGene = parent1.genes[i];
                parent1.genes[i] = parent2.genes[i];
                parent2.genes[i] = tempGene;
            }

            Debug.Log($"Crossover between {parent1.playerName} and {parent2.playerName} at point {crossoverPoint}");
            Debug.Log($"{parent1.playerName} genes after crossover: " + string.Join(", ", parent1.genes));
            Debug.Log($"{parent2.playerName} genes after crossover: " + string.Join(", ", parent2.genes));
        }

        // 각 AI 플레이어의 유전자에 대해 돌연변이 발현 (5% 확률)
        foreach (AIPlayer player in aiPlayers)
        {
            for (int i = 0; i < player.genes.Length; i++)
            {
                if (Random.value < 0.05f) // 5% 확률로 돌연변이 발생
                {
                    AIPlayer.ActionType oldGene = player.genes[i];
                    AIPlayer.ActionType newGene;
                    do
                    {
                        newGene = (AIPlayer.ActionType)Random.Range(0, 3);
                    } while (newGene == oldGene);

                    player.genes[i] = newGene;
                    Debug.Log($"Mutation {player.playerName} index {i}: {oldGene} -> {newGene}");
                }
            }
        }
    }
}
