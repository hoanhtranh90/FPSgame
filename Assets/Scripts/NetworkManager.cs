using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks {

    [SerializeField]
    private Text connectionText;
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private Camera sceneCamera;
    [SerializeField]
    private GameObject[] playerModel;
    [SerializeField]
    private GameObject serverWindow;
    [SerializeField]
    private GameObject messageWindow;
    [SerializeField]
    private GameObject sightImage;
    [SerializeField]
    private InputField username;
    [SerializeField]
    private InputField roomName;
    [SerializeField]
    private InputField roomList;
    [SerializeField]
    private InputField messagesLog;

    private GameObject player;
    private Queue<string> messages;
    private const int messageCount = 10;
    private string nickNamePrefKey = "PlayerName";

    private Text scoreBoard;

    
    void Start() {
        messages = new Queue<string> (messageCount);
        // Tìm đối tượng Text có tag là "Score"
        scoreBoard = GameObject.FindGameObjectWithTag("Score").GetComponent<Text>();
        if (PlayerPrefs.HasKey(nickNamePrefKey)) {
            username.text = PlayerPrefs.GetString(nickNamePrefKey);
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        connectionText.text = "Connecting to lobby...";
    }

    
    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
    }

    
    public override void OnDisconnected(DisconnectCause cause) {
        connectionText.text = cause.ToString();
    }

    
    public override void OnJoinedLobby() {
        serverWindow.SetActive(true);
        connectionText.text = "";
    }

    
    public override void OnRoomListUpdate(List<RoomInfo> rooms) {
        roomList.text = "";
        foreach (RoomInfo room in rooms) {
            roomList.text += room.Name + "\n";
        }
    }

    
    public void JoinRoom() {
        serverWindow.SetActive(false);
        connectionText.text = "Joining room...";
        PhotonNetwork.LocalPlayer.NickName = username.text;
        PlayerPrefs.SetString(nickNamePrefKey, username.text);
        RoomOptions roomOptions = new RoomOptions() {
            IsVisible = true,
            MaxPlayers = 8
        };
        if (PhotonNetwork.IsConnectedAndReady) {
            PhotonNetwork.JoinOrCreateRoom(roomName.text, roomOptions, TypedLobby.Default);
        } else {
            connectionText.text = "PhotonNetwork connection is not ready, try restart it.";
        }
    }

    
    public override void OnJoinedRoom() {
        connectionText.text = "";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Respawn(0.0f);

        //update score board is all player in room
        
    }

    
    void Respawn(float spawnTime) {
        sightImage.SetActive(false);
        sceneCamera.enabled = true;
        StartCoroutine(RespawnCoroutine(spawnTime));
    }

   
    IEnumerator RespawnCoroutine(float spawnTime) {
        yield return new WaitForSeconds(spawnTime);
        messageWindow.SetActive(true);
        sightImage.SetActive(true);
        int playerIndex = Random.Range(0, playerModel.Length);
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        player = PhotonNetwork.Instantiate(playerModel[playerIndex].name, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation, 0);
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.RespawnEvent += Respawn;
        playerHealth.AddMessageEvent += AddMessage;
        sceneCamera.enabled = false;
        photonView.RPC("UpdateScoreBoard_RPC", RpcTarget.All);
        if (spawnTime == 0) {
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Joined Game.");
            print("Player list: " + PhotonNetwork.PlayerList.Length);
        } else {
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Respawned.");
        }
    }

    
    void AddMessage(string message) {
        photonView.RPC("AddMessage_RPC", RpcTarget.All, message);
    }

    
    [PunRPC]
    void AddMessage_RPC(string message) {
        messages.Enqueue(message);
        if (messages.Count > messageCount) {
            messages.Dequeue();
        }
        messagesLog.text = "";
        foreach (string m in messages) {
            messagesLog.text += m + "\n";
        }
    }

    //edit score board when player kill other player
    [PunRPC]
    void UpdateScoreBoard_RPC() {
        //clear old
        scoreBoard.text = "";
        //log score of all player in room
        print("Player list: " + PhotonNetwork.PlayerList.Length);
        foreach (Player player in PhotonNetwork.PlayerList) {
            print(player.NickName + ": " + player.GetScore());
            scoreBoard.text += player.NickName + ": " + player.GetScore() + "  ";
            if(player.GetScore() >= 5) {
                scoreBoard.text = "";
                scoreBoard.text += "WINNER: " + player.NickName;
                StartCoroutine("EndGame", 5.0f);
            }
        }

    }

    
    public override void OnPlayerLeftRoom(Player other) {
        if (PhotonNetwork.IsMasterClient) {
            AddMessage("Player " + other.NickName + " Left Game.");
        }
        photonView.RPC("UpdateScoreBoard_RPC", RpcTarget.All);
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

}
