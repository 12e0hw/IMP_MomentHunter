using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeightAdjuster : MonoBehaviour
{
    public XROrigin xrOrigin;
    public InputActionReference heightMove;

    [Header("이동 설정")]
    public float moveSpeed = 1.0f;
    public float minY = 0.3f;
    public float maxY = 1.3f;
    
    void OnEnable() => heightMove.action.Enable();   // 확실히 켜 줌
    void OnDisable() => heightMove.action.Disable();

    void Update()
    {
        if (xrOrigin == null) return;

        Vector2 stick = heightMove.action.ReadValue<Vector2>();
        float vertical = stick.y;

        if (Mathf.Abs(vertical) < 0.1f) return;   // 데드존

        // ── Camera Offset 쪽만 올려서 HMD 보정과 충돌하지 않도록 ──
        var offsetTf = xrOrigin.CameraFloorOffsetObject.transform;
        Vector3 p = offsetTf.localPosition;
        p.y = Mathf.Clamp(p.y + vertical * moveSpeed * Time.deltaTime, minY, maxY);
        offsetTf.localPosition = p;
    }
 }
