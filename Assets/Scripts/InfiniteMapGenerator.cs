using UnityEngine;
using System.Collections.Generic;

public class InfiniteMapGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Row Prefabs")]
    [SerializeField] private GameObject[] rowPrefabs; 

    [Header("Generation Settings")]
    [SerializeField] private float rowLength = 5.0f;     
    [SerializeField] private float spawnYHeight = -2.0f;
    [SerializeField] private float startOffset = 15.0f; 
    [SerializeField] private int initialRowsAhead = 15; 
    [SerializeField] private int viewDistance = 10;     

    private float spawnXPosition = 0f;
    private Queue<GameObject> activeRows = new Queue<GameObject>();

    void Start()
    {
        if (playerTransform == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        
        spawnXPosition = playerTransform.position.x - startOffset;

        
        for (int i = 0; i < initialRowsAhead; i++)
        {
            SpawnRow();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        
        if (playerTransform.position.x - (viewDistance * rowLength) < spawnXPosition)
        {
            SpawnRow();
            RemoveOldRow();
        }
    }

    void SpawnRow()
    {
        
        GameObject randomPrefab = rowPrefabs[Random.Range(0, rowPrefabs.Length)];

        
        Vector3 spawnPosition = new Vector3(spawnXPosition, spawnYHeight, 0f);
        GameObject newRow = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);

        
        activeRows.Enqueue(newRow);

        
        spawnXPosition -= rowLength;
    }

    void RemoveOldRow()
    {
        if (activeRows.Count > initialRowsAhead + 5)
        {
            GameObject oldRow = activeRows.Dequeue();
            Destroy(oldRow);
        }
    }
}