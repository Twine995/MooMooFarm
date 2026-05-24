// PlayerHealthBar.cs — Scripts/UI/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;

    [Header("Colors")]
    public Color healthColor = new Color(0.75f, 0.08f, 0.08f); // deep red
    public Color healthLowColor = new Color(1.00f, 0.30f, 0.00f); // orange when < 25%
    public Color bgColor = new Color(0.08f, 0.05f, 0.05f);
    public Color borderColor = new Color(0.55f, 0.40f, 0.20f); // gold border

    [Header("Damage flash")]
    public Color flashColor = new Color(1f, 1f, 1f, 0.5f);
    public float flashDuration = 0.12f;

    // ── UI elements ────────────────────────────────────────
    Image _fill;
    Image _flash;
    float _lastHealth;

    void Awake()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        BuildBar();
        _lastHealth = player != null ? player.Health : 100f;
    }

    void Update()
    {
        if (player == null) return;

        float pct = Mathf.Clamp01(player.Health / player.maxHealth);
        _fill.fillAmount = Mathf.Lerp(_fill.fillAmount, pct, 10f * Time.deltaTime); // smooth drain
        _fill.color = pct < 0.25f ? healthLowColor : healthColor;

        // Trigger flash when health drops
        if (player.Health < _lastHealth)
            StartCoroutine(DamageFlash());
        _lastHealth = player.Health;
    }

    IEnumerator DamageFlash()
    {
        _flash.color = flashColor;
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            Color c = _flash.color;
            c.a = Mathf.Lerp(0.5f, 0f, t / flashDuration);
            _flash.color = c;
            yield return null;
        }
        _flash.color = Color.clear;
    }

    // ── Build bar procedurally ─────────────────────────────
    void BuildBar()
    {
        var canvas = GetComponentInParent<Canvas>();

        // ── Outer container (bottom center) ───────────────
        var container = new GameObject("HealthBarContainer");
        container.transform.SetParent(canvas.transform, false);
        var containerRt = container.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0f);
        containerRt.anchorMax = new Vector2(0.5f, 0f);
        containerRt.pivot = new Vector2(0.5f, 0f);
        containerRt.anchoredPosition = new Vector2(0f, 20f);  // 20px from bottom
        containerRt.sizeDelta = new Vector2(320f, 32f);

        // ── Border ────────────────────────────────────────
        var border = MakeImage(container, "Border", borderColor);
        var borderRt = border.GetComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero;
        borderRt.anchorMax = Vector2.one;
        borderRt.offsetMin = new Vector2(-3f, -3f);
        borderRt.offsetMax = new Vector2(3f, 3f);

        // ── Background ────────────────────────────────────
        var bg = MakeImage(container, "BG", bgColor);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // ── Fill ──────────────────────────────────────────
        var fillGo = MakeImage(container, "Fill", healthColor);
        _fill = fillGo.GetComponent<Image>();
        _fill.type = Image.Type.Filled;
        _fill.fillMethod = Image.FillMethod.Horizontal;
        _fill.fillOrigin = 0;
        _fill.fillAmount = 1f;
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(3f, 3f);
        fillRt.offsetMax = new Vector2(-3f, -3f);

        // ── Damage flash overlay ───────────────────────────
        var flashGo = MakeImage(container, "Flash", Color.clear);
        _flash = flashGo.GetComponent<Image>();
        var flashRt = flashGo.GetComponent<RectTransform>();
        flashRt.anchorMin = Vector2.zero;
        flashRt.anchorMax = Vector2.one;
        flashRt.offsetMin = Vector2.zero;
        flashRt.offsetMax = Vector2.zero;

        // ── Label ─────────────────────────────────────────
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(container.transform, false);
        var text = labelGo.AddComponent<Text>();
        text.text = "HP";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 13;
        text.color = new Color(0.85f, 0.70f, 0.50f); // warm gold
        text.alignment = TextAnchor.MiddleCenter;
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
    }

    GameObject MakeImage(GameObject parent, string goName, Color color)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }
}