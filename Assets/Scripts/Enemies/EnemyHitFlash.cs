// EnemyHitFlash.cs — Scripts/Enemies/
using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [Header("Flash settings")]
    public Color flashColor    = new Color(1f, 0.3f, 0.3f, 1f);
    public float flashDuration = 0.08f;

    Renderer[] _renderers;
    Color[]    _originalColors;
    bool       _flashing;

    void Awake()
    {
        _renderers      = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].material.color;
    }

    public void Flash()
    {
        if (_flashing) return;
        StartCoroutine(DoFlash());
    }

    IEnumerator DoFlash()
    {
        _flashing = true;

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].material.color = _originalColors[i];

        _flashing = false;
    }
}
