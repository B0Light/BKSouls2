using System;
using UnityEngine;

public class Mouse3D : MonoBehaviour
{
    private static Mouse3D Instance { get; set; }
    
    [SerializeField] private LayerMask layerMask;
    private readonly Vector3 _defaultPosition = new Vector3(-100, -100, -100);

    private void Awake()
    {
        Instance = this;
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
        Ray ray = Camera.main?.ScreenPointToRay(Input.mousePosition) ?? default;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            return raycastHit.point;
        }
        else
        {
            return _defaultPosition;
        }
    }

    public static bool GetRaycastHit(Vector3 screenPosition, out RaycastHit hit)
    {
        if (Instance == null)
        {
            Debug.LogError("Mouse3D Instance is null. Ensure Mouse3D is attached to an active GameObject.");
            hit = default;
            return false;
        }
        return Instance.GetRaycastHit_Instance(screenPosition, out hit);
    }

    private bool GetRaycastHit_Instance(Vector3 screenPosition, out RaycastHit hit)
    {
        Ray ray = Camera.main?.ScreenPointToRay(screenPosition) ?? default;
        return Physics.Raycast(ray, out hit, float.MaxValue, layerMask);
    }
}