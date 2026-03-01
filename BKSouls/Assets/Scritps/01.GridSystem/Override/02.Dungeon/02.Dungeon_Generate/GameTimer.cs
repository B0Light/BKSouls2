using System;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText; // UI Text 컴포넌트
    [SerializeField] private float gameTime = 600f; // 게임 시간 (초) - 기본 5분
    [SerializeField] private bool countDown = true; // true면 카운트다운, false면 카운트업
    
    private float currentTime;
    private bool isTimerRunning = false;
    
    // 이벤트
    public static event Action OnTimerStart;
    public static event Action OnTimerEnd;
    public static event Action<float> OnTimerUpdate;

    private void Awake()
    {
        // 초기 시간 설정
        currentTime = countDown ? gameTime : 0f;
        UpdateTimerDisplay();
    }

   
    private void FixedUpdate()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
        }
    }

    private void StartTimer()
    {
        isTimerRunning = true;
        OnTimerStart?.Invoke();
        Debug.Log("타이머 시작!");
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        Debug.Log("타이머 정지!");
    }

    public void ResetTimer()
    {
        currentTime = countDown ? gameTime : 0f;
        isTimerRunning = false;
        UpdateTimerDisplay();
        Debug.Log("타이머 리셋!");
    }

    private void UpdateTimer()
    {
        if (countDown)
        {
            currentTime -= Time.deltaTime;
            
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isTimerRunning = false;
                OnTimerEnd?.Invoke();
                Debug.Log("시간 종료!");
            }
        }
        else
        {
            currentTime += Time.deltaTime;
            
            if (currentTime >= gameTime)
            {
                currentTime = gameTime;
                isTimerRunning = false;
                OnTimerEnd?.Invoke();
                Debug.Log("시간 종료!");
            }
        }
        
        UpdateTimerDisplay();
        OnTimerUpdate?.Invoke(currentTime);
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // 현재 시간 반환
    public float GetCurrentTime()
    {
        return currentTime;
    }

    // 남은 시간 반환 (카운트다운의 경우)
    public float GetRemainingTime()
    {
        return countDown ? currentTime : gameTime - currentTime;
    }

    // 타이머 실행 상태 반환
    public bool IsRunning()
    {
        return isTimerRunning;
    }
}