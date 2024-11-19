using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public InputField turnsInputField; // ���� �Է��� ���� InputField
    public int turns = 2; // �÷��̾ �Է��� �� ��
    private int actionturns = 0;
    private int currentTurn = 0;
    private List<AIPlayer> aiPlayers = new List<AIPlayer>();
    public GameObject aiPlayerPrefab; // AI �÷��̾��� ������ ������Ʈ
    public GameObject TrenchPrefab; //��ȣ ������ ������Ʈ
    public Transform[] spawnPoints; // AI �÷��̾� ��ȯ ��ġ

    public Button startButton; // ���� ��ư
    private char[] playerNames = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' }; // AI �÷��̾� �̸� �迭
    private List<string> ranklist = new List<string>(); // ������ �̸��� ������ ����Ʈ
    private int rank = 1;

    public GameObject GensePrefab; // Gense ������
    public GameObject Gense_A; // Gense_A ������
    public GameObject Gense_C; // Gense_C ������
    public GameObject Gense_G; // Gense_G ������

    // Start is called before the first frame update
    void Start()
    {
        // Null üũ �߰�
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
        startButton.onClick.AddListener(OnStartButtonClicked); // Start ��ư Ŭ�� ������
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnStartButtonClicked()
    {
        // ���� �Է� �� ��������
        if (!string.IsNullOrEmpty(turnsInputField.text) && int.TryParse(turnsInputField.text, out int userTurns))
        {
            turns = Mathf.Max(1, userTurns); // �ּ� 1�� ����
            Debug.Log($"Turns set to {turns}");
            turnsInputField.gameObject.SetActive(false); // InputField �����
            startButton.gameObject.SetActive(false);    // Start ��ư �����
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
            // ������ ��ġ�� AI �÷��̾� ������Ʈ ��ȯ
            GameObject aiObject = Instantiate(aiPlayerPrefab, spawnPoints[i].position, Quaternion.identity);
            AIPlayer player = aiObject.GetComponent<AIPlayer>();
            //������ ����
            player.trenchPrefab = TrenchPrefab;
            player.GensePrefab = GensePrefab;
            player.Gense_A = Gense_A;
            player.Gense_C = Gense_C;
            player.Gense_G = Gense_G;

            player.Initialize(aiPlayers);
            player.SetName(playerNames[i].ToString()); // AI �÷��̾ �̸� ����
            aiPlayers.Add(player);

            // Gense �ð�ȭ
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

                        // ���� �׼ǿ� �ش��ϴ� Gense ���� ����
                        player.HighlightCurrentAction(actionIndex);

                    }
                }

                foreach (string deadPlayerName in AIPlayer.deathList)
                {
                    AIPlayer deadPlayer = aiPlayers.Find(player => player.playerName == deadPlayerName);
                    if (deadPlayer != null)
                    {
                        deadPlayer.IsAlive = false;                        
                        deadPlayer.UpdateVisibility(); // IsAlive ���� ���� �� ���ü� ������Ʈ
                        ranklist.Add($"{deadPlayer.playerName}: {rank}��"); // �̸��� ������ �Բ� �߰�
                        Debug.Log($"{deadPlayer.playerName} has been ranked {rank}");
                    }
                }

                // Clear deathList after updating IsAlive status
                rank = rank + AIPlayer.deathList.Count;
                AIPlayer.deathList.Clear();

                actionturns++;

                yield return new WaitForSeconds(1.0f); // �� �� ���̿� 1�� ���

                if (aiPlayers.FindAll(player => player.IsAlive).Count <= 1)
                {
                    if (aiPlayers.FindAll(player => player.IsAlive).Count == 1)
                    {
                        Debug.Log("Game Over. Winner: " + aiPlayers.Find(player => player.IsAlive)?.playerName);

                        ranklist.Add($"{aiPlayers.Find(player => player.IsAlive)?.playerName}: {8}��");
                    }
                    else
                    {
                        Debug.Log("Game Over. No clear winner.");
                    }
                    Debug.Log("Final Rank List: " + string.Join(" ", ranklist));
                    yield return StartCoroutine(ExtractGenesFromRank()); // ���� ���� �� ������ ���� ����
                }
            }
            // ��� AI �÷��̾� �ٽ� �츮��
            foreach (AIPlayer player in aiPlayers)
            {
                player.IsAlive = true;
                player.IsTakeCover = false;
                player.SwitchToAIPlayer(); // ��� �÷��̾ AI Player ���������� ����
            }

            // ���� �� ������ ���� �ʱ�ȭ
            currentTurn++;
            actionturns = 0;
            rank = 1;
            ranklist.Clear();
            // ������Ʈ�� �����ڷ� Gense �ð�ȭ ����
            foreach (AIPlayer player in aiPlayers)
            {
                if (player.IsAlive)
                {
                    Transform spawnPoint = spawnPoints[aiPlayers.IndexOf(player)];
                    player.VisualizeGenes(spawnPoint);
                }
            }
            // ���� �� ����
            //Debug.Log($"All AI players revived. Proceeding to next turn {currentTurn + 1}");

            yield return new WaitForSeconds(1.0f); // ���� �� ���� �� ���
        }
        Debug.Log("All turns completed.");
        // reset
        currentTurn = 0;
        actionturns = 0;
        rank = 1;
        ranklist.Clear();
        turnsInputField.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true); // ��� �� ���� �� Start ��ư �ٽ� Ȱ��ȭ
    }

    IEnumerator ExtractGenesFromRank()
    {
        List<AIPlayer.ActionType> newGenes = new List<AIPlayer.ActionType>();

        // ��� ������ �켱�� �� ���
        int totalPriority = 0;
        foreach (string rankedPlayer in ranklist)
        {
            string[] parts = rankedPlayer.Split(':');
            int playerRank = int.Parse(parts[1].Replace("��", "").Trim());
            totalPriority += playerRank;
        }

        // �귿�� ����� ����Ͽ� ������ ����
        for (int i = 0; i < 8; i++)
        {
            int randomValue = Random.Range(0, totalPriority);
            int cumulativePriority = 0;

            foreach (string rankedPlayer in ranklist)
            {
                string[] parts = rankedPlayer.Split(':');
                char playerName = parts[0][0];
                int playerRank = int.Parse(parts[1].Replace("��", "").Trim());
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

        // ���ο� �����ڸ� AI �÷��̾�鿡�� 8���� ������ �Ҵ��ϰ� �ð�ȭ
        for (int i = 0; i < aiPlayers.Count; i++)
        {
            AIPlayer player = aiPlayers[i];
            player.genes = newGenes.GetRange(i * 8, 8).ToArray();
            Debug.Log($"{player.playerName}: genes: " + string.Join(", ", player.genes));

            // ������ �ð�ȭ ������Ʈ
            Transform spawnPoint = spawnPoints[i]; // �� �÷��̾��� ��ȯ ��ġ
            player.VisualizeGenes(spawnPoint);

            // 0.1�� ���
            //yield return new WaitForSeconds(0.1f);
        }

        // 3���� AI �÷��̾� ���� �� ���� ����
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
            // ������ ũ��� ��ġ Ȯ��
            Transform parent1Spawn = spawnPoints[aiPlayers.IndexOf(parent1)];
            Transform parent2Spawn = spawnPoints[aiPlayers.IndexOf(parent2)];

            // �ð�ȭ �غ�
            parent1.VisualizeGenes(parent1Spawn);
            parent2.VisualizeGenes(parent2Spawn);

            parent1.currentGenseInstance.transform.localScale = new Vector3(8, 8, 1);
            parent2.currentGenseInstance.transform.localScale = new Vector3(8, 8, 1);

            parent1.currentGenseInstance.transform.position = new Vector3(0, 3, parent1Spawn.position.z);
            parent2.currentGenseInstance.transform.position = new Vector3(0, -3, parent2Spawn.position.z);

            // ������ ����
            int crossoverPoint = Random.Range(1, parent1.genes.Length - 1);

            // ���� ���� ǥ��
            parent1.HighlightCrossAction(crossoverPoint); // �ش� ������ ������ yellow��
            parent2.HighlightCrossAction(crossoverPoint); // �ش� ������ ������ yellow��

            yield return new WaitForSeconds(1.0f);

            // ���� ����
            for (int i = crossoverPoint; i < parent1.genes.Length; i++)
            {
                AIPlayer.ActionType tempGene = parent1.genes[i];
                parent1.genes[i] = parent2.genes[i];
                parent2.genes[i] = tempGene;
            }

            Debug.Log($"Crossover between {parent1.playerName} and {parent2.playerName} at point {crossoverPoint}");
            Debug.Log($"{parent1.playerName} genes after crossover: " + string.Join(", ", parent1.genes));
            Debug.Log($"{parent2.playerName} genes after crossover: " + string.Join(", ", parent2.genes));

            // ������ ũ�� �� ��ġ ����
            parent1.currentGenseInstance.transform.localScale = new Vector3(3, 3, 1);
            parent2.currentGenseInstance.transform.localScale = new Vector3(3, 3, 1);

            parent1.currentGenseInstance.transform.position = parent1Spawn.position;
            parent2.currentGenseInstance.transform.position = parent2Spawn.position;

            // ������ �ð�ȭ ������Ʈ
            parent1.VisualizeGenes(parent1Spawn);
            parent2.VisualizeGenes(parent2Spawn);
        }

        // �� AI �÷��̾��� �����ڿ� ���� �������� ���� (5% Ȯ��)
        foreach (AIPlayer player in aiPlayers)
        {
            // ���� ������ ����� �� �ð�ȭ
            Transform spawnPoint = spawnPoints[aiPlayers.IndexOf(player)]; // �÷��̾� ��ġ ��������

            // �������� �߻� ���� ����Ʈ
            List<int> mutatedIndices = new List<int>();

            for (int i = 0; i < player.genes.Length; i++)
            {
                if (Random.value < 0.05f) // 5% Ȯ���� �������� �߻�
                {
                    AIPlayer.ActionType oldGene = player.genes[i];
                    AIPlayer.ActionType newGene;
                    do
                    {
                        newGene = (AIPlayer.ActionType)Random.Range(0, 3);
                    } while (newGene == oldGene);

                    player.genes[i] = newGene;
                    Debug.Log($"Mutation {player.playerName} index {i}: {oldGene} -> {newGene}");

                    // �������� �ε��� �߰�
                    mutatedIndices.Add(i);
                }
            }
            player.VisualizeGenes(spawnPoint);
            // ��� �������� �ð�ȭ ó��
            foreach (int mutatedIndex in mutatedIndices)
            {
                player.HighlightNewAction(mutatedIndex); // ������ �������� �ð�ȭ
            }
        }
        
        yield return new WaitForSeconds(1.0f);
    }


}
