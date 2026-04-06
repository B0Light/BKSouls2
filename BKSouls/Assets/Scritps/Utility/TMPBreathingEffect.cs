using UnityEngine;
using TMPro;

public class TMPBreathingEffect : MonoBehaviour
{
    private TextMeshProUGUI _textMeshPro; // TMP 오브젝트
    private float _speed = 3f; // 브리딩 속도
    private float _minAlpha = 0.2f; // 최소 투명도
    private float _maxAlpha = 1f; // 최대 투명도

    private Color originalColor;

    void Start()
    {
        if (_textMeshPro == null) _textMeshPro = GetComponent<TextMeshProUGUI>();
        originalColor = _textMeshPro.color;
    }

    void Update()
    {
        float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, (Mathf.Sin(Time.time * _speed) + 1f) / 2f);
        _textMeshPro.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }
}