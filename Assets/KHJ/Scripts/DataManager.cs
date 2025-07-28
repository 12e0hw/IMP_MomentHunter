using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages persistent game data including player health, volume settings, and UI updates.
/// Implements singleton pattern to maintain data across scene transitions.
/// </summary>
public class DataManager : MonoBehaviour
{
    /// <summary>
    /// Static instance for accessing DataManager from anywhere in the game
    /// </summary>
    public static DataManager Data { get; private set; }
    
    [Header("Player Information")]
    private const int MAX_HEALTH = 8;
    /// <summary>
    /// Maximum health value for the player
    /// </summary>
    public int MaxHealth => MAX_HEALTH;
    [SerializeField] private int _currentHealth = MAX_HEALTH;
    /// <summary>
    /// Current player health value
    /// </summary>
    public int CurrentHealth => _currentHealth;

    [Header("Game Settings")]
    [SerializeField] [Range(0, 100)] private int _bgmVolumeLevel = 80; // BGM volume range 0~100
    [SerializeField] [Range(0, 100)] private int _sfxVolumeLevel = 80; // SFX volume range 0~100
    /// <summary>
    /// BGM volume as float (0-1) for audio systems
    /// </summary>
    private float _bgmVolume => _bgmVolumeLevel / 100f; // Convert to 0~1 range
    /// <summary>
    /// SFX volume as float (0-1) for audio systems
    /// </summary>
    private float _sfxVolume => _sfxVolumeLevel / 100f; // Convert to 0~1 range

    [Header("UI Components")]
    [SerializeField] private MissionText _missionText;

    /// <summary>
    /// Event triggered when BGM volume changes, passes the new volume as float (0-1)
    /// </summary>
    public static event Action<float> OnBgmVolumeChanged;
    
    /// <summary>
    /// Event triggered when SFX volume changes, passes the new volume as float (0-1)
    /// </summary>
    public static event Action<float> OnSfxVolumeChanged;
    
    /// <summary>
    /// Initialize singleton instance and set up scene loading events
    /// </summary>
    private void Awake()
    {
        // Implement singleton pattern
        if (!Data)
        {
            Data = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene load events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }
 
    /// <summary>
    /// Initialize UI components and update displays
    /// </summary>
    private void Start()
    {
        // Only execute if this is the active Data instance
        if (Data != this) return;
        
        FindMissionText();
        UpdateHealthUI();
    }
    
    /// <summary>
    /// Called when a new scene is loaded - refreshes UI references and broadcasts current volume
    /// </summary>
    /// <param name="scene">The loaded scene</param>
    /// <param name="mode">The scene load mode</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Refresh UI component references for the new scene
        FindMissionText();
        UpdateHealthUI();
        
        // Broadcast current volumes to all AudioManagers in the new scene
        OnBgmVolumeChanged?.Invoke(_bgmVolume);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }
    
    /// <summary>
    /// Finds and caches the MissionText component in the current scene
    /// </summary>
    private void FindMissionText()
    {
        _missionText = FindFirstObjectByType<MissionText>();
        
        if (!_missionText)
        {
            Debug.LogWarning($"DataManager: MissionText component not found in scene '{SceneManager.GetActiveScene().name}'.");
        }
    }

    /// <summary>
    /// Called when values are changed in the editor - validates ranges and updates UI in real-time
    /// </summary>
    private void OnValidate()
    {
        int previousBgmVolumeLevel = _bgmVolumeLevel;
        int previousSfxVolumeLevel = _sfxVolumeLevel;
        
        // Clamp health to valid range
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MAX_HEALTH);

        // Clamp volumes to valid range (0~100)
        _bgmVolumeLevel = Mathf.Clamp(_bgmVolumeLevel, 0, 100);
        _sfxVolumeLevel = Mathf.Clamp(_sfxVolumeLevel, 0, 100);
        
        // Only update UI during runtime
        if (Application.isPlaying)
        {
            UpdateHealthUI();
            
            // Trigger volume change events if volumes were modified
            if (previousBgmVolumeLevel != _bgmVolumeLevel)
            {
                OnBgmVolumeChanged?.Invoke(_bgmVolume);
                Debug.Log($"BGM volume set to {_bgmVolumeLevel}%. (Float: {_bgmVolume:F2})");
            }
            
            if (previousSfxVolumeLevel != _sfxVolumeLevel)
            {
                OnSfxVolumeChanged?.Invoke(_sfxVolume);
                Debug.Log($"SFX volume set to {_sfxVolumeLevel}%. (Float: {_sfxVolume:F2})");
            }
        }
    }

    /// <summary>
    /// Updates the health display in the UI
    /// </summary>
    private void UpdateHealthUI()
    {
        // Find and update MissionText component with current health
        if (_missionText)
        {
            _missionText.UpdateHealthText(_currentHealth);
        }
    }
    
    /// <summary>
    /// Clean up event subscriptions when the object is destroyed
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (Data == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            OnBgmVolumeChanged = null; // Clear all event subscribers
            OnSfxVolumeChanged = null; // Clear all event subscribers
        }
    }

    /// <summary>
    /// Decreases player health by 1. Triggers game over if health reaches 0.
    /// </summary>
    public void UseHealth()
    {
        if (_currentHealth > 0)
        {
            --_currentHealth;
            UpdateHealthUI();
        }
        else
        {
            // Trigger defeat state when health is depleted
            if (GameManager.Instance) GameManager.Instance.SetGameState(GameState.Defeat);
        }
    }
    
    /// <summary>
    /// Sets the BGM volume level and notifies all audio systems
    /// </summary>
    /// <param name="volumeLevel">Volume level as integer (0-100)</param>
    public void SetBgmVolume(int volumeLevel)
    {
        int newVolumeLevel = Mathf.Clamp(volumeLevel, 0, 100);

        // Only trigger event if volume actually changed
        if (_bgmVolumeLevel != newVolumeLevel)
        {
            _bgmVolumeLevel = newVolumeLevel;

            // Notify all AudioManagers of volume change (convert to 0~1 range)
            OnBgmVolumeChanged?.Invoke(_bgmVolume);

            Debug.Log($"BGM volume set to {_bgmVolumeLevel}%. (Float: {_bgmVolume:F2})");
        }
    }
    
    /// <summary>
    /// Sets the SFX volume level and notifies all audio systems
    /// </summary>
    /// <param name="volumeLevel">Volume level as integer (0-100)</param>
    public void SetSfxVolume(int volumeLevel)
    {
        int newVolumeLevel = Mathf.Clamp(volumeLevel, 0, 100);

        // Only trigger event if volume actually changed
        if (_sfxVolumeLevel != newVolumeLevel)
        {
            _sfxVolumeLevel = newVolumeLevel;

            // Notify all AudioManagers of volume change (convert to 0~1 range)
            OnSfxVolumeChanged?.Invoke(_sfxVolume);

            Debug.Log($"SFX volume set to {_sfxVolumeLevel}%. (Float: {_sfxVolume:F2})");
        }
    }

    /// <summary>
    /// Gets the current BGM volume as float (0-1)
    /// </summary>
    /// <returns>BGM volume as float between 0 and 1</returns>
    public float GetBgmVolume()
    {
        return _bgmVolume;
    }
    
    /// <summary>
    /// Gets the current SFX volume as float (0-1)
    /// </summary>
    /// <returns>SFX volume as float between 0 and 1</returns>
    public float GetSfxVolume()
    {
        return _sfxVolume;
    }
    
    /// <summary>
    /// Gets the current BGM volume level as integer (0-100)
    /// </summary>
    /// <returns>BGM volume level as integer between 0 and 100</returns>
    public int GetBgmVolumeLevel()
    {
        return _bgmVolumeLevel;
    }
    
    /// <summary>
    /// Gets the current SFX volume level as integer (0-100)
    /// </summary>
    /// <returns>SFX volume level as integer between 0 and 100</returns>
    public int GetSfxVolumeLevel()
    {
        return _sfxVolumeLevel;
    }

    /// <summary>
    /// Resets player health to maximum value
    /// </summary>
    public void InitializeHealth()
    {
        _currentHealth = MAX_HEALTH;
        UpdateHealthUI(); // Update UI after resetting health
    }
}