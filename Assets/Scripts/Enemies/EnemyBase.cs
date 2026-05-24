// EnemyBase.cs — Scripts/Enemies/
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 30f;
    public float moveSpeed = 3.5f;
    public float contactDamage = 8f;
    public int xpValue = 1;

    [Header("Pooling")]
    public GameObject prefabKey;

    // Public state
    public bool IsAlive { get; private set; }

    protected Transform _player;
    protected NavMeshAgent _agent;
    float _health;

    // ── Called by WaveSpawner every time this is pulled from pool ──
    public virtual void Init(Transform player)
    {
        _player = player;
        _health = maxHealth;
        IsAlive = true;

        _agent = GetComponent<NavMeshAgent>();
        _agent.enabled = true;
        _agent.speed = moveSpeed;
        _agent.ResetPath();
    }

    void Update()
    {
        if (!IsAlive || _player == null) return;
        _agent.SetDestination(_player.position);
    }

    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        _health -= amount;
        if (_health <= 0f) Die();
    }

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
        yield return null; // wait one frame so collisions settle
        PoolManager.Instance.Return(prefabKey, gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsAlive) return;
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerController>()?.TakeDamage(contactDamage * Time.deltaTime);
    }
}