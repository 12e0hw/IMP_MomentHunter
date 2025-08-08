
using UnityEngine;

public class LanguageSelector : MonoBehaviour
{
    public LanguageSwitcher switcher;

    public void SetKorean()
    {
        Debug.Log("SetKorean() 호출");
        switcher.ApplyLanguage(0);
    }

    public void SetEnglish()
    {
        Debug.Log("SetEnglish() 호출");
        switcher.ApplyLanguage(1);
    }
}
