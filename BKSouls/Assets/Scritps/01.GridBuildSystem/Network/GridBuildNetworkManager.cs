using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace BK
{
    /// <summary>
    /// 빌드 시스템 네트워크 동기화 매니저.
    ///
    /// 책임:
    ///   1. [상태 보관]    서버에서 배치/제거/업그레이드된 건물 목록을 자체적으로 유지합니다.
    ///   2. [접속 동기화]  클라이언트가 씬 초기화를 마친 뒤 서버에 동기화를 요청합니다 (클라이언트 풀 방식).
    ///   3. [실시간 전파]  서버(호스트)에서 건물을 배치/제거/업그레이드하면 접속 중인 모든 클라이언트에 전파합니다.
    ///
    /// 사용 방법:
    ///   - 씬에 NetworkObject 컴포넌트와 함께 배치합니다.
    ///   - ShelterGridBuildSystem 이 자동으로 이 매니저에 알림을 보냅니다.
    /// </summary>
    public class GridBuildNetworkManager : NetworkBehaviour
    {
        public static GridBuildNetworkManager Instance { get; private set; }

        //  서버가 직접 보관하는 건물 목록 (SaveBuildingDataList 와 독립적으로 유지)
        private readonly List<NetworkSaveBuildingData> _buildingList = new List<NetworkSaveBuildingData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            //  클라이언트(호스트 제외)는 씬이 완전히 초기화된 뒤 서버에 동기화를 요청합니다.
            //  Start() 가 완료되는 것을 보장하기 위해 한 프레임 대기 후 ServerRpc 를 호출합니다.
            if (IsClient && !IsServer)
                StartCoroutine(RequestSyncAfterFrame());
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
                Instance = null;
        }

        private IEnumerator RequestSyncAfterFrame()
        {
            //  한 프레임 대기 → ShelterGridBuildSystem.Start() 가 완료됨을 보장
            yield return null;
            RequestBuildingSyncRpc();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  서버 측 알림 메서드  (ShelterGridBuildSystem → GridBuildNetworkManager)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>건물이 배치됐을 때 서버에서 호출. 목록에 추가하고 클라이언트에 전파합니다.</summary>
        public void NotifyBuildingPlaced(SaveBuildingData data)
        {
            if (!IsServer) return;

            var netData = ToNetData(data);
            _buildingList.Add(netData);
            PlaceBuildingClientRpc(netData);
        }

        /// <summary>건물이 제거됐을 때 서버에서 호출. 목록에서 삭제하고 클라이언트에 전파합니다.</summary>
        public void NotifyBuildingRemoved(int x, int y)
        {
            if (!IsServer) return;

            _buildingList.RemoveAll(b => b.X == x && b.Y == y);
            RemoveBuildingClientRpc(x, y);
        }

        /// <summary>건물이 업그레이드됐을 때 서버에서 호출. 목록의 레벨을 갱신하고 클라이언트에 전파합니다.</summary>
        public void NotifyBuildingUpgraded(int x, int y)
        {
            if (!IsServer) return;

            for (int i = 0; i < _buildingList.Count; i++)
            {
                if (_buildingList[i].X == x && _buildingList[i].Y == y)
                {
                    var entry = _buildingList[i];
                    _buildingList[i] = new NetworkSaveBuildingData(entry.X, entry.Y, entry.Code, entry.Dir, entry.Level + 1);
                    break;
                }
            }
            UpgradeBuildingClientRpc(x, y);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  클라이언트 → 서버 동기화 요청
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 클라이언트가 씬 초기화 완료 후 서버에 현재 건물 목록 전송을 요청합니다.
        /// </summary>
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestBuildingSyncRpc(RpcParams rpcParams = default)
        {
            if (_buildingList.Count == 0)
                return;

            ulong requestingClientId = rpcParams.Receive.SenderClientId;

            SyncAllBuildingsClientRpc(_buildingList.ToArray(), new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { requestingClientId }
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ClientRpc
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>접속 시 전체 건물 목록을 수신해 일괄 적용합니다.</summary>
        [ClientRpc]
        private void SyncAllBuildingsClientRpc(NetworkSaveBuildingData[] buildings, ClientRpcParams _ = default)
        {
            if (IsServer) return; // 호스트는 이미 건물이 있으므로 스킵

            var buildSystem = BaseGridBuildSystem.Instance as ShelterGridBuildSystem;
            if (buildSystem == null) return;

            foreach (var data in buildings)
                buildSystem.ApplyNetworkBuilding(data.X, data.Y, data.Code, (Dir)data.Dir, data.Level);
        }

        /// <summary>실시간 건물 배치를 클라이언트에 적용합니다.</summary>
        [ClientRpc]
        private void PlaceBuildingClientRpc(NetworkSaveBuildingData data)
        {
            if (IsServer) return;

            var buildSystem = BaseGridBuildSystem.Instance as ShelterGridBuildSystem;
            buildSystem?.ApplyNetworkBuilding(data.X, data.Y, data.Code, (Dir)data.Dir, data.Level);
        }

        /// <summary>실시간 건물 제거를 클라이언트에 적용합니다.</summary>
        [ClientRpc]
        private void RemoveBuildingClientRpc(int x, int y)
        {
            if (IsServer) return;

            var buildSystem = BaseGridBuildSystem.Instance as ShelterGridBuildSystem;
            buildSystem?.ApplyNetworkRemove(x, y);
        }

        /// <summary>실시간 건물 업그레이드를 클라이언트에 적용합니다.</summary>
        [ClientRpc]
        private void UpgradeBuildingClientRpc(int x, int y)
        {
            if (IsServer) return;

            var buildSystem = BaseGridBuildSystem.Instance as ShelterGridBuildSystem;
            buildSystem?.ApplyNetworkUpgrade(x, y);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  헬퍼
        // ─────────────────────────────────────────────────────────────────────

        private static NetworkSaveBuildingData ToNetData(SaveBuildingData d) =>
            new NetworkSaveBuildingData(d.x, d.y, d.code, d.dir, d.level);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  네트워크 직렬화 구조체
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// RPC 전송을 위한 SaveBuildingData 직렬화 구조체.
    /// Unity Netcode INetworkSerializable 을 구현합니다.
    /// </summary>
    public struct NetworkSaveBuildingData : INetworkSerializable
    {
        public int X;
        public int Y;
        public int Code;
        public int Dir;
        public int Level;

        public NetworkSaveBuildingData(int x, int y, int code, int dir, int level)
        {
            X = x; Y = y; Code = code; Dir = dir; Level = level;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref X);
            serializer.SerializeValue(ref Y);
            serializer.SerializeValue(ref Code);
            serializer.SerializeValue(ref Dir);
            serializer.SerializeValue(ref Level);
        }
    }
}
