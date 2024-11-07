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

    // Start is called before the first frame update
    void Start()
    {
        
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
        IsTakeCover = false;
        if (target != null && target.IsAlive && !target.IsTakeCover)
        {
            if (!deathList.Contains(target.playerName))
            {
                //target.IsAlive = false;
                deathList.Add(target.playerName);
                Debug.Log($"{this.playerName}��(��) {target.playerName}��(��) �����Ͽ� �����߽��ϴ�.");
            }
        }
        else if(target != null && target.IsAlive && target.IsTakeCover)
        {
            Debug.Log($"{this.playerName}��(��) {target.playerName}��(��) �����Ͽ����� ���߽��ϴ�.");
        }
    }

    void ThrowGrenade()
    {
        // ����ź: ������ ��ġ�� ����, �ش� ��ġ�� �ִ� ��밡 ������ ����
        AIPlayer target = GetRandomOpponent();
        IsTakeCover = false;
        if (target != null && target.IsAlive && target.IsTakeCover)
        {
            if (!deathList.Contains(target.playerName))
            {
                //target.IsAlive = false;
                deathList.Add(target.playerName);
                Debug.Log($"{this.playerName}��(��) {target.playerName}�� ��ġ�� ����ź�� ���� �����߽��ϴ�.");
            }
        }
        else if (target != null && target.IsAlive && !target.IsTakeCover)
        {
            Debug.Log($"{this.playerName}��(��) {target.playerName}�� ��ġ�� ����ź�� ������ ������ �����Ǿ����ϴ�.");
        }
    }

    void TakeCover()
    {
        // ����: �ش� �� ���� ��ȣ ���·� ����
        IsTakeCover = true;
        Debug.Log($"{this.playerName}��(��) ���� ���Դϴ�.");
    }

    AIPlayer GetRandomOpponent()
    {
        List<AIPlayer> aliveOpponents = opponents.FindAll(player => player != this && player.IsAlive);
        if (aliveOpponents.Count == 0) return null;
        return aliveOpponents[Random.Range(0, aliveOpponents.Count)];
    }
}
