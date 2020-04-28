using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


//The lobby is responsible for setting up connection between players and Photon servers. And allow to join existing rooms or create new ones.

public class PhotonLobby : MonoBehaviourPunCallbacks
{

    public static PhotonLobby lobby; //references the instance of this class between our product. (singleton) 
    public GameObject matchButton; //Button to initiate search of available rooms.
    public GameObject cancelButton; //Button to cancel multiplayer match searching.
    public TextMeshProUGUI lookingText;
    public GameObject backToMenuButton;
    public GameObject OnlineScreen;
    public GameObject friendButton;
    public byte IS_FULL = 0; // Byte for custom Event 0: Used as "Room full Message" event
    int interval = 1;
    float nextTime = 0;

    private void Awake()
    {
        lobby = this; //Creates the singleton, lives within the starting menu scene.
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); //Connects to Master photon server.

        
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Player has connected to the Photon master server");
        PhotonNetwork.AutomaticallySyncScene = true;
        matchButton.SetActive(true);
        friendButton.SetActive(true);

    }

    public void OnMatchButtonClicked()
    {
        Debug.Log("Match Button was clicked!");
        matchButton.SetActive(false);
        cancelButton.SetActive(true);
        lookingText.gameObject.SetActive(true);
        lookingText.text = "Looking for a match...";
        lookingText.color = Color.black;
        backToMenuButton.SetActive(false);
        friendButton.SetActive(false);
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to join random room but failed. There must not be a room available.");
        createRoom();
    }


    void createRoom()
    {
        Debug.Log("Trying to create a new room.");
        int randomRoomName = Random.Range(0, 1000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2};
        PhotonNetwork.CreateRoom("Room" + randomRoomName, roomOps);
       
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to create a new room failed, there must be a room already with the same name");
        OnMatchButtonClicked();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("We are now in a room");
        Player[] photonPlayers = PhotonNetwork.PlayerList;
        int playersInRoom = photonPlayers.Length;
        Debug.Log("Local User ID:" + PhotonNetwork.LocalPlayer.UserId);
        Debug.Log("Local actor number (room): " + PhotonNetwork.LocalPlayer.ActorNumber);

        //PhotonNetwork.NickName = myNumberInRoom.ToString();
        //if (!PhotonNetwork.IsMasterClient)
        //{
        //    Debug.Log("OnJoinedRoom: Not master client.");
        //    return;
        //}
        if (playersInRoom >= 2){
            Debug.Log("Room is full.");
            lookingText.text = "MATCH FOUND!";
            lookingText.color = Color.red;
            object content = "CMSG";
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(IS_FULL, content, raiseEventOptions, sendOptions);
        }
        

    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var entry in roomList)
        {
            Debug.Log(entry.ToString());
        }
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        
        if (obj.Code == IS_FULL)
        {
            Debug.Log("IS_FULL EVENT RECEIVED");
            lookingText.text = "MATCH FOUND!";
            lookingText.color = Color.red;

            //SceneManager.LoadScene("Multiplayer");
            PhotonNetwork.AutomaticallySyncScene = true;
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("Multiplayer");
                OnlineScreen.SetActive(false);
            }
            //PhotonNetwork.LoadLevel(1);
        }
    }

    public void OnCancelButtonClicked()
    {
        cancelButton.SetActive(false);
        matchButton.SetActive(true);
        lookingText.gameObject.SetActive(false);
        backToMenuButton.SetActive(true);
        friendButton.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }


    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextTime) {
            if (OnlineScreen.activeSelf == true)
            {
                PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
                Debug.Log("Checking for events");
            }

            nextTime += interval;
        }
    
    }


}
