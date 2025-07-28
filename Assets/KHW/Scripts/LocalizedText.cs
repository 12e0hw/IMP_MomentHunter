using UnityEngine;
using TMPro; 

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    public string key; // ex: "start_button"

    void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        TMP_Text txt = GetComponent<TMP_Text>();
        txt.text = LanguageManager.Instance.GetText(key);
    }
}
