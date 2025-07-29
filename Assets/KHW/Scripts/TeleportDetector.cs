using UnityEngine;
public class TeleportDetector : MonoBehaviour
{
    // public Transform xrOrigin;    // XR Origin (VR player)
    // public float detectRadius = 0.5f;   // Detection radius
    // bool triggered = false;  // Has teleport detected

    // void Update()
    // {
    //     // Calculate distance to XR Origin
    //     float dist = Vector3.Distance(transform.position, xrOrigin.position);
    //     // Debug.Log($"distance : {dist}");

    //     // If already triggered or too far, skip
    //     if (triggered || dist > detectRadius) return;

    //     // First time entering detection zone
    //     triggered = true;
    //     TutorialManager.Instance?.OnTeleportDone();   // Notify TutorialManager
    // }

     public Transform targetCenter;    // 도달해야 하는 위치
    public float targetRadius = 1f;

    public float jumpThreshold = 3f;  
    public float timeWindow = 0.2f;

    bool isEnabled = false;
    Vector3 prevPos;
    float prevTime;

    public void EnableDetector(bool enable)
    {
        isEnabled = enable;
        prevPos = transform.position;
        prevTime = Time.time;
    }

    void Update()
    {
        if (!isEnabled) return;

        float dist = Vector3.Distance(transform.position, prevPos);
        float dt = Time.time - prevTime;

        if (dist >= jumpThreshold && dt <= timeWindow)
        {
            if (Vector3.Distance(transform.position, targetCenter.position) <= targetRadius)
            {
                TutorialManager.Instance?.OnTeleportDone();
                isEnabled = false;
            }
        }

        prevPos = transform.position;
        prevTime = Time.time;
    }
}

