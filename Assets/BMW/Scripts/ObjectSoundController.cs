using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(XRGrabInteractable))]
public class ObjectSoundController : MonoBehaviour
{
    public AudioClip grabSound;
    public AudioClip collisionSound;

    [Range(0f, 1f)]
    public float grabSoundVolume = 1.0f;
    [Range(0f, 1f)]
    public float collisionSoundVolume = 1.0f;

    [Header("Grab Sound Loop ¼³Á¤")]
    public bool loopGrabSound = false;

    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    private bool collisionSoundEnabled = false;
    private bool isGrabbed = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        collisionSoundEnabled = true;
        isGrabbed = true;

        if (grabSound != null)
        {
            if (loopGrabSound)
            {
                audioSource.clip = grabSound;
                audioSource.volume = grabSoundVolume;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                audioSource.loop = false;
                audioSource.PlayOneShot(grabSound, grabSoundVolume);
            }
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;

        if (loopGrabSound && audioSource.isPlaying)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionSound != null && collisionSoundEnabled && !isGrabbed)
        {
            audioSource.PlayOneShot(collisionSound, collisionSoundVolume);
        }
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }
}
