using UnityEngine;

/// <summary>
/// Basic Corrupted Cow — charges in a straight line when close to the player.
/// Inherits movement and damage handling from EnemyBase.
/// </summary>
public class CorruptedCow : EnemyBase
{
    [Header("Charge attack")]
    public float chargeRange     = 6f;
    public float chargeSpeed     = 12f;
    public float chargeDuration  = 0.4f;
    public float chargeCooldown  = 3f;

    bool  _charging;
    float _chargeTimer;
    float _chargeCooldownTimer;
    Vector3 _chargeDir;

    void Update()
    {
        if (_dead || _player == null) return;

        _chargeCooldownTimer -= Time.deltaTime;

        if (_charging)
        {
            _chargeTimer -= Time.deltaTime;
            _agent.Move(_chargeDir * (chargeSpeed * Time.deltaTime));
            if (_chargeTimer <= 0f)
            {
                _charging = false;
                _agent.speed = moveSpeed;
            }
            return;
        }

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist <= chargeRange && _chargeCooldownTimer <= 0f)
            BeginCharge();
        else
            _agent.SetDestination(_player.position);
    }

    void BeginCharge()
    {
        _charging            = true;
        _chargeTimer         = chargeDuration;
        _chargeCooldownTimer = chargeCooldown;
        _chargeDir           = (_player.position - transform.position).normalized;
        _agent.ResetPath();   // stop navmesh steering during raw charge
    }
}
