using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class InteractableBox : Interactable
    {
        [Header("Box Info")]
        [SerializeField] [Range(1, 6)] private int boxWidth;
        [SerializeField] [Range(1, 10)] private int boxHeight;

        [SerializeField] private List<GridItem> initialItems = new List<GridItem>();
        protected List<int> itemIdList = new List<int>();

        [Header("Box Animation Settings")] 
        [SerializeField] private BoxOpenType openType = BoxOpenType.TopLid;

        [SerializeField] private float animationDuration = 1.0f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Movement Settings")] 
        [SerializeField] private float slideDistance = 1.0f; 

        [SerializeField] private float rotationAngle = 90.0f; 
        [SerializeField] private float lidOpenAngle = -120.0f;
        [SerializeField] private LidRotationAxis lidRotationAxis = LidRotationAxis.X;

        [Header("Box Components")] [SerializeField]
        private Transform lid; // 뚜껑 (TopLid용)

        [SerializeField] private Transform leftDoor; 
        [SerializeField] private Transform rightDoor;

        protected bool isOpen;
        private bool _isDoorOpened;
        private bool _isAnimating;

        // 초기 위치/회전 저장
        private Vector3 _lidInitialPosition;
        private Vector3 _leftDoorInitialPosition;
        private Vector3 _rightDoorInitialPosition;
        private Quaternion _lidInitialRotation;
        private Quaternion _leftDoorInitialRotation;
        private Quaternion _rightDoorInitialRotation;

        protected override void Awake()
        {
            base.Awake();

            // 초기 위치/회전 저장
            if (lid != null)
            {
                _lidInitialPosition = lid.localPosition;
                _lidInitialRotation = lid.localRotation;
            }

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

        protected override void Start()
        {
            base.Start();

            foreach (var item in initialItems)
            {
                if (item != null) itemIdList.Add(item.itemID);
            }

            isOpen = false;
            _isDoorOpened = false;
            _isAnimating = false;
        }

        public override void Interact(PlayerManager player)
        {
            if (_isAnimating) return; // 애니메이션 중이면 상호작용 차단

            base.Interact(player);
            if (isOpen == false)
            {
                OpenBox();
                GUIController.Instance.WaitToInteraction(animationDuration, PerformInteraction);
                isOpen = true;
            }
            else
            {
                PerformInteraction();
            }
        }

        protected virtual void PerformInteraction()
        {
            GUIController.Instance.OpenInteractableBox(boxWidth, boxHeight, itemIdList, this);
        }

        protected void OpenBox()
        {
            if (_isDoorOpened || _isAnimating) return; // 이미 열려있거나 애니메이션 중이면 열지 않음

            _isDoorOpened = true;

            switch (openType)
            {
                case BoxOpenType.TopLid:
                    StartCoroutine(AnimateTopLid(true));
                    break;
                case BoxOpenType.SideSliding:
                    StartCoroutine(AnimateSideSliding(true));
                    break;
                case BoxOpenType.SideRotating:
                    StartCoroutine(AnimateSideRotating(true));
                    break;
            }
        }

        private void CloseBox()
        {
            if (!_isDoorOpened || _isAnimating) return; // 이미 닫혀있거나 애니메이션 중이면 닫지 않음

            _isDoorOpened = false;

            switch (openType)
            {
                case BoxOpenType.TopLid:
                    StartCoroutine(AnimateTopLid(false));
                    break;
                case BoxOpenType.SideSliding:
                    StartCoroutine(AnimateSideSliding(false));
                    break;
                case BoxOpenType.SideRotating:
                    StartCoroutine(AnimateSideRotating(false));
                    break;
            }
        }

        // 위에 뚜껑이 열리는 애니메이션
        private IEnumerator AnimateTopLid(bool opening)
        {
            if (lid == null) yield break;

            _isAnimating = true;

            Quaternion startRotation = lid.localRotation;
            Quaternion targetRotation;

            // 회전 축에 따라 다른 회전 적용
            Vector3 rotationVector = Vector3.zero;
            switch (lidRotationAxis)
            {
                case LidRotationAxis.X:
                    rotationVector = new Vector3(lidOpenAngle, 0, 0);
                    break;
                case LidRotationAxis.Y:
                    rotationVector = new Vector3(0, lidOpenAngle, 0);
                    break;
                case LidRotationAxis.Z:
                    rotationVector = new Vector3(0, 0, lidOpenAngle);
                    break;
            }

            targetRotation = opening ? _lidInitialRotation * Quaternion.Euler(rotationVector) : _lidInitialRotation;

            float elapsedTime = 0;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsedTime / animationDuration);

                lid.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

                yield return null;
            }

            lid.localRotation = targetRotation;
            _isAnimating = false;
        }

        // 컨테이너 박스처럼 양 옆으로 슬라이딩
        private IEnumerator AnimateSideSliding(bool opening)
        {
            _isAnimating = true;

            Vector3 lidStartPos = Vector3.zero;
            Vector3 leftStartPos = Vector3.zero;
            Vector3 rightStartPos = Vector3.zero;
            Vector3 lidTargetPos = Vector3.zero;
            Vector3 leftTargetPos = Vector3.zero;
            Vector3 rightTargetPos = Vector3.zero;

            if (lid != null)
            {
                lidStartPos = lid.localPosition;
                lidTargetPos = opening ? _lidInitialPosition + Vector3.left * slideDistance : _lidInitialPosition;
            }

            // 왼쪽 도어 설정
            if (leftDoor != null)
            {
                leftStartPos = leftDoor.localPosition;
                leftTargetPos = opening
                    ? _leftDoorInitialPosition + Vector3.left * slideDistance
                    : _leftDoorInitialPosition;
            }

            // 오른쪽 도어 설정
            if (rightDoor != null)
            {
                rightStartPos = rightDoor.localPosition;
                rightTargetPos = opening
                    ? _rightDoorInitialPosition + Vector3.right * slideDistance
                    : _rightDoorInitialPosition;
            }

            float elapsedTime = 0;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsedTime / animationDuration);

                if (lid != null)
                    lid.localPosition = Vector3.Lerp(lidStartPos, lidTargetPos, t);

                if (leftDoor != null)
                    leftDoor.localPosition = Vector3.Lerp(leftStartPos, leftTargetPos, t);

                if (rightDoor != null)
                    rightDoor.localPosition = Vector3.Lerp(rightStartPos, rightTargetPos, t);

                yield return null;
            }

            // 최종 위치 설정
            if (lid != null)
                lid.localPosition = lidTargetPos;

            if (leftDoor != null)
                leftDoor.localPosition = leftTargetPos;

            if (rightDoor != null)
                rightDoor.localPosition = rightTargetPos;

            _isAnimating = false;
        }

        // 일반 문처럼 90도 회전
        private IEnumerator AnimateSideRotating(bool opening)
        {
            _isAnimating = true;

            Quaternion leftStartRotation = Quaternion.identity;
            Quaternion rightStartRotation = Quaternion.identity;
            Quaternion leftTargetRotation = Quaternion.identity;
            Quaternion rightTargetRotation = Quaternion.identity;

            // 왼쪽 도어 설정
            if (leftDoor != null)
            {
                leftStartRotation = leftDoor.localRotation;
                leftTargetRotation = opening
                    ? _leftDoorInitialRotation * Quaternion.Euler(0, -rotationAngle, 0)
                    : _leftDoorInitialRotation;
            }

            // 오른쪽 도어 설정
            if (rightDoor != null)
            {
                rightStartRotation = rightDoor.localRotation;
                rightTargetRotation = opening
                    ? _rightDoorInitialRotation * Quaternion.Euler(0, rotationAngle, 0)
                    : _rightDoorInitialRotation;
            }

            float elapsedTime = 0;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsedTime / animationDuration);

                if (leftDoor != null)
                    leftDoor.localRotation = Quaternion.Lerp(leftStartRotation, leftTargetRotation, t);

                if (rightDoor != null)
                    rightDoor.localRotation = Quaternion.Lerp(rightStartRotation, rightTargetRotation, t);

                yield return null;
            }

            // 최종 회전 설정
            if (leftDoor != null)
                leftDoor.localRotation = leftTargetRotation;

            if (rightDoor != null)
                rightDoor.localRotation = rightTargetRotation;

            _isAnimating = false;
        }

        public override void ResetInteraction()
        {
            base.ResetInteraction();
            CloseBox();
            isOpen = false; // 상자를 닫을 때 isOpen을 false로 변경
        }
    }
}