using System;
using UnityEngine;
using UnityEngine.InputSystem;   // ✅ New Input System

public class Mouse3D : Singleton<Mouse3D>
{
    [SerializeField] private LayerMask layerMask;
    private readonly Vector3 _defaultPosition = new Vector3(-100, -100, -100);
    
    PlayerControls playerControls;
    [Header("Input")]
    [SerializeField] private Vector2 mouseInput;
    
    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.UI.MousePosition.performed += i => mouseInput = i.ReadValue<Vector2>();
        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
    
    public static Vector3 GetMouseWorldPosition()
    {
        if (Instance == null)
        {
            Debug.LogError("Mouse3D Instance is null. Ensure Mouse3D is attached to an active GameObject.");
            return Vector3.zero;
        }
        return Instance.GetMouseWorldPosition_Instance();
    }

    private Vector3 GetMouseWorldPosition_Instance()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return _defaultPosition;

        Vector2 mouseScreenPos = mouseInput;
        Ray ray = cam.ScreenPointToRay(mouseScreenPos);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
            return raycastHit.point;

        return _defaultPosition;
    }
}