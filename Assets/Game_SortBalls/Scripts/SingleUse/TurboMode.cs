using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurboMode : MonoBehaviour
{
    public Image TurboIcon;


    void Start()
    {
        if (PlayerPrefs.HasKey("TurboModeEnabled"))
        {
            bool isEnabled = PlayerPrefs.GetInt("TurboModeEnabled") == 1;
            SetTurboMode(isEnabled);
        }
        else
        {
            SetTurboMode(false);
        }
    }
    public void OnClickTurboButton()
    {
        bool isEnabled = PlayerPrefs.GetInt("TurboModeEnabled") == 1;
        SetTurboMode(!isEnabled);
    }
    public void SetTurboMode(bool isEnabled)
    {
        if (isEnabled)
        {
            Time.timeScale = 3f; // Set to 2x speed
        }
        else
        {
            Time.timeScale = 1f; // Reset to normal speed
        }
        PlayerPrefs.SetInt("TurboModeEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        TurboIcon.color = isEnabled ? Color.green : Color.white;
    }
}
