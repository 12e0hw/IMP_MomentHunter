// using UnityEngine;

// public class LeftGrabDetector : MonoBehaviour
// {
//     public GameObject targetObject;      // Target object to detect
//     public bool oneShot = true;          // Detect only once if true
//     Collider _zoneCollider;

//     void Awake()
//     {
//         _zoneCollider = GetComponent<Collider>();
//         _zoneCollider.isTrigger = true;   // Set collider as trigger
//     }

//     void OnTriggerEnter(Collider other)
//     {
//         if (other.gameObject != targetObject) return;   // Only detect target object

//         TutorialManager.Instance?.OnLeftGrabDone();    // Notify tutorial progress

//         if (oneShot) enabled = false;     // Disable after first detection
//     }
// }

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class LeftGrabDetector : MonoBehaviour
{
    [SerializeField] XRBaseInteractor leftHandInteractor;
    bool isEnabled = false;

    public void EnableDetector(bool enable)
    {
        isEnabled = enable;
        if (leftHandInteractor == null) return;

        if (enable)  leftHandInteractor.selectEntered.AddListener(OnGrab);
        else         leftHandInteractor.selectEntered.RemoveListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs _)
    {
        if (!isEnabled) return;
        TutorialManager.Instance?.OnLeftGrabDone();
    }
}
