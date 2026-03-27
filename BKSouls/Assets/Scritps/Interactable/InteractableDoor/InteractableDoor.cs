using UnityEngine;
using System.Collections;
using BK.Inventory;
using Unity.Netcode;

public enum DoorType
{
    Horizontal,
    Vertical,
    Rotate,
    Brake,
}

namespace BK
{
    public class InteractableDoor : Interactable
    {
        [Header("Door Settings")]
        [SerializeField] private DoorType doorType = DoorType.Horizontal;
        [SerializeField] private int keyItemCode = 0;

        [SerializeField] private float animationDuration = 1.0f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Movement Settings")]
        [SerializeField] private float slideDistance = 2.0f;
        [SerializeField] private float rotationAngle = 90.0f;

        [Header("Door Components")]
        [SerializeField] private Transform leftDoor;
        [SerializeField] private Transform rightDoor;

        [Header("Options")]
        [SerializeField] private bool openOnly = false; // 열리기만 가능한 문

        [Header("Server Anti-Spam (Optional)")]
        [SerializeField] private float serverToggleCooldown = 0.15f;

        // ---- Net State (서버 권한) ----
        private readonly NetworkVariable<bool> _netIsOpen = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private readonly NetworkVariable<bool> _netIsLocked = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // ---- Unlock handshake (서버 전용 상태) ----
        private ulong _pendingUnlockClientId = ulong.MaxValue;
        private int _pendingUnlockNonce = 0;

        // ---- Server debounce ----
        private double _serverNextToggleTime = 0;

        // ---- Animation (로컬) ----
        private bool _isAnimating;
        private Coroutine _animCo;

        private Vector3 _leftDoorInitialPosition;
        private Vector3 _rightDoorInitialPosition;
        private Quaternion _leftDoorInitialRotation;
        private Quaternion _rightDoorInitialRotation;
        
        public bool DoorIsOpen => _netIsOpen.Value;
        public bool DoorIsLocked => _netIsLocked.Value;

        protected override void Awake()
        {
            base.Awake();

            if (leftDoor != null)
            {
                _leftDoorInitialPosition = leftDoor.localPosition;
                _leftDoorInitialRotation = leftDoor.localRotation;
            }

            if (rightDoor != null)
            {
                _rightDoorInitialPosition = rightDoor.localPosition;
                _rightDoorInitialRotation = rightDoor.localRotation;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                _netIsLocked.Value = (keyItemCode != 0);
                _netIsOpen.Value = false;
                ClearPendingUnlock_Server();
                _serverNextToggleTime = 0;
            }

            _netIsOpen.OnValueChanged += OnOpenChanged;
            _netIsLocked.OnValueChanged += OnLockChanged;

            // late joiner 포함 즉시 반영
            ApplyUIText();
            ApplyImmediatePoseFromState();
        }

        public override void OnNetworkDespawn()
        {
            _netIsOpen.OnValueChanged -= OnOpenChanged;
            _netIsLocked.OnValueChanged -= OnLockChanged;
            base.OnNetworkDespawn();
        }

        protected override void Start()
        {
            ApplyUIText();
        }
        
        public override void OnTriggerEnter(Collider other)
        {
            if(openOnly && _netIsOpen.Value)
            {
                return;
            }
            else
            {
                base.OnTriggerEnter(other);
            }
        }

        public override void Interact(PlayerManager player)
        {
            // 로컬 애니중이면 입력만 막기
            if (_isAnimating) return;

            // Interactable이 collider disable을 한다면 멀티에서 꼬일 수 있음.
            // 해당 로직 제거했다면 호출 OK.
            base.Interact(player);

            // 클라 -> 서버 요청 (Universal RPC)
            TryToggleDoorRpc();
        }

        // =========================================================
        // Universal RPCs (최신 방식)
        // - 메서드 이름은 ...Rpc 로 끝나야 ILPP가 인식
        // =========================================================

        // Client -> Server : 문 토글 요청
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void TryToggleDoorRpc(RpcParams rpcParams = default)
        {
            if (!IsServer) return;

            // 서버 스팸 방지(선택)
            double now = GetServerTimeOrRealtime();
            if (now < _serverNextToggleTime) return;
            _serverNextToggleTime = now + Mathf.Max(0f, serverToggleCooldown);

            ulong senderClientId = rpcParams.Receive.SenderClientId;

            // 1) 잠겨있으면 -> 해당 클라에게만 "키 소비 요청"
            if (_netIsLocked.Value)
            {
                // 다른 클라가 이미 언락 시도 중이면 무시(경쟁 방지)
                if (_pendingUnlockClientId != ulong.MaxValue && _pendingUnlockClientId != senderClientId)
                    return;

                _pendingUnlockClientId = senderClientId;
                _pendingUnlockNonce++;

                // Server -> 특정 Client 1명에게만 전송
                RequestConsumeKeyRpc(
                    keyItemCode,
                    _pendingUnlockNonce,
                    RpcTarget.Single(senderClientId, RpcTargetUse.Temp)
                );

                return;
            }

            // 2) 열리기만 옵션
            if (openOnly && _netIsOpen.Value) return;

            // 3) 토글
            _netIsOpen.Value = !_netIsOpen.Value;
        }

        // Server -> 특정 Client : 키 소비 요청
        [Rpc(SendTo.SpecifiedInParams)]
        private void RequestConsumeKeyRpc(int requiredKeyItemCode, int nonce, RpcParams rpcParams = default)
        {
            // 이 RPC는 서버가 Target을 지정해서 특정 클라에게만 보냄
            bool consumed =
                WorldPlayerInventory.Instance != null &&
                WorldPlayerInventory.Instance.RemoveItemInInventory(requiredKeyItemCode);

            if (!consumed)
            {
                NotifyUnlockFailedRpc(nonce);
                return;
            }

            ConfirmUnlockRpc(nonce);
        }

        // Client -> Server : 키 소비 성공 확인
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ConfirmUnlockRpc(int nonce, RpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong senderClientId = rpcParams.Receive.SenderClientId;

            if (_pendingUnlockClientId != senderClientId) return;
            if (_pendingUnlockNonce != nonce) return;

            if (!_netIsLocked.Value)
            {
                ClearPendingUnlock_Server();
                return;
            }

            _netIsLocked.Value = false;

            // 키로 언락하면 "열기"가 자연스러움
            if (!(openOnly && _netIsOpen.Value))
                _netIsOpen.Value = true;

            ClearPendingUnlock_Server();
        }

        // Client -> Server : 키 소비 실패 통보
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void NotifyUnlockFailedRpc(int nonce, RpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong senderClientId = rpcParams.Receive.SenderClientId;

            if (_pendingUnlockClientId != senderClientId) return;
            if (_pendingUnlockNonce != nonce) return;

            ClearPendingUnlock_Server();
        }

        private void ClearPendingUnlock_Server()
        {
            if (!IsServer) return;
            _pendingUnlockClientId = ulong.MaxValue;
            // nonce는 증가형 유지(재사용 방지)
        }

        private double GetServerTimeOrRealtime()
        {
            // 서버에서 NetworkManager가 준비되어 있으면 ServerTime 사용, 아니면 realtime fallback
            if (NetworkManager != null)
                return NetworkManager.ServerTime.Time;

            return Time.realtimeSinceStartupAsDouble;
        }

        // =========================================================
        // Net state callbacks
        // =========================================================

        private void OnOpenChanged(bool previous, bool current)
        {
            if (_animCo != null) StopCoroutine(_animCo);
            _animCo = StartCoroutine(PlayDoorAnimation(current));

            ApplyUIText();

            if (interactableCollider != null)
            {
                // openOnly면 열렸을 때 상호작용 불가
                interactableCollider.enabled = !(openOnly && current);
            }

            OnDoorOpenStateChanged(current);
        }

        // 하위 클래스에서 문 열림/닫힘 시 추가 동작을 구현할 때 오버라이드
        protected virtual void OnDoorOpenStateChanged(bool isNowOpen) { }

        private void OnLockChanged(bool previous, bool current)
        {
            ApplyUIText();
        }

        // =========================================================
        // UI
        // =========================================================

        protected virtual void ApplyUIText()
        {
            if (_netIsLocked.Value)
            {
                string itemName = "Key";
                if (WorldItemDatabase.Instance != null)
                {
                    var item = WorldItemDatabase.Instance.GetItemByID(keyItemCode);
                    if (item != null) itemName = item.itemName;
                }

                interactableText = "[Locked] Need Item : " + itemName;
                return;
            }

            if (openOnly && _netIsOpen.Value)
                interactableText = "Opened";
            else
                interactableText = _netIsOpen.Value ? "Close" : "Open";
        }

        // =========================================================
        // Late join pose apply
        // =========================================================

        private void ApplyImmediatePoseFromState()
        {
            bool open = _netIsOpen.Value;

            switch (doorType)
            {
                case DoorType.Horizontal:
                    if (leftDoor != null)
                        leftDoor.localPosition = open
                            ? _leftDoorInitialPosition + Vector3.left * slideDistance
                            : _leftDoorInitialPosition;

                    if (rightDoor != null)
                        rightDoor.localPosition = open
                            ? _rightDoorInitialPosition + Vector3.right * slideDistance
                            : _rightDoorInitialPosition;
                    break;

                case DoorType.Vertical:
                    if (leftDoor != null)
                        leftDoor.localPosition = open
                            ? _leftDoorInitialPosition + Vector3.up * slideDistance
                            : _leftDoorInitialPosition;

                    if (rightDoor != null)
                        rightDoor.localPosition = open
                            ? _rightDoorInitialPosition + Vector3.down * slideDistance
                            : _rightDoorInitialPosition;
                    break;

                case DoorType.Rotate:
                    if (leftDoor != null)
                        leftDoor.localRotation = open
                            ? _leftDoorInitialRotation * Quaternion.Euler(0, -rotationAngle, 0)
                            : _leftDoorInitialRotation;

                    if (rightDoor != null)
                        rightDoor.localRotation = open
                            ? _rightDoorInitialRotation * Quaternion.Euler(0, rotationAngle, 0)
                            : _rightDoorInitialRotation;
                    break;

                case DoorType.Brake:
                    if (leftDoor != null) leftDoor.gameObject.SetActive(!open);
                    if (rightDoor != null) rightDoor.gameObject.SetActive(!open);
                    break;
            }
        }

        // =========================================================
        // Animation
        // =========================================================

        private IEnumerator PlayDoorAnimation(bool open)
        {
            _isAnimating = true;

            switch (doorType)
            {
                case DoorType.Horizontal:
                    yield return AnimateHorizontal(open);
                    break;
                case DoorType.Vertical:
                    yield return AnimateVertical(open);
                    break;
                case DoorType.Rotate:
                    yield return AnimateRotate(open);
                    break;
                case DoorType.Brake:
                    yield return AnimateExplosion(open);
                    break;
            }

            _isAnimating = false;
            ResetInteraction();
        }

        private IEnumerator AnimateHorizontal(bool open)
        {
            Vector3 leftStartPos = leftDoor != null ? leftDoor.localPosition : Vector3.zero;
            Vector3 rightStartPos = rightDoor != null ? rightDoor.localPosition : Vector3.zero;

            Vector3 leftTargetPos = leftDoor != null
                ? (open ? _leftDoorInitialPosition + Vector3.left * slideDistance : _leftDoorInitialPosition)
                : Vector3.zero;

            Vector3 rightTargetPos = rightDoor != null
                ? (open ? _rightDoorInitialPosition + Vector3.right * slideDistance : _rightDoorInitialPosition)
                : Vector3.zero;

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / animationDuration);

                if (leftDoor != null) leftDoor.localPosition = Vector3.Lerp(leftStartPos, leftTargetPos, t);
                if (rightDoor != null) rightDoor.localPosition = Vector3.Lerp(rightStartPos, rightTargetPos, t);

                yield return null;
            }

            if (leftDoor != null) leftDoor.localPosition = leftTargetPos;
            if (rightDoor != null) rightDoor.localPosition = rightTargetPos;
        }

        private IEnumerator AnimateVertical(bool open)
        {
            Vector3 leftStartPos = leftDoor != null ? leftDoor.localPosition : Vector3.zero;
            Vector3 rightStartPos = rightDoor != null ? rightDoor.localPosition : Vector3.zero;

            Vector3 leftTargetPos = leftDoor != null
                ? (open ? _leftDoorInitialPosition + Vector3.up * slideDistance : _leftDoorInitialPosition)
                : Vector3.zero;

            Vector3 rightTargetPos = rightDoor != null
                ? (open ? _rightDoorInitialPosition + Vector3.down * slideDistance : _rightDoorInitialPosition)
                : Vector3.zero;

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / animationDuration);

                if (leftDoor != null) leftDoor.localPosition = Vector3.Lerp(leftStartPos, leftTargetPos, t);
                if (rightDoor != null) rightDoor.localPosition = Vector3.Lerp(rightStartPos, rightTargetPos, t);

                yield return null;
            }

            if (leftDoor != null) leftDoor.localPosition = leftTargetPos;
            if (rightDoor != null) rightDoor.localPosition = rightTargetPos;
        }

        private IEnumerator AnimateRotate(bool open)
        {
            Quaternion leftStartRot = leftDoor != null ? leftDoor.localRotation : Quaternion.identity;
            Quaternion rightStartRot = rightDoor != null ? rightDoor.localRotation : Quaternion.identity;

            Quaternion leftTargetRot = leftDoor != null
                ? (open ? _leftDoorInitialRotation * Quaternion.Euler(0, -rotationAngle, 0) : _leftDoorInitialRotation)
                : Quaternion.identity;

            Quaternion rightTargetRot = rightDoor != null
                ? (open ? _rightDoorInitialRotation * Quaternion.Euler(0, rotationAngle, 0) : _rightDoorInitialRotation)
                : Quaternion.identity;

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / animationDuration);

                if (leftDoor != null) leftDoor.localRotation = Quaternion.Lerp(leftStartRot, leftTargetRot, t);
                if (rightDoor != null) rightDoor.localRotation = Quaternion.Lerp(rightStartRot, rightTargetRot, t);

                yield return null;
            }

            if (leftDoor != null) leftDoor.localRotation = leftTargetRot;
            if (rightDoor != null) rightDoor.localRotation = rightTargetRot;
        }

        private IEnumerator AnimateExplosion(bool open)
        {
            // open==true면 파괴
            if (!open) yield break;

            yield return new WaitForSeconds(1f);

            if (leftDoor != null) leftDoor.gameObject.SetActive(false);
            if (rightDoor != null) rightDoor.gameObject.SetActive(false);
        }
        
        protected void ForceSetDoorOpenServer(bool open)
        {
            if (!IsServer) return;

            if (openOnly && _netIsOpen.Value && !open)
                return;

            _netIsOpen.Value = open;
        }

        protected void ForceSetDoorLockedServer(bool locked)
        {
            if (!IsServer) return;

            _netIsLocked.Value = locked;
        }
    }
}