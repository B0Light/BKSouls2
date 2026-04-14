using BK;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridBuildHUDManager : MonoBehaviour
{
    public static GridBuildHUDManager Instance { get; private set; }

    [Header("BuildCam")]
    [SerializeField] private GridBuildCamera gridBuildCamera;
    [Header("BuildSystem")]
    [SerializeField] private GridBuildController gridBuildController;
    
    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup constructionCanvasGroup;
    [SerializeField] private CanvasGroup buildingPopupCanvasGroup;
    
    [Space(10)]
    [SerializeField] private HUD_BuildInfo buildInfoHUD;
    
    private Interactable _interactableObject;
    private PlacedObject _currentSelectTile = null;

    private readonly Vector3 _camOffset = new Vector3(0, 20, -10);
    
    protected virtual void Awake()
    {
        Instance = this;
    }
    
    protected virtual void Start()
    {
        ToggleMainBuildHUD(false);
        ToggleBuildPopUpHUD(false);
    }

    // Interactable Build Controller 와 상호작용해서 HUD를 열때 사용 
    public void ToggleMainBuildHUD(bool isActive, Interactable interactable = null)
    {
        if (isActive && interactable != null)
        {
            _interactableObject = interactable;
        }
        else if (!isActive && _interactableObject != null)
        {
            _interactableObject.ResetInteraction();
            _interactableObject = null;
        }
        
        ToggleConstructionHUD(isActive);
        if (isActive)
        {
            TurnOnGridBuildCamera();
        }
        else
        {
            TurnOffGridBuildCamera();
        }
    }
    
    private void ToggleConstructionHUD(bool isActive)
    {
        constructionCanvasGroup.alpha = isActive ? 1f : 0f;
        constructionCanvasGroup.blocksRaycasts = isActive;
        constructionCanvasGroup.interactable = isActive;
    }
    
    private void ToggleBuildPopUpHUD(bool isActive)
    {
        buildingPopupCanvasGroup.alpha = isActive ? 1f : 0f;
        buildingPopupCanvasGroup.blocksRaycasts = isActive;
        buildingPopupCanvasGroup.interactable = isActive;
    }

    public void OpenBuildPopUpHUD(RevenueFacilityTile revenueFacilityTile)
    {
        _currentSelectTile = revenueFacilityTile;
        _currentSelectTile.SelectObject(true);
        
        buildInfoHUD.Init(revenueFacilityTile);
        
        ToggleConstructionHUD(false);
        ToggleBuildPopUpHUD(true);
    }
    
    public void CloseBuildInfoHUD()
    {
        if (!_currentSelectTile) return;
        _currentSelectTile.SelectObject(false);
        _currentSelectTile = null;
        
        ToggleConstructionHUD(true);
        ToggleBuildPopUpHUD(false);
    }
    
    private void TurnOnGridBuildCamera()
    {
        gridBuildCamera.transform.position = _interactableObject.transform.position + _camOffset;
        gridBuildCamera.gameObject.SetActive(true);
        gridBuildController.SetControllerActive(true);
        //PlayerCameraController.Instance.TurnOffCamera();
        //PlayerCameraController.Instance.SetBuildMode();
    }
    
    private void TurnOffGridBuildCamera()
    {
        gridBuildCamera.gameObject.SetActive(false);
        gridBuildController.SetControllerActive(false);
        //PlayerCameraController.Instance.TurnOnCamera();
        //PlayerCameraController.Instance.SetExplorationMode();
    }

    public virtual void ExitBuildHUD()
    {
        BaseGridBuildSystem.Instance.SelectToBuild(null);
        GUIController.Instance.ToggleMainGUI(true);
        //InputHandlerManager.Instance.SetInputMode(StandaloneInputModule.InputMode.Exploration);
        //PlayerCameraController.Instance.TurnOnCamera();

        GUIController.HideCursor();
        PlayerInputManager.Instance.EnablePlayerActions();
        ToggleMainBuildHUD(false);
    }
}
