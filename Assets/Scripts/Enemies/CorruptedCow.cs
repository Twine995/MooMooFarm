// CorruptedCow.cs — Scripts/Enemies/
using UnityEngine;

public class CorruptedCow : EnemyBase
{
    [Header("Charge Attack")]
    public float chargeDetectRange = 8f;    // distance to trigger charge
    public float chargeSpeed = 14f;
    public float chargeDuration = 0.35f;
    public float chargeCooldown = 3f;
    public float chargeDamage = 15f;   // bonus damage on charge hit

    bool _charging;
    float _chargeTimer;
    float _cooldownTimer;
    Vector3 _chargeDir;

    // ── Reset charge state when pulled from pool ──────────
    public override void Init(Transform player)
    {
        base.Init(player);
        _charging = false;
        _chargeTimer = 0f;
        _cooldownTimer = Random.Range(0f, chargeCooldown); // stagger first charge
    }

    void Update()
    {
        if (!IsAlive || _player == null) return;

        _cooldownTimer -= Time.deltaTime;

        if (_charging)
        {
            HandleCharge();
        }
        else
        {
            ChasePlayer();
            TryBeginCharge();
        }
    }

    // ── Normal chase (delegates to NavMesh) ───────────────
    void ChasePlayer()
    {
        _agent.enabled = true;
        _agent.SetDestination(_player.position);
    }

    // ── Charge trigger ────────────────────────────────────
    void TryBeginCharge()
    {
        if (_cooldownTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > chargeDetectRange) return;

        // Lock in direction at moment of charge start
        _chargeDir = (_player.position - transform.position).normalized;
        _chargeDir.y = 0f;
        _chargeTimer = chargeDuration;
        _cooldownTimer = chargeCooldown;
        _charging = true;

        // Disable NavMesh during manual charge movement
        _agent.enabled = false;
        _agent.ResetPath();
    }

    // ── Charge movement ───────────────────────────────────
    void HandleCharge()
    {
        _chargeTimer -= Time.deltaTime;

        // Ease-out: fast start, decelerates at end
        float t = _chargeTimer / chargeDuration;
        transform.position += _chargeDir * (chargeSpeed * t * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(_chargeDir);

        if (_chargeTimer <= 0f)
        {
            _charging = false;
            _agent.enabled = true; // hand back to NavMesh
        }
    }

    // ── Override contact damage during charge ─────────────
    void OnTriggerStay(Collider other)
    {
        if (!IsAlive) return;
        if (!other.CompareTag("Player")) return;

        float dmg = _charging ? chargeDamage : contactDamage;
        other.GetComponent<PlayerController>()?.TakeDamage(dmg * Time.deltaTime);
    }
}