// EnemyBase.cs — Scripts/Enemies/
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    EnemyHealthBar _healthBar;

    [Header("Stats")]
    public float maxHealth = 30f;
    public float moveSpeed = 3.5f;
    public float contactDamage = 8f;
    public float contactRange = 1.3f;
    public int xpValue = 1;

    [Header("Pooling")]
    public GameObject prefabKey;

    public bool IsAlive { get; private set; }

    protected Transform _player;
    protected NavMeshAgent _agent;
    float _health;

    void Awake()
    {
        _healthBar = GetComponentInChildren<EnemyHealthBar>(true);
        Debug.Log("Awake — healthBar found: " + (_healthBar != null));
    }
    public virtual void Init(Transform player)
    {
        _player = player;
        _health = maxHealth;
        IsAlive = true;

        _agent = GetComponent<NavMeshAgent>();
        _agent.enabled = true;
        _agent.speed = moveSpeed;
        _agent.baseOffset = 1f;
        _agent.ResetPath();

        _healthBar?.Init(maxHealth);  // ← this is what was missing
    }

    void Update()
    {
        if (!IsAlive || _player == null) return;

        _agent.SetDestination(_player.position);
        HandleContactDamage();
    }

    // ── Contact damage via distance check ─────────────────
    // Works regardless of collider type or height differences
    void HandleContactDamage()
    {
        if (_player == null) return;

        // Use XZ distance only — ignores height so low enemies still deal damage
        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f;
        float flatDistance = toPlayer.magnitude;

        if (flatDistance < contactRange)
        {
            _player.GetComponent<PlayerController>()
                ?.TakeDamage(contactDamage * Time.deltaTime);
        }
    }

    // ── Take damage ────────────────────────────────────────
    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        _health -= amount;
        _healthBar?.OnDamaged(_health, maxHealth);
        if (_health <= 0f) Die();
    }

    // ── Death ──────────────────────────────────────────────
    protected virtual void Die()
    {
        IsAlive = false;
        _agent.enabled = false;

        GameManager.Instance.AddXP(xpValue);
        CameraFollow.Instance?.AddTrauma(0.1f);

        StartCoroutine(ReturnToPool());
    }

    IEnumerator ReturnToPool()
    {
        yield return null;
        PoolManager.Instance.Return(prefabKey, gameObject);
    }
}