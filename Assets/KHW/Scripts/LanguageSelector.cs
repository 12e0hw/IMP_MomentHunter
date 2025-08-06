
using UnityEngine;

public class LanguageSelector : MonoBehaviour
{
    public LanguageSwitcher switcher;

    public void SetKorean()
    {
        Debug.Log("SetKorean() 호출");
        PlayerPrefs.SetInt("language", 0); // 0 = 한국어
        PlayerPrefs.Save();

        switcher.ApplyLanguage(0);
    }

    public void SetEnglish()
    {
        Debug.Log("SetEnglish() 호출");
        PlayerPrefs.SetInt("language", 1); // 1 = 영어
        PlayerPrefs.Save();

        switcher.ApplyLanguage(1);
    }
}
