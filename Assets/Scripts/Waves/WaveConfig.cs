using UnityEngine;

[CreateAssetMenu(menuName = "MooMooFarm/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [System.Serializable]
    public struct SpawnEntry
    {
        public GameObject enemyPrefab;
        public int count;
        public float spawnInterval; // seconds between each spawn in this entry
    }

    public float triggerAtSecond;   // when in the run this wave fires
    public SpawnEntry[] entries;
    public bool isBossWave;
}
