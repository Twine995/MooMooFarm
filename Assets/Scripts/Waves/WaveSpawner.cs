// WaveSpawner.cs — Scripts/Waves/
using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave configs (sort by triggerAtSecond)")]
    public WaveConfig[] waves;

    [Header("Spawn ring around player")]
    public float minSpawnRadius = 12f;
    public float maxSpawnRadius = 16f;

    Transform _player;
    int _nextWaveIndex;
    bool _running;

    void OnEnable() => GameManager.OnRunStarted.AddListener(OnRunStarted);
    void OnDisable() => GameManager.OnRunStarted.RemoveListener(OnRunStarted);

    void OnRunStarted()
    {
        _player = FindObjectOfType<PlayerController>().transform;
        _nextWaveIndex = 0;
        _running = true;
        Debug.Log("WaveSpawner: run started, waves count = " + waves.Length);
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (_running)
        {
            if (_nextWaveIndex >= waves.Length)
            {
                Debug.Log("WaveSpawner: all waves complete");
                yield break;
            }

            WaveConfig wave = waves[_nextWaveIndex];
            float target = wave.triggerAtSecond;

            // Wait until the run timer reaches this wave's trigger time
            // and the game is actively playing (not paused for level-up)
            yield return new WaitUntil(() =>
                GameManager.Instance.RunTimer >= target &&
                GameManager.Instance.State == GameState.Playing);

            Debug.Log("WaveSpawner: spawning wave " + _nextWaveIndex);
            StartCoroutine(SpawnWave(wave));
            _nextWaveIndex++;
        }
    }

    IEnumerator SpawnWave(WaveConfig wave)
    {
        foreach (var entry in wave.entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                // Pause spawning if game is in level-up or paused state
                yield return new WaitUntil(() =>
                    GameManager.Instance.State == GameState.Playing);

                Vector3 spawnPos = RandomRingPosition();
                var go = PoolManager.Instance.Get(
                    entry.enemyPrefab, spawnPos, Quaternion.identity);

                var enemy = go.GetComponent<EnemyBase>();
                if (enemy != null)
                    enemy.Init(_player);
                else
                    Debug.LogWarning("WaveSpawner: spawned object has no EnemyBase — " + go.name);

                yield return new WaitForSeconds(entry.spawnInterval);
            }
        }
    }

    Vector3 RandomRingPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;

        // Sample NavMesh near the chosen point so enemies always spawn on walkable surface
        Vector3 candidate = _player.position + offset;
        if (UnityEngine.AI.NavMesh.SamplePosition(candidate, out var hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            return hit.position;

        return candidate;
    }
}