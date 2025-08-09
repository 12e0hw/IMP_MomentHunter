using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    
    [Header("Billboard Settings")]
    [SerializeField] private bool lockY = false; // Y축 회전 고정 여부
    [SerializeField] private bool reverseDirection = false; // 반대 방향을 바라볼지 여부
    
    private void Start()
    {
        // 카메라가 할당되지 않은 경우 메인 카메라를 자동으로 찾음
        if (!targetCamera)
        {
            targetCamera = Camera.main;
            
            if (!targetCamera)
            {
                Debug.LogWarning("BillboardUI: 타겟 카메라가 없습니다. Camera.main을 찾을 수 없습니다.");
            }
        }
    }
    
    private void LateUpdate()
    {
        if (!targetCamera) return;
        
        BillboardToCamera();
    }
    
    private void BillboardToCamera()
    {
        Vector3 directionToCamera = targetCamera.transform.position - transform.position;
        
        if (lockY)
        {
            // Y축을 고정하고 XZ 평면에서만 회전
            directionToCamera.y = 0;
        }
        
        if (directionToCamera != Vector3.zero)
        {
            if (reverseDirection)
            {
                directionToCamera = -directionToCamera;
            }
            
            // 카메라 방향으로 회전
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
}
