using System.Collections;
using UnityEngine;
using BK;

public class QuitGame : MonoBehaviour
{
    public void ExitGame()
    {
        WorldSaveGameManager.Instance.SaveGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 중이면 중지
#else
            Application.Quit(); // 빌드된 게임에서는 종료
#endif
    }

    public void BackToTitle()
    {
        WorldSaveGameManager.Instance.SaveGame();
        GUIController.Instance.HandleEscape();
        //WorldSceneChangeManager.Instance.LoadTitle();
    }
}