using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            GameManager.Instance.SetNextMissionState();
        }
        else if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            Debug.Log($"[PlayerPrefs] language: {PlayerPrefs.GetInt("language", -1)}");
        }
    }
}
