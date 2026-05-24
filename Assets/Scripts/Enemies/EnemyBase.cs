using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth    = 30f;
    public float moveSpeed    = 3.5f;
    public float contactDamage = 10f;
    public int   xpValue      = 1;

    [Header("Pooling — drag this object's source prefab here")]
    public GameObject prefabKey;

    protected float     _health;
    protected Transform _player;
    protected NavMeshAgent _agent;
    protected bool _dead;

    public virtual void Init(Transform player)
    {
        _player = player;
        _health = maxHealth;
        _dead   = false;
        _agent  = GetComponent<NavMeshAgent>();
        _agent.speed   = moveSpeed;
        _agent.enabled = true;
    }

    void Update()
    {
        if (_dead || _player == null) return;
        _agent.SetDestination(_player.position);
    }

    public virtual void TakeDamage(float amount)
    {
        if (_dead) return;
        _health -= amount;
        StartCoroutine(DamageFlash());
        if (_health <= 0f) Die();
    }

    protected virtual void Die()
    {
        _dead          = true;
        _agent.enabled = false;

        GameManager.Instance.AddXP(xpValue);
        // TODO: PoolManager.Instance.Get(xpGemPrefab, transform.position, Quaternion.identity)

        StartCoroutine(ReturnAfterDelay(0.05f));
    }

    IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolManager.Instance.Return(prefabKey, gameObject);
    }

    // Override this in subclasses to swap material / set shader property
    protected virtual IEnumerator DamageFlash()
    {
        yield return new WaitForSeconds(0.08f);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerController>()?.TakeDamage(contactDamage * Time.deltaTime);
    }
}
