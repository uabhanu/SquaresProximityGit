using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class NetworkManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Variables
        
        private List<Vector2Int> _syncedGridCells = new();
        
        #endregion
        
        #region Singleton

        public static NetworkManager Instance { get; private set; }

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }

        #endregion

        #region Public Properties

        public bool IsConnected => PhotonNetwork.IsConnected;
        public bool IsMasterClient => PhotonNetwork.IsMasterClient;
        public int PlayerCount => PhotonNetwork.PlayerList.Length;
        public int SyncedScore { get; private set; } 
        public List<Vector2Int> SyncedGridCells => new(_syncedGridCells);

        #endregion

        #region Public Methods

        public void Connect()
        {
            if(!PhotonNetwork.IsConnected)
            {
                Debug.Log("Connecting to Photon...");
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                Debug.Log("Already connected to Photon.");
            }
        }

        public void CreateRoom(string roomName)
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 4,
                IsVisible = true,
                IsOpen = true
            };
            
            PhotonNetwork.CreateRoom(roomName , roomOptions);
        }

        public void Disconnect()
        {
            if(PhotonNetwork.IsConnected)
            {
                Debug.Log("Disconnecting from Photon...");
                PhotonNetwork.Disconnect();
            }
        }

        public GameObject InstantiateNetworkObject(string prefabName , Vector3 position , Quaternion rotation)
        {
            return PhotonNetwork.Instantiate(prefabName , position , rotation);
        }

        public void JoinLobby()
        {
            PhotonNetwork.JoinLobby();
        }

        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
        
        public void SendObservedData(string methodName , object[] parameters)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                foreach(var player in PhotonNetwork.PlayerList)
                {
                    photonView.RPC(methodName , player , parameters);
                }
            }
            else
            {
                Debug.LogError("Only the MasterClient can broadcast observed data.");
            }
        }


        public void SetPlayerName(string playerName)
        {
            PhotonNetwork.NickName = playerName;
        }
        
        public void SetSyncedGridCells(List<Vector2Int> gridCells)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                _syncedGridCells = gridCells;
                Debug.Log("Grid data set and ready to sync.");
            }
        }
        
        public void UpdateScore(int newScore)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                SyncedScore = newScore;
                Debug.Log($"Score updated to : {SyncedScore}");
            }
        }

        #endregion

        #region Photon Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Photon Master Server.");
            PhotonNetwork.AutomaticallySyncScene = true;
            base.OnConnectedToMaster();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"Disconnected from Photon. Reason : {cause}");
            base.OnDisconnected(cause);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined room : {PhotonNetwork.CurrentRoom.Name}");
            EventsManager.Invoke(Event.PlayerJoinedRoom);
            base.OnJoinedRoom();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"Player {newPlayer.NickName} entered the room.");
            base.OnPlayerEnteredRoom(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"Player {otherPlayer.NickName} left the room.");
            base.OnPlayerLeftRoom(otherPlayer);
        }

        #endregion

        #region Photon Observables (Synchronization)
        
        public void OnPhotonSerializeView(PhotonStream stream , PhotonMessageInfo info)
        {
            if(stream.IsWriting)
            {
                stream.SendNext(_syncedGridCells.ToArray());
            }
            else
            {
                _syncedGridCells = new List<Vector2Int>((Vector2Int[])stream.ReceiveNext());
                Debug.Log("Grid data synced from MasterClient.");
            }
        }

        #endregion

        #region Event Functions

        private void OnPlayerOnlineStatusUpdate(bool playerOnlineStatus)
        {
            if (playerOnlineStatus && !PhotonNetwork.IsConnected)
            {
                Debug.Log("Connecting to Photon...");
                Connect();
            }
            
            else if(!playerOnlineStatus && PhotonNetwork.IsConnected)
            {
                Debug.Log("Disconnecting from Photon...");
                Disconnect();
            }
        }

        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Event.PlayerOnlineStatus , (Action<bool>)OnPlayerOnlineStatusUpdate);
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.PlayerOnlineStatus , (Action<bool>)OnPlayerOnlineStatusUpdate);
            }
        }

        #endregion
    }
}