namespace Managers
{
    using Data;
    using Photon.Pun;
    using Random = UnityEngine.Random;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class GridManager : MonoBehaviourPunCallbacks
    {
        #region Variables Declarations

        private static readonly int HoleSize = Shader.PropertyToID("_HoleSize");

        private bool _playerOnlineStatus;
        private bool _shouldGenerateEmptyCellsBool;
        private GridData<bool> _isCellBlockedData;
        private GridData<GameObject> _coinOnTheCellData;
        private GridData<GameObject> _cellPrefabData;
        private GridData<int> _coinValueData;
        private GridData<int> _playerIDData;
        private GridData<SpriteRenderer> _cellSpriteRenderersData;
        private int _holeCellsCount;
        private int _randomSpritesIndex;
        private int _totalCells;
        private readonly Vector2Int _invalidCellIndex = new(-1, -1);

        [HideInInspector] [SerializeField] private GridInfo gridInfo;

        [SerializeField] private bool isTestingMode;
        [SerializeField] private float holeSize;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private int columns;
        [SerializeField] private int rows;
        [SerializeField] private Material holeMaterial;
        [SerializeField] private Sprite[] availableSprites;

        #endregion

        #region Helper Properties

        public GridData<bool> IsCellBlockedData
        {
            get => _isCellBlockedData;
            set => _isCellBlockedData = value;
        }

        public GridData<GameObject> CoinOnTheCellData
        {
            get => _coinOnTheCellData;
            set => _coinOnTheCellData = value;
        }

        public GridData<int> CoinValueData
        {
            get => _coinValueData;
            set => _coinValueData = value;
        }

        public GridData<int> PlayerIDData
        {
            get => _playerIDData;
            set => _playerIDData = value;
        }

        public GridData<SpriteRenderer> CellSpriteRenderersData
        {
            get => _cellSpriteRenderersData;
            set => _cellSpriteRenderersData = value;
        }

        public GridInfo GridInfo => gridInfo;

        public int TotalCells
        {
            get => _totalCells;
            set => _totalCells = value;
        }

        public Vector2Int InvalidCellIndex => _invalidCellIndex;

        #endregion

        #region MonoBehaviour Functions

        private void Awake()
        {
            _randomSpritesIndex = Random.Range(0 , availableSprites.Length);

            if(isTestingMode)
            {
                GridInfo.Cols = 12;
                GridInfo.Rows = 1;
            }
            else
            {
                GridInfo.Cols = columns;
                GridInfo.Rows = rows;
            }

            InitializeGridData();
            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }

        #endregion

        #region User Defined Functions

        public Vector2 CellToWorld(int col, int row)
        {
            var x = (col * GridInfo.CellSize) + transform.position.x;
            var y = row * GridInfo.CellSize + transform.position.y;
            return new Vector2(x, y);
        }
        
        private void CreateCells(Vector2Int cellIndex , bool isHole)
        {
            Vector2 cellWorldPos = CellToWorld(cellIndex.x , cellIndex.y);
            GameObject cellObject = Instantiate(cellPrefab , cellWorldPos , Quaternion.identity , transform);
            SpriteRenderer cellRenderer = cellObject.GetComponentInChildren<SpriteRenderer>();
            cellRenderer.sprite = availableSprites[_randomSpritesIndex];

            if(isHole)
            {
                cellRenderer.material.SetFloat(HoleSize , holeSize);
            }
            
            _cellPrefabData.SetValue(cellIndex.x , cellIndex.y , cellObject);
            CellSpriteRenderersData.SetValue(cellIndex.x , cellIndex.y , cellRenderer);
            CoinOnTheCellData.SetValue(cellIndex.x , cellIndex.y , cellObject);
        }

        private void GenerateGridOffline()
        {
            List<Vector2Int> cellIndices = new();
            
            for(int col = 0; col < GridInfo.Cols; col++)
            {
                for(int row = 0; row < GridInfo.Rows; row++)
                {
                    cellIndices.Add(new Vector2Int(col, row));
                }
            }
            
            if(_shouldGenerateEmptyCellsBool)
            {
                TotalCells -= _holeCellsCount;

                if(_holeCellsCount > cellIndices.Count)
                {
                    _holeCellsCount = cellIndices.Count;
                }

                for(int i = 0; i < _holeCellsCount; i++)
                {
                    int randomIndex = Random.Range(0 , cellIndices.Count);
                    Vector2Int cellIndex = cellIndices[randomIndex];
                    cellIndices.RemoveAt(randomIndex);

                    IsCellBlockedData.SetValue(cellIndex.x , cellIndex.y , true);
                    CreateCells(cellIndex , isHole: true);
                }
            }
            
            foreach(Vector2Int cellIndex in cellIndices)
            {
                CreateCells(cellIndex , isHole: false);
            }
        }
        
        private void GenerateGridOnline()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                List<Vector2Int> generatedCells = new();
                
                for(int col = 0; col < GridInfo.Cols; col++)
                {
                    for(int row = 0; row < GridInfo.Rows; row++)
                    {
                        generatedCells.Add(new Vector2Int(col , row));
                    }
                }
                
                if(_shouldGenerateEmptyCellsBool)
                {
                    TotalCells -= _holeCellsCount;

                    if(_holeCellsCount > generatedCells.Count)
                    {
                        _holeCellsCount = generatedCells.Count;
                    }

                    for(int i = 0; i < _holeCellsCount; i++)
                    {
                        int randomIndex = Random.Range(0 , generatedCells.Count);
                        Vector2Int cellIndex = generatedCells[randomIndex];
                        generatedCells.RemoveAt(randomIndex);

                        IsCellBlockedData.SetValue(cellIndex.x , cellIndex.y , true);
                        CreateCells(cellIndex , isHole: true);
                    }
                }
                
                foreach(Vector2Int cellIndex in generatedCells)
                {
                    CreateCells(cellIndex, isHole: false);
                }
                
                NetworkManager.Instance.SetSyncedGridCells(generatedCells);
            }
            else
            {
                var syncedGridCells = NetworkManager.Instance.SyncedGridCells;

                foreach(var cellIndex in syncedGridCells)
                {
                    bool isHole = IsCellBlockedData.GetValue(cellIndex.x , cellIndex.y);
                    CreateCells(cellIndex , isHole);
                }
            }
        }

        private int GetRandomDivisibleNumber(int minValue , int maxValue , int divisor1 , int divisor2)
        {
            int randomValue = Random.Range(minValue , maxValue + 1);

            while(randomValue % divisor1 != 0 || randomValue % divisor2 != 0)
            {
                randomValue = Random.Range(minValue , maxValue + 1);
            }

            return randomValue;
        }

        private void InitializeGridData()
        {
            _cellPrefabData = new GridData<GameObject>(gridInfo);
            _cellSpriteRenderersData = new GridData<SpriteRenderer>(gridInfo);
            _coinOnTheCellData = new GridData<GameObject>(gridInfo);
            _coinValueData = new GridData<int>(gridInfo);
            _holeCellsCount = GetRandomDivisibleNumber(18 , 30 , 2 , 3);
            _isCellBlockedData = new GridData<bool>(gridInfo);
            _playerIDData = new GridData<int>(gridInfo);
            _totalCells = gridInfo.Cols * gridInfo.Rows;
        }

        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector2 localPosition = (worldPosition / GridInfo.CellSize - transform.position);

            int col = Mathf.FloorToInt(localPosition.x);
            int row = Mathf.FloorToInt(localPosition.y);

            if (col >= 0 && col < GridInfo.Cols && row >= 0 && row < GridInfo.Rows)
            {
                return new Vector2Int(col, row);
            }

            return InvalidCellIndex;
        }

        #endregion

        #region Events Related Functions

        private void OnGameStarted()
        {
            if(_playerOnlineStatus && PhotonNetwork.IsMasterClient)
            {
                GenerateGridOnline();
            }
            else
            {
                GenerateGridOffline();
            }
        }

        private void OnHolesToggled()
        {
            _shouldGenerateEmptyCellsBool = !_shouldGenerateEmptyCellsBool;
        }

        private void OnPlayerOnlineStatusUpdate(bool playerNowOnline)
        {
            _playerOnlineStatus = playerNowOnline;
        }

        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Event.GameStarted , new Action(OnGameStarted));
                EventsManager.SubscribeToEvent(Event.HolesToggled , new Action(OnHolesToggled));
                EventsManager.SubscribeToEvent(Event.PlayerOnlineStatus , (Action<bool>)OnPlayerOnlineStatusUpdate);
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.GameStarted , new Action(OnGameStarted));
                EventsManager.UnsubscribeFromEvent(Event.HolesToggled , new Action(OnHolesToggled));
                EventsManager.UnsubscribeFromEvent(Event.PlayerOnlineStatus , (Action<bool>)OnPlayerOnlineStatusUpdate);
            }
        }

        #endregion
    }
}