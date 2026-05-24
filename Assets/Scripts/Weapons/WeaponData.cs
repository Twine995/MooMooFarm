using UnityEngine;

public enum WeaponTag { Piercing, Fire, Summon, Aura, Bounce, Chain }

[CreateAssetMenu(menuName = "MooMooFarm/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public WeaponTag[] tags;
    public Sprite icon;

    [Header("Base stats")]
    public GameObject projectilePrefab;
    public float cooldown        = 1.2f;  // seconds between shots
    public int   projectileCount = 1;
    public float damage          = 10f;
    public float projectileSpeed = 12f;
    public float range           = 20f;   // max travel distance
    public int   pierce          = 0;     // enemies passed through before returning to pool
    public int   bounces         = 0;

    [Header("Upgrade tiers (index = tier - 1)")]
    public WeaponUpgrade[] upgrades;
}

[System.Serializable]
public struct WeaponUpgrade
{
    public string description;
    public float  damageBonus;
    public float  cooldownMultiplier;   // < 1 = faster
    public int    pierceBonus;
    public int    projectileCountBonus;
}
