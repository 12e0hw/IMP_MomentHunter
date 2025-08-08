using TMPro;
using UnityEngine;

/// <summary>
/// Manages mission-related UI displays including mission text, health display, and feedback objects.
/// Updates UI elements based on current mission state and player health.
/// </summary>
public class MissionText : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _healthText;
    
    // [Header("Mission UI Objects")]
    // [SerializeField] private GameObject _missionBG;          // MissionBG object
    // [SerializeField] private GameObject _pin;                // Pin object
    // [SerializeField] private GameObject _missionTitle;       // MissionTitle object
    // [SerializeField] private GameObject _missionContents;    // MissionContents object
    [SerializeField] private Animation _useHealthAnim;
    
    [Header("Mission GameObject Arrays")]
    // [SerializeField] private GameObject[] _missionTitleObjects;    // Mission title game objects
    // [SerializeField] private GameObject[] _missionContentObjects; // Mission content game objects

    // 미션 설명 UI
    [SerializeField] private GameObject[] _missionContent_K;
    [SerializeField] private GameObject[] _missionContent_E;

    [Header("Feedback GameObject Array")]    // 피드백 UI
    [SerializeField] private GameObject[] _feedback_K;
    [SerializeField] private GameObject[] _feedback_E;
    
    [Header("Hint GameObject Arrays")]
    [SerializeField] private GameObject[] _hint_K;  // 한국어 힌트 배열
    [SerializeField] private GameObject[] _hint_E;  // 영어 힌트 배열
    
    /// <summary>
    /// Currently active hint objects for billboard rotation
    /// </summary>
    private GameObject[] _currentActiveHints;
    
    [Header("Billboard Settings")]
    [SerializeField] private bool _enableHintBillboard = true;  // 힌트 빌보드 활성화 여부
    [SerializeField] private Transform _playerCamera;           // VR 카메라
    
    /// <summary>
    /// Initialize mission UI and health display on start
    /// </summary>
    private void Start()
    {
        UpdateMissionText();

        if (!DataManager.Data) return;
        UpdateHealthText(DataManager.Data.CurrentHealth);
        
        // if (!_playerCamera) Debug.LogError("Player Camera is null");
    }
    
    // /// <summary>
    // /// Update billboard rotation for active hints
    // /// </summary>
    // private void Update()
    // {
    //     if (_enableHintBillboard && _playerCamera && _currentActiveHints != null)
    //     {
    //         UpdateHintBillboard();
    //     }
    // }
    
    /// <summary>
    /// Update billboard rotation for all active hint objects
    /// </summary>
    private void UpdateHintBillboard()
    {
        if (_currentActiveHints == null) return;
        
        foreach (GameObject hint in _currentActiveHints)
        {
            if (hint && hint.activeInHierarchy)
            {
                // 힌트 UI가 항상 플레이어를 바라보도록 회전
                Vector3 directionToPlayer = _playerCamera.position - hint.transform.position;
                
                // Y축 회전만 적용 (수평 회전만)
                directionToPlayer.y = 0;
                
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                    hint.transform.rotation = lookRotation;
                }
            }
        }
    }
    
    /// <summary>
    /// Updates mission text display based on current mission state
    /// 현재 미션 상태에 맞는 미션 텍스트 표시
    /// </summary>
    public void UpdateMissionText()
    {
        if (!GameManager.Instance) return;
        
        MissionState currentState = GameManager.Instance.MissionState;
        
        // Hide mission UI during None and Ending states
        if (currentState == MissionState.None || currentState == MissionState.Ending)
        {
            // SetMissionUIActive(false);
            _currentActiveHints = null; // 빌보드 업데이트 중지
        }
        else
        {
            // SetMissionUIActive(true);
            int index = GetObjectIndex(currentState);
            ActivateMissionObjects(index);
        }
    }

    /// <summary>
    /// Activates/deactivates mission objects by index
    /// </summary>
    /// <param name="index">Index of the mission objects to activate</param>
    public void ActivateMissionObjects(int index)
    {
        // Deactivate all mission title objects
        // if (_missionTitleObjects != null)
        // {
        //     for (int i = 0; i < _missionTitleObjects.Length; i++)
        //     {
        //         if (_missionTitleObjects[i])
        //         {
        //             _missionTitleObjects[i].SetActive(i == index);
        //         }
        //     }
        // }

        // // Deactivate all mission content objects
        // if (_missionContentObjects != null)
        // {
        //     for (int i = 0; i < _missionContentObjects.Length; i++)
        //     {
        //         if (_missionContentObjects[i])
        //         {
        //             _missionContentObjects[i].SetActive(i == index);
        //         }
        //     }
        // }
        
        bool isEnglish = LanguageSwitcher.IsEnglish;
        var targetArray = isEnglish ? _missionContent_E : _missionContent_K;

        for (int i = 0; i < targetArray.Length; i++)
        {
            if (targetArray[i])
                targetArray[i].SetActive(i == index);
        }
    }
    
    /// <summary>
    /// Activates/deactivates hint objects by index
    /// </summary>
    /// <param name="index">Index of the hint object to activate</param>
    public void ActivateHintObject(int index)
    {
        bool isEnglish = LanguageSwitcher.IsEnglish;
        var targetArray = isEnglish ? _hint_E : _hint_K;
        
        for (int i = 0; i < targetArray.Length; i++)
        {
            if (targetArray[i])
                targetArray[i].SetActive(i == index);
        }
        
        // 빌보드를 위한 현재 활성 힌트 배열 업데이트
        _currentActiveHints = targetArray;
    }

    /// <summary>
    /// Activates/deactivates feedback objects by index
    /// </summary>
    /// <param name="index">Index of the feedback object to activate</param>
    public void ActivateFeedbackObject(int index)
    {
        // if (_feedbackObjects == null) return;

        // // Deactivate all feedback objects then activate only the specified index
        // for (int i = 0; i < _feedbackObjects.Length; i++)
        // {
        //     if (_feedbackObjects[i])
        //     {
        //         _feedbackObjects[i].SetActive(i == index);
        //     }
        // }
        
        bool isEnglish = LanguageSwitcher.IsEnglish;
        var targetArray = isEnglish ? _feedback_E : _feedback_K;

        for (int i = 0; i < targetArray.Length; i++)
        {
            if (targetArray[i])
                targetArray[i].SetActive(i == index);
        }
    }
    

    /// <summary>
    /// Sets the entire mission UI active or inactive
    /// 미션 UI 전체 표시 여부 제어
    /// </summary>
    /// <param name="isActive">Whether to activate or deactivate mission UI</param>
    // private void SetMissionUIActive(bool isActive)
    // {
    //     if (_missionBG) _missionBG.SetActive(isActive);
    //     if (_pin) _pin.SetActive(isActive);
    //     if (_missionTitle) _missionTitle.SetActive(isActive);
    //     if (_missionContents) _missionContents.SetActive(isActive);
    // }
    
    /// <summary>
    /// Updates the health text display
    /// </summary>
    /// <param name="health">Current health value (parameter is not used, gets value from DataManager)</param>
    public void UpdateHealthText(int health)
    {
        if (!DataManager.Data) return;

        if (DataManager.Data.CurrentHealth <= 3) _healthText.color = Color.red;
        else _healthText.color = Color.white;
        _healthText.text = DataManager.Data.CurrentHealth + " / "  + DataManager.Data.MaxHealth;
    }

    public void PlayUseHealthAnim()
    {
        _useHealthAnim.Play("A_UseHealth");
    }
    
    /// <summary>
    /// Converts mission state to corresponding object index
    /// </summary>
    /// <param name="missionState">The mission state to convert</param>
    /// <returns>Index corresponding to the mission state</returns>
    private int GetObjectIndex(MissionState missionState)
    {
        return missionState switch
        {
            MissionState.Mission1 => 0,    // First game object
            MissionState.Mission2 => 1,
            MissionState.Mission3 => 2,
            MissionState.Mission4 => 3,
            MissionState.Mission5 => 4,
            MissionState.Mission6 => 5,    // Sixth game object
            _ => 0 // Default value
        };
    }
}