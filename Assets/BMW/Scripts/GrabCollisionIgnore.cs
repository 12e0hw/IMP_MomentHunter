using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabCollisionIgnore : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private float ignoreDistance = 0.5f;
    [SerializeField] private float pushSmooth = 10f;
    [SerializeField] private bool useCameraAsCenter = true;

    private XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;

    private Rigidbody rb;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void Update()
    {
        if (isGrabbed && xrOrigin != null)
        {
            Vector3 playerPos = useCameraAsCenter
                ? xrOrigin.Camera.transform.position
                : xrOrigin.transform.position;

            Vector3 objPos = transform.position;
            float distance = Vector3.Distance(playerPos, objPos);

            if (distance < ignoreDistance)
            {
                Vector3 targetPos = playerPos + (objPos - playerPos).normalized * ignoreDistance;

                if (rb != null && !rb.isKinematic)
                {
                    rb.MovePosition(Vector3.Lerp(objPos, targetPos, Time.deltaTime * pushSmooth));
                }
                else
                {
                    transform.position = Vector3.Lerp(objPos, targetPos, Time.deltaTime * pushSmooth);
                }
            }
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }
}