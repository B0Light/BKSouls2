using Unity.Netcode;
using UnityEngine;

namespace BK
{
    public enum RoguelikeDoorRole
    {
        Entry,
        Exit
    }
    
    public class RoguelikeRoomDoor : InteractableDoor
    {
        [Header("Roguelike Door")]
        [SerializeField] private bool requireRoomCleared = true;
        [SerializeField] private bool moveNextRoomOnInteract = true;

        [Header("Portal VFX")]
        [Tooltip("문이 열릴 때 활성화할 포탈 VFX 오브젝트 (자식 오브젝트로 배치)")]
        [SerializeField] private GameObject portalVFX;
        [Tooltip("Entry 문에도 포탈 VFX를 표시할지 여부 (false면 Exit 전용)")]
        [SerializeField] private bool showPortalOnEntry = false;

        private readonly NetworkVariable<int> netDoorRole = new NetworkVariable<int>(
            (int)RoguelikeDoorRole.Exit,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public RoguelikeDoorRole DoorRole => (RoguelikeDoorRole)netDoorRole.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            netDoorRole.OnValueChanged += OnDoorRoleChanged;

            // 스폰 시 현재 열림 상태에 맞춰 VFX 초기화
            if (portalVFX != null)
                portalVFX.SetActive(false);
        }

        public override void OnNetworkDespawn()
        {
            netDoorRole.OnValueChanged -= OnDoorRoleChanged;
            base.OnNetworkDespawn();
        }

        private void OnDoorRoleChanged(int previousValue, int newValue)
        {
            // 역할 변경 시 필요하면 UI 갱신
        }

        protected override void OnDoorOpenStateChanged(bool isNowOpen)
        {
            if (portalVFX == null)
                return;

            // showPortalOnEntry == false 이면 Exit 문에만 표시
            bool shouldShow = isNowOpen &&
                              (showPortalOnEntry || DoorRole == RoguelikeDoorRole.Exit);

            portalVFX.SetActive(shouldShow);
        }

        public override void Interact(PlayerManager player)
        {
            // 출구 문 + 다음 방 이동 문이면
            if (DoorRole == RoguelikeDoorRole.Exit && moveNextRoomOnInteract)
            {
                if (requireRoomCleared && RoomManager.Instance != null && !RoomManager.Instance.IsCurrentRoomCleared())
                {
                    return;
                }

                if (!DoorIsOpen || DoorIsLocked)
                {
                    base.Interact(player);
                    return;
                }

                if (!IsEligibleInteractor(player))
                    return;

                CleanupInteraction(player);
                RequestMoveNextRoomRpc();
                return;
            }

            // 그 외에는 일반 문처럼 동작
            base.Interact(player);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestMoveNextRoomRpc()
        {
            if (RoomManager.Instance == null)
                return;

            RoomManager.Instance.TryMoveNextRoomServer();
        }

        public void SetDoorRoleServer(RoguelikeDoorRole role)
        {
            if (!IsServer)
                return;

            netDoorRole.Value = (int)role;
        }

        public void SetDoorOpenServer(bool open)
        {
            ForceSetDoorOpenServer(open);
        }

        public void SetDoorLockedServer(bool locked)
        {
            ForceSetDoorLockedServer(locked);
        }

        protected override void ApplyUIText()
        {
            if (DoorRole == RoguelikeDoorRole.Exit && moveNextRoomOnInteract)
            {
                interactableText = "Next Stage";
            }
            else
            {
                interactableText = "Locked";
            }
        }
    }
}