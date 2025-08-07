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

    // [Header("Step UI")]
    // public GameObject turnUI;
    // public GameObject moveUI;
    // public GameObject leftGrabUI;
    // public GameObject rightGrabUI;
    // public GameObject photoUI;
    public GameObject tutorialCanvas;

    [Header("Step UI - K")]
    public GameObject turnUI_K;
    public GameObject moveUI_K;
    public GameObject leftGrabUI_K;
    public GameObject rightGrabUI_K;
    public GameObject photoUI_K;

    [Header("Step UI - E")]
    public GameObject turnUI_E;
    public GameObject moveUI_E;
    public GameObject leftGrabUI_E;
    public GameObject rightGrabUI_E;
    public GameObject photoUI_E;

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
        GetTurnUI().SetActive(true);
        if (turnDetector != null) turnDetector.enabled = true;

        yield return null;
    }

    public void OnTurnDone()
    {
        if (Current != Step.Turn) return;

        GetTurnUI().SetActive(false);
        audioSrc.PlayOneShot(successClip);

        Current = Step.Move;
        GetMoveUI().SetActive(true);
        moveTriggerZoneObject.SetActive(true);
    }
    public void OnMoveDone()
    {
        if (Current != Step.Move) return;

        GetMoveUI().SetActive(false);
        moveTriggerZoneObject.SetActive(false);
        audioSrc.PlayOneShot(successClip);

        Current = Step.GrabLeft;
        leftGrabDetectorObject.SetActive(true);
        GetLeftGrabUI().SetActive(true);
    }

    public void OnLeftGrabDone()
    {
        if (Current != Step.GrabLeft) return;

        leftGrabDetectorObject.SetActive(false);
        GetLeftGrabUI().SetActive(false);
        audioSrc.PlayOneShot(successClip);

        Current = Step.GrabRight;
        GetRightGrabUI().SetActive(true);
    }

    public void OnRightGrabDone()
    {
        if (Current != Step.GrabRight) return;

        GetRightGrabUI().SetActive(false);
        audioSrc.PlayOneShot(successClip);

        Current = Step.TakePhoto;
        GetPhotoUI().SetActive(true);
    }

    // Called by PhotoCaptureAndJudge
    public void OnTutorialPhotoTaken()
    {
        if (Current != Step.TakePhoto) return;

        GetPhotoUI().SetActive(false);
        audioSrc.PlayOneShot(successClip);

        // All done
        Current = Step.AllDone;
        tutorialCanvas.SetActive(false);   // Hide tutorial canvas
    }
    
    // 언어에 따라 UI 선택 헬퍼 함수
    GameObject GetTurnUI() => LanguageSwitcher.IsEnglish ? turnUI_E : turnUI_K;
    GameObject GetMoveUI() => LanguageSwitcher.IsEnglish ? moveUI_E : moveUI_K;
    GameObject GetLeftGrabUI() => LanguageSwitcher.IsEnglish ? leftGrabUI_E : leftGrabUI_K;
    GameObject GetRightGrabUI() => LanguageSwitcher.IsEnglish ? rightGrabUI_E : rightGrabUI_K;
    GameObject GetPhotoUI() => LanguageSwitcher.IsEnglish ? photoUI_E : photoUI_K;
}

