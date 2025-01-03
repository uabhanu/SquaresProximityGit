namespace Managers
{
    using Interfaces;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    
    public class AIManager : IAIManager
    {
        #region Constructor
        
        public AIManager(GameManager gameManager , GridManager gridManager)
        {
            _gameManager = gameManager;
            _gridManager = gridManager;
        }
        
        #endregion

        #region Variables Declarations
        
        private GameManager _gameManager;
        private GridManager _gridManager;
        
        #endregion

        #region User Defined Functions
        
        private List<Vector2Int> GetAdjacentCellIndicesList(Vector2Int coinCellIndex)
        {
            if(coinCellIndex != _gridManager.InvalidCellIndex)
            {
                List<Vector2Int> adjacentCellIndicesList = new List<Vector2Int>
                {
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x - 1 , coinCellIndex.y)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x + 1 , coinCellIndex.y)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x - 1 , coinCellIndex.y + 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x + 1 , coinCellIndex.y + 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x , coinCellIndex.y - 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x , coinCellIndex.y + 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x - 1 , coinCellIndex.y - 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x + 1 , coinCellIndex.y - 1))
                };

                return adjacentCellIndicesList;
            }

            return new List<Vector2Int>();
        }

        private List<Vector2Int> FindAdjacentCellIndicesList(List<int> coinValuesList)
        {
            List<Vector2Int> validAdjacentCellIndicesList = new List<Vector2Int>();

            foreach(int coinValue in coinValuesList)
            {
                List<Vector2Int> coinCellIndicesList = new List<Vector2Int>();
        
                if(_gameManager.LesserCoinValuesList.Contains(coinValue))
                {
                    coinCellIndicesList = _gameManager.LesserCoinsCellIndicesList
                    .Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue)
                    .ToList();
                }
                
                else if(_gameManager.SelfCoinValuesList.Contains(coinValue))
                {
                    coinCellIndicesList = _gameManager.SelfCoinsCellIndicesList
                    .Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue)
                    .ToList();
                }

                foreach(Vector2Int coinCellIndex in coinCellIndicesList)
                {
                    if(coinCellIndex != _gridManager.InvalidCellIndex)
                    {
                        //Debug.Log("FindAdjacentCellIndicesList() -> Cell Index To Pass to GetAdjacentCellIndicesList() : " + coinCellIndex);
                        List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(coinCellIndex);
                        
                        adjacentCellIndicesList = adjacentCellIndicesList
                        .Where(adjacentCellIndex => adjacentCellIndex.x >= 0 && adjacentCellIndex.x < _gridManager.GridInfo.Cols &&
                        adjacentCellIndex.y >= 0 && adjacentCellIndex.y < _gridManager.GridInfo.Rows &&
                        !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                        .ToList();

                        validAdjacentCellIndicesList.AddRange(adjacentCellIndicesList);
                    }
                }
            }

            return validAdjacentCellIndicesList;
        }

        private void ClearLists()
        {
            _gameManager.LesserCoinsCellIndicesList.Clear();
            _gameManager.LesserCoinValuesList.Clear();
            _gameManager.SelfCoinsCellIndicesList.Clear();
            _gameManager.SelfCoinValuesList.Clear();
            _gameManager.OtherPlayerCoinsCellIndicesList.Clear();
            _gameManager.OtherPlayerCoinValuesList.Clear();
            _gameManager.UnblockedCellIndicesList.Clear();
        }

        private void PopulateLists()
        {
            for(int x = 0; x < _gridManager.GridInfo.Cols; x++)
            {
                for(int y = 0; y < _gridManager.GridInfo.Rows; y++)
                {
                    bool hasAdjacentUnblockedCell = GetAdjacentCellIndicesList(new Vector2Int(x , y)).Any(adjacentIndex =>
                    adjacentIndex.x >= 0 && adjacentIndex.x < _gridManager.GridInfo.Cols &&
                    adjacentIndex.y >= 0 && adjacentIndex.y < _gridManager.GridInfo.Rows &&
                    !_gridManager.IsCellBlockedData.GetValue(adjacentIndex.x , adjacentIndex.y));
                    
                    if(!_gridManager.IsCellBlockedData.GetValue(x , y))
                    {
                        _gameManager.UnblockedCellIndicesList.Add(new Vector2Int(x , y));
                    }
                
                    else if(_gridManager.IsCellBlockedData.GetValue(x , y))
                    {
                        GameObject coin = _gridManager.CoinOnTheCellData.GetValue(x , y);
        
                        if(coin != null && _gridManager.PlayerIDData.GetValue(x , y) != _gameManager.CurrentPlayerID)
                        {
                            _gameManager.OtherPlayerCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                            _gameManager.OtherPlayerCoinValuesList.Add(coinValue);

                            if(coinValue < _gameManager.CoinValue && hasAdjacentUnblockedCell)
                            {
                                _gameManager.LesserCoinsCellIndicesList.Add(new Vector2Int(x , y));
                                
                                _gameManager.LesserCoinValuesList.Add(coinValue);
                                List<int> sortedLesserCoinValues = _gameManager.LesserCoinValuesList.OrderByDescending(value => value).ToList();
                                _gameManager.LesserCoinValuesList = sortedLesserCoinValues;
                            }
                        }
        
                        if(coin != null && _gridManager.PlayerIDData.GetValue(x , y) == _gameManager.CurrentPlayerID && hasAdjacentUnblockedCell)
                        {
                            _gameManager.SelfCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            int coinValue = _gridManager.CoinValueData.GetValue(x , y);

                            if(coinValue < _gameManager.MaxCoinValue)
                            {
                                _gameManager.SelfCoinValuesList.Add(coinValue);
                                List<int> sortedLesserCoinValues = _gameManager.SelfCoinValuesList.OrderByDescending(value => value).ToList();
                                _gameManager.SelfCoinValuesList = sortedLesserCoinValues;   
                            }
                        }
                    }
                }
            }
        }
        
        public IEnumerator AIPlaceCoinCoroutine()
        {
            yield return new WaitForSeconds(_gameManager.AICoinPlaceDelay);
            _gameManager.ICoinPlacer.PlaceCoin(_gameManager.CellIndexToUse);
        }

        public IEnumerator AnimateCoinEffect(SpriteRenderer coinRenderer , Color? capturingColor = null)
        {
            Color originalColor = coinRenderer.color;
            float originalAlpha = originalColor.a;
            float capturingAlpha = capturingColor.HasValue ? capturingColor.Value.a : 0f;

            float fadeDuration = 0.2f;
            float fadeInterval = 0.05f;
            int fadeSteps = Mathf.RoundToInt(fadeDuration / fadeInterval);
            int fadeCycles = 3;

            for(int cycle = 0; cycle < fadeCycles; cycle++)
            {
                for(int i = 0; i < fadeSteps; i++)
                {
                    float t = (float)i / fadeSteps;
                    float alpha = Mathf.Lerp(originalAlpha , capturingAlpha, t);

                    Color newColor = new Color(originalColor.r , originalColor.g , originalColor.b , alpha);
                    coinRenderer.color = newColor;

                    yield return new WaitForSeconds(fadeInterval);
                }

                yield return new WaitForSeconds(0.1f);

                for(int i = 0; i < fadeSteps; i++)
                {
                    float t = (float)i / fadeSteps;
                    float alpha = Mathf.Lerp(capturingAlpha , originalAlpha , t);

                    Color newColor = new Color(originalColor.r , originalColor.g , originalColor.b , alpha);
                    coinRenderer.color = newColor;

                    yield return new WaitForSeconds(fadeInterval);
                }

                yield return new WaitForSeconds(0.1f);
            }

            coinRenderer.color = new Color(originalColor.r , originalColor.g , originalColor.b , originalAlpha);
        }

        public Vector2Int FindCellToPlaceCoinOn()
        {
            ClearLists();
            PopulateLists();

            Vector2Int targetCellIndex = _gridManager.InvalidCellIndex;
            List<Vector2Int> attackCellIndicesList = new List<Vector2Int>();
            List<Vector2Int> buffUpCellIndicesList = new List<Vector2Int>();

            if(_gameManager.LesserCoinValuesList.Count > 0)
            {
                attackCellIndicesList = FindAdjacentCellIndicesList(_gameManager.LesserCoinValuesList);
            }

            if(_gameManager.SelfCoinValuesList.Count > 0)
            {
                buffUpCellIndicesList = FindAdjacentCellIndicesList(_gameManager.SelfCoinValuesList);
            }

            if(attackCellIndicesList.Count > 0)
            {
                bool shouldPlaceCoin = false;
                int maxSum = 0;
                Vector2Int bestAdjacentCellIndex = _gridManager.InvalidCellIndex;

                foreach(Vector2Int adjacentCellIndex in attackCellIndicesList)
                {
                    int currentSum = 0;

                    if(adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                    {
                        List<Vector2Int> adjacentAdjacentCellIndicesList = GetAdjacentCellIndicesList(adjacentCellIndex);

                        foreach (Vector2Int adjacentAdjacentCellIndex in adjacentAdjacentCellIndicesList)
                        {
                            bool isAdjacentCellOwnedByCurrentPlayer = _gridManager.PlayerIDData.GetValue(adjacentAdjacentCellIndex.x , adjacentAdjacentCellIndex.y) == _gameManager.CurrentPlayerID;
                            int adjacentAdjacentCellCoinValue = _gridManager.CoinValueData.GetValue(adjacentAdjacentCellIndex.x , adjacentAdjacentCellIndex.y);

                            if(_gameManager.LesserCoinValuesList.Contains(adjacentAdjacentCellCoinValue))
                            {
                                if(isAdjacentCellOwnedByCurrentPlayer)
                                {
                                    currentSum++;
                                    //Debug.Log("Attack Block -> Coin(s) Belongs to the current player so current sum : " + currentSum);
                                }
                                else
                                {
                                    currentSum += adjacentAdjacentCellCoinValue;
                                    //Debug.Log("Attack Block -> Coin(s) Belongs to the other player so current sum : " + currentSum);
                                }
                            }
                        }
                        
                        bool allAdjacentCoinsOwnedByCurrentPlayer = adjacentAdjacentCellIndicesList.All(index => _gridManager.PlayerIDData.GetValue(index.x , index.y) == _gameManager.CurrentPlayerID);

                        if(allAdjacentCoinsOwnedByCurrentPlayer && currentSum > 0 && allAdjacentCoinsOwnedByCurrentPlayer)
                        {
                            currentSum = 1;
                        }

                        if(currentSum > maxSum && _gameManager.CoinValue > _gameManager.MinCoinValue && (_gameManager.CoinValue - _gameManager.LesserCoinValuesList[0] <= _gameManager.MaxDifferenceAttack || _gameManager.CoinValue == _gameManager.MaxCoinValue))
                        {
                            maxSum = currentSum;
                            bestAdjacentCellIndex = adjacentCellIndex;
                            shouldPlaceCoin = true;
                        }
                    }
                }

                if(!shouldPlaceCoin)
                {
                    foreach(Vector2Int vulnerableCoinIndex in _gameManager.LesserCoinsCellIndicesList)
                    {
                        List<Vector2Int> adjacentCellIndicesListForVulnerableCoin = GetAdjacentCellIndicesList(vulnerableCoinIndex);

                        int unblockedNeighbors = adjacentCellIndicesListForVulnerableCoin.Count(adjacentCellIndex =>
                        adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y));

                        if(unblockedNeighbors == 1)
                        {
                            targetCellIndex = adjacentCellIndicesListForVulnerableCoin
                            .FirstOrDefault(adjacentCellIndex => adjacentCellIndex != _gridManager.InvalidCellIndex &&
                            !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y));

                            //Debug.Log("Attack Block -> Placing coin to protect vulnerable coin at Cell Index : " + "(" + (targetCellIndex.x + " , " + targetCellIndex.y) + " )");
                            return targetCellIndex;
                        }
                    }
                }
                
                else if(bestAdjacentCellIndex != _gridManager.InvalidCellIndex)
                {
                    targetCellIndex = bestAdjacentCellIndex;
                    //Debug.Log("Attack Block -> Chosen Cell Index : " + "(" + (targetCellIndex.x + " , " + targetCellIndex.y) + " ) with sum : " + maxSum);
                    return targetCellIndex;
                }
            }

            if(buffUpCellIndicesList.Count > 0)
            {
                int highestValueCoin = _gameManager.SelfCoinValuesList[0];
                List<Vector2Int> vulnerableCoins = new List<Vector2Int>();
                
                foreach(int coinValue in _gameManager.SelfCoinValuesList)
                {
                    if(coinValue < highestValueCoin)
                    {
                        List<Vector2Int> coinIndices = _gameManager.SelfCoinsCellIndicesList
                        .Where(cellIndex => _gridManager.CoinValueData.GetValue(cellIndex.x , cellIndex.y) == coinValue)
                        .ToList();

                        foreach(Vector2Int coinIndex in coinIndices)
                        {
                            List<Vector2Int> adjacentCellIndicesListForVulnerableCoin = GetAdjacentCellIndicesList(coinIndex);

                            int unblockedNeighbors = adjacentCellIndicesListForVulnerableCoin
                            .Count(adjacentCellIndex =>
                            adjacentCellIndex != _gridManager.InvalidCellIndex &&
                            !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y));
                            
                            if(unblockedNeighbors == 1)
                            {
                                vulnerableCoins.Add(coinIndex);
                            }
                        }
                    }
                }
                
                if(vulnerableCoins.Count > 0)
                {
                    Vector2Int vulnerableCellIndex = vulnerableCoins.First();

                    List<Vector2Int> adjacentCells = GetAdjacentCellIndicesList(vulnerableCellIndex)
                    .Where(adjacentCellIndex =>
                    adjacentCellIndex != _gridManager.InvalidCellIndex &&
                    !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                    .ToList();
                    
                    if(adjacentCells.Count == 1)
                    {
                        targetCellIndex = adjacentCells.First();
                        //Debug.Log("Buff Up Block -> Chosen Cell Index to protect vulnerable coin: " + "(" + (targetCellIndex.x + " , " + targetCellIndex.y) + " )");
                        return targetCellIndex;
                    }
                }
                
                Vector2Int bestAdjacentCellIndex = _gridManager.InvalidCellIndex;
                int maxSum = 0;

                Vector2Int highestValueCoinCellIndex = _gameManager.SelfCoinsCellIndicesList.First(cellIndex => _gridManager.CoinValueData.GetValue(cellIndex.x , cellIndex.y) == highestValueCoin);

                List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(highestValueCoinCellIndex);

                foreach(Vector2Int adjacentCellIndex in adjacentCellIndicesList)
                {
                    int currentSum = 0;

                    if(adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                    {
                        List<Vector2Int> adjacentAdjacentCellIndicesList = GetAdjacentCellIndicesList(adjacentCellIndex);

                        foreach(Vector2Int adjacentAdjacentCellIndex in adjacentAdjacentCellIndicesList)
                        {
                            int adjacentAdjacentCellCoinValue = _gridManager.CoinValueData.GetValue(adjacentAdjacentCellIndex.x , adjacentAdjacentCellIndex.y);

                            if(adjacentAdjacentCellCoinValue > 0 && _gameManager.CurrentPlayerID == _gridManager.PlayerIDData.GetValue(adjacentAdjacentCellIndex.x , adjacentAdjacentCellIndex.y))
                            {
                                currentSum++;
                            }
                        }

                        if((currentSum > maxSum && _gameManager.CoinValue <= highestValueCoin && highestValueCoin < _gameManager.MaxCoinValue) || (currentSum > maxSum && _gameManager.CoinValue <= highestValueCoin))
                        {
                            maxSum = currentSum;
                            bestAdjacentCellIndex = adjacentCellIndex;
                        }
                    }
                }

                if(bestAdjacentCellIndex != _gridManager.InvalidCellIndex)
                {
                    targetCellIndex = bestAdjacentCellIndex;
                    //Debug.Log("Buff Up Block -> Chosen Cell Index : " + "(" + (targetCellIndex.x + " , " + targetCellIndex.y) + " ) with sum : " + maxSum);
                    return targetCellIndex;
                }
            }
            
            if(_gameManager.UnblockedCellIndicesList.Count > 0)
            {
                targetCellIndex = _gridManager.InvalidCellIndex;
                int minUnblockedAdjacentCellIndicesCount = int.MaxValue;
            
                foreach(Vector2Int cellIndex in _gameManager.UnblockedCellIndicesList)
                {
                    List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(cellIndex);
            
                    int unblockedAdjacentCellIndicesCount = adjacentCellIndicesList.Count(adjacentCellIndex => adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y));
            
                    if(unblockedAdjacentCellIndicesCount < minUnblockedAdjacentCellIndicesCount || (unblockedAdjacentCellIndicesCount == 0 && minUnblockedAdjacentCellIndicesCount > 0))
                    {
                        targetCellIndex = cellIndex;
                        minUnblockedAdjacentCellIndicesCount = unblockedAdjacentCellIndicesCount;
                    }
                }
            
                if(_gameManager.CoinValue >= _gameManager.MinHigherCoinValue && _gameManager.CoinValue <= _gameManager.MaxHigherCoinValue || (_gameManager.CoinValue == 17 || _gameManager.CoinValue == 18))
                {
                    if(minUnblockedAdjacentCellIndicesCount <= 3)
                    {
                        //Debug.Log("Random Block -> The number of neighbors of the Chosen Cell Index : " + "(" + (targetCellIndex.x + " , " + targetCellIndex.y) + " ) is : " + minUnblockedAdjacentCellIndicesCount);
                        //Debug.Log("Random Block -> " + _gameManager.CoinValue + " is greater than or equal to " + _gameManager.MinHigherCoinValue + " , less than or equal to " + _gameManager.MaxHigherCoinValue + " or coin value is 17 or 18");
                        return targetCellIndex;
                    }
                }
                else
                {
                    if(minUnblockedAdjacentCellIndicesCount == 0)
                    {
                        //Debug.Log("Random Block -> The number of neighbors of the Chosen Cell Index : " + "(" + (targetCellIndex.x + " , " + targetCellIndex.y) + " ) is : " + minUnblockedAdjacentCellIndicesCount);
                        return targetCellIndex;
                    }

                    int index = Random.Range(0 , _gameManager.UnblockedCellIndicesList.Count);
                    targetCellIndex = _gameManager.UnblockedCellIndicesList[index];
                    //Debug.Log("Random Block -> No particular condition satisfied so selected the cell index : " + "(" + (targetCellIndex.x + " , " + targetCellIndex.y) + " ) at random");
                }
                
                if(targetCellIndex != _gridManager.InvalidCellIndex)
                {
                    return targetCellIndex;
                }
            }

            return _gridManager.InvalidCellIndex;
        }
        
        #endregion
    }
}