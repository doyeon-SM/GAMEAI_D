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

    public GameObject trenchPrefab; // 참호 프리팹을 참조할 변수
    private GameObject currentPrefab; // 현재 표시되는 프리팹 (AI Player 또는 Trench)

    private GameObject currentGenseInstance; // 현재 GensePrefab의 인스턴스
    public GameObject GensePrefab; // Gense 프리팹
    public GameObject Gense_A; // Gense_A 프리팹
    public GameObject Gense_C; // Gense_C 프리팹
    public GameObject Gense_G; // Gense_G 프리팹
    // Start is called before the first frame update
    void Start()
    {
        currentPrefab = this.gameObject; // 초기 프리팹은 AI Player 자신
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
        // 공격: 랜덤으로 살아있는 상대를 선택하고 공격
        AIPlayer target = GetRandomOpponent();
        Uncover();
        if (target != null && target.IsAlive && !target.IsTakeCover)
        {
            if (!deathList.Contains(target.playerName))
            {
                //target.IsAlive = false;
                deathList.Add(target.playerName);
                //Debug.Log($"{this.playerName}이(가) {target.playerName}을(를) 공격하여 제거했습니다.");
            }
        }
        else if(target != null && target.IsAlive && target.IsTakeCover)
        {
            //Debug.Log($"{this.playerName}이(가) {target.playerName}을(를) 공격하였으나 피했습니다.");
        }
    }

    void ThrowGrenade()
    {
        // 수류탄: 무작위 위치에 던짐, 해당 위치에 있는 상대가 있으면 제거
        AIPlayer target = GetRandomOpponent();
        Uncover();
        
        if (target != null && target.IsAlive && target.IsTakeCover)
        {
            if (!deathList.Contains(target.playerName))
            {
                //target.IsAlive = false;
                deathList.Add(target.playerName);
                //Debug.Log($"{this.playerName}이(가) {target.playerName}의 위치에 수류탄을 던져 제거했습니다.");
            }
        }
        else if (target != null && target.IsAlive && !target.IsTakeCover)
        {
            //Debug.Log($"{this.playerName}이(가) {target.playerName}의 위치에 수류탄을 던지려 했으나 포착되었습니다.");
        }
    }

    void TakeCover()
    {
        // 엄패: 해당 턴 동안 보호 상태로 설정
        IsTakeCover = true;
        SwitchToTrench(); // Trench로 변경
        //Debug.Log($"{this.playerName}이(가) 엄패 중입니다.");
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
        SwitchToAIPlayer(); // AI Player로 복원
        //Debug.Log($"{playerName}이(가) 엄패에서 벗어났습니다.");
    }
    void SwitchToTrench()
    {
        if (trenchPrefab == null)
        {
            Debug.LogError($"{playerName}: Trench prefab is not assigned.");
            return;
        }

        // 이미 Trench가 활성화되어 있는지 확인
        if (currentPrefab == trenchPrefab)
        {
            //Debug.Log($"{playerName}: Trench is already active.");
            return;
        }
        // 현재 프리팹 비활성화 후 Trench Prefab 생성
        if (currentPrefab != null) currentPrefab.SetActive(false);
        // 새로운 Trench 프리팹 생성
        currentPrefab = Instantiate(trenchPrefab, transform.position, transform.rotation, transform.parent);
        this.gameObject.SetActive(false); // AI Player 비활성화
    }

    public void SwitchToAIPlayer()
    {
        if (currentPrefab != null && currentPrefab != this.gameObject)
        {
            Destroy(currentPrefab); // 참호 프리팹 삭제
        }

        currentPrefab = this.gameObject;
        currentPrefab.SetActive(true); // AI Player 복원
    }

    public void VisualizeGenes(Transform spawnPoint)
    {
        // 기존 GensePrefab 삭제
        if (currentGenseInstance != null)
        {
            Destroy(currentGenseInstance);
        }

        // GensePrefab을 플레이어 위치 아래에 생성
        Vector3 gensePosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y - 1, spawnPoint.position.z);
        currentGenseInstance = Instantiate(GensePrefab, gensePosition, Quaternion.identity);

        // GensePrefab의 각 Point에 Gense_A, Gense_C, Gense_G 배치
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
                    genseInstance.transform.localPosition = Vector3.zero; // 부모 기준 위치로 초기화
                }
            }
        }
    }

}
