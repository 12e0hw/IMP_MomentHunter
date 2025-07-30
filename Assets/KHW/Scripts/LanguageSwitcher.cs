using UnityEngine;

public class UILanguageSwitcher : MonoBehaviour
{
    public GameObject[] koreanUI; // 한글 UI 묶음
    public GameObject[] englishUI; // 영어 UI 묶음

    void Start()
    {
        int lang = PlayerPrefs.GetInt("language", 0); // 기본: 한국어

        bool isEnglish = (lang == 1);

        foreach (GameObject obj in koreanUI)
            obj.SetActive(!isEnglish);

        foreach (GameObject obj in englishUI)
            obj.SetActive(isEnglish);
    }
}
