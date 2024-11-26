using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneWinRateManager : MonoBehaviour
{
    // 초기화
    private List<string> winGenes = new List<string>(); // 승리 유전자 리스트
    private readonly float[] percentageWeights = { 50f, 25f, 12.5f, 6.25f, 3.12f, 1.56f, 0.78f, 0.39f }; // 승률 가중치
    private const int groupSize = 8;

    public float PredictWinRate(string playerGene)
    {
        if (winGenes.Count == 0)
            return 0f; // 비교할 wingene이 없으면 승률 0% 반환

        float totalWinRate = 0f;

        int numGroups = Mathf.CeilToInt(winGenes.Count / (float)groupSize);

        // 각 그룹과 비교하여 승률 계산
        for (int groupIndex = 0; groupIndex < numGroups; groupIndex++)
        {
            int startIndex = groupIndex * groupSize;
            int count = Mathf.Min(groupSize, winGenes.Count - startIndex); // 남은 유전자 수 확인

            // 그룹 추출
            List<string> group = winGenes.GetRange(startIndex, count);
            string groupGene = string.Join(" ", group);

            // 현재 그룹과 비교하여 승률 계산
            totalWinRate += CompareGenes(playerGene, groupGene);
        }

        return totalWinRate/numGroups;
    }

    private float CompareGenes(string gene1, string gene2)
    {
        float winRate = 0f;

        // 띄어쓰기를 기준으로 유전자를 분리
        string[] geneParts1 = gene1.Split(' ');
        string[] geneParts2 = gene2.Split(' ');

        int length = Mathf.Min(geneParts1.Length, geneParts2.Length); // 두 유전자의 길이 중 짧은 것을 기준으로 비교

        for (int i = 0; i < length; i++)
        {
            if (geneParts1[i] == geneParts2[i])
            {
                if (i < percentageWeights.Length)
                    winRate += percentageWeights[i];
                else
                    break; // 배열 길이를 초과하면 추가 가중치는 없음
            }
            else
            {
                break; // 다른 유전자가 나오면 비교 중단
            }
        }

        return winRate;
    }

    public void AddWinningGene(string winningGene)
    {
        string[] genes = winningGene.Split(' ');
        winGenes.AddRange(genes);
    }
}
