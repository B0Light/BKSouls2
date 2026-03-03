using Unity.Netcode;
using UnityEngine;

namespace BK.Inventory
{
    public class PlayerItemDropper : NetworkBehaviour
    {
        [SerializeField] private float dropForwardOffset = 1.0f;
        [SerializeField] private float dropUpOffset = 0.25f;

        // 클라이언트(오너)가 서버에 "드랍 요청"
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void RequestDropItemServerRpc(int itemId)
        {
            if (itemId <= 0) return;
            
            Vector3 basePos = transform.position;
            Vector3 forward = transform.forward;

            Vector3 spawnPos = basePos + forward * dropForwardOffset + Vector3.up * dropUpOffset;

            // 서버에서만 스폰
            GameObject go = Instantiate(WorldItemDatabase.Instance.pickUpItemPrefab, spawnPos, Quaternion.identity);

            var netObj = go.GetComponent<NetworkObject>();
            var pickup = go.GetComponent<PickUpItemInteractable>();

            if (netObj == null || pickup == null)
            {
                Debug.LogError("[Dropper] Prefab missing NetworkObject or PickUpItemInteractable.");
                Destroy(go);
                return;
            }

            netObj.Spawn(true);

            // 네 NetworkVariable 세팅(서버에서만)
            pickup.itemID.Value = itemId;
            pickup.networkPosition.Value = spawnPos;
            pickup.droppingCreatureID.Value = NetworkObjectId;
        }
    }
}