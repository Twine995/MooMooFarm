// ArenaManager.cs — place in Scripts/Core/
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [Header("Must match your Terrain size")]
    public float arenaSize = 100f;
    public float wallHeight = 6f;
    public float wallThickness = 2f;

    void Start()
    {
        SpawnWall(new Vector3(arenaSize / 2f, wallHeight / 2f, 0f),
                  new Vector3(wallThickness, wallHeight, arenaSize));            // East
        SpawnWall(new Vector3(-wallThickness / 2f, wallHeight / 2f, 0f),
                  new Vector3(wallThickness, wallHeight, arenaSize));           // West
        SpawnWall(new Vector3(arenaSize / 2f, wallHeight / 2f, arenaSize),
                  new Vector3(arenaSize, wallHeight, wallThickness));           // North
        SpawnWall(new Vector3(arenaSize / 2f, wallHeight / 2f, -wallThickness / 2f),
                  new Vector3(arenaSize, wallHeight, wallThickness));           // South
    }

    void SpawnWall(Vector3 center, Vector3 size)
    {
        var go = new GameObject("Wall");
        var col = go.AddComponent<BoxCollider>();
        col.size = Vector3.one;
        go.transform.position = center;
        go.transform.localScale = size;
        go.transform.parent = transform;
        go.layer = LayerMask.NameToLayer("Environment");
    }
}