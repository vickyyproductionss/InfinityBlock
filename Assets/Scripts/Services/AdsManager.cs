using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    #region BannerAd Ids
#if UNITY_ANDROID
    private string _banner_adUnitId = "ca-app-pub-1648950075295248/6639065851";
#elif UNITY_IPHONE
    private string _banner_adUnitId = "ca-app-pub-3940256099942544/2435281174";
#else
  private string _banner_adUnitId = "unused";
#endif
    #endregion
    #region RewardedAd Ids
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-1648950075295248/4472616251";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
  private string _adUnitId = "unused";
#endif

#if UNITY_ANDROID
    private string _adUnitId_Gems = "ca-app-pub-1648950075295248/1447831811";
#elif UNITY_IPHONE
    private string _adUnitId_Gems = "ca-app-pub-3940256099942544/1712485313";
#else
  private string _adUnitId_Gems = "unused";
#endif
    #endregion

    BannerView _bannerView;
    private RewardedAd _rewardedAd;
    private RewardedAd _rewardedAd_Gem;


    /// <summary>
    /// Instance of the ads manager
    /// </summary>
    public static AdsManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { onInitSuccess(); });
    }
    void onInitSuccess()
    {
        LoadRewardedAd(_adUnitId_Gems);
        LoadRewardedAd(_adUnitId);
        LoadBannerAd();
    }
    public void LoadBannerAd()
    {
        if (_bannerView == null)
        {
            CreateBannerView();
        }

        var adRequest = new AdRequest();

        _bannerView.LoadAd(adRequest);
    }
    #region RewardedAds
    public void LoadRewardedAd(string ad_id)
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }


        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(ad_id, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                if (ad_id == _adUnitId)
                {
                    _rewardedAd = ad;
                    RegisterRewardedEventHandlers(_rewardedAd);
                }
                else if (ad_id == _adUnitId_Gems)
                {
                    _rewardedAd_Gem = ad;
                    RegisterRewardedEventHandlers(_rewardedAd_Gem);
                }
            });
    }
    public void ShowRewardedForGems()
    {
        DestroyBannerView();
        const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd_Gem != null && _rewardedAd_Gem.CanShowAd())
        {
            _rewardedAd_Gem.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                PlayfabManager pf = GameObject.Find("HomeUIController").GetComponent<PlayfabManager>();
                pf.AddVirtualCurrency(20);
            });
        }
    }
    public void ShowRewardedAd()
    {
        DestroyBannerView();
        const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
            });
        }
    }
    private void RegisterRewardedEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }
    #endregion
    #region BannerAds
    public void CreateBannerView()
    {

        if (_bannerView != null)
        {
            DestroyBannerView();
        }

        _bannerView = new BannerView(_banner_adUnitId, AdSize.Banner, AdPosition.Bottom);
        ListenToBannerAdEvents();
    }

    public void DestroyBannerView()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }
    }
    private void ListenToBannerAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + _bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }
    #endregion
}
