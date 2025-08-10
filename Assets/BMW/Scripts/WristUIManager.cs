using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class WristUIManager : MonoBehaviour
{
    // UI Component References
    [Header("UIComponents")]
    [SerializeField] private GameObject WristCanvus; // Parent canvas for wrist UI
    [SerializeField] private GameObject WristUI;     // Main wrist UI panel
    [SerializeField] private GameObject ManualUI;  // Manual UI panel
                     private GameObject Manual1Page; // First Manual page
                     private GameObject Manual2Page; // Second Manual page
    [SerializeField] private TextMeshProUGUI ManualPageText; // Manual page indicator text
    [SerializeField] private GameObject AudioUI;     // Audio settings UI panel
    [SerializeField] private TextMeshProUGUI BGMText; // Background music volume text
    [SerializeField] private Slider BGMSlider;        // BGM volume slider
    [SerializeField] private TextMeshProUGUI SFXText; // SFX volume text
    [SerializeField] private Slider SFXSlider;        // SFX volume slider
    [SerializeField] private GameObject HomeBackUI;   // Main back UI panel
    [SerializeField] private GameObject CamUI;        // Camera UI element

    private string SelectedMenu; // Current selected menu/button

    [SerializeField] private Animator animator;
    private Coroutine closeUICoroutine = null;
    public bool isYGrap;

    // Interactor Component References
    [Header("InteractorComponents")]
    [SerializeField] private GameObject rightController; // Right VR controller
                     private GameObject R_directInteractor; // Right direct interactor
                     private GameObject R_rayInteractor;    // Right ray interactor
    [SerializeField] private GameObject leftController;     // Left VR controller
    [SerializeField] private GameObject uiRayInteractor;    // UI ray interactor
                     private GameObject L_directInteractor; // Left direct interactor

    // Camera Reference
    [Header("CameraComponents")]
    [SerializeField] private GameObject Camera; // Main camera holder

    // Debug and State Flags
    [Header("Debug Log")]
    [SerializeField] private bool isDebug = true; // Enable debug logging
    private bool isWristUI;      // Wrist UI active state
    private bool isManualUI;   // Manual UI active state
    private bool isAudioUI;      // Audio UI active state
    private bool isHomeBackUI;   // Main back UI active state
    private int pagesNum;        // Current Manual page number
    private int BGMValue;        // Current BGM value (0-100)
    private int SFXValue;        // Current SFX value (0-100)

    // External Script References
    private GameManager gameManager; // Reference to GameManager script
    private DataManager dataManager; // Reference to DataManager script

    void Awake()
    {
        // Find and cache references to GameManager and DataManager
        gameManager = FindAnyObjectByType<GameManager>();
        dataManager = FindAnyObjectByType<DataManager>();

        // Find and cache references to interactors, UI, and camera
        FindInteractorComponents();
        FindUIComponents();
        FindCameraComponents();

        // Initialize UI state flags
        isWristUI = false;
        isManualUI = false;
        isAudioUI = false;
        isHomeBackUI = false;
        isYGrap = false;

        // Set initial UI active states
        if (WristUI != null) WristUI.SetActive(isWristUI);
        if (ManualUI != null) ManualUI.SetActive(isManualUI);
        if (AudioUI != null) AudioUI.SetActive(isAudioUI);
        if (HomeBackUI != null) HomeBackUI.SetActive(isHomeBackUI);
        if (uiRayInteractor != null) uiRayInteractor.SetActive(isWristUI);

        // Initialize BGM slider
        BGMSlider.minValue = 0;
        BGMSlider.maxValue = 100;
        BGMSlider.wholeNumbers = true;
        if (dataManager != null) BGMValue = Mathf.RoundToInt(dataManager.GetBgmVolume() * 100f);
        else BGMValue = 100;
        BGMSlider.value = BGMValue;
        BGMSlider.onValueChanged.AddListener(OnBGMSliderValueChanged);
        OnBGMSliderValueChanged(BGMValue);

        // Initialize SFX slider
        SFXSlider.minValue = 0;
        SFXSlider.maxValue = 100;
        SFXSlider.wholeNumbers = true;
        if (dataManager != null) SFXValue = Mathf.RoundToInt(dataManager.GetSfxVolume() * 100f);
        else SFXValue = 100;
        SFXSlider.value = SFXValue;
        SFXSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
        OnSFXSliderValueChanged(SFXValue);

        ResetAction(); // Reset menu selection
    }

    // Returns true if any sub-UI (Manual, audio, main back) is active
    public bool GetActWristUI()
    {
        if (isManualUI || isAudioUI || isHomeBackUI) { return true; }
        else { return false; }
    }

    // Called when the Y button is pressed: toggles the wrist UI open/close
    public void GetOnYButtonPressed()
    {
        if (isDebug) Debug.Log("WristUI Called");
        if (isWristUI || isManualUI || isAudioUI || isHomeBackUI) {
            CloseAction();
        }
        else {
            OpenAction();
        }
    }

    // Called when the B button is pressed: goes back within sub-UIs
    public void GetOnBButtonPressed()
    {
        if (isManualUI || isAudioUI || isHomeBackUI) { BackAction(); }
    }

    // Handles menu button clicks
    public void OnClickMenu()
    {
        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;

        if (clickedObj != null)
        {
            SelectedMenu = clickedObj.name;

            CheckMenu();   // Handle menu logic
            UpdateText();  // Update debug text
        }
    }

    // Switches logic based on which menu/button was selected
    private void CheckMenu()
    {
        switch (SelectedMenu)
        {
            case "CloseButton":
                CloseAction();
                break;
            case "BackButton":
                BackAction();
                break;
            case "ManualUIButton":
                ManualUIAction();
                break;
            case "PreButton":
                TurningPageAction();
                break;
            case "NextButton":
                TurningPageAction();
                break;
            case "AudioUIButton":
                AudioUIAction();
                break;
            case "HomeBackUIButton":
                HomeBackUIAction();
                break;
            case "HomeBackButton":
                HomeBackAction();
                break;
            default:
                if (isDebug) Debug.LogError("Unknown menu: " + SelectedMenu);
                break;
        }
    }

    // Updates debug text for menu selection
    private void UpdateText()
    {
        if (isDebug) Debug.Log($"Clicked Menu {SelectedMenu}");
    }

    // Resets menu selection to default
    private void ResetAction()
    {
        SelectedMenu = "Not Selected";
        UpdateText();
    }

    // Opens the wrist UI and sets relevant states
    private void OpenAction()
    {
        SelectedMenu = "OpenButton";
        
        isYGrap = true;
        isWristUI = true;
        WristUI.SetActive(isWristUI);
        ToggleUIRayInteractor();
        ToggleInteractor();
        ToggleCamera();

        ResetAction();

        animator.SetTrigger("WristOpen");
        if (closeUICoroutine != null)
        {
            StopCoroutine(closeUICoroutine);
            closeUICoroutine = null;
        }
        closeUICoroutine = StartCoroutine(DelayedActionCoroutine(0.8f));


        if (isDebug) Debug.Log("The WristUI has been activated.");
    }

    // Closes all sub-UIs and wrist UI
    private void CloseAction()
    {
        SelectedMenu = "CloseButton";

        if (IsCurrentAnimState("ManualClose") || IsCurrentAnimState("AudioClose") ||
        IsCurrentAnimState("HomeBackClose") || IsCurrentAnimState("WristClose"))
        {
            if (isDebug) Debug.Log("Already playing close animation. Ignore duplicate trigger.");
            return;
        }

        if (isWristUI) animator.SetTrigger("WristClose");
        else if (isManualUI) animator.SetTrigger("ManualClose");
        else if (isAudioUI) animator.SetTrigger("AudioClose");
        else if (isHomeBackUI) animator.SetTrigger("HomeBackClose");

        if (closeUICoroutine != null)
        {
            StopCoroutine(closeUICoroutine);
            closeUICoroutine = null;
        }
        closeUICoroutine = StartCoroutine(CloseDelayedUI(0.8f));

    }

    // Goes back to the main wrist UI from sub-UIs
    private void BackAction()
    {
        SelectedMenu = "BackButton";
        
        isManualUI = false;
        ManualUI.SetActive(isManualUI);
        isAudioUI = false;
        AudioUI.SetActive(isAudioUI);
        isHomeBackUI = false;
        HomeBackUI.SetActive(isHomeBackUI);
        isWristUI = true;
        WristUI.SetActive(isWristUI);

        animator.SetTrigger("WristBack");

        if (isDebug) Debug.Log("The Back Menu has been activated.");
    }

    // Opens the Manual UI and shows the first page
    private void ManualUIAction()
    {
        SelectedMenu = "ManualUIButton";
        if (isDebug) Debug.Log("The Manual Menu has been activated.");

        pagesNum = 1;
        isWristUI = false;
        WristUI.SetActive(isWristUI);
        isManualUI = true;
        ManualUI.SetActive(isManualUI);
        if (isManualUI) animator.SetTrigger("ManualOpen");

        ManualPageText.text = pagesNum.ToString();
        Manual2Page.SetActive(false);
        Manual1Page.SetActive(true);
    }

    // Handles Manual page turning logic
    private void TurningPageAction()
    {
        SelectedMenu = "TurningPageButton";
        if (isDebug) Debug.Log("TurningPageButton has been activated.");

        pagesNum++;
        switch (pagesNum)
        {
            case 2:
                Manual1Page.SetActive(false);
                Manual2Page.SetActive(true);
                break;
            case 1:
            default:
                pagesNum = 1;
                Manual2Page.SetActive(false);
                Manual1Page.SetActive(true);
                break;
        }
        ManualPageText.text = pagesNum.ToString();
    }

    // Opens the audio settings UI
    private void AudioUIAction()
    {
        SelectedMenu = "AudioUIButton";
        if (isDebug) Debug.Log("The Audio Menu has been activated.");

        isWristUI = false;
        WristUI.SetActive(isWristUI);
        isAudioUI = true;
        AudioUI.SetActive(isAudioUI);
        if (isAudioUI) animator.SetTrigger("AudioOpen");
    }

    // Called when BGM slider value changes
    void OnBGMSliderValueChanged(float value)
    {
        BGMValue = Mathf.RoundToInt(value);
        if (BGMText != null)
            BGMText.text = BGMValue.ToString();
        if (dataManager != null)
        {
            if (isDebug) Debug.Log("dataManager found.");
            dataManager.SetBgmVolume(BGMValue);
        }
        else
        {
            if (isDebug) Debug.LogWarning("dataManager not found");
        }
    }

    // Called when SFX slider value changes
    void OnSFXSliderValueChanged(float value)
    {
        SFXValue = Mathf.RoundToInt(value);
        if (SFXText != null)
            SFXText.text = SFXValue.ToString();
        if (dataManager != null)
        {
            if (isDebug) Debug.Log("dataManager found.");
            dataManager.SetSfxVolume(SFXValue);
        }
        else
        {
            if (isDebug) Debug.LogWarning("dataManager not found");
        }
    }

    // Opens the main back UI
    private void HomeBackUIAction()
    {
        SelectedMenu = "HomeBackUIButton";
        if (isDebug) Debug.Log("The HomeBack Menu has been activated.");

        isWristUI = false;
        WristUI.SetActive(isWristUI);
        isHomeBackUI = true;
        HomeBackUI.SetActive(isHomeBackUI);
        if (isHomeBackUI) animator.SetTrigger("HomeBackOpen");
    }

    // Handles main back button logic (scene transition)
    private void HomeBackAction()
    {
        if (gameManager != null)
        {
            if (isDebug) Debug.Log("gameManager found.");
            animator.SetTrigger("HomeBackClose");

            animator.ResetTrigger("WristOpen");
            animator.ResetTrigger("WristClose");
            animator.ResetTrigger("ManualOpen");
            animator.ResetTrigger("ManualClose");
            animator.ResetTrigger("AudioOpen");
            animator.ResetTrigger("AudioClose");
            animator.ResetTrigger("HomeBackOpen");
            animator.ResetTrigger("HomeBackClose");
            animator.ResetTrigger("HomeBackClose");
            animator.ResetTrigger("WristBack");

            gameManager.TransitionToScene(0);
        }
        else
        {
            if (isDebug) Debug.LogWarning("gameManager not found");
        }
    }

    // Finds and caches references to interactor components
    private void FindInteractorComponents()
    {
        if (rightController == null) rightController = GameObject.Find("Right Controller");
        if (rightController != null)
        {
            R_directInteractor = rightController.transform.Find("Direct Interactor").gameObject;
            R_rayInteractor = rightController.transform.Find("Ray Interactor").gameObject;

            if (R_directInteractor != null && R_rayInteractor != null)
            {
                if (isDebug) Debug.Log("Interactor found and cached.");
            }
            else
            {
                if (isDebug) Debug.LogWarning("Interactor not found under Right Controller!");
            }
        }
        else
        {
            if (isDebug) Debug.LogWarning("Right Controller not found!");
        }

        if (leftController == null) leftController = GameObject.Find("Left Controller");
        if (leftController != null)
        {
            L_directInteractor = leftController.transform.Find("Direct Interactor").gameObject;

            if (L_directInteractor != null)
            {
                if (isDebug) Debug.Log("Interactor found and cached.");
            }
            else
            {
                if (isDebug) Debug.LogWarning("Interactor not found under Left Controller!");
            }
        }
        else
        {
            if (isDebug) Debug.LogWarning("Left Controller not found!");
        }
    }

    // Finds and caches references to UI components
    private void FindUIComponents()
    {
        if (uiRayInteractor == null) uiRayInteractor = rightController.transform.Find("UI Ray Interactor").gameObject;
        if (uiRayInteractor != null)
        {
            if (isDebug) Debug.Log("UI Ray Interactor found and cached.");
        }
        else
        {
            if (isDebug) Debug.LogWarning("UI Ray Interactor not found under Right Controller!");
        }

        if (WristCanvus != null)
        {
            if (isDebug) Debug.Log("WristCanvus found!");
        }
        else
        {
            if (isDebug) Debug.LogWarning("WristCanvus not found!");
        }
        if (WristUI == null) WristUI = WristCanvus.transform.Find("WristUI").gameObject;
        if (WristUI != null)
        {
            if (isDebug) Debug.Log("WristUI found!");
        }
        else
        {
            if (isDebug) Debug.LogWarning("WristUI not found!");
        }

        if (ManualUI == null) ManualUI = WristCanvus.transform.Find("ManualUI").gameObject;
        if (ManualUI != null)
        {
            if (isDebug) Debug.Log("ManualUI found!");

            Manual1Page = ManualUI.transform.Find("Manual1Page").gameObject;
            if (Manual1Page != null)
            {
                if (isDebug) Debug.Log("Manual1Page found!");
            }
            else
            {
                if (isDebug) Debug.LogWarning("Manual1Page not found!");
            }

            Manual2Page = ManualUI.transform.Find("Manual2Page").gameObject;
            if (Manual2Page != null)
            {
                if (isDebug) Debug.Log("Manual2Page found!");
            }
            else
            {
                if (isDebug) Debug.LogWarning("Manual2Page not found!");
            }
        }
        else
        {
            if (isDebug) Debug.LogWarning("ManualUI not found!");
        }

        if (AudioUI == null) AudioUI = WristCanvus.transform.Find("AudioUI").gameObject;
        if (AudioUI != null)
        {
            if (isDebug) Debug.Log("AudioUI found!");
        }
        else
        {
            if (isDebug) Debug.LogWarning("AudioUI not found!");
        }

        if (HomeBackUI == null) HomeBackUI = WristCanvus.transform.Find("HomeBackUI").gameObject;
        if (HomeBackUI != null)
        {
            if (isDebug) Debug.Log("HomeBackUI found!");
        }
        else
        {
            if (isDebug) Debug.LogWarning("HomeBackUI not found!");
        }

        if (CamUI != null)
        {
            if (isDebug) Debug.Log("CamUI found!");
        }
        else
        {
            if (isDebug) Debug.LogWarning("CamUI not found!");
        }
    }

    // Finds and caches reference to the camera
    private void FindCameraComponents()
    {
        if (Camera == null) Camera = GameObject.Find("Camera Holder");
        if (Camera != null)
        {
            if (isDebug) Debug.Log("Camera found!");
        }
        else
        {
            if (isDebug) Debug.LogWarning("Camera not found!");
        }
    }

    // Toggles the interactors' active state based on wrist UI state
    private void ToggleInteractor()
    {
        if (R_directInteractor != null && R_rayInteractor != null && L_directInteractor != null)
        {
            R_directInteractor.SetActive(!isWristUI);
            R_rayInteractor.SetActive(!isWristUI);
            L_directInteractor.SetActive(!isWristUI);
            if (CamUI != null && isWristUI) CamUI.SetActive(!isWristUI);
            if (isDebug) Debug.Log("Interactor modified.");
        }
        else
        {
            FindInteractorComponents();
            if (uiRayInteractor != null)
            {
                R_directInteractor.SetActive(!isWristUI);
                R_rayInteractor.SetActive(!isWristUI);
                L_directInteractor.SetActive(!isWristUI);
                if (CamUI != null) CamUI.SetActive(!isWristUI);
                if (isDebug) Debug.Log("Interactor found and modified.");
            }
            else
            {
                if (isDebug) Debug.LogError("Cannot find Interactor to modify!");
            }
        }
    }

    // Toggles the UI ray interactor based on wrist UI state
    private void ToggleUIRayInteractor()
    {
        if (uiRayInteractor != null)
        {
            uiRayInteractor.SetActive(isWristUI);
            if (isDebug) Debug.Log("UI Ray Interactor modified.");
        }
        else
        {
            FindInteractorComponents();
            FindUIComponents();
            if (uiRayInteractor != null)
            {
                uiRayInteractor.SetActive(isWristUI);
                if (isDebug) Debug.Log("UI Ray Interactor found and modified.");
            }
            else
            {
                if (isDebug) Debug.LogError("Cannot find UI Ray Interactor to modified!");
            }
        }
    }

    // Toggles the camera active state based on wrist UI state
    private void ToggleCamera()
    {
        if (Camera != null)
        {
            Camera.SetActive(!isWristUI);
            if (isDebug) Debug.Log("Camera modified.");
        }
        else
        {
            FindCameraComponents();
            if (Camera != null)
            {
                Camera.SetActive(!isWristUI);
                if (isDebug) Debug.Log("Camera found and modified.");
            }
            else
            {
                if (isDebug) Debug.LogError("Cannot find Camera to modified!");
            }
        }
    }

    private IEnumerator DelayedActionCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        closeUICoroutine = null;

        if (isDebug) Debug.Log("reaction");
    }

    private IEnumerator CloseDelayedUI(float maxWait = 2.0f)
    {
        float elapsed = 0f;

        string closeState =
            isManualUI ? "ManualClose" :
            isAudioUI ? "AudioClose" :
            isHomeBackUI ? "HomeBackClose" :
            "WristClose";

        while (!IsCurrentAnimState(closeState) && elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        while (GetCurrentAnimNormalizedTime() < 0.95f && elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        animator.ResetTrigger("WristOpen");
        animator.ResetTrigger("WristClose");
        animator.ResetTrigger("ManualOpen");
        animator.ResetTrigger("ManualClose");
        animator.ResetTrigger("AudioOpen");
        animator.ResetTrigger("AudioClose");
        animator.ResetTrigger("HomeBackOpen");
        animator.ResetTrigger("HomeBackClose");
        animator.ResetTrigger("HomeBackClose");
        animator.ResetTrigger("WristBack");

        closeUICoroutine = null;

        isWristUI = false;
        WristUI.SetActive(isWristUI);
        isManualUI = false;
        ManualUI.SetActive(isManualUI);
        isAudioUI = false;
        AudioUI.SetActive(isAudioUI);
        isHomeBackUI = false;
        HomeBackUI.SetActive(isHomeBackUI);

        ToggleUIRayInteractor();
        ToggleInteractor();
        ToggleCamera();
        isYGrap = false;

        if (isDebug) Debug.Log("The WristUI has been disabled after animation finished.");
    }

    private bool IsCurrentAnimState(string stateName, int layer = 0)
    {
        if (animator == null) return false;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layer);
        return info.IsName(stateName);
    }

    private float GetCurrentAnimNormalizedTime(int layer = 0)
    {
        if (animator == null) return 0f;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layer);
        return info.normalizedTime;
    }

}
