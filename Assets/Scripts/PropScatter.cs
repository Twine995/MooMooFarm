// PropScatter.cs — Scripts/Core/
using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class PropScatter : MonoBehaviour
{
    [Header("Arena bounds")]
    public float arenaSize = 100f;
    public float edgeMargin = 5f;

    [Header("Clear zone around barn center")]
    public Vector3 barnCenter = new Vector3(50f, 0f, 50f);
    public float clearRadius = 14f;

    [Header("Prop counts")]
    public int hayBaleCount = 20;
    public int fenceCount = 30;
    public int troughCount = 8;

    [Header("NavMesh")]
    public NavMeshSurface navMeshSurface;

    // ── Entry point ────────────────────────────────────────
    IEnumerator Start()
    {
        // Wait one frame for BarnBuilder.Start() to finish
        yield return null;

        for (int i = 0; i < hayBaleCount; i++) SpawnHayBale();
        for (int i = 0; i < fenceCount; i++) SpawnFenceSegment();
        for (int i = 0; i < troughCount; i++) SpawnTrough();

        // Wait 3 frames for all NavMeshObstacle carvers to register
        yield return null;
        yield return null;
        yield return null;

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh rebaked. Prop root count: " + transform.childCount);
        }
        else
        {
            Debug.LogWarning("PropScatter: NavMeshSurface not assigned in Inspector.");
        }
    }

    // ── Hay bale ───────────────────────────────────────────
    void SpawnHayBale()
    {
        Vector3 pos = SafePosition(clearRadius);
        pos.y = 0.55f;

        var go = CreateProp(PrimitiveType.Cube, pos,
            new Vector3(1.4f, 1.1f, 1.4f),
            Random.Range(0f, 360f),
            new Color(0.78f, 0.66f, 0.29f),
            "HayBale");

        AddObstacle(go, Vector3.zero, Vector3.one);
    }

    // ── Trough ─────────────────────────────────────────────
    void SpawnTrough()
    {
        Vector3 pos = SafePosition(clearRadius);
        float yaw = Random.Range(0, 2) * 90f;

        var body = CreateProp(PrimitiveType.Cube,
            new Vector3(pos.x, 0.4f, pos.z),
            new Vector3(3f, 0.8f, 0.9f),
            yaw,
            new Color(0.23f, 0.16f, 0.09f),
            "Trough");

        AddObstacle(body, Vector3.zero, Vector3.one);

        // Water fill — decorative only
        var water = CreateProp(PrimitiveType.Cube,
            new Vector3(pos.x, 0.62f, pos.z),
            new Vector3(2.6f, 0.5f, 0.55f),
            yaw,
            new Color(0.10f, 0.23f, 0.29f),
            "Trough_Water");
        Destroy(water.GetComponent<Collider>());
        water.transform.parent = body.transform;
    }

    // ── Fence segment ──────────────────────────────────────
    void SpawnFenceSegment()
    {
        Vector3 origin = SafePosition(clearRadius);
        origin.y = 0f;
        float yaw = Random.Range(0, 4) * 90f; // snap to 0/90/180/270

        // Root object — rotation handled here, children use local space only
        var root = new GameObject("FenceSegment");
        root.transform.parent = transform;
        root.transform.position = origin;
        root.transform.eulerAngles = new Vector3(0f, yaw, 0f);
        root.layer = LayerMask.NameToLayer("Environment");

        Color postColor = new Color(0.32f, 0.20f, 0.10f);
        Color woodColor = new Color(0.42f, 0.28f, 0.14f);

        const int picketCount = 5;
        const float picketSpacing = 0.45f;
        float segWidth = picketCount * picketSpacing; // 2.25m

        // Left and right anchor posts
        BuildPost(root, new Vector3(0f, 0f, 0f), postColor);
        BuildPost(root, new Vector3(segWidth, 0f, 0f), postColor);

        // Pickets evenly spaced between posts
        for (int i = 0; i < picketCount; i++)
        {
            float x = (i + 0.5f) * picketSpacing;
            BuildPicket(root, new Vector3(x, 0f, 0f), woodColor);
        }

        // Top rail
        BuildLocalCube(root,
            new Vector3(segWidth * 0.5f, 1.55f, 0f),
            new Vector3(segWidth + 0.1f, 0.10f, 0.18f),
            woodColor, "Rail_Top", removeCollider: true);

        // Bottom rail
        BuildLocalCube(root,
            new Vector3(segWidth * 0.5f, 0.55f, 0f),
            new Vector3(segWidth + 0.1f, 0.10f, 0.18f),
            woodColor, "Rail_Bottom", removeCollider: true);

        // One collider + one obstacle covering the whole segment
        var col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(segWidth * 0.5f, 0.9f, 0f);
        col.size = new Vector3(segWidth + 0.1f, 1.8f, 0.28f);

        AddObstacle(root,
            new Vector3(segWidth * 0.5f, 0.9f, 0f),
            new Vector3(segWidth + 0.1f, 1.8f, 0.28f));
    }

    // ── Post: shaft + pointed cap ──────────────────────────
    void BuildPost(GameObject parent, Vector3 localPos, Color color)
    {
        // Shaft
        BuildLocalCube(parent,
            localPos + new Vector3(0f, 0.9f, 0f),
            new Vector3(0.18f, 1.8f, 0.18f),
            color, "Post_Shaft", removeCollider: true); // obstacle on root covers it

        // Pointed cap (rotated cube)
        var cap = BuildLocalCube(parent,
            localPos + new Vector3(0f, 1.87f, 0f),
            new Vector3(0.15f, 0.15f, 0.15f),
            color, "Post_Cap", removeCollider: true);
        cap.transform.localEulerAngles = new Vector3(0f, 45f, 45f);
    }

    // ── Picket: narrow plank + pointed tip ─────────────────
    void BuildPicket(GameObject parent, Vector3 localPos, Color color)
    {
        BuildLocalCube(parent,
            localPos + new Vector3(0f, 0.75f, 0f),
            new Vector3(0.10f, 1.30f, 0.10f),
            color, "Picket", removeCollider: true);

        var tip = BuildLocalCube(parent,
            localPos + new Vector3(0f, 1.47f, 0f),
            new Vector3(0.10f, 0.12f, 0.10f),
            color, "Picket_Tip", removeCollider: true);
        tip.transform.localEulerAngles = new Vector3(0f, 45f, 45f);
    }

    // ── Local-space cube — all transforms relative to parent ──
    GameObject BuildLocalCube(GameObject parent, Vector3 localPos, Vector3 localScale,
                               Color color, string goName, bool removeCollider)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = goName;
        go.transform.parent = parent.transform;
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.transform.localEulerAngles = Vector3.zero;
        go.layer = LayerMask.NameToLayer("Environment");

        if (removeCollider)
            Destroy(go.GetComponent<Collider>());

        SetColor(go, color);
        return go;
    }

    // ── World-space primitive (hay bales, troughs) ─────────
    GameObject CreateProp(PrimitiveType type, Vector3 worldPos, Vector3 scale,
                          float yawDeg, Color color, string goName)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = goName;
        go.transform.parent = transform;
        go.transform.position = worldPos;
        go.transform.localScale = scale;
        go.transform.eulerAngles = new Vector3(0f, yawDeg, 0f);
        go.layer = LayerMask.NameToLayer("Environment");
        SetColor(go, color);
        return go;
    }

    // ── NavMeshObstacle helper ─────────────────────────────
    void AddObstacle(GameObject go, Vector3 center, Vector3 size)
    {
        var obs = go.AddComponent<NavMeshObstacle>();
        obs.shape = NavMeshObstacleShape.Box;
        obs.center = center;
        obs.size = size;
        obs.carving = true;
        obs.carveOnlyStationary = true;
    }

    // ── Material color helper ──────────────────────────────
    void SetColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        var mat = rend.material; // instantiates a copy — no shader lookup needed
        mat.color = color;
        rend.material = mat;
    }

    // ── Random position avoiding barn center and edges ─────
    Vector3 SafePosition(float avoidRadius)
    {
        Vector3 pos = Vector3.zero;
        int guard = 100;
        do
        {
            float x = Random.Range(edgeMargin, arenaSize - edgeMargin);
            float z = Random.Range(edgeMargin, arenaSize - edgeMargin);
            pos = new Vector3(x, 0f, z);
        }
        while (Vector3.Distance(pos, barnCenter) < avoidRadius && --guard > 0);
        return pos;
    }
}