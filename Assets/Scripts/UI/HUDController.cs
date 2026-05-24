using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Updates run timer, health bar, XP bar, and level label every frame.
/// Requires TextMeshPro (package: com.unity.textmeshpro).
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Run info")]
    public TMP_Text timerLabel;
    public TMP_Text levelLabel;

    [Header("Health bar")]
    public Slider   healthSlider;
    public TMP_Text healthLabel;

    [Header("XP bar")]
    public Slider   xpSlider;

    PlayerController _player;

    void Start()
    {
        _player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (GameManager.Instance == null || _player == null) return;

        // Run timer
        float t   = GameManager.Instance.RunTimer;
        int   min = Mathf.FloorToInt(t / 60f);
        int   sec = Mathf.FloorToInt(t % 60f);
        if (timerLabel) timerLabel.text = $"{min:00}:{sec:00}";

        // Level
        if (levelLabel) levelLabel.text = $"Lv {GameManager.Instance.PlayerLevel}";

        // Health
        if (healthSlider)
        {
            healthSlider.value = _player.Health / 100f; // assumes maxHealth=100; extend if dynamic
            if (healthLabel)
                healthLabel.text = $"{Mathf.CeilToInt(_player.Health)}";
        }

        // XP
        if (xpSlider)
            xpSlider.value = (float)GameManager.Instance.PlayerXP /
                             GameManager.Instance.XPToNextLevel;
    }
}
