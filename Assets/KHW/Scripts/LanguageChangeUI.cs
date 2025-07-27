using UnityEngine;

public class LanguageChangeUI : MonoBehaviour
{
    public void SetEnglish()
    {
        LanguageManager.Instance.LoadLanguage(LanguageManager.Language.English);
        RefreshAllTexts();
    }

    public void SetKorean()
    {
        LanguageManager.Instance.LoadLanguage(LanguageManager.Language.Korean);
        RefreshAllTexts();
    }

    void RefreshAllTexts()
    {
        var allTexts = Object.FindObjectsByType<LocalizedText>(FindObjectsSortMode.None);
        foreach (LocalizedText t in allTexts)
        {
            t.UpdateText();
        }
    }
}
