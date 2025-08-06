using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using System;

public class TutorialManager : MonoBehaviour
{
    // Tutorial steps
    public enum Step { None, Turn, Move, GrabLeft, GrabRight, TakePhoto, AllDone }
    public Step Current { get; private set; }   // Current tutorial step
    
    public static TutorialManager Instance;   // Singleton instance
    void Awake() => Instance = this;

    [Header("Step UI")]
    public GameObject turnUI;
    public GameObject moveUI;
    public GameObject leftGrabUI;
    public GameObject rightGrabUI;
    public GameObject photoUI;
    public GameObject tutorialCanvas;

    [Header("Trigger")]
    public GameObject moveTriggerZoneObject;
    public GameObject leftGrabDetectorObject;
    public TurnDetector turnDetector;

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
        Debug.Log("start");
        Current = Step.Turn;
        turnUI.SetActive(true);
        if (turnDetector != null) turnDetector.enabled = true;

        yield return null;
    }

    public void OnTurnDone()
    {
        if (Current != Step.Turn) return;

        turnUI.SetActive(false);
        audioSrc.PlayOneShot(successClip);  

        Current = Step.Move;
        moveUI.SetActive(true);
        moveTriggerZoneObject.SetActive(true);
    }
    public void OnMoveDone()
    {
        if (Current != Step.Move) return;

        moveUI.SetActive(false);
        moveTriggerZoneObject.SetActive(false);
        audioSrc.PlayOneShot(successClip);  
        
        Current = Step.GrabLeft;
        leftGrabDetectorObject.SetActive(true);
        leftGrabUI.SetActive(true);
    }

    public void OnLeftGrabDone()
    {
        if (Current != Step.GrabLeft) return;

        leftGrabDetectorObject.SetActive(false);
        leftGrabUI.SetActive(false);
        audioSrc.PlayOneShot(successClip);  

        Current = Step.GrabRight;
        rightGrabUI.SetActive(true);
    }

    public void OnRightGrabDone()
    {
        if (Current != Step.GrabRight) return;

        rightGrabUI.SetActive(false);
        audioSrc.PlayOneShot(successClip);   

        Current = Step.TakePhoto;
        photoUI.SetActive(true);
    }

    // Called by PhotoCaptureAndJudge
    public void OnTutorialPhotoTaken()
    {
        if (Current != Step.TakePhoto) return;

        photoUI.SetActive(false);   
        audioSrc.PlayOneShot(successClip);   

        // All done
        Current = Step.AllDone;
        tutorialCanvas.SetActive(false);   // Hide tutorial canvas
    }
}

