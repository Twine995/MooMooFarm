// CameraFollow.cs — Scripts/Core/
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // ── Isometric angle ───────────────────────────────────
    [Header("Isometric angle")]
    public float pitch = 60f;    // vertical tilt  (55-65 for Diablo feel)
    public float yaw = 45f;    // compass bearing (45 = true isometric)
    public float distance = 24f;   // camera pull-back

    // ── Follow ────────────────────────────────────────────
    [Header("Follow")]
    public Transform target;
    public float smoothing = 10f;

    // ── Lookahead toward cursor ───────────────────────────
    [Header("Lookahead")]
    public float lookaheadStrength = 2.5f;
    public float lookaheadSmoothing = 5f;

    // ── Screen shake ──────────────────────────────────────
    [Header("Screen Shake")]
    public float traumaDecay = 2f;

    // ── Singleton ─────────────────────────────────────────
    public static CameraFollow Instance { get; private set; }

    // ── Private ───────────────────────────────────────────
    float _trauma;
    Vector3 _lookaheadOffset;
    Vector3 _smoothPos;

    void Awake()
    {
        Instance = this;
        // Fix the rotation permanently — isometric never rotates
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (target != null)
            _smoothPos = DesiredPosition(target.position);
    }

    void LateUpdate()
    {
        if (target == null) return;

        UpdateLookahead();
        UpdateShake();

        Vector3 desired = DesiredPosition(target.position + _lookaheadOffset);
        _smoothPos = Vector3.Lerp(_smoothPos, desired, smoothing * Time.deltaTime);
        transform.position = _smoothPos + ShakeOffset();
    }

    Vector3 DesiredPosition(Vector3 focus) =>
        focus - transform.forward * distance;

    // ── Lookahead: camera drifts slightly toward cursor ───

    void UpdateLookahead()
    {
        if (Camera.main == null) return;

        // Project cursor onto the ground plane to get a world position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out float enter)) return;

        Vector3 cursorWorld = ray.GetPoint(enter);
        Vector3 towardCursor = (cursorWorld - target.position);
        towardCursor.y = 0f;

        // Clamp so camera never drifts too far from player
        Vector3 targetOffset = Vector3.ClampMagnitude(towardCursor, 1f) * lookaheadStrength;

        _lookaheadOffset = Vector3.Lerp(
            _lookaheadOffset, targetOffset, lookaheadSmoothing * Time.deltaTime);
    }

    // ── Screen shake (trauma model) ───────────────────────

    void UpdateShake() =>
        _trauma = Mathf.Max(_trauma - traumaDecay * Time.deltaTime, 0f);

    Vector3 ShakeOffset()
    {
        float shake = _trauma * _trauma;
        if (shake < 0.001f) return Vector3.zero;

        float t = Time.unscaledTime; // unscaled so it works during LevelUp pause
        return new Vector3(
            (Mathf.PerlinNoise(t * 38f, 0f) - 0.5f) * shake * 2f,
            (Mathf.PerlinNoise(0f, t * 38f) - 0.5f) * shake * 2f,
            0f);
    }

    public void AddTrauma(float amount) =>
        _trauma = Mathf.Clamp01(_trauma + amount);
}