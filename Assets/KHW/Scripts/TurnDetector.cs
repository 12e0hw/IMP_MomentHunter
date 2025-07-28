using UnityEngine;
using UnityEngine.InputSystem;

public class TurnDetector : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty leftStick;   // (-1~1, -1~1)

    [Header("Thresholds")]
    public float requiredYawDegrees = 90f;  
    public float requiredPitchDegrees = 45f; 

    float accumYaw, accumPitch;

    void OnEnable()
    {
        leftStick.action.Enable();
        accumYaw = accumPitch = 0f;
    }

    void OnDisable() => leftStick.action.Disable();

    void Update()
    {
        Vector2 axis = leftStick.action.ReadValue<Vector2>();
        accumYaw   += Mathf.Abs(axis.x) * 90f * Time.deltaTime;  
        accumPitch += Mathf.Abs(axis.y) * 90f * Time.deltaTime;

        if (accumYaw >= requiredYawDegrees || accumPitch >= requiredPitchDegrees)
        {
            TutorialManager.Instance?.OnTurnDone();
            enabled = false;
        }
    }
}
