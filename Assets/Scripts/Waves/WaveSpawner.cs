using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave data (sorted by triggerAtSecond)")]
    public WaveConfig[] waves;

    [Header("Spawn radius around player")]
    public float minSpawnRadius = 12f;
    public float maxSpawnRadius = 16f;

    Transform _player;
    int _nextWaveIndex;

    void OnEnable()
    {
        GameManager.OnRunStarted.AddListener(OnRunStarted);
    }

    void OnDisable()
    {
        GameManager.OnRunStarted.RemoveListener(OnRunStarted);
    }

    void OnRunStarted()
    {
        _player = FindObjectOfType<PlayerController>().transform;
        _nextWaveIndex = 0;
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (GameManager.Instance.State == GameState.Playing ||
               GameManager.Instance.State == GameState.LevelUp)
        {
            if (_nextWaveIndex >= waves.Length) yield break;

            float target = waves[_nextWaveIndex].triggerAtSecond;
            yield return new WaitUntil(() =>
                GameManager.Instance.RunTimer >= target &&
                GameManager.Instance.State == GameState.Playing);

            StartCoroutine(SpawnWave(waves[_nextWaveIndex]));
            _nextWaveIndex++;
        }
    }

    IEnumerator SpawnWave(WaveConfig wave)
    {
        foreach (var entry in wave.entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                // Wait during level-up pause
                yield return new WaitUntil(() =>
                    GameManager.Instance.State == GameState.Playing);

                Vector3 pos = RandomSpawnPosition();
                var go = PoolManager.Instance.Get(
                    entry.enemyPrefab, pos, Quaternion.identity);
                go.GetComponent<EnemyBase>()?.Init(_player);

                yield return new WaitForSeconds(entry.spawnInterval);
            }
        }
    }

    Vector3 RandomSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist  = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;
        return _player.position + offset;
    }
}
