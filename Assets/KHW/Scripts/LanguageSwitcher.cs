
using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    // 항상 켜져 있어야 하는 UI만
    public GameObject[] koreanUI; 
    public GameObject[] englishUI;

    public static bool IsEnglish { get; private set; }  // 현재 언어 상태 기억

    public void ApplyLanguage(int lang)
    {

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
    }

    void Start()
    {
        int lang = PlayerPrefs.GetInt("language", 0);
        ApplyLanguage(lang);
    }
}
