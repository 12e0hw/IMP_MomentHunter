using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Defines the overall game states
/// </summary>
public enum GameState
{
    Intro,
    Room1,
    Room2,
    Victory,
    Defeat
}

/// <summary>
/// Defines the mission progression states
/// </summary>
public enum MissionState
{
    None,
    Mission1,
    Mission2,
    Mission3,
    Mission4,
    Mission5,
    Mission6,
    Ending
}

/// <summary>
/// Central game manager that controls game flow, mission progression, and scene transitions.
/// Implements singleton pattern for global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access
    /// </summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// Event triggered when mission state changes
    /// </summary>
    public static event Action<MissionState> OnMissionStateChanged;

    [SerializeField] private GameState _gameState = GameState.Intro;
    /// <summary>
    /// Current game state
    /// </summary>
    public GameState GameState => _gameState;

    [SerializeField] private MissionState _missionState = MissionState.None;
    /// <summary>
    /// Current mission state
    /// </summary>
    public MissionState MissionState => _missionState;

    [Header("UI Components")]
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private MissionText _missionText;
    [SerializeField] private OpeningUIManager _openingUIManager;
    
    [Header("Fail UI Components")]
    [SerializeField] private Canvas _failCanvas;
    [SerializeField] private GameObject _failUI_Korean;
    [SerializeField] private GameObject _failUI_English;
    
    [Header("Mission5 Components")]
    [SerializeField] private Letter _mission5Letter;
    [SerializeField] private TransformAnimator _mission5Animator;
    
    [Header("Fade System")]
    [SerializeField] private Animation _fadeUI;
    [SerializeField] private string _fadeOutClipName = "A_FadeOut";
    [SerializeField] private string _fadeInClipName = "A_FadeIn";

    [Header("Audio Components")]
    [SerializeField] public AudioManager AudioManager;

    [Header("UI Settings")]
    [SerializeField] private bool _isMainCanvasActive = true;

    [Header("Scene Configuration")]
#if UNITY_EDITOR
    [SerializeField] private List<SceneAsset> _sceneAssets = new List<SceneAsset>(); // Scene asset list for editor
#endif
    [SerializeField] private List<string> _sceneNames = new List<string>(); // Scene name list for build

    [Header("Mission Object Settings")]
    [SerializeField, HideInInspector] private List<int> _missionObjectCounts = new List<int>(); // Hidden from inspector
    /// <summary>
    /// Read-only array defining required mission object counts for each mission
    /// </summary>
    private readonly int[] missionObjectCountsReadOnly = { 0, 2, 6, 2, 2, 1, 1, 0 }; // Read-only array

    // Inspector display for verification (non-editable)
    [Space]
    [Header("Mission Object Counts (Read Only)")]
    [SerializeField] private string[] _missionObjectDisplay = new string[0];
    /// <summary>
    /// Current count of captured mission objects
    /// </summary>
    private int _currentMissionObjectCount = 0;

    // Flags for scene initialization
    private bool _shouldInitializeScene0Load = false;
    private bool _isDead = false;

    /// <summary>
    /// Initialize the AudioManager instance and set up DontDestroyOnLoad if enabled
    /// </summary>
    void Awake()
    {
        // Implement singleton pattern
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Copy read-only array to list for runtime use
        _missionObjectCounts = new List<int>(missionObjectCountsReadOnly);
    }

    /// <summary>
    /// Initialize UI components and handle victory state transitions
    /// </summary>
    void Start()
    {
        SetMainCanvasActive(true);
        SetFailUIActive(false);

        // Handle victory state transition with delay
        if (_gameState == GameState.Victory) StartCoroutine(TransitionToSceneWithDelay(0, 10f));
        
        if(_missionText) _missionText.ActivateHintObject(0);
    }

    /// <summary>
    /// Clean up event subscriptions when destroyed
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when a scene finishes loading
    /// </summary>
    /// <param name="scene">The loaded scene</param>
    /// <param name="mode">The scene load mode</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Initialize Scene 0 if flag is set
        if (_shouldInitializeScene0Load && scene.name == _sceneNames[0])
        {
            _shouldInitializeScene0Load = false;
            Initialize();
        }

        // 씬 시작 시 무조건 페이드인으로 시작
        StartCoroutine(FadeInOnSceneStart());
    }

    /// <summary>
    /// Changes the current game state and handles state-specific logic
    /// </summary>
    /// <param name="newGameState">The new game state to set</param>
    public void SetGameState(GameState newGameState)
    {
        // Avoid redundant state changes
        if (_gameState == newGameState)
            return;

        this._gameState = newGameState;

        // Handle state-specific logic
        switch (_gameState)
        {
            case GameState.Victory:
                // Transition to ending scene after delay
                StartCoroutine(TransitionToSceneWithDelay(3, 3f));
                break;
            case GameState.Defeat:
                _isDead = true;
                Debug.Log("Defeat");
                SetFailUIActive(true);
                if (AudioManager) AudioManager.PlayAudio(1);
                SetMissionState(MissionState.Ending);

                // Return to main scene after delay
                StartCoroutine(TransitionToSceneWithDelay(0, 6f));
                break;
            default:
                break;
        }
    }
    
    /// <summary>
    /// Sets the fail UI active/inactive based on current language
    /// 현재 언어에 맞는 Fail UI 활성화/비활성화
    /// </summary>
    /// <param name="isActive">Whether to show or hide the fail UI</param>
    private void SetFailUIActive(bool isActive)
    {
        if (!_failCanvas) return;

        _failCanvas.gameObject.SetActive(isActive);

        if (isActive)
        {
            bool isEnglish = LanguageSwitcher.IsEnglish;
            
            // 한국어 버전 처리
            if (_failUI_Korean)
                _failUI_Korean.SetActive(!isEnglish);
            
            // 영어 버전 처리
            if (_failUI_English)
                _failUI_English.SetActive(isEnglish);
        }
        else
        {
            // Fail UI 비활성화 시 모든 자식 UI도 비활성화
            if (_failUI_Korean) _failUI_Korean.SetActive(false);
            if (_failUI_English) _failUI_English.SetActive(false);
        }
    }

    /// <summary>
    /// Coroutine to transition to a scene after a specified delay
    /// </summary>
    /// <param name="sceneIndex">Index of the target scene</param>
    /// <param name="delay">Delay in seconds before transition</param>
    /// <returns>Coroutine enumerator</returns>
    private System.Collections.IEnumerator TransitionToSceneWithDelay(int sceneIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        yield return StartCoroutine(TransitionToSceneWithFadeCoroutine(sceneIndex));
    }

    /// <summary>
    /// Changes the current mission state and handles mission-specific logic
    /// </summary>
    /// <param name="newMissionState">The new mission state to set</param>
    public static event Action OnMissionSuccess;
    public void SetMissionState(MissionState newMissionState)
    {
        // Avoid redundant state changes
        if (_missionState == newMissionState)
            return;

        this._missionState = newMissionState;

        // Reset mission object count when mission state changes
        ResetMissionObjectCount();

        // Notify listeners of mission state change
        OnMissionStateChanged?.Invoke(_missionState);

        // Update mission text UI
        if (_missionText) _missionText.UpdateMissionText();

        // Handle mission-specific logic
        switch (_missionState)
        {
            case MissionState.None:
                _isDead = false;
                TransitionToScene(0);
                SetGameState(GameState.Intro);
                break;
            case MissionState.Mission1:
                TransitionToScene(1);
                SetGameState(GameState.Room1);
                break;
            case MissionState.Mission2:
                _missionText.ActivateHintObject(1);
                break;
            case MissionState.Mission3:
                _missionText.ActivateHintObject(2);
                break;
            case MissionState.Mission4:
                // Transition to Room2 with delay
                StartCoroutine(TransitionToSceneWithDelay(2, 3f));
                SetGameState(GameState.Room2);
                break;
            case MissionState.Mission5:
                _missionText.ActivateHintObject(1);
                break;
            case MissionState.Mission6:
                if (_mission5Letter) _mission5Letter.ShowLetter();
                if (_mission5Animator) _mission5Animator.AnimateTransform();
                _missionText.ActivateHintObject(2);
                break;
            case MissionState.Ending:
                // Only set victory if player is not dead
                if (!_isDead) SetGameState(GameState.Victory);
                break;
        }
    }

    /// <summary>
    /// Advances to the next mission state in sequence
    /// </summary>
    public void SetNextMissionState()
    {
        int nextIndex = ((int)_missionState + 1) % System.Enum.GetValues(typeof(MissionState)).Length;
        SetMissionState((MissionState)nextIndex);
    }

    /// <summary>
    /// Updates the mission object count and checks for mission completion
    /// </summary>
    /// <param name="capturedCount">Number of mission objects captured</param>
    public void SetMissionObjectCount(int capturedCount)
    {
        _currentMissionObjectCount = capturedCount;

        int currentMissionIndex = (int)_missionState;

        // Validate mission index range
        if (currentMissionIndex >= 0 && currentMissionIndex < _missionObjectCounts.Count)
        {
            int requiredCount = _missionObjectCounts[currentMissionIndex];

            Debug.Log($"Mission object count updated: {_currentMissionObjectCount}/{requiredCount}");

            // Check if mission is completed
            if (_currentMissionObjectCount == requiredCount)
            {
                Debug.Log("Mission Clear!");
                int feedbackIndex = GetMissionFeedbackIndex(_missionState);
                _missionText.ActivateFeedbackObject(feedbackIndex);
                OnMissionSuccess?.Invoke();
                // Decrease health
                if (DataManager.Data) DataManager.Data.UseHealth();
                SetNextMissionState();
            }
            else
            {
                Debug.Log("Mission Failed!");
                _missionText.ActivateFeedbackObject(0);
                // Decrease health
                if (DataManager.Data) DataManager.Data.UseHealth();
            }
        }
        else
        {
            Debug.LogWarning($"Mission index out of range: {currentMissionIndex}");
        }
    }

    /// <summary>
    /// Returns the appropriate feedback index for the given mission state
    /// </summary>
    /// <param name="missionState">The mission state to get feedback index for</param>
    /// <returns>Feedback index for UI display</returns>
    private int GetMissionFeedbackIndex(MissionState missionState)
    {
        return missionState switch
        {
            MissionState.Mission1 => 1,
            MissionState.Mission2 => 2,
            MissionState.Mission3 => 3,
            MissionState.Mission4 => 4,
            MissionState.Mission5 => 5,
            MissionState.Mission6 => 6,
            _ => 0 // Default value (for failure)
        };
    }

    /// <summary>
    /// Resets the current mission object count to zero
    /// </summary>
    private void ResetMissionObjectCount()
    {
        _currentMissionObjectCount = 0;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Called when values are changed in the editor - syncs scene assets with scene names
    /// </summary>
    private void OnValidate()
    {
        // Synchronize sceneNames list with sceneAssets
        _sceneNames.Clear();
        foreach (var sceneAsset in _sceneAssets)
        {
            if (sceneAsset)
            {
                _sceneNames.Add(sceneAsset.name);
            }
        }

        // Update inspector display array for mission object counts
        var missionNames = System.Enum.GetNames(typeof(MissionState));
        _missionObjectDisplay = new string[missionObjectCountsReadOnly.Length];
        for (int i = 0; i < missionObjectCountsReadOnly.Length && i < missionNames.Length; i++)
        {
            _missionObjectDisplay[i] = $"{missionNames[i]}: {missionObjectCountsReadOnly[i]}";
        }

        // Update main canvas state in editor
        if (_mainCanvas)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (_mainCanvas)
                    _mainCanvas.enabled = _isMainCanvasActive;
            };
        }
    }
#endif

    /// <summary>
    /// Transitions to the specified scene by index (페이드와 함께)
    /// </summary>
    /// <param name="sceneIndex">Index of the target scene in the scene list</param>
    public void TransitionToScene(int sceneIndex)
    {
        StartCoroutine(TransitionToSceneWithFadeCoroutine(sceneIndex));
    }

    /// <summary>
    /// 페이드아웃 → 씬 전환 → 새 씬에서 페이드인
    /// </summary>
    private System.Collections.IEnumerator TransitionToSceneWithFadeCoroutine(int sceneIndex)
    {
        // 페이드아웃
        yield return StartCoroutine(FadeOut());

        // Validate scene index
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}. Available scenes: {_sceneNames.Count}");
            yield break;
        }

        // Special handling for Scene 0 (main scene)
        if (sceneIndex == 0)
        {
            _shouldInitializeScene0Load = true;
            Initialize();

            // Destroy persistent AudioManager when returning to Scene 0
            if (AudioManager.BGMInstance)
            {
                Debug.Log("Destroying DontDestroyOnLoad AudioManager when returning to Scene 0");
                Destroy(AudioManager.BGMInstance.gameObject);
            }
        }

        // 씬 전환
        string targetScene = _sceneNames[sceneIndex];
        SceneManager.LoadScene(targetScene);
    }

    /// <summary>
    /// Sets the main canvas active state
    /// </summary>
    /// <param name="isActive">Whether the main canvas should be active</param>
    public void SetMainCanvasActive(bool isActive)
    {
        _isMainCanvasActive = isActive;
        _mainCanvas.enabled = _isMainCanvasActive;
    }

    /// <summary>
    /// Initializes game data when returning to the main scene
    /// </summary>
    private void Initialize()
    {
        // Reset player health through DataManager
        if (DataManager.Data) DataManager.Data.InitializeHealth();

        // SetGameState(GameState.Intro);
        // SetMissionState(MissionState.None);
        // SetMainCanvasActive(true);
        // if (_openingUIManager) _openingUIManager.SetPrologueActive(false);
    }

    // ---- 정답미션 사진저장 확인용
    public int GetCurrentMissionObjectCount() => _currentMissionObjectCount;
    public int GetMissionObjectRequirement(int missionIndex)
    {
        if (missionIndex >= 0 && missionIndex < _missionObjectCounts.Count)
            return _missionObjectCounts[missionIndex];
        else return -1;
    }

    // ===== FADE SYSTEM =====

    /// <summary>
    /// 씬 시작 시 페이드인
    /// </summary>
    private System.Collections.IEnumerator FadeInOnSceneStart()
    {
        yield return new WaitForSeconds(0.1f); // 씬 로드 완료 대기
        yield return StartCoroutine(FadeIn());
    }

    /// <summary>
    /// 같은 씬 내에서 페이드 전환 (프롤로그 등)
    /// </summary>
    /// <param name="action">페이드 중간에 실행할 액션</param>
    public void StartFadeTransition(System.Action action)
    {
        StartCoroutine(FadeTransition(action));
    }

    /// <summary>
    /// 페이드아웃 → 액션 실행 → 페이드인 (같은 씬 내)
    /// </summary>
    private System.Collections.IEnumerator FadeTransition(System.Action action)
    {
        // 페이드아웃
        yield return StartCoroutine(FadeOut());

        // 액션 실행 (화면 전환 등)
        action?.Invoke();

        // 잠시 대기
        yield return new WaitForSeconds(0.1f);

        // 페이드인
        yield return StartCoroutine(FadeIn());
    }

    /// <summary>
    /// 페이드아웃 후 씬 로드 (씬 간 전환)
    /// </summary>
    private System.Collections.IEnumerator FadeOutAndLoadScene(int sceneIndex)
    {
        // 페이드아웃
        yield return StartCoroutine(FadeOut());

        // Validate scene index
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}. Available scenes: {_sceneNames.Count}");
            yield break;
        }

        // Special handling for Scene 0 (main scene)
        if (sceneIndex == 0)
        {
            _shouldInitializeScene0Load = true;
            Initialize();

            // Destroy persistent AudioManager when returning to Scene 0
            if (AudioManager.BGMInstance)
            {
                Debug.Log("Destroying DontDestroyOnLoad AudioManager when returning to Scene 0");
                Destroy(AudioManager.BGMInstance.gameObject);
            }
        }

        // 씬 전환 (새 씬에서 자동으로 페이드인 됨)
        string targetScene = _sceneNames[sceneIndex];
        SceneManager.LoadScene(targetScene);
    }

    /// <summary>
    /// 페이드아웃 애니메이션 재생
    /// </summary>
    private System.Collections.IEnumerator FadeOut()
    {
        if (!_fadeUI || !_fadeUI.GetClip(_fadeOutClipName))
        {
            Debug.LogWarning("FadeOut animation not found!");
            yield break;
        }

        _fadeUI.Play(_fadeOutClipName);
        yield return new WaitForSeconds(_fadeUI.GetClip(_fadeOutClipName).length);
    }

    /// <summary>
    /// 페이드인 애니메이션 재생
    /// </summary>
    private System.Collections.IEnumerator FadeIn()
    {
        if (!_fadeUI || !_fadeUI.GetClip(_fadeInClipName))
        {
            Debug.LogWarning("FadeIn animation not found!");
            yield break;
        }

        _fadeUI.Play(_fadeInClipName);
        yield return new WaitForSeconds(_fadeUI.GetClip(_fadeInClipName).length);
    }
}