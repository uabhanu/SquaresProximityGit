namespace Utils
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class SceneInitializer : MonoBehaviour
    {
        private Dictionary<string , int> _platformToIndex = new()
        {
            { "UNITY_STANDALONE" , 0 },
            { "UNITY_ANDROID" , 1 },
            { "UNITY_WEBGL" , 2 }
        };
        
        [SerializeField] private List<GameObject> sceneObjectsList;
        [SerializeField] private List<GameObject> uiObjectsList;

        private void Start()
        {
            InstantiateSceneObjects();
            InstantiateUIObjects();
            Destroy(gameObject);
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

        private void InstantiateSceneObjects()
        {
            foreach(var sceneObj in sceneObjectsList)
            {
                Instantiate(sceneObj);
            }
        }

        private void InstantiateUIObjects()
        {
            string currentPlatform = GetPlatformName();
            
            if(_platformToIndex.ContainsKey(currentPlatform))
            {
                int index = _platformToIndex[currentPlatform];
                Instantiate(uiObjectsList[index]);
            }
            else
            {
                Debug.LogError("Platform not supported for UI object instantiation: " + currentPlatform);
            }
        }
    }
}