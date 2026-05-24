using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    WeaponData _weaponData;
    float _damage;
    float _speed;
    float _maxRange;
    int   _pierceLeft;
    int   _bouncesLeft;

    Vector3 _startPos;
    Vector3 _direction;

    public void Init(WeaponData data, float damage, float speed,
                     float range, int pierce, int bounces)
    {
        _weaponData  = data;
        _damage      = damage;
        _speed       = speed;
        _maxRange    = range;
        _pierceLeft  = pierce;
        _bouncesLeft = bounces;
        _startPos    = transform.position;
        _direction   = transform.forward;
    }

    void Update()
    {
        transform.position += _direction * (_speed * Time.deltaTime);

        if (Vector3.Distance(_startPos, transform.position) >= _maxRange)
            ReturnToPool();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        other.GetComponent<EnemyBase>()?.TakeDamage(_damage);

        if (_pierceLeft > 0)
        {
            _pierceLeft--;
            return;
        }

        if (_bouncesLeft > 0)
        {
            _bouncesLeft--;
            Bounce(other);
            return;
        }

        ReturnToPool();
    }

    void Bounce(Collider hitCollider)
    {
        var cols = Physics.OverlapSphere(transform.position, 8f,
            LayerMask.GetMask("Enemy"));
        Transform best    = null;
        float     bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            if (c == hitCollider) continue;
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = c.transform; }
        }

        if (best != null)
        {
            _direction = (best.position - transform.position).normalized;
            _startPos  = transform.position; // reset range from new bounce origin
        }
        else
        {
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        PoolManager.Instance.Return(_weaponData.projectilePrefab, gameObject);
    }
}
