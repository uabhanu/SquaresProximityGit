namespace Managers
{
    #if UNITY_ANDROID
        using GooglePlayGames;
        using GooglePlayGames.BasicApi;
    #endif
    
    using Interfaces;
    using UnityEngine;

    public class GooglePlayGamesManager : MonoBehaviour, IGooglePlayGamesManager
    {
        private bool _isAuthenticated;

        public bool IsAuthenticated => _isAuthenticated;
        public static GooglePlayGamesManager Instance { get; private set; }

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            #if UNITY_ANDROID
                PlayGamesPlatform.Activate();
            #endif
        }

        public void Authenticate()
        {
            #if UNITY_ANDROID
                Debug.Log("Starting authentication with Google Play Games...");
                
                PlayGamesPlatform.Instance.Authenticate(success =>
                {
                    if(success == SignInStatus.Success)
                    {
                        _isAuthenticated = true;
                        Debug.Log("Signed in to Google Play Games successfully.");
                    }
                    else
                    {
                        _isAuthenticated = false;
                        Debug.LogError("Failed to sign in to Google Play Games.");
                    }
                });
            #else
                Debug.LogWarning("Google Play Games is only supported on Android.");
            #endif
        }
    }
}