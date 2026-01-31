using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ExpandingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("버튼 설정")]
    [SerializeField] private RectTransform buttonRectTransform;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private string normalText = "";
    [SerializeField] private string expandedText = "Expanded Text";
    
    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float normalWidth = 140f;
    private readonly float _expandedWidthPadding = 160f; // icon Size
    
    private Coroutine _animationCoroutine;
    private float _targetWidth;
    
    private void Awake()
    {
        // 컴포넌트가 할당되지 않았다면 자동으로 찾기
        if (buttonRectTransform == null)
            buttonRectTransform = GetComponent<RectTransform>();
            
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            
        // 초기 상태 설정
        buttonText.text = normalText;
        buttonRectTransform.sizeDelta = new Vector2(normalWidth, buttonRectTransform.sizeDelta.y);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 확장된 텍스트로 변경
        buttonText.text = expandedText;
    
        // 텍스트 크기에 맞게 너비 계산
        float textWidth = buttonText.preferredWidth;
        _targetWidth = textWidth + _expandedWidthPadding;
    
        // 이전 애니메이션 중단
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        
        // 새 애니메이션 시작
        _animationCoroutine = StartCoroutine(AnimateButton(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 기본 텍스트로 변경
        buttonText.text = normalText;
    
        // 이전 애니메이션 중단
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        
        // 새 애니메이션 시작
        _animationCoroutine = StartCoroutine(AnimateButton(false));
    }

    private IEnumerator AnimateButton(bool expand)
    {
        float startWidth = expand ? normalWidth : buttonRectTransform.sizeDelta.x;
        float endWidth = expand ? _targetWidth : normalWidth;
        float elapsedTime = 0f;
    
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            t = EaseInOutQuad(t);
        
            float currentWidth = Mathf.Lerp(startWidth, endWidth, t);
            buttonRectTransform.sizeDelta = new Vector2(currentWidth, buttonRectTransform.sizeDelta.y);
        
            yield return null; // 다음 프레임까지 대기
        }
    
        // 정확한 최종 크기 설정
        buttonRectTransform.sizeDelta = new Vector2(endWidth, buttonRectTransform.sizeDelta.y);
    }
    
    // 부드러운 애니메이션을 위한 이징 함수
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}