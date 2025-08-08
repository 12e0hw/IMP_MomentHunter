using UnityEngine;

public class Letter : MonoBehaviour
{
    [SerializeField] private GameObject _letterMesh;
    
    [Header("Letter Array")]
    [SerializeField] private GameObject[] _letterArray;

    void Start()
    {
        _letterMesh.SetActive(false);
    }
    
    public void ShowLetter()
    {
        _letterMesh.SetActive(true);
        
        bool isEnglish = LanguageSwitcher.IsEnglish;
        int targetIndex = isEnglish ? 1 : 0;  // 영어면 1, 한국어면 0
    
        for (int i = 0; i < _letterArray.Length; i++)
        {
            if (_letterArray[i])
                _letterArray[i].SetActive(i == targetIndex);
        }
    }
}
