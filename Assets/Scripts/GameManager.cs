using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public InputField turnsInputField; // 유저 입력을 받을 InputField
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

    public GameObject GensePrefab; // Gense 프리팹
    public GameObject Gense_A; // Gense_A 프리팹
    public GameObject Gense_C; // Gense_C 프리팹
    public GameObject Gense_G; // Gense_G 프리팹

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
        startButton.onClick.AddListener(OnStartButtonClicked); // Start 버튼 클릭 리스너
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnStartButtonClicked()
    {
        // 유저 입력 값 가져오기
        if (!string.IsNullOrEmpty(turnsInputField.text) && int.TryParse(turnsInputField.text, out int userTurns))
        {
            turns = Mathf.Max(1, userTurns); // 최소 1로 제한
            Debug.Log($"Turns set to {turns}");
            turnsInputField.gameObject.SetActive(false); // InputField 숨기기
            startButton.gameObject.SetActive(false);    // Start 버튼 숨기기
            StartCoroutine(GameLoop());
        }
        else
        {
            Debug.LogError("Invalid input for turns. Please enter a valid number.");
        }
    }

    void InitializePlayers()
    {
        for (int i = 0; i < 8; i++)
        {
            // 지정된 위치에 AI 플레이어 오브젝트 소환
            GameObject aiObject = Instantiate(aiPlayerPrefab, spawnPoints[i].position, Quaternion.identity);
            AIPlayer player = aiObject.GetComponent<AIPlayer>();
            //프리팹 설정
            player.trenchPrefab = TrenchPrefab;
            player.GensePrefab = GensePrefab;
            player.Gense_A = Gense_A;
            player.Gense_C = Gense_C;
            player.Gense_G = Gense_G;

            player.Initialize(aiPlayers);
            player.SetName(playerNames[i].ToString()); // AI 플레이어에 이름 설정
            aiPlayers.Add(player);

            // Gense 시각화
            player.VisualizeGenes(spawnPoints[i]);
        }
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

                        // 현재 액션에 해당하는 Gense 색상 강조
                        player.HighlightCurrentAction(actionIndex);

                    }
                }

                foreach (string deadPlayerName in AIPlayer.deathList)
                {
                    AIPlayer deadPlayer = aiPlayers.Find(player => player.playerName == deadPlayerName);
                    if (deadPlayer != null)
                    {
                        deadPlayer.IsAlive = false;                        
                        deadPlayer.UpdateVisibility(); // IsAlive 상태 변경 후 가시성 업데이트
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
                    yield return StartCoroutine(ExtractGenesFromRank()); // 게임 종료 후 유전자 추출 실행
                }
            }
            // 모든 AI 플레이어 다시 살리기
            foreach (AIPlayer player in aiPlayers)
            {
                player.IsAlive = true;
                player.IsTakeCover = false;
                player.SwitchToAIPlayer(); // 모든 플레이어를 AI Player 프리팹으로 복원
            }

            // 다음 턴 진행을 위해 초기화
            currentTurn++;
            actionturns = 0;
            rank = 1;
            ranklist.Clear();
            // 업데이트된 유전자로 Gense 시각화 갱신
            foreach (AIPlayer player in aiPlayers)
            {
                if (player.IsAlive)
                {
                    Transform spawnPoint = spawnPoints[aiPlayers.IndexOf(player)];
                    player.VisualizeGenes(spawnPoint);
                }
            }
            // 다음 턴 진행
            //Debug.Log($"All AI players revived. Proceeding to next turn {currentTurn + 1}");

            yield return new WaitForSeconds(1.0f); // 다음 턴 시작 전 대기
        }
        Debug.Log("All turns completed.");
        // reset
        currentTurn = 0;
        actionturns = 0;
        rank = 1;
        ranklist.Clear();
        turnsInputField.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true); // 모든 턴 종료 후 Start 버튼 다시 활성화
    }

    IEnumerator ExtractGenesFromRank()
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

        // 새로운 유전자를 AI 플레이어들에게 8개씩 나누어 할당하고 시각화
        for (int i = 0; i < aiPlayers.Count; i++)
        {
            AIPlayer player = aiPlayers[i];
            player.genes = newGenes.GetRange(i * 8, 8).ToArray();
            Debug.Log($"{player.playerName}: genes: " + string.Join(", ", player.genes));

            // 유전자 시각화 업데이트
            Transform spawnPoint = spawnPoints[i]; // 각 플레이어의 소환 위치
            player.VisualizeGenes(spawnPoint);

            // 0.1초 대기
            //yield return new WaitForSeconds(0.1f);
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
            // 유전자 크기와 위치 확장
            Transform parent1Spawn = spawnPoints[aiPlayers.IndexOf(parent1)];
            Transform parent2Spawn = spawnPoints[aiPlayers.IndexOf(parent2)];

            // 시각화 준비
            parent1.VisualizeGenes(parent1Spawn);
            parent2.VisualizeGenes(parent2Spawn);

            parent1.currentGenseInstance.transform.localScale = new Vector3(8, 8, 1);
            parent2.currentGenseInstance.transform.localScale = new Vector3(8, 8, 1);

            parent1.currentGenseInstance.transform.position = new Vector3(0, 3, parent1Spawn.position.z);
            parent2.currentGenseInstance.transform.position = new Vector3(0, -3, parent2Spawn.position.z);

            // 교차점 결정
            int crossoverPoint = Random.Range(1, parent1.genes.Length - 1);

            // 교차 색상 표시
            parent1.HighlightCrossAction(crossoverPoint); // 해당 유전자 색상을 yellow로
            parent2.HighlightCrossAction(crossoverPoint); // 해당 유전자 색상을 yellow로

            yield return new WaitForSeconds(1.0f);

            // 교차 수행
            for (int i = crossoverPoint; i < parent1.genes.Length; i++)
            {
                AIPlayer.ActionType tempGene = parent1.genes[i];
                parent1.genes[i] = parent2.genes[i];
                parent2.genes[i] = tempGene;
            }

            Debug.Log($"Crossover between {parent1.playerName} and {parent2.playerName} at point {crossoverPoint}");
            Debug.Log($"{parent1.playerName} genes after crossover: " + string.Join(", ", parent1.genes));
            Debug.Log($"{parent2.playerName} genes after crossover: " + string.Join(", ", parent2.genes));

            // 유전자 크기 및 위치 복구
            parent1.currentGenseInstance.transform.localScale = new Vector3(3, 3, 1);
            parent2.currentGenseInstance.transform.localScale = new Vector3(3, 3, 1);

            parent1.currentGenseInstance.transform.position = parent1Spawn.position;
            parent2.currentGenseInstance.transform.position = parent2Spawn.position;

            // 유전자 시각화 업데이트
            parent1.VisualizeGenes(parent1Spawn);
            parent2.VisualizeGenes(parent2Spawn);
        }

        // 각 AI 플레이어의 유전자에 대해 돌연변이 발현 (5% 확률)
        foreach (AIPlayer player in aiPlayers)
        {
            // 기존 유전자 재생성 및 시각화
            Transform spawnPoint = spawnPoints[aiPlayers.IndexOf(player)]; // 플레이어 위치 가져오기

            // 돌연변이 발생 추적 리스트
            List<int> mutatedIndices = new List<int>();

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

                    // 돌연변이 인덱스 추가
                    mutatedIndices.Add(i);
                }
            }
            player.VisualizeGenes(spawnPoint);
            // 모든 돌연변이 시각화 처리
            foreach (int mutatedIndex in mutatedIndices)
            {
                player.HighlightNewAction(mutatedIndex); // 누적된 돌연변이 시각화
            }
        }
        
        yield return new WaitForSeconds(1.0f);
    }


}
