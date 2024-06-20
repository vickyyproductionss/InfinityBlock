using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

public class PlayfabManager : MonoBehaviour
{
    public delegate void CurrencyChangeEvent(int amount);
    public static event CurrencyChangeEvent OnCurrencyChange;

    /// <summary>
    /// Player Login Status
    /// </summary>
    private bool _isPlayerLoggedIn;
    public bool IsPlayerLoggedIn
    {
        get { return _isPlayerLoggedIn;}
        set { _isPlayerLoggedIn = value;}
    }

    /// <summary>
    /// Player PlayfabID
    /// </summary>
    private string _playerID;
    public string PlayerID
    {
        get { return _playerID;}
        set { _playerID = value; }
    }

    //Currency Name On Server
    [SerializeField] private string _currencyCode = "GM";

    public GameObject NamePanel;
    void Start()
    {
        Login();
    }
    void Login()
    {
        var request = new LoginWithCustomIDRequest 
        { 
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true 
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.Log("Login Failed!!!");
        IsPlayerLoggedIn = false;
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Success!!!");
        IsPlayerLoggedIn = true;
        PlayerID = result.PlayFabId;
        GetVirtualCurrencyBalance();
        if(result.NewlyCreated)
        {
            NamePanel.SetActive(true);
        }
    }

    public void ChangePlayerName(TMP_InputField nameinput)
    {
        if(string.IsNullOrEmpty(nameinput.text))
        {
            return;
        }
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nameinput.text,
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnNameSuccess, OnNameFailure);
    }

    private void OnNameFailure(PlayFabError error)
    {
        Debug.Log("Name change Failure");
    }

    private void OnNameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Name changed: " + result.DisplayName);
    }
    #region Virtual Currency
    public void AddVirtualCurrency(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = _currencyCode,
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request, OnVirtualCurrencyUpdate, OnVirtualCurrencyUpdateError);
    }

    public void DeductVirtualCurrency(int amount)
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = _currencyCode,
            Amount = amount
        };

        PlayFabClientAPI.SubtractUserVirtualCurrency(request, OnVirtualCurrencyUpdate, OnVirtualCurrencyUpdateError);
    }

    private void OnVirtualCurrencyUpdate(ModifyUserVirtualCurrencyResult result)
    {
        OnCurrencyChange.Invoke(result.Balance);
    }

    private void OnVirtualCurrencyUpdateError(PlayFabError error)
    {

    }
    public void GetVirtualCurrencyBalance()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), 
            result =>
            {
                if (result.VirtualCurrency.TryGetValue(_currencyCode, out int balance))
                {
                    OnCurrencyChange.Invoke(balance);
                    Debug.Log($"Current balance of {_currencyCode}: {balance}");
                }
                else
                {
                    Debug.LogError($"Currency code {_currencyCode} not found.");
                }
            },
            error =>
            {
                Debug.LogError("Error retrieving virtual currency balance: " + error.GenerateErrorReport());
            });
    }
    #endregion
}
