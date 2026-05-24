using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens for LevelUp events, picks 3 random upgrades, and applies the chosen one.
/// Wire up the UI buttons to call ChooseUpgrade(index).
/// </summary>
public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem Instance { get; private set; }

    [Header("All available upgrades")]
    public UpgradeData[] upgradePool;

    [Header("UI panel (shown during level-up pause)")]
    public GameObject upgradePickerPanel;

    // The 3 options currently shown to the player
    public UpgradeData[] CurrentOptions { get; private set; } = new UpgradeData[3];

    void Awake() => Instance = this;

    void OnEnable()  => GameManager.OnLevelUp.AddListener(OnLevelUp);
    void OnDisable() => GameManager.OnLevelUp.RemoveListener(OnLevelUp);

    void OnLevelUp(int newLevel)
    {
        CurrentOptions = PickOptions(3);
        upgradePickerPanel.SetActive(true);
        // HUDController or UpgradePickerUI reads CurrentOptions and refreshes buttons
    }

    UpgradeData[] PickOptions(int count)
    {
        var shuffled = new List<UpgradeData>(upgradePool);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        var result = new UpgradeData[Mathf.Min(count, shuffled.Count)];
        for (int i = 0; i < result.Length; i++)
            result[i] = shuffled[i];
        return result;
    }

    // Called by UI button OnClick, passing 0, 1, or 2
    public void ChooseUpgrade(int index)
    {
        if (index < 0 || index >= CurrentOptions.Length) return;
        ApplyUpgrade(CurrentOptions[index]);
        upgradePickerPanel.SetActive(false);
        GameManager.Instance.ResumePlaying();
    }

    void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.NewWeapon:
            case UpgradeType.WeaponTier:
                WeaponSystem.Instance.Equip(upgrade.weaponData);
                break;

            case UpgradeType.PassiveStat:
                var player = FindObjectOfType<PlayerController>();
                if (upgrade.healthBonus    > 0f) player.Heal(upgrade.healthBonus);
                // move speed and damage multiplier: extend PlayerController as needed
                break;
        }
    }
}
