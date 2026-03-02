
namespace BK
{
    public class InteractableBuildController : Interactable
    {
        public override void Interact(PlayerManager player)
        {
            base.Interact(player);
            EnterController();
        }

        private void EnterController()
        {
            GUIController.Instance.ToggleMainGUI(false);
            //InputHandlerManager.Instance.SetInputMode(InputMode.OpenUI);
            GridBuildHUDManager.Instance.ToggleMainBuildHUD(true, this);
        }
    }
}