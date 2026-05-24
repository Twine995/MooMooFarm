// PlayerController.cs — Scripts/Player/
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    // ── Movement ──────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 6f;

    // ── Dodge ─────────────────────────────────────────────
    [Header("Dodge Roll")]
    public float dodgeSpeed = 20f;
    public float dodgeDuration = 0.22f;
    public float dodgeCooldown = 1.1f;

    // ── Health ────────────────────────────────────────────
    [Header("Health")]
    public float maxHealth = 100f;

    // ── Layers ────────────────────────────────────────────
    [Header("Layers")]
    public LayerMask groundLayer;        // set to "Environment" in inspector

    // ── Public state ──────────────────────────────────────
    public float Health { get; private set; }
    public bool IsDodging { get; private set; }
    public bool IsAlive { get; private set; } = true;
    public Vector3 FaceDir { get; private set; } // direction player is facing (toward cursor)


    // ── Private ───────────────────────────────────────────
    NavMeshAgent _agent;
    Camera _cam;
    float _dodgeCooldownTimer;

    // Click indicator (optional: small world-space marker at click point)
    Vector3 _clickDestination;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _cam = Camera.main;

        Debug.Log(_cam == null ? "Camera is NULL — tag Main Camera correctly" : "Camera found: " + _cam.name);
        Debug.Log("groundLayer value: " + groundLayer.value + " (0 means nothing is selected)");
    
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = moveSpeed;
        _agent.angularSpeed = 0f;
        _agent.acceleration = 30f;
        _agent.stoppingDistance = 0.2f;
        _agent.baseOffset = 1f;   // ← lifts capsule so feet sit on ground
        _cam = Camera.main;
        Health = maxHealth;
        FaceDir = transform.forward;
    }

    void Update()
    {
        if (!IsAlive || GameManager.Instance.State != GameState.Playing) return;

        HandleClickMove();
        HandleDodge();
        FaceMouseCursor();
    }

    // ── Click to move ─────────────────────────────────────

    void HandleClickMove()
    {
        if (IsDodging) return;

        if (Input.GetMouseButton(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            // No layer mask — hits everything
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                Debug.Log("Hit: " + hit.collider.name + " on layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
                _agent.SetDestination(hit.point);
            }
            else
            {
                Debug.Log("Raycast missed everything");
            }
        }
    }

    // ── Dodge ─────────────────────────────────────────────

    void HandleDodge()
    {
        _dodgeCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && !IsDodging && _dodgeCooldownTimer <= 0f)
            StartCoroutine(DodgeCoroutine());
    }

    IEnumerator DodgeCoroutine()
    {
        IsDodging = true;
        _dodgeCooldownTimer = dodgeCooldown;

        // Roll toward cursor direction
        Vector3 rollDir = FaceDir.sqrMagnitude > 0.01f ? FaceDir : transform.forward;

        _agent.ResetPath();
        _agent.enabled = false;     // take manual control during roll

        float timer = dodgeDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            float t = timer / dodgeDuration;   // ease-out
            transform.position += rollDir * (dodgeSpeed * t * Time.deltaTime);
            yield return null;
        }

        _agent.enabled = true;
        IsDodging = false;
    }

    // ── Face mouse cursor ─────────────────────────────────

    void FaceMouseCursor()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer)) return;

        Vector3 dir = (hit.point - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        FaceDir = dir.normalized;
        transform.rotation = Quaternion.LookRotation(FaceDir);
    }

    // ── Damage / Health ───────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (!IsAlive || IsDodging) return;

        Health = Mathf.Max(Health - amount, 0f);

        // Camera shake on hit
        CameraFollow.Instance?.AddTrauma(0.3f);

        if (Health <= 0f) Die();
    }

    public void Heal(float amount) =>
        Health = Mathf.Min(Health + amount, maxHealth);

    void Die()
    {
        IsAlive = false;
        _agent.enabled = false;
        GameManager.Instance.PlayerDied();
    }
}