using UnityEngine;
using System.Collections.Generic;

public class TrashSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<TrashData> _trashLibrary; // Drag all your SOs here
    [SerializeField] private GameObject _trashPrefab;     // The generic prefab with TrashInstance.cs
    [SerializeField] private float _spawnInterval = 3f;    // Time between drops
    [SerializeField] private Transform _spawnPoint;        // Where the trash comes out (e.g., the back of the boat)

    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _spawnInterval)
        {
            SpawnTrash();
            _timer = 0;
        }
    }

    private void SpawnTrash()
    {
        if (_trashLibrary.Count == 0 || _trashPrefab == null) return;

        // 1. Pick a random piece of trash from your SO collection
        TrashData randomTrash = _trashLibrary[Random.Range(0, _trashLibrary.Count)];

        // 2. Spawn the generic trash object
        Vector3 pos = _spawnPoint != null ? _spawnPoint.position : transform.position;
        GameObject newTrash = Instantiate(_trashPrefab, pos, Quaternion.identity);

        // 3. "Dress" the object with the data
        newTrash.GetComponent<TrashInstance>().Initialize(randomTrash);
    }
}