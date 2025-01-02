namespace Managers
{
    using Photon.Pun;
    using Photon.Realtime;
    //using UnityEngine;

    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        private string _lobbyName = "ProximityLobby";

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            if(!PhotonNetwork.IsConnected)
            {
                //Debug.Log("Connecting to Photon...");
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            //Debug.Log("Connected to Photon Master Server.");
        }

        public override void OnJoinedLobby()
        {
            //Debug.Log("Joined the lobby.");
            EventsManager.Invoke(Event.LobbyJoinButtonPressed);
            JoinOrCreateRoom();
        }

        public override void OnJoinedRoom()
        {
            //Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
            EventsManager.Invoke(Event.PlayerJoinedRoom);
            UpdatePlayerList();
        }

        private void JoinOrCreateRoom()
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 4,
                IsVisible = true,
                IsOpen = true
            };
        
            PhotonNetwork.JoinOrCreateRoom(_lobbyName , roomOptions , TypedLobby.Default);
        }
        
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            //Debug.Log($"{newPlayer.NickName} has joined the room.");
            EventsManager.Invoke(Event.PlayerJoinedRoom);
            UpdatePlayerList();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            //Debug.Log($"{otherPlayer.NickName} has left the room.");
            UpdatePlayerList();
        }
        
        private void UpdatePlayerList()
        {
            Player[] players = PhotonNetwork.PlayerList;
            string playerNames = "Players Joined the Lobby\n";

            foreach(Player player in players)
            {
                playerNames += $"{player.NickName}\n";
            }
            
            EventsManager.Invoke(Event.LobbyPlayersListUpdated , players);
        }
    }
}