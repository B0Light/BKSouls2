using Unity.Netcode;
using UnityEngine;

public class NetworkRewardChest : NetworkBehaviour
{
    private readonly NetworkVariable<bool> opened = new(false);

    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openedVisual;

    public override void OnNetworkSpawn()
    {
        ApplyVisual(opened.Value);
        opened.OnValueChanged += OnOpenedChanged;
    }

    public override void OnNetworkDespawn()
    {
        opened.OnValueChanged -= OnOpenedChanged;
    }

    private void OnOpenedChanged(bool prev, bool next)
    {
        ApplyVisual(next);
    }

    private void ApplyVisual(bool isOpened)
    {
        if (closedVisual != null) closedVisual.SetActive(!isOpened);
        if (openedVisual != null) openedVisual.SetActive(isOpened);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void OpenChestRpc()
    {
        if (opened.Value) return;

        opened.Value = true;

        // TODO:
        // 서버에서 플레이어 보상 지급
        // 예: 골드 지급, 아이템 드랍 생성, 선택 보상 UI 송신
    }
}