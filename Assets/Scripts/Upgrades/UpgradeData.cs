using UnityEngine;

public enum UpgradeType { NewWeapon, WeaponTier, PassiveStat }

[CreateAssetMenu(menuName = "MooMooFarm/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string        upgradeName;
    [TextArea] public string description;
    public Sprite        icon;
    public UpgradeType   type;

    // For NewWeapon / WeaponTier upgrades
    public WeaponData    weaponData;

    // For PassiveStat upgrades
    public float         healthBonus;
    public float         moveSpeedBonus;
    public float         damageMult;     // multiplier stacked additively, e.g. 0.1 = +10%
}
