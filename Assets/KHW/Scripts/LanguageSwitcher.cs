
using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    // 항상 켜져 있어야 하는 UI만
    public GameObject[] koreanUI; 
    public GameObject[] englishUI;

    /// <summary>
    /// 현재 언어가 영어인지 확인 (PlayerPrefs 기반)
    /// </summary>
    public static bool IsEnglish 
    { 
        get { return PlayerPrefs.GetInt("language", 0) == 1; }
    }

    public void ApplyLanguage(int lang)
    {
        // PlayerPrefs에 저장
        PlayerPrefs.SetInt("language", lang);
        PlayerPrefs.Save();
        
        bool isEnglish = (lang == 1);

        foreach (GameObject obj in koreanUI)
        {
            if (obj != null)
            {
                obj.SetActive(!isEnglish);
            }
        }

        foreach (GameObject obj in englishUI)
        {
            if (obj != null)
            {
                obj.SetActive(isEnglish);
            }
        }
        
        Debug.Log($"Language applied: {(isEnglish ? "English" : "Korean")}");
    }

    void Start()
    {
        int lang = PlayerPrefs.GetInt("language", 0);
        ApplyLanguage(lang);
    }
}
