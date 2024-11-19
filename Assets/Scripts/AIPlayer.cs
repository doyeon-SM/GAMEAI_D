using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public enum ActionType { Attack, Grenade, Cover }
    public ActionType[] genes;
    public bool IsAlive = true;
    public bool IsTakeCover = false;
    public float fitness;
    private List<AIPlayer> opponents;
    public string playerName;
    public static List<string> deathList = new List<string>();

    public GameObject trenchPrefab; // ��ȣ �������� ������ ����
    private GameObject currentPrefab; // ���� ǥ�õǴ� ������ (AI Player �Ǵ� Trench)

    private GameObject currentGenseInstance; // ���� GensePrefab�� �ν��Ͻ�
    public GameObject GensePrefab; // Gense ������
    public GameObject Gense_A; // Gense_A ������
    public GameObject Gense_C; // Gense_C ������
    public GameObject Gense_G; // Gense_G ������
    // Start is called before the first frame update
    void Start()
    {
        currentPrefab = this.gameObject; // �ʱ� �������� AI Player �ڽ�
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(List<AIPlayer> allPlayers)
    {
        genes = new ActionType[8]; // 8���� �ൿ ������
        opponents = allPlayers;
        InitializeGenes();
    }

    void InitializeGenes()
    {
        for (int i = 0; i < genes.Length; i++)
        {
            genes[i] = (ActionType)Random.Range(0, 3); // 3���� �ൿ �� �ϳ� ���� ����
        }
    }
    public void SetName(string name)
    {
        playerName = name;
        //Debug.Log($"AI Player {playerName} initialized.");
    }
    public void UpdateVisibility()
    {
        Uncover();
        gameObject.SetActive(IsAlive);
    }

    public void Act(int actionIndex)
    {
        if (!IsAlive) return;

        ActionType currentAction = genes[actionIndex % genes.Length];
        switch (currentAction)
        {
            case ActionType.Cover:
                TakeCover();
                break;
            case ActionType.Attack:
                Attack();
                break;
            case ActionType.Grenade:
                ThrowGrenade();
                break;
        }
    }
    void Attack()
    {
        // ����: �������� ����ִ� ��븦 �����ϰ� ����
        AIPlayer target = GetRandomOpponent();
        Uncover();
        if (target != null && target.IsAlive && !target.IsTakeCover)
        {
            if (!deathList.Contains(target.playerName))
            {
                //target.IsAlive = false;
                deathList.Add(target.playerName);
                //Debug.Log($"{this.playerName}��(��) {target.playerName}��(��) �����Ͽ� �����߽��ϴ�.");
            }
        }
        else if(target != null && target.IsAlive && target.IsTakeCover)
        {
            //Debug.Log($"{this.playerName}��(��) {target.playerName}��(��) �����Ͽ����� ���߽��ϴ�.");
        }
    }

    void ThrowGrenade()
    {
        // ����ź: ������ ��ġ�� ����, �ش� ��ġ�� �ִ� ��밡 ������ ����
        AIPlayer target = GetRandomOpponent();
        Uncover();
        
        if (target != null && target.IsAlive && target.IsTakeCover)
        {
            if (!deathList.Contains(target.playerName))
            {
                //target.IsAlive = false;
                deathList.Add(target.playerName);
                //Debug.Log($"{this.playerName}��(��) {target.playerName}�� ��ġ�� ����ź�� ���� �����߽��ϴ�.");
            }
        }
        else if (target != null && target.IsAlive && !target.IsTakeCover)
        {
            //Debug.Log($"{this.playerName}��(��) {target.playerName}�� ��ġ�� ����ź�� ������ ������ �����Ǿ����ϴ�.");
        }
    }

    void TakeCover()
    {
        // ����: �ش� �� ���� ��ȣ ���·� ����
        IsTakeCover = true;
        SwitchToTrench(); // Trench�� ����
        //Debug.Log($"{this.playerName}��(��) ���� ���Դϴ�.");
    }

    AIPlayer GetRandomOpponent()
    {
        List<AIPlayer> aliveOpponents = opponents.FindAll(player => player != this && player.IsAlive);
        if (aliveOpponents.Count == 0) return null;
        return aliveOpponents[Random.Range(0, aliveOpponents.Count)];
    }

    void Uncover()
    {
        IsTakeCover = false;
        SwitchToAIPlayer(); // AI Player�� ����
        //Debug.Log($"{playerName}��(��) ���п��� ������ϴ�.");
    }
    void SwitchToTrench()
    {
        if (trenchPrefab == null)
        {
            Debug.LogError($"{playerName}: Trench prefab is not assigned.");
            return;
        }

        // �̹� Trench�� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
        if (currentPrefab == trenchPrefab)
        {
            //Debug.Log($"{playerName}: Trench is already active.");
            return;
        }
        // ���� ������ ��Ȱ��ȭ �� Trench Prefab ����
        if (currentPrefab != null) currentPrefab.SetActive(false);
        // ���ο� Trench ������ ����
        currentPrefab = Instantiate(trenchPrefab, transform.position, transform.rotation, transform.parent);
        this.gameObject.SetActive(false); // AI Player ��Ȱ��ȭ
    }

    public void SwitchToAIPlayer()
    {
        if (currentPrefab != null && currentPrefab != this.gameObject)
        {
            Destroy(currentPrefab); // ��ȣ ������ ����
        }

        currentPrefab = this.gameObject;
        currentPrefab.SetActive(true); // AI Player ����
    }

    public void VisualizeGenes(Transform spawnPoint)
    {
        // ���� GensePrefab ����
        if (currentGenseInstance != null)
        {
            Destroy(currentGenseInstance);
        }

        // GensePrefab�� �÷��̾� ��ġ �Ʒ��� ����
        Vector3 gensePosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y - 1, spawnPoint.position.z);
        currentGenseInstance = Instantiate(GensePrefab, gensePosition, Quaternion.identity);

        // GensePrefab�� �� Point�� Gense_A, Gense_C, Gense_G ��ġ
        for (int i = 0; i < genes.Length; i++)
        {
            string gensePointName = $"gensepoint{i + 1}";
            Transform gensePoint = currentGenseInstance.transform.Find(gensePointName);

            if (gensePoint != null)
            {
                GameObject genseInstance = null;
                switch (genes[i])
                {
                    case ActionType.Attack:
                        genseInstance = Instantiate(Gense_A, gensePoint.position, Quaternion.identity, gensePoint);
                        break;
                    case ActionType.Cover:
                        genseInstance = Instantiate(Gense_C, gensePoint.position, Quaternion.identity, gensePoint);
                        break;
                    case ActionType.Grenade:
                        genseInstance = Instantiate(Gense_G, gensePoint.position, Quaternion.identity, gensePoint);
                        break;
                }

                if (genseInstance != null)
                {
                    genseInstance.transform.localPosition = Vector3.zero; // �θ� ���� ��ġ�� �ʱ�ȭ
                }
            }
        }
    }

}
