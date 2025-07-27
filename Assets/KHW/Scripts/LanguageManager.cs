using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public enum Language { English, Korean }
    public static LanguageManager Instance;

    public Language currentLanguage = Language.English;

    public Dictionary<string, string> localizedText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSavedLanguage();
        }
    }

    public void LoadLanguage(Language lang)
    {
        currentLanguage = lang;
        localizedText = (lang == Language.English) ? LanguageData.English : LanguageData.Korean;
    }

    public string GetText(string key)
    {
        if (localizedText != null && localizedText.ContainsKey(key))
        {
            return localizedText[key];
        }
        return $"[{key}]";
    }

    private void SaveLanguage()
    {
        PlayerPrefs.SetInt("lang", (int)currentLanguage);
    }

    private void LoadSavedLanguage()
    {
        int savedLang = PlayerPrefs.GetInt("lang", 0); 
        LoadLanguage((Language)savedLang);
    }
}
