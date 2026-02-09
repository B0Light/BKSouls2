using UnityEngine;

namespace BK
{

    [RequireComponent(typeof(CanvasGroup))]
    public class GUIComponent : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OpenGUI()
        {
            //InputHandlerManager.Instance.SetInputMode(InputMode.OpenUI);
            ToggleGUI(true);
        }

        public virtual void CloseGUI()
        {
            //InputHandlerManager.Instance.SetLastInputMode();
            ToggleGUI(false);
        }

        private void ToggleGUI(bool value)
        {
            _canvasGroup.alpha = value ? 1 : 0;
            _canvasGroup.interactable = value;
            _canvasGroup.blocksRaycasts = value;
        }

        public virtual void SelectNextGUI()
        {
        }
    }
}