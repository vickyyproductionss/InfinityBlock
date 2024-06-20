using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.MultiplayerModels;
using TMPro;
using UnityEngine;

public class HomeUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text[] _gemsTexts;

    void OnEnable()
    {
        PlayfabManager.OnCurrencyChange += OnCurrencyChange;
    }

    private void OnCurrencyChange(int amount)
    {
        foreach(var gemtext in _gemsTexts)
        {
            gemtext.text = amount.ToString();
        }
    }
}
