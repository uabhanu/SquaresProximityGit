namespace Managers
{
    using System;
    using UnityEngine;
    
    public class ScoreManager : MonoBehaviour
    {
        #region Variables Declarations
        
        private int _numberOfPlayers;
        private int[] _coinScoreValuesArray;
        private string[] _playerNamesArray;
        
        #endregion

        #region MonoBehaviour Functions
        
        private void Start()
        {
            ToggleEventSubscription(true);
            EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }
        
        #endregion

        #region User Defined Functions

        private void CoinBuffedUpScore(int buffedUpCoinPlayerID , int buffedUpCoinIncrement)
        {
            _coinScoreValuesArray[buffedUpCoinPlayerID] += buffedUpCoinIncrement;
            EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);
        }

        private void CoinCapturedScore(int capturingPlayerID , int capturedPlayerID , int capturedCoinValue)
        {
            _coinScoreValuesArray[capturingPlayerID] += capturedCoinValue;

            if(_coinScoreValuesArray[capturedPlayerID] > 0)
            {
                _coinScoreValuesArray[capturedPlayerID] -= capturedCoinValue;
                EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);   
            }
        }
        
        #endregion

        #region Events Related Functions
        
        private void OnCoinBuffedUp(int playerID , int coinValue)
        {
            CoinBuffedUpScore(playerID , coinValue);
        }

        private void OnCoinCaptured(int currentPlayerID , int adjacentPlayerID , int adjacentPlayerCoinValue)
        {
            CoinCapturedScore(currentPlayerID , adjacentPlayerID , adjacentPlayerCoinValue);
        }

        private void OnCoinPlaced(int coinValue , int playerID)
        {
            _coinScoreValuesArray[playerID] += coinValue;
            EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);
        }

        private void OnGameOver()
        {
            int highestScore = int.MinValue;
            int winningPlayer = -1;

            for(int i = 0; i < _numberOfPlayers; i++)
            {
                if(_coinScoreValuesArray[i] > highestScore)
                {
                    highestScore = _coinScoreValuesArray[i];
                    winningPlayer = i;
                }
            }

            bool isTie = false;

            for(int i = 0; i < _numberOfPlayers; i++)
            {
                if(_coinScoreValuesArray[i] == highestScore && i != winningPlayer)
                {
                    isTie = true;
                    break;
                }
            }

            if(isTie)
            {
                EventsManager.Invoke(Event.GameTied);
            }
            else
            {
                EventsManager.Invoke(Event.PlayerWins , winningPlayer);
            }
        }

        private void OnNumberOfPlayersSelected(int numberOfPlayers)
        {
            _numberOfPlayers = numberOfPlayers;
            _coinScoreValuesArray = new int[_numberOfPlayers];
            _playerNamesArray = new string[_numberOfPlayers];
        }
    
        private void OnPlayerNamesUpdated(int playerID , string playerName)
        {
            _playerNamesArray[playerID] = playerName;
            EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);
        }
        
        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Event.CoinBuffedUp , (Action<int , int>)OnCoinBuffedUp);
                EventsManager.SubscribeToEvent(Event.CoinCaptured , (Action<int , int , int>)OnCoinCaptured);
                EventsManager.SubscribeToEvent(Event.CoinPlaced , (Action<int , int>)OnCoinPlaced);
                EventsManager.SubscribeToEvent(Event.GameOver , new Action(OnGameOver));
                EventsManager.SubscribeToEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
                EventsManager.SubscribeToEvent(Event.PlayerNamesUpdated , (Action<int , string>)OnPlayerNamesUpdated);
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.CoinBuffedUp , (Action<int , int>)OnCoinBuffedUp);
                EventsManager.UnsubscribeFromEvent(Event.CoinCaptured , (Action<int , int , int>)OnCoinCaptured);
                EventsManager.UnsubscribeFromEvent(Event.CoinPlaced , (Action<int , int>)OnCoinPlaced);
                EventsManager.UnsubscribeFromEvent(Event.GameOver , new Action(OnGameOver));
                EventsManager.UnsubscribeFromEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
                EventsManager.UnsubscribeFromEvent(Event.PlayerNamesUpdated , (Action<int , string>)OnPlayerNamesUpdated);
            }
        }
        
        #endregion
    }
}