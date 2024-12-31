namespace Misc
{
    using Interfaces;
    using Managers;
    using System;
    using TMPro;
    using UnityEngine;
    
    public class CoinPlacer : ICoinPlacer
    {
        #region Constructor

        public CoinPlacer(GameManager gameManager , GridManager gridManager)
        {
            _gameManager = gameManager;
            _gridManager = gridManager;
            _onlineMode = ServiceLocator.Get<OnlineMode>();
        }

        #endregion

        #region Variables Declarations

        private GameManager _gameManager;
        private OnlineMode _onlineMode;
        private GridManager _gridManager;

        #endregion

        #region Functions

        public void BuffUpAdjacentCoin(Vector2Int cellIndexAtMousePosition)
        {
            if(_onlineMode.PlayerIsOnline)
            {
                // Multiplayer synchronization logic, e.g., notify other players or server.
                return;
            }

            int currentPlayerID = _gridManager.PlayerIDData.GetValue(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y);

            ProcessAdjacentCells(cellIndexAtMousePosition , (x , y , adjacentCoinObj) =>
            {
                if(adjacentCoinObj != null)
                {
                    int adjacentPlayerID = _gridManager.PlayerIDData.GetValue(x , y);
                    int adjacentCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                    if(adjacentPlayerID == currentPlayerID)
                    {
                        int newAdjacentCoinValue = adjacentCoinValue + 1;
                        _gridManager.CoinValueData.SetValue(x , y , newAdjacentCoinValue);

                        _gameManager.IPlayerTurnsManager.UpdateAdjacentCoinText(x , y , newAdjacentCoinValue);
                        EventsManager.Invoke(Managers.Event.CoinBuffedUp , adjacentPlayerID , newAdjacentCoinValue - adjacentCoinValue);
                    }
                }
            });
        }

        public void CaptureAdjacentCoin(Vector2Int cellIndexAtMousePosition)
        {
            if(_onlineMode.PlayerIsOnline)
            {
                // Multiplayer synchronization logic, e.g., notify other players or server.
                return;
            }

            int currentPlayerID = _gridManager.PlayerIDData.GetValue(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y);

            ProcessAdjacentCells(cellIndexAtMousePosition , (x , y , adjacentCoinObj) =>
            {
                if(adjacentCoinObj != null)
                {
                    int adjacentPlayerID = _gridManager.PlayerIDData.GetValue(x , y);
                    int adjacentPlayerCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                    if(adjacentPlayerID != currentPlayerID && adjacentPlayerCoinValue < _gameManager.CoinValue)
                    {
                        _gridManager.PlayerIDData.SetValue(x , y , currentPlayerID);
                        EventsManager.Invoke(Managers.Event.CoinCaptured , currentPlayerID , adjacentPlayerID , adjacentPlayerCoinValue);
                        _gameManager.IPlayerTurnsManager.UpdateCoinColor(x , y);
                    }
                }
            });
        }

        public void PlaceCoin(Vector2Int cellIndexAtMousePosition)
        {
            if(_onlineMode.PlayerIsOnline)
            {
                EventsManager.Invoke(Managers.Event.CoinPlaced , _gameManager.CoinValue , _gameManager.CurrentPlayerID);
                return;
            }

            _gridManager.CoinValueData.SetValue(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y , _gameManager.CoinValue);
            _gridManager.PlayerIDData.SetValue(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y , _gameManager.CurrentPlayerID);
            _gridManager.TotalCells--;

            EventsManager.Invoke(Managers.Event.CoinPlaced , _gameManager.CoinValue , _gameManager.CurrentPlayerID);

            if(!_gridManager.IsCellBlockedData.GetValue(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y))
            {
                BuffUpAdjacentCoin(cellIndexAtMousePosition);
                CaptureAdjacentCoin(cellIndexAtMousePosition);

                Vector2 spawnPos = _gridManager.CellToWorld(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y);
                GameObject newCoinObj = UnityEngine.Object.Instantiate(_gameManager.CoinObj , spawnPos , Quaternion.identity , _gameManager.gameObject.transform);
                _gridManager.CoinOnTheCellData.SetValue(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y , newCoinObj);
                newCoinObj.GetComponentInChildren<TextMeshPro>().text = _gameManager.CoinValue.ToString();

                _gameManager.IPlayerTurnsManager.UpdateCoinColor(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y);

                for(int i = 0; i < _gameManager.IsAIArray.Length; i++)
                {
                    if(_gameManager.IsAIArray[i] && i == _gameManager.CurrentPlayerID)
                    {
                        _gameManager.StartCoroutine(_gameManager.IAIManager.AnimateCoinEffect(newCoinObj.GetComponentInChildren<SpriteRenderer>()));
                    }
                }

                _gridManager.IsCellBlockedData.SetValue(cellIndexAtMousePosition.x , cellIndexAtMousePosition.y , true);
                _gameManager.IsMoving = false;

                _gameManager.IPlayerTurnsManager.UpdateCoinUIImageColors();
                _gameManager.UpdateTrailVisibility();

                _gameManager.IPlayerTurnsManager.EndPlayerTurn();
                _gameManager.IPlayerTurnsManager.StartPlayerTurn();
            }
        }

        private void ProcessAdjacentCells(Vector2Int cellIndexAtMousePosition , Action<int , int , GameObject> processAction)
        {
            int minX = Mathf.Max(cellIndexAtMousePosition.x - 1 , 0);
            int maxX = Mathf.Min(cellIndexAtMousePosition.x + 1 , _gridManager.GridInfo.Cols - 1);
            int minY = Mathf.Max(cellIndexAtMousePosition.y - 1 , 0);
            int maxY = Mathf.Min(cellIndexAtMousePosition.y + 1 , _gridManager.GridInfo.Rows - 1);

            for(int x = minX; x <= maxX; x++)
            {
                for(int y = minY; y <= maxY; y++)
                {
                    if(x == cellIndexAtMousePosition.x && y == cellIndexAtMousePosition.y) continue;

                    bool isCellBlocked = _gridManager.IsCellBlockedData.GetValue(x , y);

                    if(isCellBlocked)
                    {
                        GameObject adjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(x , y);
                        processAction(x , y , adjacentCoinObj);
                    }
                }
            }
        }

        #endregion
    }
}