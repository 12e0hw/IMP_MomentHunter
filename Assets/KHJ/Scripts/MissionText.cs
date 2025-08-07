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
    
    [Header("Mission UI Objects")]
    [SerializeField] private GameObject _missionBG;          // MissionBG object
    [SerializeField] private GameObject _pin;                // Pin object
    [SerializeField] private GameObject _missionTitle;       // MissionTitle object
    [SerializeField] private GameObject _missionContents;    // MissionContents object
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
    
    /// <summary>
    /// Initialize mission UI and health display on start
    /// </summary>
    private void Start()
    {
        UpdateMissionText();

        if (!DataManager.Data) return;
        UpdateHealthText(DataManager.Data.CurrentHealth);
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
            SetMissionUIActive(false);
        }
        else
        {
            SetMissionUIActive(true);
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
        
        var targetArray = LanguageSwitcher.IsEnglish ? _missionContent_E : _missionContent_K;

        for (int i = 0; i < targetArray.Length; i++)
        {
            if (targetArray[i])
                targetArray[i].SetActive(i == index);
        }
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
        
        var targetArray = LanguageSwitcher.IsEnglish ? _feedback_E : _feedback_K;

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
    private void SetMissionUIActive(bool isActive)
    {
        if (_missionBG) _missionBG.SetActive(isActive);
        if (_pin) _pin.SetActive(isActive);
        if (_missionTitle) _missionTitle.SetActive(isActive);
        if (_missionContents) _missionContents.SetActive(isActive);
    }
    
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