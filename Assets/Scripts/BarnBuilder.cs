// BarnBuilder.cs — attach to an empty GameObject, runs in Start, then you can delete the script
using UnityEngine;

public class BarnBuilder : MonoBehaviour
{
    void Start()
    {
        // Main barn body
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Barn_Body";
        body.transform.position = new Vector3(50f, 3f, 50f);
        body.transform.localScale = new Vector3(14f, 6f, 20f);
        body.layer = LayerMask.NameToLayer("Environment");

        // Roof (rotated cube acting as a wedge — good enough for proto)
        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Barn_Roof";
        roof.transform.position = new Vector3(50f, 7.5f, 50f);
        roof.transform.localScale = new Vector3(16f, 3f, 22f);
        roof.transform.eulerAngles = new Vector3(0f, 0f, 45f);
        roof.layer = LayerMask.NameToLayer("Environment");

        // Barn doors (open gap — just visual, colliders remain on body)
        Destroy(this); // self-cleanup after building
    }
}