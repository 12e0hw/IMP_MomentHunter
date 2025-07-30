using UnityEngine;

public class LanguageSelector : MonoBehaviour
{
    public void SetKorean()
    {
        PlayerPrefs.SetInt("language", 0); // 0 = 한국어
        PlayerPrefs.Save();
    }

    public void SetEnglish()
    {
        PlayerPrefs.SetInt("language", 1); // 1 = 영어
        PlayerPrefs.Save();
    }
}

