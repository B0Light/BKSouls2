using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

namespace BK
{
public class KeyRebindingManager : MonoBehaviour
{
    [Header("Input Action Settings")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("UI Settings")]
    [SerializeField] private KeyBindHeader keyBindHeaderPrefab;
    [SerializeField] private KeyBindingUI keyBindingPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button resetButton;
    
    [Header("Control Scheme Selection")]
    [SerializeField] private Button leftSchemeButton;    // 왼쪽 버튼 (이전 스키마)
    [SerializeField] private Button rightSchemeButton;   // 오른쪽 버튼 (다음 스키마)
    [SerializeField] private TextMeshProUGUI currentSchemeText; // 현재 스키마 표시 텍스트

    [Header("Rebinding Panel")]
    [SerializeField] private GameObject rebindPanel;
    [SerializeField] private TextMeshProUGUI rebindText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Dictionary<string, KeyBindingUI> _actionUIMap = new Dictionary<string, KeyBindingUI>();
    private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
    private InputAction _currentAction;
    private int _currentBindingIndex;
    private string _currentControlScheme; // 현재 활성화된 컨트롤 스키마
    private int _currentSchemeIndex;      // 현재 스키마 인덱스
    private List<InputControlScheme> _availableSchemes = new List<InputControlScheme>(); // 사용 가능한 스키마 목록

    private bool _hasSchemes;

    private void Awake()
    {
        rebindPanel.SetActive(false);

        resetButton.onClick.AddListener(ResetBinding);
        confirmButton.onClick.AddListener(ConfirmRebinding);
        cancelButton.onClick.AddListener(CancelRebinding);

        InitializeControlSchemeButtons();
        CreateBindingUI();
    }

    private void InitializeControlSchemeButtons()
    {
        _availableSchemes.Clear();
        foreach (var controlScheme in inputActions.controlSchemes)
            _availableSchemes.Add(controlScheme);

        _hasSchemes = _availableSchemes.Count > 0;

        // 스키마가 없으면 선택 UI 전체 숨김
        if (!_hasSchemes)
        {
            if (leftSchemeButton != null)  leftSchemeButton.gameObject.SetActive(false);
            if (rightSchemeButton != null) rightSchemeButton.gameObject.SetActive(false);
            if (currentSchemeText != null) currentSchemeText.gameObject.SetActive(false);
            _currentControlScheme = null;
            return;
        }

        if (leftSchemeButton != null)
            leftSchemeButton.onClick.AddListener(PreviousControlScheme);
        if (rightSchemeButton != null)
            rightSchemeButton.onClick.AddListener(NextControlScheme);

        int savedSchemeIndex = PlayerPrefs.GetInt("SelectedControlScheme", 0);
        _currentSchemeIndex = savedSchemeIndex < _availableSchemes.Count ? savedSchemeIndex : 0;
        _currentControlScheme = _availableSchemes[_currentSchemeIndex].name;
        UpdateSchemeDisplay();
    }

    // 추가: 이전 컨트롤 스키마로 변경
    private void PreviousControlScheme()
    {
        if (_availableSchemes.Count <= 1) return;
        
        _currentSchemeIndex--;
        if (_currentSchemeIndex < 0)
        {
            _currentSchemeIndex = _availableSchemes.Count - 1;
        }
        
        OnControlSchemeChanged(_currentSchemeIndex);
    }

    // 추가: 다음 컨트롤 스키마로 변경
    private void NextControlScheme()
    {
        if (_availableSchemes.Count <= 1) return;
        
        _currentSchemeIndex++;
        if (_currentSchemeIndex >= _availableSchemes.Count)
        {
            _currentSchemeIndex = 0;
        }
        
        OnControlSchemeChanged(_currentSchemeIndex);
    }

    // 수정: 컨트롤 스키마 변경 처리
    private void OnControlSchemeChanged(int index)
    {
        if (index < _availableSchemes.Count)
        {
            _currentSchemeIndex = index;
            _currentControlScheme = _availableSchemes[index].name;
            
            // 선택한 스키마 저장
            PlayerPrefs.SetInt("SelectedControlScheme", index);
            PlayerPrefs.Save();
            
            // UI 업데이트
            UpdateSchemeDisplay();
            
            // 바인딩 UI 새로 생성
            CreateBindingUI();
        }
    }

    // 추가: 스키마 표시 업데이트
    private void UpdateSchemeDisplay()
    {
        if (currentSchemeText != null)
        {
            currentSchemeText.text = _currentControlScheme;
        }
        
        // 버튼 활성화/비활성화 (스키마가 1개뿐이면 버튼 비활성화)
        bool hasMultipleSchemes = _availableSchemes.Count > 1;
        if (leftSchemeButton != null)
            leftSchemeButton.interactable = hasMultipleSchemes;
        if (rightSchemeButton != null)
            rightSchemeButton.interactable = hasMultipleSchemes;
    }

    // 수정: CreateBindingUI 메서드 - 현재 컨트롤 스키마에 맞는 바인딩만 표시
    private void CreateBindingUI()
    {
        // 기존 UI 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        
        _actionUIMap.Clear();
        
        // 각 액션 맵 순회
        foreach (var actionMap in inputActions.actionMaps)
        {
            bool createdHeader = false;
            KeyBindHeader header = null;
            
            // 각 액션 순회
            foreach (var action in actionMap.actions)
            {
                bool actionHasBindingsForScheme = false;
                string actionName = action.name;
                KeyBindingUI keyUI = null;
                
                // 각 바인딩 순회
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];

                    // 컴포지트 자식(isPartOfComposite)은 개별 표시하지 않음
                    // 컴포지트 부모(isComposite)는 표시 대상 - 아래에서 처리
                    if (binding.isPartOfComposite)
                        continue;

                    // 현재 컨트롤 스키마에 맞는 바인딩인지 확인
                    if (!IsBindingMatchingControlScheme(binding))
                        continue;

                    // 헤더 생성
                    if (!createdHeader)
                    {
                        header = Instantiate(keyBindHeaderPrefab, contentParent);
                        header.Initialize(actionMap.name);
                        createdHeader = true;
                    }

                    // 액션 UI 생성
                    if (keyUI == null)
                    {
                        keyUI = Instantiate(keyBindingPrefab, contentParent);
                        keyUI.Initialize(actionName);
                        actionHasBindingsForScheme = true;
                    }

                    string bindingId = action.id + "/" + i;
                    int currentIndex = i;

                    // 컴포지트 바인딩은 묶음 전체 표시 문자열 사용
                    string displayString = binding.isComposite
                        ? action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontOmitDevice)
                        : action.GetBindingDisplayString(i);

                    keyUI.AddButton(
                        currentIndex,
                        displayString,
                        () => StartRebinding(action, currentIndex, actionName, bindingId)
                    );

                    _actionUIMap[bindingId] = keyUI;
                }
                
                // 액션에 현재 스키마에 맞는 바인딩이 없으면 UI 제거
                if (keyUI != null && !actionHasBindingsForScheme)
                {
                    Destroy(keyUI.gameObject);
                }
            }
            
            // 액션 맵에 현재 스키마에 맞는 바인딩이 없으면 헤더 제거
            if (header != null && !createdHeader)
            {
                Destroy(header.gameObject);
            }
        }
    }
    
    // 추가: 바인딩이 현재 컨트롤 스키마에 맞는지 확인하는 메서드
    private bool IsBindingMatchingControlScheme(InputBinding binding)
    {
        // 바인딩에 지정된 그룹이 없으면 모든 스키마에 표시
        if (string.IsNullOrEmpty(binding.groups))
        {
            return true;
        }
        
        // 스키마가 선택되지 않았으면 모든 바인딩 표시
        if (string.IsNullOrEmpty(_currentControlScheme))
        {
            return true;
        }
        
        // 바인딩 그룹이 현재 컨트롤 스키마를 포함하는지 확인
        string[] groups = binding.groups.Split(';');
        foreach (var group in groups)
        {
            if (group.Trim().Equals(_currentControlScheme, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }

    private void StartRebinding(InputAction action, int bindingIndex, string actionName, string bindingId)
    {
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError($"바인딩 인덱스 오류: {bindingIndex}는 액션 '{actionName}'의 유효한 인덱스가 아닙니다.");
            return;
        }

        // 컴포지트 바인딩은 리바인딩 불가 (자식 파트를 직접 리바인딩해야 함)
        if (action.bindings[bindingIndex].isComposite)
        {
            Debug.LogWarning($"'{actionName}'은 컴포지트 바인딩입니다. 개별 키를 리바인딩해 주세요.");
            return;
        }
        
        // 현재 리바인딩 중인 액션 정보 저장
        _currentAction = action;
        _currentBindingIndex = bindingIndex;
        
        // 바인딩 UI 활성화
        rebindPanel.SetActive(true);
        rebindText.text = $"{actionName} 키 바인딩을 위해 키를 누르세요...";
        
        // 일시적으로 현재 액션을 비활성화 (바인딩 중에는 액션이 트리거되지 않도록)
        _currentAction.Disable();
        
        try
        {
            // 리바인딩 작업 시작
            _rebindOperation = _currentAction.PerformInteractiveRebinding(_currentBindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    rebindText.text = _currentAction.GetBindingDisplayString(_currentBindingIndex);
                    // 리바인딩 작업이 완료되면 바인딩 텍스트 업데이트
                    if (_actionUIMap.TryGetValue(bindingId, out KeyBindingUI keyUI))
                    {
                        keyUI.UpdateKeyText(bindingIndex,_currentAction.GetBindingDisplayString(_currentBindingIndex));
                    }
                })
                .Start();
        }
        catch (System.ArgumentOutOfRangeException ex)
        {
            Debug.LogError($"리바인딩 오류: {ex.Message}");
            // 오류 발생 시 패널 닫기
            rebindPanel.SetActive(false);
            // 액션 다시 활성화
            _currentAction.Enable();
        }
    }

    private void OnEnable()
    {
        // 인풋 액션 활성화
        inputActions.Enable();
    }

    private void OnDisable()
    {
        // 인풋 액션 비활성화
        inputActions.Disable();
    }

    private void ConfirmRebinding()
    {
        if (_rebindOperation != null)
        {
            // 리바인딩 작업 완료 처리
            _rebindOperation.Dispose();
            _rebindOperation = null;
            
            // 바인딩 저장 (PlayerPrefs 사용 예시)
            SaveBindings();
            
            // 액션 다시 활성화
            _currentAction.Enable();
            
            // UI 비활성화
            rebindPanel.SetActive(false);
        }
    }

    private void CancelRebinding()
    {
        if (_rebindOperation != null)
        {
            // 리바인딩 작업 취소
            _rebindOperation.Dispose();
            _rebindOperation = null;
            
            // 이전 바인딩으로 복원
            _currentAction.RemoveBindingOverride(_currentBindingIndex);
            
            // UI 업데이트 (이전 바인딩 값으로 복원)
            string bindingId = _currentAction.id.ToString() + "/" + _currentBindingIndex;
            if (_actionUIMap.TryGetValue(bindingId, out KeyBindingUI keyUI))
            {
                keyUI.UpdateKeyText(_currentBindingIndex, _currentAction.GetBindingDisplayString(_currentBindingIndex));
            }
            
            // 액션 다시 활성화
            _currentAction.Enable();
            
            // UI 비활성화
            rebindPanel.SetActive(false);
        }
    }

    private void ResetBinding()
    {
        PlayerPrefs.DeleteKey("InputBindings");
        PlayerPrefs.Save();

        // 모든 액션의 바인딩 오버라이드 제거
        foreach (var actionMap in inputActions.actionMaps)
        {
            actionMap.RemoveAllBindingOverrides();
        }

        // UI 업데이트 (기본 바인딩으로 복원)
        CreateBindingUI(); // 모든 UI를 다시 생성하도록 변경
    }

    private void SaveBindings()
    {
        // 바인딩 설정을 JSON 형태로 저장
        string rebinds = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("InputBindings", rebinds);
        PlayerPrefs.Save();
    }

    private void LoadBindings()
    {
        // 저장된 바인딩 불러오기
        string rebinds = PlayerPrefs.GetString("InputBindings");
        if (!string.IsNullOrEmpty(rebinds))
        {
            inputActions.LoadBindingOverridesFromJson(rebinds);
            
            // UI 갱신 (현재 컨트롤 스키마에 맞게)
            CreateBindingUI();
        }
    }

    // 게임 시작 시 바인딩 로드
    private void Start()
    {
        LoadBindings();
    }
}
}