using Unity.Cinemachine;
using UnityEngine;

public class GridBuildCamera : MonoBehaviour
{
    public float moveSpeed = 10f; // 기본 이동 속도
    public float boostMultiplier = 2f; // Shift로 증가하는 이동 속도 배율
    public float zoomSpeed = 10f; // 줌 속도
    public float rotationSpeed = 100f; // 회전 속도
    public float minZoomDistance = 2f; // 최소 줌 거리
    public float maxZoomDistance = 50f; // 최대 줌 거리

    private readonly float _minOrthographicSize = 1f;
    private readonly float _maxOrthographicSize = 50f;

    private Vector3 _targetPosition; // 카메라가 바라볼 중심점
    private float _distance; // 중심점과 카메라의 거리
    private float _orthographicSize;
    
    private Vector3 _defaultPosition;
    private Quaternion _defaultQuaternion;
    
    // 탑다운 모드 상태 관리용 변수
    private bool _isTopDownMode = false;
    private Quaternion _beforeTopDownRotation; // 탑다운 진입 전 회전값 저장
    private CinemachineCamera _vCam;
    
    PlayerControls playerControls;
    private Vector2 movement;
    private bool projectileInput;
    private float scrollValue;
    private bool upInput;
    private bool downInput;
    private bool fastInput;

    private void Awake()
    {
        _vCam = GetComponent<CinemachineCamera>();
    }
    
    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.BuildCamera.Movement.performed += i => movement = i.ReadValue<Vector2>();
            playerControls.BuildCamera.Scroll.performed += i => scrollValue = i.ReadValue<float>();
            playerControls.BuildCamera.ProjectileInput.performed += i => projectileInput = true;
            playerControls.BuildCamera.ProjectileInput.canceled += i => projectileInput = false;
            
            playerControls.BuildCamera.upInput.performed += i => upInput = true;
            playerControls.BuildCamera.upInput.canceled += i => upInput = false;
            
            playerControls.BuildCamera.downInput.performed += i => downInput = true;
            playerControls.BuildCamera.downInput.canceled += i => downInput= false;
            
            playerControls.BuildCamera.fastInput.performed += i => fastInput = true;
            playerControls.BuildCamera.fastInput.canceled += i => fastInput = false;
        }

        playerControls.Enable();
    }

    private void Start()
    {
        _targetPosition = transform.position + transform.forward * 10f;
        _distance = Vector3.Distance(transform.position, _targetPosition);

        _defaultPosition = transform.position;
        _defaultQuaternion = transform.rotation;
        
        SetProjectCam();
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
        HandleFreeMove();
    }

    private void HandleMovement()
    {
        if (projectileInput) // 중간 버튼으로 이동
        {
            float moveX = -movement.x * moveSpeed * Time.deltaTime;
            float moveY = -movement.y * moveSpeed * Time.deltaTime;

            Vector3 move;

            if (_isTopDownMode)
            {
                // 탑다운 모드일 때는 월드 좌표 기준 X(좌우), Z(상하) 평면 이동
                // 카메라가 90도 숙여져 있으므로 transform.up이 월드의 Z축(앞뒤)과 유사함
                move = transform.right * moveX + transform.up * moveY;
            }
            else
            {
                // 기존 방식: 카메라 로컬 좌표 기준 이동
                move = transform.right * moveX + transform.up * moveY;
            }

            transform.position += move;
            _targetPosition += move;
        }
    }

    private void HandleZoom()
    {
        float scroll = scrollValue;
        if (scroll != 0f)
        {
            if (_isTopDownMode)
            {
                _orthographicSize -= scroll * zoomSpeed;
                _orthographicSize = Mathf.Clamp(_orthographicSize, _minOrthographicSize, _maxOrthographicSize);
                _vCam.Lens.OrthographicSize = _orthographicSize;
            }
            else
            {
                _distance -= scroll * zoomSpeed;
                _distance = Mathf.Clamp(_distance, minZoomDistance, maxZoomDistance);

                // 탑다운 모드에서도 줌은 타겟 방향(수직)으로 작동해야 하므로 동일 로직 사용 가능
                Vector3 direction = (transform.position - _targetPosition).normalized;
            
                // 안전장치: 벡터가 0이 되는 경우 방지 (탑다운에서 타겟과 겹칠 때)
                if (direction == Vector3.zero) direction = _isTopDownMode ? Vector3.up : -transform.forward;

                transform.position = _targetPosition + direction * _distance;
            }
            
        }
    }

    private void HandleRotation()
    {
        // 탑다운 모드에서는 회전 불가능 (각도 고정 요청)
        if (_isTopDownMode) return;

        if (Input.GetMouseButton(1)) // 오른쪽 버튼으로 회전
        {
            float rotX = movement.x * rotationSpeed * Time.deltaTime;
            float rotY = -movement.y * rotationSpeed * Time.deltaTime;

            transform.RotateAround(_targetPosition, Vector3.up, rotX);
            transform.RotateAround(_targetPosition, transform.right, rotY);
            
            Vector3 direction = (transform.position - _targetPosition).normalized;
            _distance = Vector3.Distance(transform.position, _targetPosition);
            transform.LookAt(_targetPosition);
        }
    }

    private void HandleFreeMove()
    {
        float currentSpeed = moveSpeed;

        if (fastInput)
        {
            currentSpeed *= boostMultiplier;
        }

        float moveX = movement.x * currentSpeed * Time.deltaTime; // A/D
        float moveZ = movement.y * currentSpeed * Time.deltaTime;   // W/S
        
        Vector3 move;

        if (_isTopDownMode)
        {
            // 탑다운 모드:
            // W/S (moveZ) -> 월드 상의 앞/뒤 (화면상 위/아래) -> transform.up 사용
            // A/D (moveX) -> 월드 상의 좌/우 (화면상 좌/우) -> transform.right 사용
            // 카메라 각도(90도) 유지
            
            move = transform.right * moveX + transform.up * moveZ;
            
        }
        else
        {
            // 기존 방식: 3D 자유 시점 이동
            float moveY = 0f;
            if (upInput) moveY -= currentSpeed * Time.deltaTime;
            if (downInput) moveY += currentSpeed * Time.deltaTime;

            move = transform.forward * moveZ + transform.right * moveX + transform.up * moveY;
        }

        transform.position += move;
        _targetPosition += move;
    }
    
    public void ResetCamPosition()
    {
        _isTopDownMode = false;
        _vCam.Lens.ModeOverride = LensSettings.OverrideModes.None;
        transform.position = _defaultPosition;
        transform.rotation = _defaultQuaternion;
        
        _targetPosition = transform.position + transform.forward * 10f;
        _distance = Vector3.Distance(transform.position, _targetPosition);
    }
    
    public void SetProjectCam()
    {
        _isTopDownMode = true;
        _vCam.Lens.ModeOverride = LensSettings.OverrideModes.None;
        _orthographicSize = 20;
        _vCam.Lens.OrthographicSize = _orthographicSize;
       
        _beforeTopDownRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        transform.position = new Vector3(_targetPosition.x, _targetPosition.y + _distance, _targetPosition.z);
    }
}