using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.MultiplayerModels;
using TMPro;
using UnityEngine;

public class HomeUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text[] _gemsTexts;
    [SerializeField] private TMP_Text _highscoreText;
    [SerializeField] private UIBlock MaxUIBlock;

    void OnEnable()
    {
        PlayfabManager.OnCurrencyChange += OnCurrencyChange;
    }
    void Start()
    {
        UpdateHighscoreUI();
        UpdateMaxBlockUI();
    }

    private void OnCurrencyChange(int amount)
    {
        foreach(var gemtext in _gemsTexts)
        {
            gemtext.text = amount.ToString();
        }
    }
    void UpdateMaxBlockUI()
    {
        double.TryParse(PlayerPrefs.GetString("x2HighestBlock", "2"), out double highestBlock);
        MaxUIBlock.GetComponent<UIBlock>().BlockValue = highestBlock;
    }
    private void UpdateHighscoreUI()
    {
        int score = PlayerPrefs.GetInt("x2BlocksHighscore");
        _highscoreText.text = ConvertToSuffixedString(score);
    }

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
