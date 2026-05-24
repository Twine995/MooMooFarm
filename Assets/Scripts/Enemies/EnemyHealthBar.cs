// EnemyHealthBar.cs — Scripts/Enemies/
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Vector3 worldOffset = new Vector3(0f, 2.6f, 0f);

    Image _fill;
    Transform _cam;

    void Awake()
    {
        _cam = Camera.main?.transform;
        Build();
    }

    void LateUpdate()
    {
        if (transform.parent != null)
            transform.position = transform.parent.position + worldOffset;

        if (_cam != null)
            transform.forward = _cam.forward;
    }

    public void Init(float maxHealth)
    {
        UpdateBar(maxHealth, maxHealth);
    }

    public void OnDamaged(float current, float max)
    {
        UpdateBar(current, max);
    }

    void UpdateBar(float current, float max)
    {
        float pct = Mathf.Clamp01(current / max);
        _fill.fillAmount = pct;
        _fill.color = Color.Lerp(Color.red, Color.green, pct);
    }

    void Build()
    {
        var go = new GameObject("Canvas");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = Vector2.one;
        rt.localScale = new Vector3(2f, 0.2f, 1f);

        // Background
        var bg = new GameObject("BG");
        bg.transform.SetParent(go.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        Stretch(bgImg.rectTransform, Vector2.zero, Vector2.zero);

        // Fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(go.transform, false);
        _fill = fill.AddComponent<Image>();
        _fill.color = Color.green;
        _fill.type = Image.Type.Filled;
        _fill.fillMethod = Image.FillMethod.Horizontal;
        _fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        _fill.fillAmount = 1f;
        Stretch(_fill.rectTransform, new Vector2(0.02f, 0.02f), new Vector2(-0.02f, -0.02f));
    }

    void Stretch(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = min;
        rt.offsetMax = max;
    }
}