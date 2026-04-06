using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomTooltipSystem : MonoBehaviour
{
    [Header("툴팁 설정")]
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    [Header("툴팁 메시지들")]
    [TextArea(2, 4)]
    [SerializeField] private string[] tooltipMessages = {
        "팁: 마우스 휠로 화면을 확대/축소할 수 있습니다",
        "알림: 저장을 자주 하세요!",
        "도움말: Ctrl+Z로 실행 취소가 가능합니다",
        "정보: 우클릭으로 메뉴를 열 수 있습니다"
    };
    
    [Header("타이밍 설정")]
    [SerializeField] private float displayDuration = 3f; // 툴팁 표시 시간
    [SerializeField] private float fadeDuration = 0.5f;  // 페이드 인/아웃 시간
    [SerializeField] private bool autoStart = true;      // 자동 시작 여부
    
    [Header("애니메이션 설정")]
    [SerializeField] private bool useFadeAnimation = true;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private Coroutine tooltipCoroutine;
    private bool isRunning = false;
    private int currentTooltipIndex = -1;
    
    void Start()
    {
        // TextMeshPro 컴포넌트 자동 찾기
        if (tooltipText == null)
        {
            tooltipText = GetComponent<TextMeshProUGUI>();
        }
        
        // 초기 상태 설정
        if (tooltipText != null)
        {
            tooltipText.alpha = 0f;
        }
        
        // 자동 시작
        if (autoStart && tooltipMessages.Length > 0)
        {
            StartTooltipSystem();
        }
    }
    
    /// <summary>
    /// 툴팁 시스템 시작
    /// </summary>
    public void StartTooltipSystem()
    {
        if (tooltipText == null || tooltipMessages.Length == 0)
        {
            Debug.LogWarning("TextMeshPro 컴포넌트나 툴팁 메시지가 설정되지 않았습니다!");
            return;
        }
        
        if (!isRunning)
        {
            isRunning = true;
            tooltipCoroutine = StartCoroutine(TooltipLoop());
        }
    }
    
    /// <summary>
    /// 툴팁 시스템 정지
    /// </summary>
    public void StopTooltipSystem()
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
            tooltipCoroutine = null;
        }
        
        isRunning = false;
        
        // 현재 표시된 툴팁 숨기기
        if (tooltipText != null && useFadeAnimation)
        {
            StartCoroutine(FadeOut());
        }
        else if (tooltipText != null)
        {
            tooltipText.alpha = 0f;
        }
    }
    
    /// <summary>
    /// 메인 툴팁 루프
    /// </summary>
    private IEnumerator TooltipLoop()
    {
        while (isRunning)
        {
            // 랜덤 툴팁 선택 (이전과 다른 툴팁)
            int newIndex = GetRandomTooltipIndex();
            
            // 툴팁 표시
            yield return StartCoroutine(ShowTooltip(newIndex));
            
            // 지정된 시간만큼 대기
            yield return new WaitForSeconds(displayDuration);
            
            // 툴팁 숨기기
            yield return StartCoroutine(HideTooltip());
        }
    }
    
    /// <summary>
    /// 이전과 다른 랜덤 인덱스 가져오기
    /// </summary>
    private int GetRandomTooltipIndex()
    {
        if (tooltipMessages.Length == 1)
            return 0;
        
        int newIndex;
        do
        {
            newIndex = Random.Range(0, tooltipMessages.Length);
        }
        while (newIndex == currentTooltipIndex);
        
        return newIndex;
    }
    
    /// <summary>
    /// 툴팁 표시
    /// </summary>
    private IEnumerator ShowTooltip(int index)
    {
        currentTooltipIndex = index;
        tooltipText.text = tooltipMessages[index];
        
        if (useFadeAnimation)
        {
            yield return StartCoroutine(FadeIn());
        }
        else
        {
            tooltipText.alpha = 1f;
        }
    }
    
    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    private IEnumerator HideTooltip()
    {
        if (useFadeAnimation)
        {
            yield return StartCoroutine(FadeOut());
        }
        else
        {
            tooltipText.alpha = 0f;
        }
    }
    
    /// <summary>
    /// 페이드 인 애니메이션
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeDuration;
            float alpha = fadeInCurve.Evaluate(normalizedTime);
            
            tooltipText.alpha = alpha;
            yield return null;
        }
        
        tooltipText.alpha = 1f;
    }
    
    /// <summary>
    /// 페이드 아웃 애니메이션
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeDuration;
            float alpha = fadeOutCurve.Evaluate(normalizedTime);
            
            tooltipText.alpha = alpha;
            yield return null;
        }
        
        tooltipText.alpha = 0f;
    }
    
    /// <summary>
    /// 특정 툴팁 즉시 표시
    /// </summary>
    public void ShowSpecificTooltip(int index)
    {
        if (index >= 0 && index < tooltipMessages.Length)
        {
            StopTooltipSystem();
            StartCoroutine(ShowTooltip(index));
        }
    }
    
    /// <summary>
    /// 런타임에서 새 툴팁 추가
    /// </summary>
    public void AddTooltip(string newTooltip)
    {
        List<string> tooltipList = new List<string>(tooltipMessages);
        tooltipList.Add(newTooltip);
        tooltipMessages = tooltipList.ToArray();
    }
    
    /// <summary>
    /// 툴팁 메시지 업데이트
    /// </summary>
    public void UpdateTooltipMessages(string[] newMessages)
    {
        bool wasRunning = isRunning;
        
        if (wasRunning)
        {
            StopTooltipSystem();
        }
        
        tooltipMessages = newMessages;
        
        if (wasRunning && newMessages.Length > 0)
        {
            StartTooltipSystem();
        }
    }
    
    void OnDisable()
    {
        //StopTooltipSystem();
    }
    
    void OnDestroy()
    {
        //StopTooltipSystem();
    }
}