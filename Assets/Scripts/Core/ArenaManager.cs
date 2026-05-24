// ArenaManager.cs — Scripts/Core/
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    public float arenaSize = 100f;
    public float wallHeight = 6f;
    public float wallThickness = 1f;

    void Start()
    {
        float s = arenaSize;
        float h = wallHeight;
        float t = wallThickness;

        // Position is the CENTER of each wall
        // South edge (Z = 0)
        MakeWall("Wall_South",
            new Vector3(s / 2f, h / 2f, -t / 2f),
            new Vector3(s, h, t));

        // North edge (Z = 100)
        MakeWall("Wall_North",
            new Vector3(s / 2f, h / 2f, s + t / 2f),
            new Vector3(s, h, t));

        // West edge (X = 0)
        MakeWall("Wall_West",
            new Vector3(-t / 2f, h / 2f, s / 2f),
            new Vector3(t, h, s));

        // East edge (X = 100)
        MakeWall("Wall_East",
            new Vector3(s + t / 2f, h / 2f, s / 2f),
            new Vector3(t, h, s));
    }

    void MakeWall(string wallName, Vector3 center, Vector3 size)
    {
        var go = new GameObject(wallName);
        go.transform.parent = transform;
        go.transform.position = center;
        go.transform.localScale = size;        // scale sets the visual size
        go.layer = LayerMask.NameToLayer("Environment");

        var col = go.AddComponent<BoxCollider>();
        col.size = Vector3.one;                   // size 1,1,1 × localScale = correct size
    }
}