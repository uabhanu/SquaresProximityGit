namespace Misc
{
    using Managers;
    using UnityEngine;
    
    public class OnlineMode
    {
        private bool _playerOnline;

        public bool PlayerIsOnline
        {
            get => _playerOnline;
            
            set
            {
                if(_playerOnline != value)
                {
                    _playerOnline = value;
                    EventsManager.Invoke(Managers.Event.PlayerNowOnline , _playerOnline);
                }
            }
        }
        
        public void SetOnlineMode(bool playerOnline)
        {
            _playerOnline = playerOnline;
            Debug.Log("Game Mode set to " + (playerOnline ? "Online" : "Offline"));
        }
    }
}