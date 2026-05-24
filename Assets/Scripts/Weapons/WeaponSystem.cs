using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    public static WeaponSystem Instance { get; private set; }

    [Header("Starting weapon")]
    public WeaponData startingWeapon;

    // ---- Runtime state per equipped weapon ----
    class WeaponSlot
    {
        public WeaponData data;
        public int   tier;
        public float cooldownTimer;

        public float Damage   => data.damage          + (tier > 0 ? data.upgrades[tier - 1].damageBonus          : 0);
        public float Cooldown => data.cooldown        * (tier > 0 ? data.upgrades[tier - 1].cooldownMultiplier    : 1f);
        public int   Pierce   => data.pierce          + (tier > 0 ? data.upgrades[tier - 1].pierceBonus           : 0);
        public int   Count    => data.projectileCount + (tier > 0 ? data.upgrades[tier - 1].projectileCountBonus  : 0);
    }

    readonly List<WeaponSlot> _slots = new();
    Transform _player;

    void Awake() => Instance = this;

    void OnEnable()  => GameManager.OnRunStarted.AddListener(OnRunStarted);
    void OnDisable() => GameManager.OnRunStarted.RemoveListener(OnRunStarted);

    void OnRunStarted()
    {
        _player = FindObjectOfType<PlayerController>().transform;
        _slots.Clear();
        Equip(startingWeapon);
    }

    // Call this from UpgradeSystem when the player picks a weapon upgrade
    public void Equip(WeaponData data)
    {
        var existing = _slots.Find(s => s.data == data);
        if (existing != null)
        {
            existing.tier = Mathf.Min(existing.tier + 1, data.upgrades.Length);
            return;
        }
        _slots.Add(new WeaponSlot { data = data, tier = 0 });
    }

    void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        foreach (var slot in _slots)
        {
            slot.cooldownTimer -= Time.deltaTime;
            if (slot.cooldownTimer <= 0f)
            {
                slot.cooldownTimer = slot.Cooldown;
                Fire(slot);
            }
        }
    }

    void Fire(WeaponSlot slot)
    {
        // Fire toward mouse cursor in world space
        Vector3 fireDir = GetCursorDirection();

        float angleStep = slot.Count > 1 ? 20f / (slot.Count - 1) : 0f;
        float startAngle = -angleStep * (slot.Count - 1) / 2f;

        for (int i = 0; i < slot.Count; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, startAngle + angleStep * i, 0f) * fireDir;

            var go = PoolManager.Instance.Get(
                slot.data.projectilePrefab,
                _player.position + dir * 0.6f + Vector3.up * 0.8f,
                Quaternion.LookRotation(dir));

            go.GetComponent<ProjectileBase>()?.Init(
                slot.data, slot.Damage,
                slot.data.projectileSpeed,
                slot.data.range,
                slot.Pierce, slot.data.bounces);
        }
    }

    Vector3 GetCursorDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.up * 0.8f); // at projectile height
        if (ground.Raycast(ray, out float enter))
        {
            Vector3 cursorWorld = ray.GetPoint(enter);
            Vector3 dir = (cursorWorld - _player.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f) return dir.normalized;
        }
        return _player.forward;
    }

    /*Transform FindNearestEnemy(float range)
    {
        var cols = Physics.OverlapSphere(_player.position, range,
            LayerMask.GetMask("Enemy"));
        Transform nearest = null;
        float best = float.MaxValue;
        foreach (var c in cols)
        {
            float d = (c.transform.position - _player.position).sqrMagnitude;
            if (d < best) { best = d; nearest = c.transform; }
        }
        return nearest;
    }*/
}
