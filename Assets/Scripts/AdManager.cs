#if UNITY_WEBGL && !UNITY_EDITOR
using Playgama;
#endif

public static class AdManager
{
    public static void ShowInterstitial(string placement)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (Bridge.advertisement.isInterstitialSupported)
            Bridge.advertisement.ShowInterstitial(placement);
#endif
    }
}
