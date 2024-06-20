using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "Q", "P", "E", "Z", "Y" };
    public GameManager gameManager;


    [Header("Upcoming Blocks")]
    public TMP_Text NextBlockNumberText;
    [Header("Game Over Stuff")]
    public GameObject GameoverPanel;
    void OnEnable()
    {
        gameManager.OnUIUpdate += UpdateUI;
        gameManager.OnGameOver += GameOver;
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
    }
    #endregion

    #region Upcoming Blocks Region
    void UpdateUI()
    {
        NextBlockNumberText.text = ConvertToSuffixedString(gameManager.NextBlockNumber);
    }
    #endregion
    #region Utility Methods
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
