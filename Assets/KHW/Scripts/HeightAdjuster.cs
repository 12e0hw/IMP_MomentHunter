using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class HeightAdjuster : MonoBehaviour
{
    [Header("필수 설정")]
    public XROrigin xrOrigin;  
    public InputActionReference leftStickMove; 

    [Header("이동 설정")]
    public float moveSpeed = 1.0f;
    public float minY = 0.5f;   
    public float maxY = 2.5f;  

    void Update()
    {
        if (xrOrigin == null || leftStickMove == null) return;

        Vector2 input = leftStickMove.action.ReadValue<Vector2>();
        float verticalInput = input.y;

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            Vector3 currentPos = xrOrigin.transform.position;
            float newY = Mathf.Clamp(currentPos.y + verticalInput * moveSpeed * Time.deltaTime, minY, maxY);

            xrOrigin.transform.position = new Vector3(currentPos.x, newY, currentPos.z);
        }
    }
}
