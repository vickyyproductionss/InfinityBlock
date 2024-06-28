using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;


    [Header("TotalScore Blocks")]
    public TMP_Text TotalScoreText;
    [Header("Game Over Stuff")]
    public GameObject GameoverPanel;
    void OnEnable()
    {
        gameManager.OnUIUpdate += UpdateUI;
        gameManager.OnGameOver += GameOver;
    }

    private void UpdateUI()
    {
        TotalScoreText.text = ConvertToSuffixedString(gameManager.Score);
    }

    void OnDisable()
    {
        gameManager.OnUIUpdate -= UpdateUI;
        gameManager.OnGameOver -= GameOver;
    }

    #region Game Over Handle
    private void GameOver()
    {
        Debug.Log("Game Over!!!");
        GameoverPanel.SetActive(true);
        PlayerPrefs.DeleteKey("x2HighestBlock");
        PlayerPrefs.DeleteKey("SavedBlocks");
    }
    #endregion

    #region Upcoming Blocks Region
    #endregion
    #region Utility Methods
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "Q", "P", "E", "Z", "Y" };
    public string ConvertToSuffixedString(double num)
    {
        int suffixIndex = 0;

        // Continue to next suffix only if num is 10000 or greater
        while (num >= 10000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }

        // If number is extremely large, handle it using scientific notation
        if (num >= 10000 && suffixIndex == suffixes.Length - 1)
        {

        }
        string newtext = num.ToString("0.#") + suffixes[suffixIndex];
        return newtext;
    }
    #endregion
}
