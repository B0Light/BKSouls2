using Unity.Netcode;
using UnityEngine;

public class NetworkRoomDoor : NetworkBehaviour
{
    [SerializeField] private Collider doorCollider;
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openedVisual;

    private readonly NetworkVariable<bool> isOpen = new(false);

    public override void OnNetworkSpawn()
    {
        ApplyVisual(isOpen.Value);
        isOpen.OnValueChanged += OnDoorStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        isOpen.OnValueChanged -= OnDoorStateChanged;
    }

    private void OnDoorStateChanged(bool previousValue, bool newValue)
    {
        ApplyVisual(newValue);
    }

    private void ApplyVisual(bool open)
    {
        if (doorCollider != null)
            doorCollider.enabled = !open;

        if (closedVisual != null)
            closedVisual.SetActive(!open);

        if (openedVisual != null)
            openedVisual.SetActive(open);
    }

    public void SetDoorStateServer(bool open)
    {
        if (!IsServer) return;
        isOpen.Value = open;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestNextRoomRpc()
    {
        if (RoomManager.Instance == null) return;
        RoomManager.Instance.TryMoveNextRoomServer();
    }
}