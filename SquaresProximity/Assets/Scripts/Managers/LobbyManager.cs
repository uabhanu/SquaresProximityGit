namespace Managers
{
    using Photon.Pun;
    using Photon.Realtime;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_InputField playerNameTMPInputField;
        [SerializeField] private TMP_Text lobbyPlayersListTMPText;
        [SerializeField] private string lobbyName = "ProximityLobby";

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            
            startButton.gameObject.SetActive(false);
            startButton.onClick.AddListener(OnStartButtonPressed);

            if(!PhotonNetwork.IsConnected)
            {
                Debug.Log("Connecting to Photon...");
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Photon Master Server.");
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("Joined the lobby.");
            EventsManager.Invoke(Event.LobbyJoinButtonPressed);
            JoinOrCreateRoom();
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
            UpdatePlayerList();
            UpdateStartButtonState();
        }
        
        public void OnJoinLobbyButtonClicked()
        {
            string playerName = playerNameTMPInputField.text.Trim();

            if(string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("Player name cannot be empty!");
                return;
            }

            PhotonNetwork.NickName = playerName;

            if(PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinLobby();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        private void JoinOrCreateRoom()
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 4,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.JoinOrCreateRoom(lobbyName , roomOptions , TypedLobby.Default);
        }
        
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"{newPlayer.NickName} has joined the room.");
            UpdatePlayerList();
            UpdateStartButtonState();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"{otherPlayer.NickName} has left the room.");
            UpdatePlayerList();
            UpdateStartButtonState();
        }

        private void OnStartButtonPressed()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Starting the game...");
                int totalPlayers = PhotonNetwork.PlayerList.Length;
                Debug.Log("Total Number Of Players Joined : " + totalPlayers);
                //TODO Create an event and invoke it to send the totalPlayers and GameManager should listen to this and set the numberOfPlayers to this value if _onlineMode is true or something like that
            }
            else
            {
                Debug.LogError("Only the MasterClient can start the game.");
            }
        }
        
        private void UpdatePlayerList()
        {
            if(lobbyPlayersListTMPText == null)
            {
                Debug.LogError("lobbyPlayersListTMPText is not assigned in the Inspector!");
                return;
            }

            Player[] players = PhotonNetwork.PlayerList;
            string playerNames = "Players Joined the Lobby\n";

            foreach(Player player in players)
            {
                playerNames += $"{player.NickName}\n";
            }

            lobbyPlayersListTMPText.text = playerNames;
        }
        
        private void UpdateStartButtonState()
        {
            bool isMasterClient = PhotonNetwork.IsMasterClient;
            startButton.gameObject.SetActive(isMasterClient);
            startButton.interactable = isMasterClient && PhotonNetwork.PlayerList.Length > 1;
        }
    }
}