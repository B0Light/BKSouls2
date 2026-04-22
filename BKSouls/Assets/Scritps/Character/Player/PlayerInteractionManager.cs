using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerInteractionManager : MonoBehaviour
    {
        PlayerManager player;

        private List<Interactable> currentInteractableActions; //   DO NOT SERIALIZE IF USING UNITY V 2022.3.11f1 IT CAUSES A BUG IN THE INSPECTOR

        private void Awake()
        {
            player = GetComponent<PlayerManager>();
        }

        private void Start()
        {
            currentInteractableActions = new List<Interactable>();
        }

        private void FixedUpdate()
        {
            if (!player.IsOwner)
                return;

            //  IF OUR UI MENU IS NOT OPEN, AND WE DONT HAVE A POP UP (CURRENT INTERACTION MESSAGE) CHECK FOR INTERACTABLE
            if (!GUIController.Instance.menuWindowIsOpen && !GUIController.Instance.popUpWindowIsOpen)
                CheckForInteractable();
        }

        private void CheckForInteractable()
        {
            if (currentInteractableActions.Count == 0)
                return;

            if (currentInteractableActions[0] == null)
            {
                currentInteractableActions.RemoveAt(0); //  IF THE CURRENT INTERACTABLE ITEM AT POSITION 0 BECOMES NULL (REMOVED FROM GAME), WE REMOVE POSITION 0 FROM THE LIST
                return;
            }

            // IF WE HAVE AN INTERACTABLE ACTION AND HAVE NOT NOTIFIED OUR PLAYER, WE DO SO HERE
            if (currentInteractableActions[0] != null)
                GUIController.Instance.playerUIPopUpManager.SendPlayerMessagePopUp(currentInteractableActions[0].interactableText);
        }

        private void RefreshInteractionList()
        {
            for (int i = currentInteractableActions.Count - 1; i > -1; i--)
            {
                if (currentInteractableActions[i] == null)
                    currentInteractableActions.RemoveAt(i);
            }
        }

        public void AddInteractionToList(Interactable interactableObject)
        {
            RefreshInteractionList();

            if (!currentInteractableActions.Contains(interactableObject))
                currentInteractableActions.Add(interactableObject);
        }

        public void RemoveInteractionFromList(Interactable interactableObject)
        {
            if (currentInteractableActions.Contains(interactableObject))
                currentInteractableActions.Remove(interactableObject);

            RefreshInteractionList();
        }

        public void Interact()
        {
            //  IF WE PRESS THE INTERACT BUTTON WITH OR WITHOUT AN INTERACTABLE, IT WILL CLEAR THE POP UP WINDOWS (ITEM PICK UPS, MESSAGES, ECT)
            GUIController.Instance.playerUIPopUpManager.CloseAllPopUpWindows();

            if (currentInteractableActions.Count == 0)
                return;

            if (currentInteractableActions[0] != null)
            {
                player.playerLocomotionManager.ResetMovementState();
                currentInteractableActions[0].Interact(player);
                RefreshInteractionList();
            }
        }
    }
}
