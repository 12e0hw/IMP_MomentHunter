using UnityEngine;
using UnityEngine.InputSystem;

public class TurnDetector : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty leftStick;   // (-1~1, -1~1)

    [Header("Thresholds")]
    public float requiredYawDegrees = 30f;  

    float accumYaw;

    void OnEnable()
    {
        accumYaw = 0f;
    }

    void OnDisable() => leftStick.action.Disable();

    void Update()
    {
        Vector2 axis = leftStick.action.ReadValue<Vector2>();
        accumYaw   += Mathf.Abs(axis.x) * 90f * Time.deltaTime;  

        if (accumYaw >= requiredYawDegrees)
        {
            TutorialManager.Instance?.OnTurnDone();
        }
    }
}
