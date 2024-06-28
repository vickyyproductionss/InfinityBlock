using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private string _leaderboardName = "Highscore";
    public GameObject NamePanel;


    [Header("Leaderboard")]
    public GameObject PlayerPrefab;
    public GameObject PlayerParent;

    public TMP_Text OwnRankText;
    public TMP_Text OwnScoreText;

    public static PlayfabManager instance;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(instance.gameObject);
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

    }
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
        NamePanel.SetActive(false);
    }
    #region Playfab Stats
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
                    if(SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        OnCurrencyChange.Invoke(balance);
                    }
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
    public void SendHighscore()
    {
        int score = PlayerPrefs.GetInt("x2BlocksHighscore");
        Debug.Log("Score sent is: " + score);
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = _leaderboardName,
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdateSuccess, OnLeaderboardUpdateFailure);
    }

    private void OnLeaderboardUpdateFailure(PlayFabError error)
    {
        Debug.Log("Failed to update stats");
    }

    private void OnLeaderboardUpdateSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Stats updated successfully!!!");
        GetLeaderboard();
        GetPlayerLeaderboard(_playerID);
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = _leaderboardName,
            StartPosition = 0,
            MaxResultsCount = 100
        };
        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, OnGetLeaderboardFailure);
    }

    private void OnGetLeaderboardSuccess(GetLeaderboardResult result)
    {
        for(int i = PlayerParent.transform.childCount - 1; i >= 1;i--)
        {
            Destroy(PlayerParent.transform.GetChild(i).gameObject);
        }
        Debug.Log("Leaderboard retrieved successfully!");
        foreach (var entry in result.Leaderboard)
        {
            GameObject player = Instantiate(PlayerPrefab,PlayerParent.transform,false);
            player.transform.GetChild(0).GetComponent<TMP_Text>().text = (entry.Position + 1).ToString();
            player.transform.GetChild(1).GetComponent<TMP_Text>().text = entry.DisplayName;
            player.transform.GetChild(2).GetComponent<TMP_Text>().text = entry.StatValue.ToString();
        }
    }

    private void OnGetLeaderboardFailure(PlayFabError error)
    {
        Debug.LogError("Leaderboard retrieval failed: " + error.GenerateErrorReport());
    }
    public void GetPlayerLeaderboard(string playFabId)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            PlayFabId = playFabId,
            StatisticName = _leaderboardName,
            MaxResultsCount = 1
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetPlayerLeaderboardSuccess, OnGetPlayerLeaderboardFailure);
    }

    private void OnGetPlayerLeaderboardFailure(PlayFabError error)
    {
        Debug.Log("Nothing found for the player");
    }

    private void OnGetPlayerLeaderboardSuccess(GetLeaderboardAroundPlayerResult result)
    {
        foreach (var entry in result.Leaderboard)
        {
            OwnRankText.text = (entry.Position + 1).ToString();
            OwnScoreText.text = entry.StatValue.ToString();
        }
    }

    #endregion
}
