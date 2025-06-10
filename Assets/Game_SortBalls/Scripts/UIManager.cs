using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public string HighestLevelString = "STBHighestLevel";
    public static UIManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public TMP_Text LevelText;
    void OnEnable()
    {
        if(PlayerPrefs.HasKey(HighestLevelString))
        {
            int highestLevel = int.Parse(PlayerPrefs.GetString(HighestLevelString));
            LevelText.text = "Level " + (highestLevel + 1).ToString();
        }
        else
        {
            LevelText.text = "Level 1";
        }
    }
}
