using UnityEngine;
using System.Collections;

public class FishSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Attach your Salmon or Shark prefab here.")]
    [SerializeField] private GameObject _fishPrefab;
    [SerializeField] private float _delayBetweenSpawns = 3f;
    [SerializeField] private Transform _spawnPoint;

    private GameObject _currentActiveFish;

    private void Start()
    {
        // Start the spawning loop
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // If there is no fish in the scene, spawn one
            if (_currentActiveFish == null)
            {
                SpawnFish();
            }

            // Wait until the fish is destroyed/eaten
            // This 'yield' waits as long as the reference is not null
            while (_currentActiveFish != null)
            {
                yield return new WaitForSeconds(1f); // Check every second to save CPU
            }

            // Once it's gone, wait for the designated Cooldown
            Debug.Log("Fish consumed or destroyed. Waiting to respawn...");
            yield return new WaitForSeconds(_delayBetweenSpawns);
        }
    }

    private void SpawnFish()
    {
        if (_fishPrefab == null) return;

        Vector3 pos = (_spawnPoint != null) ? _spawnPoint.position : transform.position;
        _currentActiveFish = Instantiate(_fishPrefab, pos, Quaternion.identity);

        Debug.Log($"Fish spawned: {_currentActiveFish.name}");
    }
}