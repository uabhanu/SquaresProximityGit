namespace Managers
{
    using JetBrains.Annotations;
    using System;
    using UnityEngine;
    
    public static class PlayerPrefsManager
    {
        private static T Deserialize<T>(string serializedData)
        {
            try
            {
                return (T)System.Convert.ChangeType(serializedData , typeof(T));
            }
            catch
            {
                return default;
            }
        }
        
        private static string Serialize<T>(T data)
        {
            return data.ToString();
        }
        
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
        
        public static void LoadData<T>([NotNull] ref T data , string key)
        {
            if(data == null) throw new ArgumentNullException(nameof(data));
            
            if(PlayerPrefs.HasKey(key))
            {
                string serializedData = PlayerPrefs.GetString(key);
                data = Deserialize<T>(serializedData);
            }
            else
            {
                data = default!;
            }
        }

        public static void LoadData<T>(ref T[] dataArray , string[] keys)
        {
            for(int i = 0; i < dataArray.Length; i++)
            {
                if(PlayerPrefs.HasKey(keys[i]))
                {
                    string serializedData = PlayerPrefs.GetString(keys[i]);
                    dataArray[i] = Deserialize<T>(serializedData);
                }
                else
                {
                    dataArray[i] = default;
                }
            }
        }
        
        public static void SaveData<T>(T data , string key)
        {
            string serializedData = Serialize(data);
            PlayerPrefs.SetString(key , serializedData);
            PlayerPrefs.Save();
        }

        public static void SaveData<T>(T[] dataArray , string[] keys)
        {
            for(int i = 0; i < dataArray.Length; i++)
            {
                string serializedData = Serialize(dataArray[i]);
                PlayerPrefs.SetString(keys[i] , serializedData);
            }

            PlayerPrefs.Save();
        }
    }
}