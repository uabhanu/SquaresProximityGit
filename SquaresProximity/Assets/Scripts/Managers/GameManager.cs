namespace Managers
{
    using Interfaces;
    using Misc;
    using Random = UnityEngine.Random;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.SceneManagement;
    
    public class GameManager : MonoBehaviour
    {
        #region Constructor
    
        public GameManager(IAIManager _iaiManager , ICoinPlacer iCoinPlacer , IPlayerTurnsManager iPlayerTurnsManager)
        {
            _iAIManager = _iaiManager;
            _iCoinPlacer = iCoinPlacer;
            _iPlayerTurnsManager = iPlayerTurnsManager;
        }
    
        #endregion
    
        #region Interfaces Declarations

        private IAIManager _iAIManager;
        private ICoinPlacer _iCoinPlacer;
        private IPlayerTurnsManager _iPlayerTurnsManager;

        #endregion
    
        #region Variables Declarations
    
        private bool _isGameStarted;
        private bool _isGamePaused;
        private bool _isMoving;
        private bool _isRandomTurns;
        private bool[] _isAIArray;
        private GameObject _coinUIObj;
        private GameObject _trailObj;
        private GridManager _gridManager;
        private int _coinValue;
        private int _currentPlayerID;
        private int _numberOfPlayers;
        private int _playersListsCapacity;
        private int[] _totalReceivedArray;
        private List<int> _lesserCoinValuesList;
        private List<int> _otherPlayerCoinValuesList;
        private List<int> _selfCoinValuesList;
        private List<Vector2Int> _otherPlayerCoinsCellIndicesList;
        private List<Vector2Int> _lesserCoinsCellIndicesList;
        private List<Vector2Int> _selfCoinsCellIndicesList;
        private List<Vector2Int> _unblockedCellIndicesList;
        private List<int> _playersRemainingList;
        private List<List<int>> _playerNumbersList;
        private Vector2Int _cellIndexAtJoystickPosition;
        private Vector2Int _cellIndexAtMousePosition;
        private Vector2Int _cellIndexToUse;

        [SerializeField] private bool isTestingMode;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Color[] coinBackgroundColours;
        [SerializeField] private Color[] coinForegroundColours;
        [SerializeField] private float aiCoinPlaceDelay;
        [SerializeField] private GameObject coinObj;
        [SerializeField] private GameObject trailObj;
        [SerializeField] private int coinValueForTesting;
        [SerializeField] private int maxCoinValue;
        [SerializeField] private int maxDifferenceAttack;
        [SerializeField] private int maxHigherCoinValue;
        [SerializeField] private int minCoinValue;
        [SerializeField] private int minHigherCoinValue;

        public bool IsRandomTurns => _isRandomTurns;
        public bool[] IsAIArray => _isAIArray;
        public float AICoinPlaceDelay => aiCoinPlaceDelay;
        public GameObject CoinObj => coinObj;
        public GameObject CoinUIObj => _coinUIObj;
        public GameObject TrailObj => _trailObj;
        public IAIManager IAIManager => _iAIManager;
        public ICoinPlacer ICoinPlacer => _iCoinPlacer;
        public int MaxCoinValue => maxCoinValue;
        public int MaxDifferenceAttack => maxDifferenceAttack;
        public int MaxHigherCoinValue => maxHigherCoinValue;
        public int MinCoinValue => minCoinValue;

        public int MinHigherCoinValue => minHigherCoinValue;
        public int NumberOfPlayers => _numberOfPlayers;
        public List<List<int>> PlayerNumbersList => _playerNumbersList;
        public List<int> PlayersRemainingList => _playersRemainingList;
        public IPlayerTurnsManager IPlayerTurnsManager => _iPlayerTurnsManager;
        
        public bool IsGamePaused
        {
            get => _isGamePaused;
            set => _isGamePaused = value;
        }
        
        public bool IsMoving
        {
            get => _isMoving;
            set => _isMoving = value;
        }
        
        public int CoinValue
        {
            get => _coinValue;
            set => _coinValue = value;
        }
    
        public int CurrentPlayerID
        {
            get => _currentPlayerID;
            set => _currentPlayerID = value;
        }
    
        public int[] TotalReceivedArray
        {
            get => _totalReceivedArray;
            set => _totalReceivedArray = value;
        }
        
        public List<int> LesserCoinValuesList
        {
            get => _lesserCoinValuesList;
            set => _lesserCoinValuesList = value;
        }
        
        public List<int> OtherPlayerCoinValuesList
        {
            get => _otherPlayerCoinValuesList;
            set => _otherPlayerCoinValuesList = value;
        }
        
        public List<int> SelfCoinValuesList
        {
            get => _selfCoinValuesList;
            set => _selfCoinValuesList = value;
        }
        
        public List<Vector2Int> LesserCoinsCellIndicesList
        {
            get => _lesserCoinsCellIndicesList;
            set => _lesserCoinsCellIndicesList = value;
        }
        
        public List<Vector2Int> OtherPlayerCoinsCellIndicesList
        {
            get => _otherPlayerCoinsCellIndicesList;
            set => _otherPlayerCoinsCellIndicesList = value;
        }
        
        public List<Vector2Int> SelfCoinsCellIndicesList
        {
            get => _selfCoinsCellIndicesList;
            set => _selfCoinsCellIndicesList = value;
        }
        
        public List<Vector2Int> UnblockedCellIndicesList
        {
            get => _unblockedCellIndicesList;
            set => _unblockedCellIndicesList = value;
        }

        public Vector2Int CellIndexToUse
        {
            get => _cellIndexToUse;
            set => _cellIndexToUse = value;
        }

        #endregion

        #region MonoBehaviour Functions
    
        private void Start()
        {
            _coinUIObj = GameObject.Find("CoinUI");
        
            #if UNITY_STANDALONE || UNITY_WEBGL
                _trailObj = Instantiate(trailObj , Vector3.zero , Quaternion.identity , gameObject.transform);
            #endif
            
            ServiceLocator.Register(new OnlineMode());
            ToggleEventSubscription(true);
            UpdateTrailVisibility();
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }

        private void Update()
        {
            if(!_isGameStarted) return;
        
            if(_gridManager.TotalCells == 0)
            {
                EventsManager.Invoke(Event.GameOver);
                EventsManager.Invoke(Event.PlayerTotalReceived , TotalReceivedArray);
            }
        }
    
        #endregion

        #region User Defined Functions
        
        public Color GetCoinBackgroundColour(int index)
        {
            return coinBackgroundColours[CurrentPlayerID];
        }
        
        public Color GetCoinForegroundColour(int index)
        {
            return coinForegroundColours[CurrentPlayerID];
        }
    
        public int GetCurrentCoinValue()
        {
            int currentPlayerID = CurrentPlayerID;
        
            if(PlayerNumbersList[currentPlayerID].Count > 0)
            {
                return PlayerNumbersList[currentPlayerID][0];
            }
        
            return 0;
        }
        
        private Vector2Int GetPlayerCellIndex()
        {
            if(IsAIArray[CurrentPlayerID])
            {
                return _iAIManager.FindCellToPlaceCoinOn();
            }

            return _cellIndexAtMousePosition;
        }
        
        public void ResetPlayersRemaining()
        {
            PlayersRemainingList.Clear();

            for(int i = 0; i < NumberOfPlayers; i++)
            {
                PlayersRemainingList.Add(i);
            }
        }

        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
        
            while(n > 1)
            {
                n--;
                int k = Random.Range(0 , n + 1);
                (list[k] , list[n]) = (list[n] , list[k]);
            }
        }

        public void UpdateTrailVisibility()
        {
            #if UNITY_STANDALONE || UNITY_WEBGL
                TrailObj.SetActive(IsMoving);
            #endif
        }
    
        #endregion
    
        #region Events Related Functions

        private void OnAIHumanToggled(int playerID , bool isAI)
        {
            IsAIArray[playerID] = isAI;
        }

        private void OnGameOver()
        {
            _isGameStarted = false;
        }

        private void OnGamePaused()
        {
            IsGamePaused = true;
            Time.timeScale = 0;
        }

        private void OnGameRestarted()
        {
            _isGameStarted = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
        }
    
        private void OnGameResumed()
        {
            IsGamePaused = false;
            Time.timeScale = 1;
        }
    
        private void OnGameStarted()
        {
            _isGameStarted = true;

            _gridManager = FindAnyObjectByType<GridManager>();
            
            _iAIManager = new AIManager(this , _gridManager);
            _iPlayerTurnsManager = new PlayerTurnsManager(this , _gridManager);
            _iCoinPlacer = new CoinPlacer(this , _gridManager);

            IPlayerTurnsManager.UpdateTrailColor();

            _playersListsCapacity = _gridManager.TotalCells / NumberOfPlayers;

            SelfCoinsCellIndicesList = new List<Vector2Int>();
            SelfCoinValuesList = new List<int>();
            LesserCoinsCellIndicesList = new List<Vector2Int>();
            LesserCoinValuesList = new List<int>();
            OtherPlayerCoinsCellIndicesList = new List<Vector2Int>();
            OtherPlayerCoinValuesList = new List<int>();
            UnblockedCellIndicesList = new List<Vector2Int>();
            _playerNumbersList = new List<List<int>>();
            _playersRemainingList = new List<int>();

            for(int i = 0; i < NumberOfPlayers; i++)
            {
                PlayersRemainingList.Add(i);
            }

            for(int i = 0; i < NumberOfPlayers; i++)
            {
                List<int> numbers = new List<int>(_playersListsCapacity);

                if(isTestingMode)
                {
                    CoinValue = coinValueForTesting;
                    numbers.Add(CoinValue);
                }
                else
                {
                    for(int j = 0; j < _playersListsCapacity; j++)
                    {
                        int randomValue = Random.Range(1 , 21);
                        CoinValue = randomValue;
                        numbers.Add(CoinValue);
                    }   
                }

                for(i = 0; i < NumberOfPlayers; i++)
                {
                    ShuffleList(numbers);
                    PlayerNumbersList.Add(new List<int>(numbers));
                }
            }
        
            IPlayerTurnsManager.StartPlayerTurn();
        }
        
        private void OnJoystickDownPressed()
        {
            if(!_isGameStarted) return;

            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            IsMoving = true;
            
            _cellIndexAtJoystickPosition = new Vector2Int(_cellIndexAtJoystickPosition.x , _cellIndexAtJoystickPosition.y - 1);
            
            Vector3 worldPosition = _gridManager.CellToWorld(_cellIndexAtJoystickPosition.x , _cellIndexAtJoystickPosition.y);
            Vector2Int cellIndex = _gridManager.WorldToCell(worldPosition);

            if(cellIndex != _gridManager.InvalidCellIndex)
            {
                Vector2 snapPos = _gridManager.CellToWorld(cellIndex.x , cellIndex.y);

                #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
                    TrailObj.transform.position = snapPos;
                    UpdateTrailVisibility();
                #endif
            }
        }
        
        private void OnJoystickLeftPressed()
        {
            if(!_isGameStarted) return;

            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            IsMoving = true;
            
            _cellIndexAtJoystickPosition = new Vector2Int(_cellIndexAtJoystickPosition.x - 1 , _cellIndexAtJoystickPosition.y);
            
            Vector3 worldPosition = _gridManager.CellToWorld(_cellIndexAtJoystickPosition.x , _cellIndexAtJoystickPosition.y);
            Vector2Int cellIndex = _gridManager.WorldToCell(worldPosition);

            if(cellIndex != _gridManager.InvalidCellIndex)
            {
                Vector2 snapPos = _gridManager.CellToWorld(cellIndex.x , cellIndex.y);

                #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
                    TrailObj.transform.position = snapPos;
                    UpdateTrailVisibility();
                #endif
            }
        }
        
        private void OnJoystickRightPressed()
        {
            if(!_isGameStarted) return;

            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            IsMoving = true;
            
            _cellIndexAtJoystickPosition = new Vector2Int(_cellIndexAtJoystickPosition.x + 1 , _cellIndexAtJoystickPosition.y);
            
            Vector3 worldPosition = _gridManager.CellToWorld(_cellIndexAtJoystickPosition.x , _cellIndexAtJoystickPosition.y);
            Vector2Int cellIndex = _gridManager.WorldToCell(worldPosition);

            if(cellIndex != _gridManager.InvalidCellIndex)
            {
                Vector2 snapPos = _gridManager.CellToWorld(cellIndex.x , cellIndex.y);

                #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
                    TrailObj.transform.position = snapPos;
                    UpdateTrailVisibility();
                #endif
            }
        }

        
        private void OnJoystickUpPressed()
        {
            if(!_isGameStarted) return;

            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            IsMoving = true;
            
            _cellIndexAtJoystickPosition = new Vector2Int(_cellIndexAtJoystickPosition.x , _cellIndexAtJoystickPosition.y + 1);
            
            Vector3 worldPosition = _gridManager.CellToWorld(_cellIndexAtJoystickPosition.x , _cellIndexAtJoystickPosition.y);
            Vector2Int cellIndex = _gridManager.WorldToCell(worldPosition);

            if(cellIndex != _gridManager.InvalidCellIndex)
            {
                Vector2 snapPos = _gridManager.CellToWorld(cellIndex.x , cellIndex.y);

                #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
                    TrailObj.transform.position = snapPos;
                    UpdateTrailVisibility();
                #endif
            }
        }
        
        private void OnJoystickXPressed()
        {
            if(!_isGameStarted) return;

            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            CellIndexToUse = _cellIndexAtJoystickPosition;
            
            Debug.Log("Joystick Cell Index To Use : " + CellIndexToUse);

            if(CellIndexToUse == _gridManager.InvalidCellIndex || _gridManager.IsCellBlockedData.GetValue(CellIndexToUse.x , CellIndexToUse.y))
            {
                return;
            }

            ICoinPlacer.PlaceCoin(CellIndexToUse);
        }

        private void OnMouseLeftClicked()
        {
            if(!_isGameStarted) return;

            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }
            
            CellIndexToUse = _cellIndexAtMousePosition;

            if(CellIndexToUse == _gridManager.InvalidCellIndex || _gridManager.IsCellBlockedData.GetValue(CellIndexToUse.x , CellIndexToUse.y))
            {
                return;
            }

            ICoinPlacer.PlaceCoin(CellIndexToUse);
        }
        
        private void OnMouseMoved()
        {
            if(!_isGameStarted) return;

            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            IsMoving = true;

            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            mouseScreenPos.z = Camera.main.nearClipPlane;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            _cellIndexAtMousePosition = _gridManager.WorldToCell(mouseWorldPos); 
        
            if(_cellIndexAtMousePosition != _gridManager.InvalidCellIndex)
            {
                Vector2 snapPos = _gridManager.CellToWorld(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
                
                #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL

                    if(TrailObj != null)
                    {
                        TrailObj.transform.position = snapPos;    
                    }
                
                #endif
            }
            else
            {
                #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
                    TrailObj.transform.position = mouseWorldPos;
                #endif
            }

                #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
                    UpdateTrailVisibility();
                #endif
        }

        private void OnNumberOfPlayersSelected(int numberOfPlayers)
        {
            _numberOfPlayers = numberOfPlayers;
            _isAIArray = new bool[NumberOfPlayers];
            TotalReceivedArray = new int[NumberOfPlayers];
        }

        private void OnRandomTurnsToggled()
        {
            _isRandomTurns = !IsRandomTurns;
        }
    
        private void OnTouchscreenTapped(Vector2 touchscreenPosition)
        {
            if(!_isGameStarted) return;
            
            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(touchscreenPosition.x , touchscreenPosition.y , mainCamera.nearClipPlane));
            CellIndexToUse = _gridManager.WorldToCell(worldPosition);
            Debug.Log("Tapped on Cell Index : " + CellIndexToUse);

            if(CellIndexToUse == _gridManager.InvalidCellIndex || _gridManager.IsCellBlockedData.GetValue(CellIndexToUse.x , CellIndexToUse.y))
            {
                return;
            }

            ICoinPlacer.PlaceCoin(CellIndexToUse);
        }
    
        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Event.AIHumanToggled , (Action<int , bool>)OnAIHumanToggled);
                EventsManager.SubscribeToEvent(Event.GameOver , new Action(OnGameOver));
                EventsManager.SubscribeToEvent(Event.GamePaused , new Action(OnGamePaused));
                EventsManager.SubscribeToEvent(Event.GameRestarted , new Action(OnGameRestarted));
                EventsManager.SubscribeToEvent(Event.GameResumed , new Action(OnGameResumed));
                EventsManager.SubscribeToEvent(Event.GameStarted , new Action(OnGameStarted));
                EventsManager.SubscribeToEvent(Event.JoystickDownPressed , new Action(OnJoystickDownPressed));
                EventsManager.SubscribeToEvent(Event.JoystickLeftPressed , new Action(OnJoystickLeftPressed));
                EventsManager.SubscribeToEvent(Event.JoystickRightPressed , new Action(OnJoystickRightPressed));
                EventsManager.SubscribeToEvent(Event.JoystickUpPressed , new Action(OnJoystickUpPressed));
                EventsManager.SubscribeToEvent(Event.JoystickXPressed , new Action(OnJoystickXPressed));
                EventsManager.SubscribeToEvent(Event.MouseLeftClicked , new Action(OnMouseLeftClicked));
                EventsManager.SubscribeToEvent(Event.MouseMoved , new Action(OnMouseMoved));
                EventsManager.SubscribeToEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
                EventsManager.SubscribeToEvent(Event.RandomTurnsToggled , new Action(OnRandomTurnsToggled));
                EventsManager.SubscribeToEvent(Event.TouchscreenTapped , (Action<Vector2>)OnTouchscreenTapped);
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.AIHumanToggled , (Action<int , bool>)OnAIHumanToggled);
                EventsManager.UnsubscribeFromEvent(Event.GameOver , new Action(OnGameOver));
                EventsManager.UnsubscribeFromEvent(Event.GamePaused , new Action(OnGamePaused));
                EventsManager.UnsubscribeFromEvent(Event.GameRestarted , new Action(OnGameRestarted));
                EventsManager.UnsubscribeFromEvent(Event.GameResumed , new Action(OnGameResumed));
                EventsManager.UnsubscribeFromEvent(Event.GameStarted , new Action(OnGameStarted));
                EventsManager.UnsubscribeFromEvent(Event.JoystickDownPressed , new Action(OnJoystickDownPressed));
                EventsManager.UnsubscribeFromEvent(Event.JoystickLeftPressed , new Action(OnJoystickLeftPressed));
                EventsManager.UnsubscribeFromEvent(Event.JoystickRightPressed , new Action(OnJoystickRightPressed));
                EventsManager.UnsubscribeFromEvent(Event.JoystickUpPressed , new Action(OnJoystickUpPressed));
                EventsManager.UnsubscribeFromEvent(Event.JoystickXPressed , new Action(OnJoystickXPressed));
                EventsManager.UnsubscribeFromEvent(Event.MouseLeftClicked , new Action(OnMouseLeftClicked));
                EventsManager.UnsubscribeFromEvent(Event.MouseMoved , new Action(OnMouseMoved));
                EventsManager.UnsubscribeFromEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
                EventsManager.UnsubscribeFromEvent(Event.RandomTurnsToggled , new Action(OnRandomTurnsToggled));
                EventsManager.UnsubscribeFromEvent(Event.TouchscreenTapped , (Action<Vector2>)OnTouchscreenTapped);
            }
        }

        #endregion
    }
}
