using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialManager : MonoBehaviour
{
    // Tutorial steps
    public enum Step { None, Move, Turn, Teleport, GrabLeft, GrabRight, TakePhoto, AllDone }
    public Step Current { get; private set; }   // Current tutorial step
    
    public static TutorialManager Instance;   // Singleton instance
    void Awake() => Instance = this;



    [Header("Step UI")]
    public GameObject moveUI;
    public GameObject turnUI;
    public GameObject teleportUI;
    public GameObject leftGrabUI;
    public GameObject rightGrabUI;
    public GameObject photoUI;
    public GameObject tutorialCanvas;

    [Header("Trigger Zones")]
    public GameObject moveTriggerZoneObject;
    public GameObject teleportTriggerZoneObject;
    public GameObject leftGrabDetectorObject;

    [Header("Success Sound")]
    public AudioSource audioSrc;        
    public AudioClip successClip;
  

    void Start()
    {
        StartCoroutine(RunTutorial());
    }

    // Start tutorial flow
    IEnumerator RunTutorial()
    {
        // Step 1: Move
        Current = Step.Move;
        moveTriggerZoneObject.SetActive(true);
        moveUI.SetActive(true);

        yield return null;
    }

    // Called when Move is completed
    public void OnMoveDone()
    {
        if (Current != Step.Move) return;

        // Hide move step UI
        moveTriggerZoneObject.SetActive(false);
        moveUI.SetActive(false);

        audioSrc.PlayOneShot(successClip);  // Play success sound

        // Step 2: Turn
        Current = Step.Turn;
        // teleportTriggerZoneObject.SetActive(true);
        turnUI.SetActive(true);
    }
    public void OnTurnDone()
    {
        if (Current != Step.Teleport) return;

        // Hide teleport step UI
        // teleportTriggerZoneObject.SetActive(false);
        turnUI.SetActive(false);

        audioSrc.PlayOneShot(successClip);  // Play success sound
        // Step 3: Teleport
        Current = Step.Teleport;
        teleportTriggerZoneObject.SetActive(true);
        teleportUI.SetActive(true);
    }

    public void OnTeleportDone()
    {
        if (Current != Step.Teleport) return;

        // Hide teleport step UI
        teleportTriggerZoneObject.SetActive(false);
        teleportUI.SetActive(false);

        audioSrc.PlayOneShot(successClip);  // Play success sound

        // Step 4: Left grab
        Current = Step.GrabLeft;
        leftGrabDetectorObject.SetActive(true);
        leftGrabUI.SetActive(true);
    }

    public void OnLeftGrabDone()
    {
        if (Current != Step.GrabLeft) return;

        // Hide left grab step UI
        leftGrabDetectorObject.SetActive(false);
        leftGrabUI.SetActive(false);

        audioSrc.PlayOneShot(successClip);   // Play success sound

        // Step 5: Right grab
        Current = Step.GrabRight;
        rightGrabUI.SetActive(true);
    }

    public void OnRightGrabDone()
    {
        if (Current != Step.GrabRight) return;

        // Hide right grab step UI
        rightGrabUI.SetActive(false);

        audioSrc.PlayOneShot(successClip);   // Play success sound

        // Step 6: Take photo
        Current = Step.TakePhoto;
        photoUI.SetActive(true);
    }

    // Called by PhotoCaptureAndJudge
    public void OnTutorialPhotoTaken()
    {
        if (Current != Step.TakePhoto) return;

        photoUI.SetActive(false);   // Hide photo step UI
        audioSrc.PlayOneShot(successClip);   // Play success sound

        // All done
        Current = Step.AllDone;
        tutorialCanvas.SetActive(false);   // Hide tutorial canvas
    }
}
