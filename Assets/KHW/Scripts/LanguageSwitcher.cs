using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    public GameObject[] koreanUI; // 한글 UI 묶음
    public GameObject[] englishUI; // 영어 UI 묶음

    public void ApplyLanguage(int lang)
    {
        bool isEnglish = (lang == 1);

        foreach (GameObject obj in koreanUI)
            if (obj != null) obj.SetActive(!isEnglish);

        foreach (GameObject obj in englishUI)
            if (obj != null) obj.SetActive(isEnglish);
    }

    void Start()
    {
        int lang = PlayerPrefs.GetInt("language", 0);
        ApplyLanguage(lang); 
    }


}
