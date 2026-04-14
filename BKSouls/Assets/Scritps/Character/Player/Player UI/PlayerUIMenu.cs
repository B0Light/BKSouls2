using UnityEngine;
using System.Collections;

namespace BK
{
    public class PlayerUIMenu : MonoBehaviour
    {
        [Header("Menu")]
        [SerializeField] GameObject menu;

        public virtual void OpenMenu()
        {
            GUIController.Instance.menuWindowIsOpen = true;
            menu.SetActive(true);
            GUIController.ShowCursor();
        }

        //  THIS IS FINE, BUT IF YOU'RE USING THE "A" BUTTON TO CLOSE MENUS YOU WILL JUMP AS YOU CLOSE THE MENU
        public virtual void CloseMenu()
        {
            GUIController.Instance.menuWindowIsOpen = false;
            menu.SetActive(false);
            GUIController.HideCursor();
        }

        public virtual void CloseMenuAfterFixedFrame()
        {
            if (!menu.activeInHierarchy)
                return;

            StartCoroutine(WaitThenCloseMenu());
        }

        protected virtual IEnumerator WaitThenCloseMenu()
        {
            yield return new WaitForFixedUpdate();

            GUIController.Instance.menuWindowIsOpen = false;
            menu.SetActive(false);
        }
    }
}
