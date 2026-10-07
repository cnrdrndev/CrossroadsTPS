using UnityEngine;
using System.Collections.Generic;

public class InfiniteMapGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Row Prefabs")]
    [SerializeField] private GameObject[] rowPrefabs; // Drag your Grass, Road, River prefabs here

    [Header("Generation Settings")]
    [SerializeField] private float rowLength = 5.0f;     // Distance between each row
    [SerializeField] private float spawnYHeight = -2.0f;// Height of the generated rows (-2 Y)
    [SerializeField] private float startOffset = 15.0f; // Distance away from player before infinite rows start
    [SerializeField] private int initialRowsAhead = 15; // How many rows to spawn at the start
    [SerializeField] private int viewDistance = 10;     // How far ahead to keep spawning rows

    private float spawnXPosition = 0f;
    private Queue<GameObject> activeRows = new Queue<GameObject>();

    void Start()
    {
        if (playerTransform == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Push the starting generation point ahead of your spawn area along the -X axis
        spawnXPosition = playerTransform.position.x - startOffset;

        // Spawn initial rows ahead along the -X axis
        for (int i = 0; i < initialRowsAhead; i++)
        {
            SpawnRow();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // If the player moves further into negative X, spawn new rows ahead and clean up old ones behind
        if (playerTransform.position.x - (viewDistance * rowLength) < spawnXPosition)
        {
            SpawnRow();
            RemoveOldRow();
        }
    }

    void SpawnRow()
    {
        // Pick a random row prefab
        GameObject randomPrefab = rowPrefabs[Random.Range(0, rowPrefabs.Length)];

        // Instantiate along the -X axis at Y = -2
        Vector3 spawnPosition = new Vector3(spawnXPosition, spawnYHeight, 0f);
        GameObject newRow = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);

        // Track it in our queue
        activeRows.Enqueue(newRow);

        // Move the spawn pointer further into negative X for the next row
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