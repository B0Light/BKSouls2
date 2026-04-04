using System.Collections;
using System.Collections.Generic;
using BK;
using UnityEngine;

[System.Serializable]
public class DungeonBGMData
{
    [Header("던전 정보")]
    public string dungeonName;
    public int dungeonID;
    
    [Header("BGM 설정")]
    public AudioClip normalBGM;        // 일반 던전 BGM
    public AudioClip bossBGMIntro;     // 보스 인트로 BGM
    public AudioClip bossBGMLoop;      // 보스 루프 BGM
    public AudioClip clearBGM;         // 던전 클리어 BGM
    
    [Header("BGM 설정")]
    public bool fadeInOnEnter = true;   // 입장 시 페이드인
    public bool fadeOutOnExit = true;   // 퇴장 시 페이드아웃
    public float fadeTime = 2f;         // 페이드 시간
}

public class DungeonBGMManager : MonoBehaviour
{
    [Header("던전 BGM 데이터")]
    [SerializeField] private List<DungeonBGMData> dungeonBGMList = new List<DungeonBGMData>();
    
    [Header("현재 상태")]
    [SerializeField] private int currentDungeonID;
    [SerializeField] private DungeonBGMData currentDungeonData;
    
    private bool _isBossMode = false;
    private Coroutine _currentFadeCoroutine;
    
    
    #region 던전 BGM 제어 메서드
    
    /// <summary>
    /// 던전 입장 시 호출
    /// </summary>
    private void OnDungeonEnter(int dungeonID)
    {
        DungeonBGMData dungeonData = GetDungeonBGMData(dungeonID);
        if (dungeonData == null)
        {
            Debug.LogWarning($"던전 ID '{dungeonID}'에 해당하는 BGM 데이터를 찾을 수 없습니다.");
            return;
        }
        
        currentDungeonID = dungeonID;
        currentDungeonData = dungeonData;
        _isBossMode = false;
        
        // 기존 BGM 정지
        WorldSoundFXManager.Instance.StopBGM();
        
        // 던전 BGM 재생
        if (dungeonData.normalBGM != null)
        {
            if (dungeonData.fadeInOnEnter)
            {
                StartFadeBGM(dungeonData.normalBGM, dungeonData.fadeTime, true);
            }
            else
            {
                WorldSoundFXManager.Instance.PlayBGM(dungeonData.normalBGM);
            }
        }
        
        Debug.Log($"던전 '{dungeonData.dungeonName}' 입장 - BGM 재생 시작");
    }
    
    /// <summary>
    /// 던전 퇴장 시 호출
    /// </summary>
    public void OnDungeonExit()
    {
        if (currentDungeonData == null) return;
        
        if (currentDungeonData.fadeOutOnExit)
        {
            StartFadeBGM(null, currentDungeonData.fadeTime, false);
        }
        else
        {
            WorldSoundFXManager.Instance.StopBGM();
        }
        
        Debug.Log($"던전 '{currentDungeonData.dungeonName}' 퇴장 - BGM 정지");
        
        currentDungeonID = 0;
        currentDungeonData = null;
        _isBossMode = false;
    }
    
    /// <summary>
    /// 보스 전투 시작 시 호출
    /// </summary>
    public void OnBossStart()
    {
        if (currentDungeonData == null) return;
        
        _isBossMode = true;
        
        // 보스 BGM이 설정되어 있는 경우
        if (currentDungeonData.bossBGMIntro != null && currentDungeonData.bossBGMLoop != null)
        {
            WorldSoundFXManager.Instance.StopBGM();
            WorldSoundFXManager.Instance.PlayBossTrack(currentDungeonData.bossBGMIntro, currentDungeonData.bossBGMLoop);
            Debug.Log("보스 BGM 재생 시작");
        }
        else if (currentDungeonData.bossBGMLoop != null)
        {
            // 인트로 없이 루프만 있는 경우
            WorldSoundFXManager.Instance.PlayBGM(currentDungeonData.bossBGMLoop);
            Debug.Log("보스 BGM (루프만) 재생 시작");
        }
    }
    
    /// <summary>
    /// 보스 전투 종료 시 호출
    /// </summary>
    public void OnBossEnd()
    {
        if (currentDungeonData == null) return;
        
        _isBossMode = false;
        
        // 보스 BGM 정지
        WorldSoundFXManager.Instance.StopBossMusic();
        
        // 일반 던전 BGM으로 복귀
        if (currentDungeonData.normalBGM != null)
        {
            StartCoroutine(DelayedBGMPlay(currentDungeonData.normalBGM, 1f));
        }
        
        Debug.Log("보스 BGM 종료 - 일반 던전 BGM으로 복귀");
    }
    
    /// <summary>
    /// 던전 클리어 시 호출
    /// </summary>
    public void OnDungeonClear()
    {
        if (currentDungeonData == null) return;
        
        // 클리어 BGM이 있는 경우 재생
        if (currentDungeonData.clearBGM != null)
        {
            WorldSoundFXManager.Instance.StopBGM();
            WorldSoundFXManager.Instance.StopBossMusic();
            WorldSoundFXManager.Instance.PlayBGM(currentDungeonData.clearBGM);
            Debug.Log("던전 클리어 BGM 재생");
        }
    }
    
    #endregion
    
    #region 헬퍼 메서드
    
    /// <summary>
    /// 던전 ID로 BGM 데이터 찾기
    /// </summary>
    private DungeonBGMData GetDungeonBGMData(int dungeonID)
    {
        foreach (var data in dungeonBGMList)
        {
            if (data.dungeonID == dungeonID)
            {
                return data;
            }
        }
        return null;
    }
    
    /// <summary>
    /// BGM 페이드 인/아웃
    /// </summary>
    private void StartFadeBGM(AudioClip newClip, float fadeTime, bool fadeIn)
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
        }
        
        _currentFadeCoroutine = StartCoroutine(FadeBGMCoroutine(newClip, fadeTime, fadeIn));
    }
    
    private IEnumerator FadeBGMCoroutine(AudioClip newClip, float fadeTime, bool fadeIn)
    {
        if (fadeIn && newClip != null)
        {
            // 페이드 인
            WorldSoundFXManager.Instance.PlayBGM(newClip);
            
            float currentTime = 0f;
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                float volume = Mathf.Lerp(0f, 1f, currentTime / fadeTime);
                WorldSoundFXManager.Instance.SetBGMVolume(volume);
                yield return null;
            }
            
            WorldSoundFXManager.Instance.SetBGMVolume(1f);
        }
        else
        {
            // 페이드 아웃
            float currentTime = 0f;
            float startVolume = WorldSoundFXManager.Instance.GetBGMVolume();
            
            while (currentTime < fadeTime)
            {
                currentTime += Time.deltaTime;
                float volume = Mathf.Lerp(startVolume, 0f, currentTime / fadeTime);
                WorldSoundFXManager.Instance.SetBGMVolume(volume);
                yield return null;
            }
            
            WorldSoundFXManager.Instance.StopBGM();
            WorldSoundFXManager.Instance.SetBGMVolume(startVolume); // 원래 볼륨으로 복구
        }
        
        _currentFadeCoroutine = null;
    }
    
    /// <summary>
    /// 지연된 BGM 재생
    /// </summary>
    private IEnumerator DelayedBGMPlay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        WorldSoundFXManager.Instance.PlayBGM(clip);
    }
    
    #endregion
    
    #region 공개 메서드
    
    /// <summary>
    /// 현재 던전 BGM 강제 변경
    /// </summary>
    public void ChangeDungeonBGM(AudioClip newBGM)
    {
        if (newBGM != null && !_isBossMode)
        {
            WorldSoundFXManager.Instance.PlayBGM(newBGM);
        }
    }
    
    /// <summary>
    /// 현재 던전 정보 반환
    /// </summary>
    public DungeonBGMData GetCurrentDungeonData()
    {
        return currentDungeonData;
    }
    
    /// <summary>
    /// 보스 모드 상태 반환
    /// </summary>
    public bool IsBossMode()
    {
        return _isBossMode;
    }
    
    #endregion
}