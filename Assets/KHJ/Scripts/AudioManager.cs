using UnityEngine;

/// <summary>
/// Manages audio playback including background music and sound effects.
/// Supports singleton pattern for persistent background music across scenes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Static instance for persistent background music manager across scenes
    /// </summary>
    public static AudioManager BGMInstance { get; private set; }
    
    [Header("Destroy Settings")]
    [SerializeField] private bool _dontDestroyOnLoad = false;
    
    [Header("Audio Components")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _audioClips;
    
    [Header("Auto Play Settings")]
    [SerializeField] private bool _autoPlayOnStart = false;
    [SerializeField] private int _backgroundMusicClipId = 0;
    [SerializeField] private bool _loopBackgroundMusic = true;
    
    [Header("Audio Type")]
    [SerializeField] private bool _isBgmSource = false; // true for BGM, false for SFX
    
    /// <summary>
    /// Initialize the AudioManager instance and set up DontDestroyOnLoad if enabled
    /// </summary>
    void Awake()
    {
        if (_dontDestroyOnLoad)
        {
            // Check if an instance already exists
            if (BGMInstance)
            {
                // Destroy the new duplicate instance if one already exists
                Debug.Log("DontDestroyOnLoad AudioManager already exists. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            // Set as the first and only instance
            BGMInstance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManager set to DontDestroyOnLoad and assigned as BGMInstance");
        }
        else
        {
            // Don't set BGMInstance if DontDestroyOnLoad is disabled
            Debug.Log("AudioManager created without DontDestroyOnLoad");
        }
    }
    
    /// <summary>
    /// Initialize audio components, subscribe to volume events, and start auto-play if enabled
    /// </summary>
    void Start()
    {
        // Get AudioSource component if not assigned in inspector
        if (!_audioSource) _audioSource = GetComponent<AudioSource>();
        
        // Subscribe to appropriate volume change events based on audio type
        if (_isBgmSource)
        {
            DataManager.OnBgmVolumeChanged += OnVolumeChanged;
        }
        else
        {
            DataManager.OnSfxVolumeChanged += OnVolumeChanged;
        }
        
        // Apply current volume immediately
        ApplyCurrentVolume();
        
        // Only play background music if this is the BGM instance and auto-play is enabled
        if (_autoPlayOnStart && BGMInstance == this && _isBgmSource)
        {
            PlayBackgroundMusic();
        }
    }
    
    /// <summary>
    /// Clean up event subscriptions and reset BGMInstance reference when destroyed
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (_isBgmSource)
        {
            DataManager.OnBgmVolumeChanged -= OnVolumeChanged;
        }
        else
        {
            DataManager.OnSfxVolumeChanged -= OnVolumeChanged;
        }
        
        // Reset BGMInstance to null if this was the active instance
        if (BGMInstance == this)
        {
            BGMInstance = null;
        }
    }
    
    /// <summary>
    /// Called when values are changed in the editor - validates settings and applies changes in real-time
    /// </summary>
    private void OnValidate()
    {
        // Clamp background music clip ID to valid range
        if (_audioClips != null && _audioClips.Length > 0)
        {
            _backgroundMusicClipId = Mathf.Clamp(_backgroundMusicClipId, 0, _audioClips.Length - 1);
        }
        
        // Only update during runtime and when AudioSource is available and enabled
        if (Application.isPlaying && IsAudioSourceReady())
        {
            // Apply current volume immediately
            ApplyCurrentVolume();
            
            // Handle auto-play setting changes (only for BGM)
            if (_autoPlayOnStart && _isBgmSource && !_audioSource.isPlaying)
            {
                PlayBackgroundMusic();
            }
            else if (!_autoPlayOnStart && _audioSource.isPlaying)
            {
                StopAudio();
            }
        }
    }
    
    /// <summary>
    /// Event handler for volume changes from DataManager
    /// </summary>
    /// <param name="newVolume">The new volume value</param>
    private void OnVolumeChanged(float newVolume)
    {
        ApplyCurrentVolume();
    }
    
    /// <summary>
    /// Plays the designated background music clip with loop settings
    /// </summary>
    public void PlayBackgroundMusic()
    {
        // Check if AudioSource is ready before attempting to play
        if (!IsAudioSourceReady())
        {
            Debug.LogWarning("AudioSource is not ready for playback");
            return;
        }
        
        // Validate background music clip ID
        if (!IsValidClipId(_backgroundMusicClipId))
        {
            Debug.LogWarning($"Invalid background music clip ID: {_backgroundMusicClipId}");
            return;
        }
        
        // Set up audio source with background music settings
        _audioSource.clip = _audioClips[_backgroundMusicClipId];
        _audioSource.loop = _loopBackgroundMusic;
        
        // Apply appropriate volume based on audio type
        ApplyCurrentVolume();
        
        _audioSource.Play();
        
        Debug.Log($"Background music started: {_audioClips[_backgroundMusicClipId].name}");
    }

    /// <summary>
    /// Plays a specific audio clip by ID (non-looping)
    /// </summary>
    /// <param name="clipId">Index of the audio clip to play</param>
    public void PlayAudio(int clipId)
    {
        // Check if AudioSource is ready before attempting to play
        if (!IsAudioSourceReady())
        {
            Debug.LogWarning("AudioSource is not ready for playback");
            return;
        }
        
        // Safety check for valid clip ID
        if (!IsValidClipId(clipId))
        {
            Debug.LogWarning($"Invalid clip ID: {clipId}");
            return;
        }

        // Set up audio source for one-time playback
        _audioSource.clip = _audioClips[clipId];
        _audioSource.loop = false; // Regular audio clips don't loop
        
        // Apply appropriate volume based on audio type
        ApplyCurrentVolume();
        
        _audioSource.Play();
    }
    
    /// <summary>
    /// Plays an audio clip without changing the AudioSource's main clip
    /// Allows multiple sounds to play simultaneously without interrupting each other
    /// </summary>
    /// <param name="clipId">Index of the audio clip to play as one-shot</param>
    public void PlayOneShot(int clipId)
    {
        // Check if AudioSource is ready before attempting to play
        if (!IsAudioSourceReady())
        {
            Debug.LogWarning("AudioSource is not ready for one-shot playback");
            return;
        }
        
        // Validate clip ID
        if (!IsValidClipId(clipId))
        {
            Debug.LogWarning($"Invalid clip ID: {clipId}");
            return;
        }
        
        // Get current volume and play one-shot
        float volumeScale = GetCurrentVolume();
        _audioSource.PlayOneShot(_audioClips[clipId], volumeScale);
    }
    
    /// <summary>
    /// Stops the currently playing audio
    /// </summary>
    public void StopAudio()
    {
        if (IsAudioSourceReady())
        {
            _audioSource.Stop();
            Debug.Log("Audio stopped");
        }
    }
    
    /// <summary>
    /// Checks if the AudioSource is ready for playback
    /// </summary>
    /// <returns>True if AudioSource is available, enabled, and the GameObject is active</returns>
    private bool IsAudioSourceReady()
    {
        return _audioSource != null && 
               _audioSource.enabled && 
               _audioSource.gameObject.activeInHierarchy;
    }
    
    /// <summary>
    /// Validates if the given clip ID is within bounds and references a valid AudioClip
    /// </summary>
    /// <param name="clipId">The clip ID to validate</param>
    /// <returns>True if the clip ID is valid, false otherwise</returns>
    private bool IsValidClipId(int clipId)
    {
        return _audioClips != null && clipId >= 0 && clipId < _audioClips.Length && _audioClips[clipId];
    }
    
    /// <summary>
    /// Retrieves the appropriate volume from DataManager based on audio type
    /// </summary>
    /// <returns>Volume value, or 1.0 if DataManager is unavailable</returns>
    private float GetCurrentVolume()
    {
        if (DataManager.Data)
        {
            return _isBgmSource ? DataManager.Data.GetBgmVolume() : DataManager.Data.GetSfxVolume();
        }
        
        return 1f; // Return default value 1.0 if DataManager is not available
    }
    
    /// <summary>
    /// Applies the current appropriate volume to the AudioSource
    /// </summary>
    private void ApplyCurrentVolume()
    {
        if (IsAudioSourceReady())
        {
            _audioSource.volume = GetCurrentVolume();
        }
    }
}