// ProjectileBase.cs — Scripts/Weapons/
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    WeaponData _weaponData;
    float _damage;
    float _speed;
    float _maxRange;
    int _pierceLeft;
    int _bouncesLeft;
    Vector3 _startPos;
    Vector3 _direction;

    // Tracks enemies already hit this frame to prevent double-hits on pierce
    readonly System.Collections.Generic.HashSet<EnemyBase> _hitThisShot = new();

    public void Init(WeaponData data, float damage, float speed,
                     float range, int pierce, int bounces)
    {
        _weaponData = data;
        _damage = damage;
        _speed = speed;
        _maxRange = range;
        _pierceLeft = pierce;
        _bouncesLeft = bounces;
        _startPos = transform.position;
        _direction = transform.forward;
        _hitThisShot.Clear();
    }

    void Update()
    {
        transform.position += _direction * (_speed * Time.deltaTime);

        // Range check
        if (Vector3.Distance(_startPos, transform.position) >= _maxRange)
        {
            ReturnToPool();
            return;
        }

        // Hit detection
        var hits = Physics.OverlapSphere(transform.position, 0.3f,
            LayerMask.GetMask("Enemy"));

        foreach (var h in hits)
        {
            var enemy = h.GetComponent<EnemyBase>()
                     ?? h.GetComponentInParent<EnemyBase>();

            if (enemy == null || _hitThisShot.Contains(enemy)) continue;

            _hitThisShot.Add(enemy);
            enemy.TakeDamage(_damage);

            if (_pierceLeft > 0)
            {
                _pierceLeft--;
                continue; // keep going, hit next enemy
            }

            if (_bouncesLeft > 0)
            {
                _bouncesLeft--;
                Bounce(h);
                return;
            }

            ReturnToPool();
            return;
        }
    }

    void Bounce(Collider hitCollider)
    {
        var cols = Physics.OverlapSphere(transform.position, 8f,
            LayerMask.GetMask("Enemy"));
        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            if (c == hitCollider) continue;
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = c.transform; }
        }

        if (best != null)
        {
            _direction = (best.position - transform.position).normalized;
            _startPos = transform.position;
            _hitThisShot.Clear(); // allow hitting already-hit enemies after bounce
        }
        else
        {
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        if (_weaponData == null || _weaponData.projectilePrefab == null)
        {
            gameObject.SetActive(false);
            return;
        }
        _hitThisShot.Clear();
        PoolManager.Instance.Return(_weaponData.projectilePrefab, gameObject);
    }
}