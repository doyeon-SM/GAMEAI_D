using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public float[] genes;
    public bool IsAlive = true;
    public float fitness;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public AIPlayer()
    {
        genes = new float[10]; // ������ ������ �迭
        InitializeGenes();
    }

    void InitializeGenes()
    {
        for (int i = 0; i < genes.Length; i++)
        {
            genes[i] = Random.Range(0f, 1f); // �����ڴ� 0�� 1 ������ ������
        }
    }
}
