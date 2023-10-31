using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoogleAdmobManager : MonoBehaviour
{
    private BannerView bannerView;

    void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((initStatus) =>
        {
            this.RequestBanner();
        });


    }
    private void RequestBanner()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-4439186819450031/4802241902";
#elif UNITY_IPHONE
            string adUnitId = "";
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);

        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);
    }

}
