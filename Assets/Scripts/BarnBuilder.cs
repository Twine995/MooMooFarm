// BarnBuilder.cs — Scripts/Core/
using UnityEngine;
using UnityEngine.AI;

public class BarnBuilder : MonoBehaviour
{
    public Vector3 barnCenter = new Vector3(50f, 0f, 50f);

    void Start()
    {
        BuildPart("Barn_Body",
            barnCenter + Vector3.up * 3f,
            new Vector3(14f, 6f, 20f),
            new Color(0.42f, 0.23f, 0.14f));

        BuildPart("Barn_Roof",
            barnCenter + Vector3.up * 7.5f,
            new Vector3(16f, 3f, 22f),
            new Color(0.29f, 0.18f, 0.09f));

        BuildPart("Barn_WallL",
            barnCenter + new Vector3(-8f, 1.5f, 0f),
            new Vector3(2f, 3f, 10f),
            new Color(0.42f, 0.23f, 0.14f));

        BuildPart("Barn_WallR",
            barnCenter + new Vector3(8f, 1.5f, 0f),
            new Vector3(2f, 3f, 10f),
            new Color(0.42f, 0.23f, 0.14f));
    }

    GameObject BuildPart(string partName, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = partName;
        go.transform.parent = transform;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.layer = LayerMask.NameToLayer("Environment");

        // Fix material — use Unity's already-assigned material, just change color
        var rend = go.GetComponent<Renderer>();
        var mat = rend.material;
        mat.color = color;
        rend.material = mat;

        // NavMeshObstacle carves a hole in the NavMesh for this barn part
        var obs = go.AddComponent<NavMeshObstacle>();
        obs.shape = NavMeshObstacleShape.Box;
        obs.center = Vector3.zero;
        obs.size = Vector3.one;
        obs.carving = true;
        obs.carveOnlyStationary = true;

        return go;
    }
}