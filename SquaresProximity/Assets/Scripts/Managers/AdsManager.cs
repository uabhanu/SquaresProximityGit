namespace Managers
{
    using UnityEngine;
    
    public class AdsManager : MonoBehaviour
    {
        private static AdsManager _instance;

        public static AdsManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    GameObject adsManagerObject = new GameObject("AdsManager");
                    _instance = adsManagerObject.AddComponent<AdsManager>();
                    DontDestroyOnLoad(adsManagerObject);
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if(_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeAds();
        }
        
        public void HideBannerAd()
        {
            Debug.Log("AdsManager: Hiding banner ad...");
            IronSource.Agent.hideBanner();
        }

        private void InitializeAds()
        {
            Debug.Log("AdsManager: Ads initialized.");
            IronSource.Agent.init("1f7dd8ea5");
        }
        
        public bool IsInterstitialAdReady()
        {
            return IronSource.Agent.isInterstitialReady();
        }
        
        public void LoadBannerAd()
        {
            Debug.Log("AdsManager: Loading banner ad...");
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER , IronSourceBannerPosition.BOTTOM);
        }

        public void LoadInterstitialAd()
        {
            Debug.Log("AdsManager: Loading interstitial ad...");
            IronSource.Agent.loadInterstitial();
        }
        
        public void ShowBannerAd()
        {
            Debug.Log("AdsManager: Showing banner ad...");
            IronSource.Agent.displayBanner();
        }

        public void ShowInterstitialAd()
        {
            if(IsInterstitialAdReady())
            {
                Debug.Log("AdsManager: Showing interstitial ad...");
                IronSource.Agent.showInterstitial();
            }
            else
            {
                Debug.LogWarning("AdsManager: Interstitial ad is not ready yet.");
                LoadInterstitialAd();
            }
        }
    }
}