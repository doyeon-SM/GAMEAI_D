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
        genes = new ActionType[8]; // 8개의 행동 유전자
        opponents = allPlayers;
        InitializeGenes();
    }

    void InitializeGenes()
    {
        for (int i = 0; i < genes.Length; i++)
        {
            genes[i] = (ActionType)Random.Range(0, 3); // 3가지 행동 중 하나 랜덤 선택
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
        // 공격: 랜덤으로 살아있는 상대를 선택하고 공격
        AIPlayer target = GetRandomOpponent();
        IsTakeCover = false;
        if (target != null && target.IsAlive && !target.IsTakeCover)
        {
            if (!deathList.Contains(target.playerName))
            {
                //target.IsAlive = false;
                deathList.Add(target.playerName);
                Debug.Log($"{this.playerName}이(가) {target.playerName}을(를) 공격하여 제거했습니다.");
            }
        }
        else if(target != null && target.IsAlive && target.IsTakeCover)
        {
            Debug.Log($"{this.playerName}이(가) {target.playerName}을(를) 공격하였으나 피했습니다.");
        }
    }

    void ThrowGrenade()
    {
        // 수류탄: 무작위 위치에 던짐, 해당 위치에 있는 상대가 있으면 제거
        AIPlayer target = GetRandomOpponent();
        IsTakeCover = false;
        if (target != null && target.IsAlive && target.IsTakeCover)
        {
            if (!deathList.Contains(target.playerName))
            {
                //target.IsAlive = false;
                deathList.Add(target.playerName);
                Debug.Log($"{this.playerName}이(가) {target.playerName}의 위치에 수류탄을 던져 제거했습니다.");
            }
        }
        else if (target != null && target.IsAlive && !target.IsTakeCover)
        {
            Debug.Log($"{this.playerName}이(가) {target.playerName}의 위치에 수류탄을 던지려 했으나 포착되었습니다.");
        }
    }

    void TakeCover()
    {
        // 엄패: 해당 턴 동안 보호 상태로 설정
        IsTakeCover = true;
        Debug.Log($"{this.playerName}이(가) 엄패 중입니다.");
    }

    AIPlayer GetRandomOpponent()
    {
        List<AIPlayer> aliveOpponents = opponents.FindAll(player => player != this && player.IsAlive);
        if (aliveOpponents.Count == 0) return null;
        return aliveOpponents[Random.Range(0, aliveOpponents.Count)];
    }
}
