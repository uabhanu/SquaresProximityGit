namespace Managers
{
    using Interfaces;
    using Misc;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlayerTurnsManager : IPlayerTurnsManager
    {
        #region Constructor

        public PlayerTurnsManager(GameManager gameManager , GridManager gridManager)
        {
            _gameManager = gameManager;
            _gridManager = gridManager;
            _onlineMode = ServiceLocator.Get<OnlineMode>();
        }

        #endregion

        #region Variables Declarations

        private OnlineMode _onlineMode;
        private GameManager _gameManager;
        private GridManager _gridManager;

        #endregion

        #region Functions

        private void UpdateCoinValueAfterPlacement()
        {
            int currentPlayerID = _gameManager.CurrentPlayerID;

            if(_gameManager.PlayerNumbersList[currentPlayerID].Count > 0)
            {
                _gameManager.CoinValue = _gameManager.GetCurrentCoinValue();
                TMP_Text coinUITMP = _gameManager.CoinUIObj.GetComponentInChildren<TMP_Text>();
                coinUITMP.text = _gameManager.CoinValue.ToString();

                for(int i = 0; i < _gameManager.TotalReceivedArray.Length; i++)
                {
                    if(currentPlayerID == i)
                    {
                        _gameManager.TotalReceivedArray[i] += _gameManager.CoinValue;
                    }
                }

                _gameManager.PlayerNumbersList[currentPlayerID].RemoveAt(0);
            }
        }

        public void EndPlayerTurn()
        {
            if(_onlineMode.PlayerIsOnline)
            {
                // Handle multiplayer end turn logic (e.g., notify server, wait for network events)
            }
            else
            {
                _gameManager.CurrentPlayerID = (_gameManager.CurrentPlayerID + 1) % _gameManager.NumberOfPlayers;
            }
        }

        public void StartPlayerTurn()
        {
            if(_onlineMode.PlayerIsOnline)
            {
                // Handle multiplayer start turn logic here (e.g., sync with server)
            }
            else
            {
                if(_gameManager.IsRandomTurns)
                {
                    int remainingPlayersCount = _gameManager.PlayersRemainingList.Count;
                    int randomIndex = Random.Range(0 , remainingPlayersCount);
                    _gameManager.CurrentPlayerID = _gameManager.PlayersRemainingList[randomIndex];
                    _gameManager.PlayersRemainingList.RemoveAt(randomIndex);
                }

                bool foundUnblockedCell = false;
                int maxIterations = _gridManager.GridInfo.Cols * _gridManager.GridInfo.Rows;
                int currentIteration = 0;

                UpdateCoinValueAfterPlacement();

                if(_gameManager.PlayersRemainingList.Count == 0)
                {
                    _gameManager.ResetPlayersRemaining();
                }

                while(!foundUnblockedCell && currentIteration < maxIterations)
                {
                    for(int i = 0; i < _gameManager.IsAIArray.Length; i++)
                    {
                        if(_gameManager.IsAIArray[i] && _gameManager.CurrentPlayerID == i)
                        {
                            _gameManager.CellIndexToUse = _gameManager.IAIManager.FindCellToPlaceCoinOn();

                            if(_gameManager.CellIndexToUse != _gridManager.InvalidCellIndex)
                            {
                                _gameManager.StartCoroutine(_gameManager.IAIManager.AIPlaceCoinCoroutine());
                                foundUnblockedCell = true;
                            }
                        }
                    }
                    
                    currentIteration++;
                }

                UpdateCoinUIImageColors();
                UpdateTrailColor();
            }
        }

        public void UpdateAdjacentCoinText(int x , int y , int newCoinValue)
        {
            GameObject adjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(x , y);
            TMP_Text adjacentCoinValueText;

            if(adjacentCoinObj != null)
            {
                adjacentCoinValueText = adjacentCoinObj.GetComponentInChildren<TMP_Text>();

                if(adjacentCoinValueText == null)
                {
                    int[] offsetX = { -1 , 0 , 1 , -1 , 1 , -1 , 0 , 1 };
                    int[] offsetY = { -1 , -1 , -1 , 0 , 0 , 1 , 1 , 1 };

                    for(int i = 0; i < 8; i++)
                    {
                        int adjacentCellIndexX = x + offsetX[i];
                        int adjacentCellIndexY = y + offsetY[i];

                        GameObject adjacentAdjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(adjacentCellIndexX , adjacentCellIndexY);

                        if(adjacentAdjacentCoinObj != null)
                        {
                            TMP_Text adjacentAdjacentCoinValueText = adjacentAdjacentCoinObj.GetComponentInChildren<TMP_Text>();

                            if(adjacentAdjacentCoinValueText != null)
                            {
                                adjacentCoinValueText = adjacentAdjacentCoinValueText;
                                break;
                            }
                        }
                    }
                }

                if(adjacentCoinValueText != null)
                {
                    adjacentCoinValueText.text = newCoinValue.ToString();
                }
                else
                {
                    Debug.LogWarning("Could not find adjacent cell with TMP_Text component.");
                }
            }
        }

        public void UpdateCoinColor(int x , int y)
        {
            GameObject coin = _gridManager.CoinOnTheCellData.GetValue(x , y);

            if(coin != null)
            {
                SpriteRenderer coinRenderer = coin.GetComponentInChildren<SpriteRenderer>();
                TMP_Text coinValueTMP = coin.GetComponentInChildren<TMP_Text>();

                coinRenderer.color = _gameManager.GetCoinBackgroundColour(_gameManager.CurrentPlayerID);
                coinValueTMP.color = _gameManager.GetCoinForegroundColour(_gameManager.CurrentPlayerID);

                for(int i = 0; i < _gameManager.IsAIArray.Length; i++)
                {
                    if(_gameManager.IsAIArray[i])
                    {
                        _gameManager.StartCoroutine(_gameManager.IAIManager.AnimateCoinEffect(coinRenderer , coinRenderer.color));
                    }
                }
            }
        }

        public void UpdateCoinUIImageColors()
        {
            if(_gameManager.CoinUIObj != null)
            {
                Color coinColour = _gameManager.GetCoinBackgroundColour(_gameManager.CurrentPlayerID);
                Image coinUIImage = _gameManager.CoinUIObj.GetComponent<Image>();
                TMP_Text coinUIText = _gameManager.CoinUIObj.GetComponentInChildren<TMP_Text>();

                coinUIImage.color = coinColour;
                coinUIText.color = _gameManager.GetCoinForegroundColour(_gameManager.CurrentPlayerID);
            }
        }

        public void UpdateTrailColor()
        {
            if(_gameManager.TrailObj != null)
            {
                SpriteRenderer trailRenderer = _gameManager.TrailObj.GetComponentInChildren<SpriteRenderer>();
                Color playerColour = _gameManager.GetCoinBackgroundColour(_gameManager.CurrentPlayerID);

                playerColour.a *= 0.5f;
                trailRenderer.color = playerColour;
            }
        }

        #endregion
    }
}