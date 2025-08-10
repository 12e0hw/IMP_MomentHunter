using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class GrabYAxisOnlyController : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private bool isGrabbed = false;
    private IXRSelectInteractor currentInteractor;

    [Tooltip("Attach Transform")]
    public Transform attachTransform;

    [Tooltip("Correction rotationX")]
    public float initialGrabXAngle = -90f;

    [Tooltip("Correction rotationZ")]
    public float initialGrabZAngle = 180f;

    [Tooltip("Correction position")]
    public Vector3 positionOffset = Vector3.zero;

    private float originalXAngle;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (attachTransform == null)
        {
            GameObject attachObj = new GameObject("Attach_YAxis");
            attachObj.transform.SetParent(transform, false);
            attachTransform = attachObj.transform;
        }
        grabInteractable.attachTransform = attachTransform;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        currentInteractor = args.interactorObject as IXRSelectInteractor;

        originalXAngle = transform.eulerAngles.x;

        if (currentInteractor != null)
        {
            Transform interactorAttach = currentInteractor.GetAttachTransform(grabInteractable);

            Vector3 worldOffset = interactorAttach.TransformVector(positionOffset);

            attachTransform.position = interactorAttach.position + worldOffset;
            attachTransform.rotation = interactorAttach.rotation;

            transform.position = interactorAttach.position + worldOffset;

            Vector3 handEuler = interactorAttach.eulerAngles;
            transform.rotation = Quaternion.Euler(initialGrabXAngle, handEuler.y, initialGrabZAngle);
        }

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        currentInteractor = null;

        Vector3 currentEuler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(originalXAngle, currentEuler.y, currentEuler.z);

        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void LateUpdate()
    {
        if (isGrabbed && currentInteractor != null)
        {
            Transform interactorAttach = currentInteractor.GetAttachTransform(grabInteractable);

            Vector3 worldOffset = interactorAttach.TransformVector(positionOffset);

            transform.position = interactorAttach.position + worldOffset;

            Vector3 handEuler = interactorAttach.eulerAngles;
            transform.rotation = Quaternion.Euler(initialGrabXAngle, handEuler.y, initialGrabZAngle);

            if (attachTransform != null)
            {
                attachTransform.position = interactorAttach.position + worldOffset;
                attachTransform.rotation = Quaternion.Euler(0f, handEuler.y, 0f);
            }
        }
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }
}
