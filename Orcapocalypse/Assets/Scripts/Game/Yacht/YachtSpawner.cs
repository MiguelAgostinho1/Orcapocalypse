using UnityEngine;
using System.Collections;

public class YachtSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject _yachtPrefab;
    [SerializeField] private float _delayBetweenSpawns = 3f;
    [SerializeField] private Transform _spawnPoint;

    private GameObject _currentActiveYacht;

    private void Start()
    {
        // Start the spawning loop
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // If there is no yacht in the scene, spawn one
            if (_currentActiveYacht == null)
            {
                SpawnYacht();
            }

            // Wait until the yacht is destroyed
            // This 'yield' waits as long as the reference is not null
            while (_currentActiveYacht != null)
            {
                yield return new WaitForSeconds(1f); // Check every second to save CPU
            }

            // Once it's gone, wait for the designated Cooldown
            Debug.Log("Yacht destroyed. Waiting to respawn...");
            yield return new WaitForSeconds(_delayBetweenSpawns);
        }
    }

    private void SpawnYacht()
    {
        if (_yachtPrefab == null) return;

        Vector3 pos = (_spawnPoint != null) ? _spawnPoint.position : transform.position;
        _currentActiveYacht = Instantiate(_yachtPrefab, pos, Quaternion.identity);

        Debug.Log("Yacht spawned");
    }
}
