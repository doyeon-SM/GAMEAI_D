using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneWinRateManager : MonoBehaviour
{
    // �ʱ�ȭ
    private List<string> winGenes = new List<string>(); // �¸� ������ ����Ʈ
    private readonly float[] percentageWeights = { 50f, 25f, 12.5f, 6.25f, 3.12f, 1.56f, 0.78f, 0.39f }; // �·� ����ġ
    private const int groupSize = 8;

    public float PredictWinRate(string playerGene)
    {
        if (winGenes.Count == 0)
            return 0f; // ���� wingene�� ������ �·� 0% ��ȯ

        float totalWinRate = 0f;

        int numGroups = Mathf.CeilToInt(winGenes.Count / (float)groupSize);

        // �� �׷�� ���Ͽ� �·� ���
        for (int groupIndex = 0; groupIndex < numGroups; groupIndex++)
        {
            int startIndex = groupIndex * groupSize;
            int count = Mathf.Min(groupSize, winGenes.Count - startIndex); // ���� ������ �� Ȯ��

            // �׷� ����
            List<string> group = winGenes.GetRange(startIndex, count);
            string groupGene = string.Join(" ", group);

            // ���� �׷�� ���Ͽ� �·� ���
            totalWinRate += CompareGenes(playerGene, groupGene);
        }

        return totalWinRate/numGroups;
    }

    private float CompareGenes(string gene1, string gene2)
    {
        float winRate = 0f;

        // ���⸦ �������� �����ڸ� �и�
        string[] geneParts1 = gene1.Split(' ');
        string[] geneParts2 = gene2.Split(' ');

        int length = Mathf.Min(geneParts1.Length, geneParts2.Length); // �� �������� ���� �� ª�� ���� �������� ��

        for (int i = 0; i < length; i++)
        {
            if (geneParts1[i] == geneParts2[i])
            {
                if (i < percentageWeights.Length)
                    winRate += percentageWeights[i];
                else
                    break; // �迭 ���̸� �ʰ��ϸ� �߰� ����ġ�� ����
            }
            else
            {
                break; // �ٸ� �����ڰ� ������ �� �ߴ�
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
