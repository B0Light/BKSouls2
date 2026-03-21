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
        [SerializeField] private RoguelikeDoorRole doorRole = RoguelikeDoorRole.Exit;
        [SerializeField] private bool requireRoomCleared = true;
        [SerializeField] private bool moveNextRoomOnInteract = true;

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
    }
}