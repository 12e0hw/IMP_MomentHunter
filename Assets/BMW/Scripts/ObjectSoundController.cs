using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(XRGrabInteractable))]
public class ObjectSoundController : MonoBehaviour
{
    public AudioClip grabSound;
    public AudioClip collisionSound;

    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        audioSource.playOnAwake = false;
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (grabSound != null)
            audioSource.PlayOneShot(grabSound);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionSound != null)
        {
            audioSource.PlayOneShot(collisionSound);
        }
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
    }
}