using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(XRGrabInteractable))]
public class ObjectSoundController : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip grabSound;
    [SerializeField] private AudioClip collisionSound;

    [Header("Local Volume")]
    [Range(0f, 1f)][SerializeField] private float grabSoundVolume = 1.0f;
    [Range(0f, 1f)][SerializeField] private float collisionSoundVolume = 1.0f;

    [Header("Loop Grab Sound")]
    [SerializeField] private bool loopGrabSound = false;

    [Header("Collision Cooldown")]
    [SerializeField] private float collisionCooldown = 3f;

    private Dictionary<GameObject, float> lastCollisionTimes = new Dictionary<GameObject, float>();
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

    private void OnEnable()
    {
        if (DataManager.Data != null)
        {
            DataManager.OnSfxVolumeChanged += OnSfxVolumeChanged;
        }
    }

    private void OnDisable()
    {
        if (DataManager.Data != null)
        {
            DataManager.OnSfxVolumeChanged -= OnSfxVolumeChanged;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        collisionSoundEnabled = true;
        isGrabbed = true;

        float masterSfx = GetMasterSfxVolume();

        if (grabSound != null)
        {
            if (loopGrabSound)
            {
                audioSource.clip = grabSound;
                audioSource.volume = grabSoundVolume * masterSfx;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                audioSource.loop = false;
                audioSource.PlayOneShot(grabSound, grabSoundVolume * masterSfx);
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
            GameObject otherObject = collision.gameObject;
            float currentTime = Time.time;
            float masterSfx = GetMasterSfxVolume();

            if (!lastCollisionTimes.ContainsKey(otherObject) || currentTime - lastCollisionTimes[otherObject] >= collisionCooldown)
            {
                audioSource.PlayOneShot(collisionSound, collisionSoundVolume * masterSfx);
                lastCollisionTimes[otherObject] = currentTime;
            }
        }
    }

    private void OnSfxVolumeChanged(float newVolume)
    {
        if (loopGrabSound && audioSource.isPlaying)
        {
            audioSource.volume = grabSoundVolume * newVolume;
        }
    }

    private float GetMasterSfxVolume()
    {
        if (DataManager.Data != null)
            return DataManager.Data.GetSfxVolume();
        return 1.0f;
    }

    public void UpdateLoopGrabSoundVolume()
    {
        if (loopGrabSound && audioSource.isPlaying)
        {
            audioSource.volume = grabSoundVolume * GetMasterSfxVolume();
        }
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);

        if (DataManager.Data != null)
        {
            DataManager.OnSfxVolumeChanged -= OnSfxVolumeChanged;
        }
    }
}
