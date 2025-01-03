namespace Utils
{
    using UnityEngine;
    
    public static class CustomColoursLogger
    {
        public static void LogError(string message)
        {
            Debug.LogError($"<color=red>{message}</color>");
        }
        
        public static void LogInfo(string message)
        {
            Debug.Log($"<color=green>{message}</color>");
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning($"<color=yellow>{message}</color>");
        }
    }
}