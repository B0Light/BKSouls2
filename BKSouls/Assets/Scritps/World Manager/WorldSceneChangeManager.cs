using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BK
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WorldSceneChangeManager : Singleton<WorldSceneChangeManager>
    {
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private GameObject continueText;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        private CanvasGroup _canvasGroup;
        [SerializeField] private RandomTooltipSystem randomTooltipSystem;

        [Header("Loading Progress Control")]
        [SerializeField] private float maxProgressSpeed = 0.5f;
        [SerializeField] private float minProgressSpeed = 0.1f;
        [SerializeField] private bool useProgressSpeedLimit = true;

        private float _currentDisplayProgress = 0f;
        private readonly WaitForSeconds _waitHalfSecond = new(0.5f);
        [SerializeField] private string titleSceneName = "01.TitleScene";
        [SerializeField] private string shelterSceneName = "03.Shelter";

        public static event Action OnSceneEndPhase;
        public static event Action OnSceneChanged;

        protected override void Awake()
        {
            base.Awake();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            continueText.SetActive(false);
            loadingScreen.SetActive(false);
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void LoadSceneAsync(string sceneCode)
        {
            StartCoroutine(LoadSceneCoroutine(sceneCode));
        }

        public void LoadShelter()
        {
            StartCoroutine(LoadSceneCoroutine(shelterSceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneToLoad)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);
            OnSceneEndPhase?.Invoke();
            yield return StartCoroutine(HandleSceneLoading(asyncOperation, sceneToLoad));
        }

        private IEnumerator HandleSceneLoading(AsyncOperation asyncOperation, string sceneToLoad)
        {
            asyncOperation.allowSceneActivation = false;

            if (sceneToLoad == titleSceneName)
                GUIController.Instance.playerUIHudManager.ToggleHUD(false);
            else
                GUIController.Instance.playerUIHudManager.ToggleHUD(true);

            loadingScreen.SetActive(true);
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _currentDisplayProgress = 0f;

            bool sceneLoadingCompleted = false;
            randomTooltipSystem?.StartTooltipSystem();

            while (!asyncOperation.isDone)
            {
                if (asyncOperation.progress < 0.9f)
                {
                    _currentDisplayProgress = useProgressSpeedLimit
                        ? UpdateProgressWithSpeedLimit(asyncOperation.progress, _currentDisplayProgress)
                        : asyncOperation.progress;

                    UpdateLoadingUI(_currentDisplayProgress);
                }

                if (asyncOperation.progress >= 0.9f && !sceneLoadingCompleted)
                {
                    sceneLoadingCompleted = true;
                    yield return StartCoroutine(AnimateProgressTo100Percent());

                    asyncOperation.allowSceneActivation = true;
                    yield return _waitHalfSecond;
                    continueText.SetActive(true);
                }

                yield return null;
            }

            randomTooltipSystem?.StopTooltipSystem();
        }

        private float UpdateProgressWithSpeedLimit(float targetProgress, float currentProgress)
        {
            float progressDifference = targetProgress - currentProgress;
            if (progressDifference > 0)
            {
                float maxProgressThisFrame = maxProgressSpeed * Time.deltaTime;
                float minProgressThisFrame = minProgressSpeed * Time.deltaTime;
                float progressToAdd = Mathf.Clamp(progressDifference, minProgressThisFrame, maxProgressThisFrame);
                return currentProgress + progressToAdd;
            }
            return targetProgress;
        }

        private void UpdateLoadingUI(float progress)
        {
            if (loadingBar)
                loadingBar.value = progress;
            if (loadingText)
                loadingText.text = (progress * 100f).ToString("F0") + "%";
        }

        private IEnumerator AnimateProgressTo100Percent()
        {
            float duration = 3.0f;
            float startValue = _currentDisplayProgress;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
                float currentValue = Mathf.Lerp(startValue, 1.0f, t);

                if (loadingBar)
                    loadingBar.value = currentValue;
                if (loadingText)
                    loadingText.text = currentValue >= 1f ? "Ready" : (currentValue * 100f).ToString("F0") + "%";

                yield return null;
            }

            if (loadingBar) loadingBar.value = 1.0f;
            if (loadingText) loadingText.text = "Ready";
        }

        public void LoadTitle() => LoadSceneAsync(titleSceneName);

        // Button Binding
        public void CloseLoadingScreen()
        {
            continueText.SetActive(false);
            loadingScreen.SetActive(false);

            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            GUIController.HideCursor();
            OnSceneChanged?.Invoke();
        }
    }
}
