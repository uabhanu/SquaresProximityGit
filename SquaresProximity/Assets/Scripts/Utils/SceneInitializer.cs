namespace Utils
{
    using Managers;
    using Misc;
    using Photon.Pun;
    using System.Collections.Generic;
    using UnityEngine;

    public class SceneInitializer : MonoBehaviour
    {
        private Dictionary<string, int> _platformToIndex = new()
        {
            { "UNITY_STANDALONE", 0 },
            { "UNITY_ANDROID", 1 },
            { "UNITY_WEBGL", 2 }
        };

        [SerializeField] private bool isOnlineMultiplayer; // TODO Dynamically set this in production
        [SerializeField] private List<GameObject> sceneObjectsList;
        [SerializeField] private List<GameObject> uiObjectsList;

        private void Start()
        {
            if(isOnlineMultiplayer)
            {
                Debug.Log("Initializing online mode...");
            }
            else
            {
                Debug.Log("Initializing offline mode...");
            }
            
            InstantiateObjects(sceneObjectsList);
            
            string currentPlatform = GetPlatformName();
            
            if(_platformToIndex.ContainsKey(currentPlatform))
            {
                int index = _platformToIndex[currentPlatform];
                GameObject uiPrefab = uiObjectsList[index];
                InstantiateObjects(new List<GameObject> { uiPrefab });
            }
            else
            {
                Debug.LogError($"Platform not supported for UI object instantiation: {currentPlatform}");
            }

            Destroy(gameObject);
        }
        
        private void AddPhotonView(GameObject obj)
        {
            if(obj.GetComponent<PhotonView>() == null)
            {
                obj.AddComponent<PhotonView>();
                Debug.Log($"PhotonView dynamically added to: {obj.name}");
            }
        }

        private void InstantiateObjects(List<GameObject> objectList)
        {
            foreach(var prefab in objectList)
            {
                GameObject instantiatedObject = isOnlineMultiplayer ? PhotonInstantiate(prefab) : Instantiate(prefab);

                if(isOnlineMultiplayer && NeedsPhotonView(prefab) && instantiatedObject.GetComponent<PhotonView>() == null)
                {
                    AddPhotonView(instantiatedObject);
                }
            }
        }

        private string GetPlatformName()
        {
            #if UNITY_ANDROID
                return "UNITY_ANDROID";
            #elif UNITY_IOS
                return "UNITY_IOS";
            #elif UNITY_STANDALONE
                return "UNITY_STANDALONE";
            #elif UNITY_WEBGL
                return "UNITY_WEBGL";
            #else
                return "UNKNOWN_PLATFORM";
            #endif
        }

        private bool NeedsPhotonView(GameObject obj)
        {
            var photonViewRequiredComponents = new[]
            {
                typeof(InGameUIManager),
                typeof(PlayerController),
                typeof(GameManager),
                typeof(GridManager),
                typeof(ScoreManager)
            };

            foreach (var componentType in photonViewRequiredComponents)
            {
                if(obj.GetComponent(componentType) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private GameObject PhotonInstantiate(GameObject prefab)
        {
            if(PhotonNetwork.IsConnected && isOnlineMultiplayer)
            {
                return PhotonNetwork.Instantiate(prefab.name , Vector3.zero , Quaternion.identity);
            }

            return Instantiate(prefab);
        }
    }
}