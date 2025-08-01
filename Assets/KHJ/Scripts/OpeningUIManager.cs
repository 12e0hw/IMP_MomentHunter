using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Manages VR UI interactions for the opening scene including main menu, settings, and quit functionality.
/// Handles VR input through XR controllers and manages popup states.
/// </summary>
public class OpeningUIManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private Text _debugText; // Debug information display for VR

    [Header("UI Components")]
    [SerializeField] private GameObject _playUI;
    [SerializeField] private GameObject _settingsUI;
    [SerializeField] private GameObject _quitUI;
    [SerializeField] private Canvas _openingCanvas;
    [SerializeField] private Canvas _prologueCanvas;
    
    [Header("Popup UI Components")]
    [SerializeField] private GameObject _settingsPopupUI;  // Settings popup UI
    [SerializeField] private GameObject _quitPopupUI;      // Quit popup UI
    
    // Internal components for settings and quit UI
    [Header("Settings UI Components")]
    [SerializeField] private Slider _bgmSlider;           // BGM volume slider
    [SerializeField] private Slider _sfxSlider;           // SFX volume slider
    // [SerializeField] private Text _bgmValueText;          // BGM value display text (optional)
    // [SerializeField] private Text _sfxValueText;          // SFX value display text (optional)
    [SerializeField] private Toggle _KoreanToggle;
    [SerializeField] private Toggle _EnglishToggle;
    [SerializeField] private LanguageSelector _languageSelector;
    
    /// <summary>
    /// Flags to check if toggle event listeners are registered
    /// </summary>
    private bool _koreanToggleListenerRegistered = false;
    private bool _englishToggleListenerRegistered = false;
    
    [Header("Quit UI Components")]
    [SerializeField] private Button _mainBackButton;      // Return to main button
    
    [SerializeField] private TrackedDeviceGraphicRaycaster _trackedRaycaster;
    [SerializeField] private EventSystem _eventSystem;

    [Header("VR Components")]
    [SerializeField] private XRRayInteractor _leftRayInteractor;  // Left controller ray interactor
    [SerializeField] private XRRayInteractor _rightRayInteractor; // Right controller ray interactor
    [SerializeField] private InputActionAsset _inputActions;      // VR Input Actions

    /// <summary>
    /// Canvas state management (true: prologue, false: opening)
    /// </summary>
    private bool _isPrologueActive = false;
    
    /// <summary>
    /// UI popup state management
    /// </summary>
    private bool _isSettingsPopupActive = false;
    private bool _isQuitPopupActive = false;
    
    /// <summary>
    /// Input prevention variables
    /// </summary>
    private bool _isProcessingInput = false;
    private bool _isQuittingGame = false;  // Game quit flag
    private float _inputCooldown = 0.5f; // 0.5 second cooldown
    private float _lastInputTime = 0f;
    
    /// <summary>
    /// VR Input Actions
    /// </summary>
    private InputAction _aButton;
    private InputAction _yButton;
    
    /// <summary>
    /// Currently hovered UI element by ray
    /// </summary>
    private GameObject _currentHoveredUI = null;
    
    /// <summary>
    /// Flags to check if slider event listeners are registered
    /// </summary>
    private bool _bgmSliderListenerRegistered = false;
    private bool _sfxSliderListenerRegistered = false;
    
    /// <summary>
    /// Initialize UI state and VR input systems
    /// </summary>
    private void Start()
    {
        SetPrologueActive(_isPrologueActive);
        InitializePopupUI();
        SetupVRInputActions();
        SetupSliderEvents(); // Add slider event setup
        SetupToggleEvents();
    }
    
    /// <summary>
    /// Initialize popup UIs to inactive state
    /// </summary>
    private void InitializePopupUI()
    {
        // Initially deactivate popup UIs
        if (_settingsPopupUI)
            _settingsPopupUI.SetActive(false);
        if (_quitPopupUI)
            _quitPopupUI.SetActive(false);
    }
    
    /// <summary>
    /// Set up slider event listeners and configuration
    /// </summary>
    private void SetupSliderEvents()
    {
        // BGM Slider setup
        if (_bgmSlider && !_bgmSliderListenerRegistered)
        {
            _bgmSlider.onValueChanged.AddListener(OnBgmSliderValueChanged);
            _bgmSliderListenerRegistered = true;
            
            // Configure slider (0~1 range, continuous values)
            _bgmSlider.minValue = 0f;
            _bgmSlider.maxValue = 1f;
            _bgmSlider.wholeNumbers = false;
            
            Debug.Log("BGM slider events setup completed");
        }
        
        // SFX Slider setup
        if (_sfxSlider && !_sfxSliderListenerRegistered)
        {
            _sfxSlider.onValueChanged.AddListener(OnSfxSliderValueChanged);
            _sfxSliderListenerRegistered = true;
            
            // Configure slider (0~1 range, continuous values)
            _sfxSlider.minValue = 0f;
            _sfxSlider.maxValue = 1f;
            _sfxSlider.wholeNumbers = false;
            
            Debug.Log("SFX slider events setup completed");
        }
    }
    
    /// <summary>
    /// Set up toggle event listeners and initial configuration
    /// </summary>
    private void SetupToggleEvents()
    {
        PlayerPrefs.SetInt("language", 0);
        PlayerPrefs.Save();
        Debug.Log("Language forced to Korean (0)");
    
        // Korean Toggle setup
        if (_KoreanToggle && !_koreanToggleListenerRegistered)
        {
            _KoreanToggle.onValueChanged.AddListener(OnLanguageToggleChanged);
            _koreanToggleListenerRegistered = true;
            Debug.Log("Korean toggle events setup completed");
        }

        // English Toggle setup
        if (_EnglishToggle && !_englishToggleListenerRegistered)
        {
            _EnglishToggle.onValueChanged.AddListener(OnLanguageToggleChanged);
            _englishToggleListenerRegistered = true;
            Debug.Log("English toggle events setup completed");
        }
    
        // Set initial toggle states based on saved preference
        int currentLanguage = PlayerPrefs.GetInt("language", 0);
        if (currentLanguage == 0) // Korean
        {
            _KoreanToggle.isOn = true;
            _EnglishToggle.isOn = false;
        }
        else // English
        {
            _KoreanToggle.isOn = false;
            _EnglishToggle.isOn = true;
        }
    }
    
    /// <summary>
    /// Called when any language toggle value changes
    /// </summary>
    /// <param name="isOn">Toggle state</param>
    private void OnLanguageToggleChanged(bool isOn)
    {
        // This function mainly handles cases where toggles are changed programmatically
        // Most user interactions are handled directly in HandleSettingsPopupClick
    
        if (!isOn) return; // Ignore deactivation events
    
        // Safety check - ensure at least one toggle is always active
        if (_KoreanToggle.isOn)
        {
            if (_EnglishToggle.isOn)
            {
                _EnglishToggle.isOn = false;
            }
        
            if (_languageSelector)
            {
                _languageSelector.SetKorean();
            }
        
            Debug.Log("Language changed to Korean");
        }
        else if (_EnglishToggle.isOn)
        {
            if (_KoreanToggle.isOn)
            {
                _KoreanToggle.isOn = false;
            }
        
            if (_languageSelector)
            {
                _languageSelector.SetEnglish();
            }
        
            Debug.Log("Language changed to English");
        }
    }

    /// <summary>
    /// Safely set language toggle states and trigger language change
    /// </summary>
    /// <param name="koreanState">Korean toggle state</param>
    /// <param name="englishState">English toggle state</param>
    private void SetLanguageToggles(bool koreanState, bool englishState)
    {
        // Temporarily remove listeners to prevent recursive calls
        if (_KoreanToggle && _koreanToggleListenerRegistered)
        {
            _KoreanToggle.onValueChanged.RemoveListener(OnLanguageToggleChanged);
        }
    
        if (_EnglishToggle && _englishToggleListenerRegistered)
        {
            _EnglishToggle.onValueChanged.RemoveListener(OnLanguageToggleChanged);
        }
    
        // Set toggle states directly
        if (_KoreanToggle) _KoreanToggle.isOn = koreanState;
        if (_EnglishToggle) _EnglishToggle.isOn = englishState;
    
        // Re-add listeners
        if (_KoreanToggle && _koreanToggleListenerRegistered)
        {
            _KoreanToggle.onValueChanged.AddListener(OnLanguageToggleChanged);
        }
    
        if (_EnglishToggle && _englishToggleListenerRegistered)
        {
            _EnglishToggle.onValueChanged.AddListener(OnLanguageToggleChanged);
        }
    
        // Manually trigger language change
        if (koreanState && _languageSelector)
        {
            _languageSelector.SetKorean();
            Debug.Log("Language changed to Korean");
        }
        else if (englishState && _languageSelector)
        {
            _languageSelector.SetEnglish();
            Debug.Log("Language changed to English");
        }
    }
    
    /// <summary>
    /// Called whenever BGM slider value changes (including during drag)
    /// </summary>
    /// <param name="value">New slider value (0-1)</param>
    private void OnBgmSliderValueChanged(float value)
    {
        int volumeLevel = Mathf.RoundToInt(value * 100f);
        
        // Set BGM volume in DataManager
        if (DataManager.Data)
        {
            DataManager.Data.SetBgmVolume(volumeLevel);
        }
        
        // Optional: Display volume value as text
        // if (_bgmValueText)
        // {
        //     _bgmValueText.text = volumeLevel.ToString() + "%";
        // }
        
        Debug.Log($"BGM volume updated to: {volumeLevel}% (Slider value: {value:F2})");
    }
    
    /// <summary>
    /// Called whenever SFX slider value changes (including during drag)
    /// </summary>
    /// <param name="value">New slider value (0-1)</param>
    private void OnSfxSliderValueChanged(float value)
    {
        int volumeLevel = Mathf.RoundToInt(value * 100f);
        
        // Set SFX volume in DataManager
        if (DataManager.Data)
        {
            DataManager.Data.SetSfxVolume(volumeLevel);
        }
        
        // Optional: Display volume value as text
        // if (_sfxValueText)
        // {
        //     _sfxValueText.text = volumeLevel.ToString() + "%";
        // }
        
        Debug.Log($"SFX volume updated to: {volumeLevel}% (Slider value: {value:F2})");
    }
    
    /// <summary>
    /// Set up VR input action bindings for controllers
    /// </summary>
    private void SetupVRInputActions()
    {
        if (!_inputActions)
        {
            Debug.LogError("InputActionAsset not found!");
            return;
        }

        // Right controller A button setup (from XRI Right Action Map)
        var rightActionMap = _inputActions?.FindActionMap("XRI Right");
        if (rightActionMap != null)
        {
            _aButton = rightActionMap.FindAction("AButton");
            if (_aButton != null)
            {
                _aButton.Enable();
                _aButton.performed += OnAButtonPressed;
            }
            else
            {
                Debug.LogError("AButton action not found!");
            }
        }
        else
        {
            Debug.LogError("XRI Right action map not found!");
        }

        // Left controller Y button setup (from XRI Left Action Map)
        var leftActionMap = _inputActions?.FindActionMap("XRI Left");
        if (leftActionMap != null)
        {
            _yButton = leftActionMap.FindAction("YButton");
            if (_yButton != null)
            {
                _yButton.Enable();
                _yButton.performed += OnYButtonPressed;
            }
            else
            {
                Debug.LogError("YButton action not found!");
            }
        }
        else
        {
            Debug.LogError("XRI Left action map not found!");
        }
    }
    
    /// <summary>
    /// Update loop for VR UI hover detection
    /// </summary>
    private void Update()
    {
        // Check UI hover with raycast in VR environment
        if (!_isPrologueActive)
        {
            CheckVRUIHover();
        }
    }
    
    /// <summary>
    /// Check which UI element is currently being hovered by VR rays
    /// </summary>
    void CheckVRUIHover()
    {
        if (_isPrologueActive) 
        {
            UpdateDebugText("Prologue is active");
            return;
        }
        
        GameObject hitUI = null;
        string raySource = "";
        
        // Check left ray first
        if (_leftRayInteractor && _leftRayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult leftRaycastResult))
        {
            hitUI = leftRaycastResult.gameObject;
            raySource = "Left Ray";
        }
        // If not on left, check right ray
        else if (_rightRayInteractor && _rightRayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult rightRaycastResult))
        {
            hitUI = rightRaycastResult.gameObject;
            raySource = "Right Ray";
        }
        
        if (hitUI)
        {
            UpdateDebugText($"{raySource} hit: {hitUI.name}");
            
            // Update currently hovered UI
            if (IsTargetUI(hitUI))
            {
                _currentHoveredUI = hitUI;
                UpdateDebugText($"{raySource} - Target UI: {hitUI.name}");
            }
            else
            {
                _currentHoveredUI = null;
                UpdateDebugText($"{raySource} - Non-target: {hitUI.name}");
            }
        }
        else
        {
            _currentHoveredUI = null;
            UpdateDebugText("No UI hit");
        }
    }
    
    /// <summary>
    /// Update debug text display
    /// </summary>
    /// <param name="message">Debug message to display</param>
    private void UpdateDebugText(string message)
    {
        if (_debugText)
        {
            _debugText.text = message;
        }
    }
    
    /// <summary>
    /// Helper method to get hit UI from specific ray interactor
    /// </summary>
    /// <param name="rayInteractor">Ray interactor to check</param>
    /// <returns>Hit UI GameObject or null</returns>
    private GameObject GetRayHitUI(XRRayInteractor rayInteractor)
    {
        if (rayInteractor && rayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            return raycastResult.gameObject;
        }
        return null;
    }
    
    /// <summary>
    /// Update slider value based on right ray position when A button/trigger is pressed
    /// </summary>
    /// <param name="targetSlider">Target slider to update</param>
    /// <returns>True if slider was successfully updated</returns>
    private bool UpdateSliderFromRaycast(Slider targetSlider)
    {
        if (!targetSlider)
        {
            Debug.LogWarning("Target slider is null");
            return false;
        }
        
        // Get hit point from right ray
        if (_rightRayInteractor && _rightRayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult raycastResult))
        {
            // Get slider's RectTransform
            RectTransform sliderRect = targetSlider.GetComponent<RectTransform>();
            if (!sliderRect) 
            {
                Debug.LogWarning("Slider RectTransform is null");
                return false;
            }
            
            // Convert world point to slider's local point
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                sliderRect, raycastResult.screenPosition, raycastResult.module.eventCamera, out localPoint))
            {
                // Convert from slider's local coordinates to 0~1 value
                Rect rect = sliderRect.rect;
                float normalizedValue = Mathf.Clamp01((localPoint.x - rect.xMin) / rect.width);
                
                // Set slider value (appropriate OnSliderValueChanged will be called automatically)
                targetSlider.value = normalizedValue;
                
                string sliderType = (targetSlider == _bgmSlider) ? "BGM" : "SFX";
                Debug.Log($"{sliderType} slider updated from button press: {normalizedValue:F2} (Volume level: {Mathf.RoundToInt(normalizedValue * 100f)}%)");
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to convert screen point to local point");
            }
        }
        else
        {
            Debug.LogWarning("No raycast hit from right ray interactor");
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if the hit object is a valid target UI based on current state
    /// </summary>
    /// <param name="hitObject">GameObject to check</param>
    /// <returns>True if object is a valid target UI</returns>
    private bool IsTargetUI(GameObject hitObject)
    {
        if (!hitObject) return false;
    
        // When popup is open, only allow that popup's UI
        if (_isSettingsPopupActive)
        {
            // In settings popup, allow both BGM and SFX slider-related UI elements
            bool isBgmSlider = _bgmSlider && 
                               (hitObject == _bgmSlider.gameObject || 
                                hitObject.transform.IsChildOf(_bgmSlider.transform) ||
                                hitObject.GetComponent<Slider>() == _bgmSlider);
            
            bool isSfxSlider = _sfxSlider && 
                               (hitObject == _sfxSlider.gameObject || 
                                hitObject.transform.IsChildOf(_sfxSlider.transform) ||
                                hitObject.GetComponent<Slider>() == _sfxSlider);
        
            bool isKoreanToggle = _KoreanToggle && 
                                  (hitObject == _KoreanToggle.gameObject || 
                                   hitObject.transform.IsChildOf(_KoreanToggle.transform) ||
                                   hitObject.GetComponent<Toggle>() == _KoreanToggle);
        
            bool isEnglishToggle = _EnglishToggle && 
                                   (hitObject == _EnglishToggle.gameObject || 
                                    hitObject.transform.IsChildOf(_EnglishToggle.transform) ||
                                    hitObject.GetComponent<Toggle>() == _EnglishToggle);
    
            return isBgmSlider || isSfxSlider || isKoreanToggle || isEnglishToggle;
        }
    
        if (_isQuitPopupActive)
        {
            // In quit popup, only allow main back button
            bool isMainBackButton = _mainBackButton && 
                                    (hitObject == _mainBackButton.gameObject || 
                                     hitObject.transform.IsChildOf(_mainBackButton.transform));
        
            return isMainBackButton;
        }
    
        // When no popup is open, only allow main menu UI
        bool isMainMenuUI = (hitObject == _playUI || hitObject.transform.IsChildOf(_playUI.transform)) ||
                            (hitObject == _settingsUI || hitObject.transform.IsChildOf(_settingsUI.transform)) ||
                            (hitObject == _quitUI || hitObject.transform.IsChildOf(_quitUI.transform));
    
        return isMainMenuUI;
    }
    
    /// <summary>
    /// Handle A button press events
    /// </summary>
    /// <param name="context">Input action callback context</param>
    private void OnAButtonPressed(InputAction.CallbackContext context)
    {
        if (_isProcessingInput || Time.time - _lastInputTime < _inputCooldown)
            return;
            
        _isProcessingInput = true;
        _lastInputTime = Time.time;
        
        if (_isPrologueActive)
        {
            // In prologue, A button advances to next scene (only A button works in prologue)
            GameManager.Instance.TransitionToScene(1);
        }
        else
        {
            // A button only checks right ray
            GameObject rightHitUI = GetRayHitUI(_rightRayInteractor);
            if (rightHitUI && IsTargetUI(rightHitUI))
            {
                HandleUIClick(rightHitUI);
            }
        }
        
        _isProcessingInput = false;
    }
    
    /// <summary>
    /// Handle Y button press events (popup closing)
    /// </summary>
    /// <param name="context">Input action callback context</param>
    private void OnYButtonPressed(InputAction.CallbackContext context)
    {
        if (_isProcessingInput || Time.time - _lastInputTime < _inputCooldown)
            return;
            
        _isProcessingInput = true;
        _lastInputTime = Time.time;
        
        // Y button closes popups
        if (_isSettingsPopupActive)
        {
            CloseSettingsPopup();
        }
        else if (_isQuitPopupActive)
        {
            CloseQuitPopup();
        }
        
        _isProcessingInput = false;
    }
    
    /// <summary>
    /// Route UI click events to appropriate handlers based on current state
    /// </summary>
    /// <param name="clickedUI">The clicked UI GameObject</param>
    private void HandleUIClick(GameObject clickedUI)
    {
        Debug.Log($"HandleUIClick called with: {clickedUI.name}");
        
        // When popup is open, only handle that popup's UI
        if (_isSettingsPopupActive)
        {
            HandleSettingsPopupClick(clickedUI);
            return;
        }
        
        if (_isQuitPopupActive)
        {
            HandleQuitPopupClick(clickedUI);
            return;
        }
        
        // Handle main menu UI (only when no popup is open)
        if (clickedUI == _playUI || clickedUI.transform.IsChildOf(_playUI.transform))
        {
            OnPlayUIClicked();
        }
        else if (clickedUI == _settingsUI || clickedUI.transform.IsChildOf(_settingsUI.transform))
        {
            OnSettingsUIClicked();
        }
        else if (clickedUI == _quitUI || clickedUI.transform.IsChildOf(_quitUI.transform))
        {
            OnQuitUIClicked();
        }
        else
        {
            Debug.Log($"No matching UI handler found for: {clickedUI.name}");
        }
    }
    
    /// <summary>
    /// Handle clicks within the settings popup
    /// </summary>
    /// <param name="clickedUI">The clicked UI GameObject</param>
    private void HandleSettingsPopupClick(GameObject clickedUI)
    {
        // Check BGM slider interaction
        if (_bgmSlider && (clickedUI == _bgmSlider.gameObject || clickedUI.transform.IsChildOf(_bgmSlider.transform)))
        {
            Debug.Log("BGM slider button interaction");
            UpdateSliderFromRaycast(_bgmSlider);
        }
        // Check SFX slider interaction
        else if (_sfxSlider && (clickedUI == _sfxSlider.gameObject || clickedUI.transform.IsChildOf(_sfxSlider.transform)))
        {
            Debug.Log("SFX slider button interaction");
            UpdateSliderFromRaycast(_sfxSlider);
        }
        // Check Korean toggle interaction
        else if (_KoreanToggle && (clickedUI == _KoreanToggle.gameObject || clickedUI.transform.IsChildOf(_KoreanToggle.transform)))
        {
            Debug.Log("Korean toggle button interaction");
            // Force Korean toggle ON and English OFF (bypass Unity's default toggle behavior)
            SetLanguageToggles(true, false); // Korean ON, English OFF
        }
        // Check English toggle interaction
        else if (_EnglishToggle && (clickedUI == _EnglishToggle.gameObject || clickedUI.transform.IsChildOf(_EnglishToggle.transform)))
        {
            Debug.Log("English toggle button interaction");
            // Force English toggle ON and Korean OFF (bypass Unity's default toggle behavior)
            SetLanguageToggles(false, true); // Korean OFF, English ON
        }
    }
    
    /// <summary>
    /// Handle clicks within the quit popup
    /// </summary>
    /// <param name="clickedUI">The clicked UI GameObject</param>
    private void HandleQuitPopupClick(GameObject clickedUI)
    {
        // Only main back button (game quit)
        if (_mainBackButton && (clickedUI == _mainBackButton.gameObject || clickedUI.transform.IsChildOf(_mainBackButton.transform)))
        {
            Debug.Log("Main back button detected - calling OnMainBackButtonClicked");
            OnMainBackButtonClicked();
        }
    }

    /// <summary>
    /// Handle play UI button click - switches to prologue canvas
    /// </summary>
    private void OnPlayUIClicked()
    {
        SetPrologueActive(true); // Switch to prologue canvas
    }
    
    /// <summary>
    /// Handle settings UI button click - opens settings popup
    /// </summary>
    private void OnSettingsUIClicked()
    {
        Debug.Log("Settings clicked");
        OpenSettingsPopup();
    }
    
    /// <summary>
    /// Handle quit UI button click - opens quit popup
    /// </summary>
    private void OnQuitUIClicked()
    {
        Debug.Log("Quit clicked");
        OpenQuitPopup();
    }
    
    /// <summary>
    /// Open the settings popup and initialize slider values
    /// </summary>
    private void OpenSettingsPopup()
    {
        if (_settingsPopupUI)
        {
            _settingsPopupUI.SetActive(true);
            _isSettingsPopupActive = true;
            
            // Reflect current volumes in sliders when opening settings popup
            UpdateSliderValues();
            
            Debug.Log("Settings popup opened");
        }
    }
    
    /// <summary>
    /// Update slider values to match current DataManager volume settings
    /// </summary>
    private void UpdateSliderValues()
    {
        if (DataManager.Data)
        {
            // Update BGM slider
            if (_bgmSlider)
            {
                float bgmSliderValue = DataManager.Data.GetBgmVolumeLevel() / 100f;
                _bgmSlider.value = bgmSliderValue;
                Debug.Log($"BGM slider value updated to: {_bgmSlider.value} (Volume Level: {DataManager.Data.GetBgmVolumeLevel()}%)");
                
                // Update BGM text
                // if (_bgmValueText)
                // {
                //     _bgmValueText.text = DataManager.Data.GetBgmVolumeLevel().ToString() + "%";
                // }
            }
            
            // Update SFX slider
            if (_sfxSlider)
            {
                float sfxSliderValue = DataManager.Data.GetSfxVolumeLevel() / 100f;
                _sfxSlider.value = sfxSliderValue;
                Debug.Log($"SFX slider value updated to: {_sfxSlider.value} (Volume Level: {DataManager.Data.GetSfxVolumeLevel()}%)");
                
                // Update SFX text
                // if (_sfxValueText)
                // {
                //     _sfxValueText.text = DataManager.Data.GetSfxVolumeLevel().ToString() + "%";
                // }
            }
            
            int currentLanguage = PlayerPrefs.GetInt("language", 0); // Default is 0 (Korean)
            if (_KoreanToggle && _EnglishToggle)
            {
                if (currentLanguage == 0) // Korean
                {
                    _KoreanToggle.isOn = true;
                    _EnglishToggle.isOn = false;
                }
                else if (currentLanguage == 1) // English
                {
                    _KoreanToggle.isOn = false;
                    _EnglishToggle.isOn = true;
                }
                Debug.Log($"Language toggles updated - Current language: {(currentLanguage == 0 ? "Korean" : "English")}");
            }
        }
    }
    
    /// <summary>
    /// Close the settings popup
    /// </summary>
    private void CloseSettingsPopup()
    {
        if (_settingsPopupUI)
        {
            _settingsPopupUI.SetActive(false);
            _isSettingsPopupActive = false;
            Debug.Log("Settings popup closed");
        }
    }
    
    /// <summary>
    /// Open the quit confirmation popup
    /// </summary>
    private void OpenQuitPopup()
    {
        if (_quitPopupUI)
        {
            _quitPopupUI.SetActive(true);
            _isQuitPopupActive = true;
            Debug.Log("Quit popup opened");
        }
    }
    
    /// <summary>
    /// Close the quit confirmation popup
    /// </summary>
    private void CloseQuitPopup()
    {
        if (_quitPopupUI)
        {
            _quitPopupUI.SetActive(false);
            _isQuitPopupActive = false;
            Debug.Log("Quit popup closed");
        }
    }
    
    /// <summary>
    /// Handle main back button click - initiates game quit sequence
    /// </summary>
    private void OnMainBackButtonClicked()
    {
        Debug.Log("=== OnMainBackButtonClicked START ===");
        
        // Check if already quitting game
        if (_isQuittingGame)
        {
            Debug.Log("Already quitting game, ignoring click");
            return;
        }
            
        Debug.Log("Main back button clicked - Quitting game");
        
        // Set game quit flag (ensure it only executes once)
        _isQuittingGame = true;
        
        // Disable Input Actions (before game quit)
        DisableAllInputActions();
        
        Debug.Log("Attempting to quit game...");
        
        // Quit game after short delay (give Input System time to clean up)
        StartCoroutine(QuitGameCoroutine());
        
        Debug.Log("=== OnMainBackButtonClicked END ===");
    }
    
    /// <summary>
    /// Coroutine to handle game quit with delay
    /// </summary>
    /// <returns>Coroutine enumerator</returns>
    private System.Collections.IEnumerator QuitGameCoroutine()
    {
        // Wait 0.1 seconds (Input System cleanup time)
        yield return new WaitForSeconds(0.1f);
        
        // Quit game
        #if UNITY_EDITOR
            Debug.Log("Stopping play mode in editor");
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Debug.Log("Calling Application.Quit()");
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Disable all input actions and unsubscribe from events
    /// </summary>
    private void DisableAllInputActions()
    {
        Debug.Log("Disabling all input actions...");
        
        if (_aButton != null)
        {
            _aButton.performed -= OnAButtonPressed;
            _aButton.Disable();
        }
        if (_yButton != null)
        {
            _yButton.performed -= OnYButtonPressed;
            _yButton.Disable();
        }
        
        Debug.Log("All input actions disabled");
    }
    
    /// <summary>
    /// Set canvas state between opening and prologue
    /// </summary>
    /// <param name="isPrologueActive">True for prologue canvas, false for opening canvas</param>
    public void SetPrologueActive(bool isPrologueActive)
    {
        _isPrologueActive = isPrologueActive;
        _openingCanvas.enabled = !_isPrologueActive;
        _prologueCanvas.enabled = _isPrologueActive;
    }
    
    /// <summary>
    /// Clean up event listeners and input actions when destroyed
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe slider event listeners
        if (_bgmSlider && _bgmSliderListenerRegistered)
        {
            _bgmSlider.onValueChanged.RemoveListener(OnBgmSliderValueChanged);
        }
        
        if (_sfxSlider && _sfxSliderListenerRegistered)
        {
            _sfxSlider.onValueChanged.RemoveListener(OnSfxSliderValueChanged);
        }
        
        if (_KoreanToggle && _koreanToggleListenerRegistered)
        {
            _KoreanToggle.onValueChanged.RemoveListener(OnLanguageToggleChanged);
        }
    
        if (_EnglishToggle && _englishToggleListenerRegistered)
        {
            _EnglishToggle.onValueChanged.RemoveListener(OnLanguageToggleChanged);
        }
        
        // Prevent duplicate unsubscription since DisableAllInputActions already handled it
        if (!_isQuittingGame)
        {
            if (_aButton != null) 
            {
                _aButton.performed -= OnAButtonPressed;
            }
            if (_yButton != null)
            {
                _yButton.performed -= OnYButtonPressed;
            }
        }
    }
}