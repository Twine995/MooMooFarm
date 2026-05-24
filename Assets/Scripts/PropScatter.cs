// PropScatter.cs — place in Scripts/Core/
using UnityEngine;

public class PropScatter : MonoBehaviour
{
    [System.Serializable]
    public struct PropConfig
    {
        public PrimitiveType shape;
        public Vector3 scale;
        public int count;
        public float exclusionRadius; // keep clear of barn center
    }

    public PropConfig[] props = new PropConfig[]
    {
        // Hay bales
        new PropConfig { shape = PrimitiveType.Cube,
            scale = new Vector3(1.5f, 1.2f, 1.5f), count = 20, exclusionRadius = 14f },
        // Fence posts
        new PropConfig { shape = PrimitiveType.Cylinder,
            scale = new Vector3(0.3f, 1.5f, 0.3f), count = 40, exclusionRadius = 5f },
        // Troughs
        new PropConfig { shape = PrimitiveType.Cube,
            scale = new Vector3(3f, 0.6f, 0.8f), count = 8, exclusionRadius = 12f },
    };

    public float arenaSize = 100f;
    public float edgeMargin = 4f;
    public Vector3 barnCenter = new Vector3(50f, 0f, 50f);

    void Start()
    {
        foreach (var cfg in props)
        {
            for (int i = 0; i < cfg.count; i++)
            {
                Vector3 pos = RandomPosition();
                if (Vector3.Distance(pos, barnCenter) < cfg.exclusionRadius) continue;

                var go = GameObject.CreatePrimitive(cfg.shape);
                go.transform.position = pos;
                go.transform.localScale = cfg.scale;
                go.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                go.transform.parent = transform;
                go.layer = LayerMask.NameToLayer("Environment");
            }
        }
    }

    Vector3 RandomPosition()
    {
        float x = Random.Range(edgeMargin, arenaSize - edgeMargin);
        float z = Random.Range(edgeMargin, arenaSize - edgeMargin);
        return new Vector3(x, 1f, z);
    }
}