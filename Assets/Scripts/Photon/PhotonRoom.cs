using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public int turn_who;
    public int turnCounter; // Number of turns played
    public GameObject[] turnIcons; // Displays who's turn it is
    public GameObject gameOverMenu; //Game object that contains the menu when a match has ended and the win message
    public Sprite[] playerIcons; // 0 = x icon, 1 = O icon
    public Button[] ticTacToe_Spaces; //the grid buttons
    public int[] markedSpaces; //Id's which space was marked by which player
    public TextMeshProUGUI[] ScoreText; //score texts
    public TextMeshProUGUI winMsg_text;
    public int[] scores;
    public GameObject tie_msg; //Text box indicating there was a tie
    public Image[] winLines; //Contains the different lines that show when there's a winner
    public GameObject Panel; //To prevent interaction with board in win transitions
    public TextMeshProUGUI turnMessage;
    public int nRounds = 3;
    public TextMeshProUGUI backToMenuTxt;
    int interval = 1;
    float nextTime = 0;//(int)Time.time;

    //PHOTON NETWORK
    public Button quitButton;
    public GameObject quitMessage;
    private byte MOVEMENT_EVENT_ID = 1;
    private byte WIN_EVENT_ID = 2;
    private byte TIE_EVENT_ID = 3;
    private byte REMATCH_RQST_EVENT_ID = 4;
    private byte REMATCH_ACK_EVENT_ID = 5;
    private byte REMATCH_NACK_EVENT_ID = 6;
    private int PLAYER_ID = 0; //will mark the local player's turn 

    // Start is called before the first frame update
    void Start()
    {
        PLAYER_ID = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        //string icon = (PLAYER_ID == 0) ? ("X") : ("O");
        if(PLAYER_ID == 0)
        {
            Debug.Log("Your player id is 0. You are the master client");
            Debug.Log("You play with the X's, It's your turn.");
        }
        else
        {
            Debug.Log("Your player id is 1. You play with the O's, wait for your turn.");
        }
       
        if (PLAYER_ID == 0)
        { 
            startTurn();
        }
        else
        {
            waitForTurn();
        }
        backToMenuTxt.text = "[MENU]";
        nextTime = (int)Time.time;

        StartOnlineMatch();
        
    }

    private void startTurn()
    {
        Debug.Log("It's your turn");
        turnMessage.text = "It's your turn";
        Panel.SetActive(false);
    }
    
    private void waitForTurn()
    {
        Debug.Log("It's your opponent's turn");
        turnMessage.text = "It's your opponent's turn.";
        Panel.SetActive(true);
    }

    public void StartOnlineMatch()
    {
        Debug.Log("Attempting to start online match");
        turn_who = 0;
        tie_msg.SetActive(false);
        quitButton.gameObject.SetActive(true);
        gameOverMenu.SetActive(false);
        turnIcons[0].SetActive(true);
        turnIcons[1].SetActive(false);
        scores[0] = 0;
        scores[1] = 0;
        ScoreText[0].text = scores[0].ToString();
        ScoreText[1].text = scores[1].ToString();
        GameSetup();
    }

    void GameSetup()
    {
        Debug.Log("Setting game up");
        turnCounter = 0;
        var col = ticTacToe_Spaces[0].colors;
        col.normalColor = Color.clear;
        col.disabledColor = Color.white;

        for (int i = 0; i < ticTacToe_Spaces.Length; i++)
        {
            ticTacToe_Spaces[i].colors = col;
            ticTacToe_Spaces[i].interactable = true;
            ticTacToe_Spaces[i].GetComponent<Image>().sprite = null;
        }
        for (int i = 0; i < markedSpaces.Length; i++)
        {

            markedSpaces[i] = -1;

        }
    }


    public void OnClickedTicTacToe_Button(int nCell)
    {
        Debug.Log("Button CLICKED");
        ticTacToe_Spaces[nCell].image.sprite = playerIcons[turn_who];
        var col = ticTacToe_Spaces[nCell].colors;
        col.normalColor = Color.white;
        ticTacToe_Spaces[nCell].colors = col;
        markedSpaces[nCell] = turn_who;
        turnCounter++;
        ticTacToe_Spaces[nCell].interactable = false;
        RaiseMovementEvent(nCell);
        CheckWin();

    }

    // To check the other player's movement is their last turn and update the grid accordingly. 
    private void updateGrid(int nCell, int whoMoved)
    {
        Debug.Log("Updating grid to player " + whoMoved + "'s movement");
        ticTacToe_Spaces[nCell].image.sprite = playerIcons[whoMoved];
        var col = ticTacToe_Spaces[nCell].colors;
        col.normalColor = Color.white;
        ticTacToe_Spaces[nCell].colors = col;
        markedSpaces[nCell] = whoMoved;
        ticTacToe_Spaces[nCell].interactable = false;
        CheckWin();
    }

    void switchTurns()
    {
        for (int i = 0; i < ticTacToe_Spaces.Length; i++)
        {
            ticTacToe_Spaces[i].interactable = false;
        }

        Debug.Log("Switching turns");
        if (turn_who == 0)
        {
            turn_who = 1;
            turnIcons[0].SetActive(false);
            turnIcons[1].SetActive(true);
        }
        else
        {
            turn_who = 0;
            turnIcons[0].SetActive(true);
            turnIcons[1].SetActive(false);
        }

    }

    public void CheckWin()
    {
        bool isWin = false;
        for (int i = 0; i < 3; i++)
        {
            int counter = 0;
            // TO CHECK HORIZONTALLY
            for (int j = 0; j < 3; j++)
            {
                if (ticTacToe_Spaces[(i * 3) + j].image.sprite == playerIcons[turn_who]) { counter++; }
            }
            if (counter == 3)
            {
                // *something cool*
                StartCoroutine(showLine(GetLine(2, i), isWin));
                isWin = true;
            }
            else
            {
                counter = 0;
                // TO CHECK VERTICALLY
                for (int k = 0; k < 3; k++)
                {
                    int vIndex = i + 3 * k;
                    if (ticTacToe_Spaces[vIndex].image.sprite == playerIcons[turn_who])
                    {
                        counter++;
                    }
                }
                if (counter == 3)
                {
                    // *something cool*
                    StartCoroutine(showLine(GetLine(1, i), isWin));
                    isWin = true;
                }
                else
                {
                    counter = 0;
                    // TO CHECK DIAGONALLY
                    for (int l = 0; l < 3; l++)
                    {
                        if (i < 2)
                        {
                            if (ticTacToe_Spaces[i * 2 + (4 - i * 2) * l].image.sprite == playerIcons[turn_who]) { counter++; }
                        }
                    }
                    if (counter == 3)
                    {
                        // *something cool*
                        StartCoroutine(showLine(GetLine(3, i), isWin));
                        isWin = true;
                    }
                    else { counter = 0; }
                }
            }

        }

        if (isWin == false)
        {
            if(turn_who == PLAYER_ID)
            {
                //REGULAR EVENT, NO ROUND WON OR A TIE 
                switchTurns();
                waitForTurn();
            }
            
        }

        if (turnCounter == 9 & isWin == false)
        {
            // There was a tie!
            StartCoroutine(tieMsg(0.65f));
            RaiseTieEvent();
            //GameSetup();
        }
        //turnCounter = 0;
    }

    // To get the line corresponding to the type of win
    Image GetLine(int nOrientation, int i) //nOrientation: 1 = Vertical; 2 = Horizontal; 3 = Diagonal
    {
        if (nOrientation == 1)
        {
            if (i == 0)
            {
                return winLines[0];
            }
            if (i == 1)
            {
                return winLines[1];
            }
            if (i == 2)
            {
                return winLines[2];
            }
        }
        else if (nOrientation == 2)
        {
            if (i == 0)
            {
                return winLines[3];
            }
            if (i == 1)
            {
                return winLines[4];
            }
            if (i == 2)
            {
                return winLines[5];
            }
        }
        else
        {
            if (i == 0)
            {
                return winLines[6];
            }
        }
        return winLines[7];

    }

    IEnumerator tieMsg(float delay)
    {
        tie_msg.SetActive(true);
        yield return new WaitForSeconds(delay);
        tie_msg.SetActive(false);
        GameSetup();
    }

    IEnumerator showLine(Image line, bool isWin)
    {
        line.gameObject.SetActive(true);
        Panel.SetActive(true);
        yield return new WaitForSeconds(1.25f);
        Panel.SetActive(false);
        line.gameObject.SetActive(false);
        if (isWin == false)
        {
            theresWinner();
        }
    }

    public bool theresWinner()
    {
        scores[turn_who]++;
        ScoreText[turn_who].text = scores[turn_who].ToString();

        if (scores[turn_who] >= nRounds)
        {
            //*[player] won this game!!*
            gameOverScreen();
            RaiseWinEvent();
            return true;
        }
        else
        {
            // *[player] won this round!!*
            Debug.Log("A player has won the round");
            if(turn_who == PLAYER_ID)
            {
                switchTurns();
                waitForTurn();
                GameSetup();
            }
            return false;
        }
    }

    public void gameOverScreen(int WhoIsVictor = -1)
    {
        if (WhoIsVictor == -1)
        {
            WhoIsVictor = turn_who;
        }
        string victorIcon = (WhoIsVictor == 0) ? ("X") : ("O");
        if (WhoIsVictor == PLAYER_ID)
        {
            winMsg_text.text = "YOU WON THIS MATCH!";
        }
        else
        {
            winMsg_text.text = "YOU LOST THIS MATCH!";
        }
        Panel.SetActive(true);
        gameOverMenu.SetActive(true);
    }

    public void OnClickedRematch_Buttton()
    {
        Panel.SetActive(false);
        turnMessage.text = "Waiting for opponent...";
        backToMenuTxt.text = "[Cancel]";
        Button RematchButton = GameObject.Find("rematch_button").GetComponent<Button>() ;
        RematchButton.interactable = false;
        RaiseRematchRequestEvent();
    }

    public void OnClickedQuitButton()
    {
        if(gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
        }
        quitMessage.SetActive(true);
        quitButton.gameObject.SetActive(false);
    }

    public void OnClickedYesButton()
    {
        Debug.Log("Going back to menu and leaving room.");
        quitMessage.SetActive(false);
        PhotonNetwork.LeaveRoom();
        //SceneManager.LoadScene(0);
        PhotonNetwork.LoadLevel(0);
    }

    

    public void OnClickedNoButton()
    {
        quitMessage.SetActive(false);
        quitButton.gameObject.SetActive(true);
    }

    // To send your move to the network everytime your turn ends.
    private void RaiseMovementEvent(int nCell)
    {
        object[] content = new object[] { nCell, PLAYER_ID };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(MOVEMENT_EVENT_ID, content, raiseEventOptions, sendOptions);
        Debug.Log("Movement event Raised.");
    }

    //Send a message to your opponent that you won. 
    private void RaiseWinEvent()
    {
        object[] content = new object[] { PLAYER_ID };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(WIN_EVENT_ID, content, raiseEventOptions, sendOptions);
        Debug.Log("Win event Raised.");
    }

    private void RaiseTieEvent()
    {
        object[] content = new object[] { PLAYER_ID };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(TIE_EVENT_ID, content, raiseEventOptions, sendOptions);
        Debug.Log("Tie event Raised.");
    }

    private void RaiseRematchRequestEvent()
    {
        object[] content = new object[] { PLAYER_ID } ;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.DoNotCache }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(REMATCH_RQST_EVENT_ID, content, raiseEventOptions, sendOptions);
        Debug.Log("Rematch request event Raised.");
    }

    void NetworkingClient_EventReceived(EventData obj)
    {
        
        if (obj.Code == MOVEMENT_EVENT_ID & turn_who != PLAYER_ID)
        {
            Debug.Log("MOVEMENT_EVENT received");
            // To collect the event's data:
            object[] content = (object[])obj.CustomData;
            int n_Cell = (int)content[0];
            int who_Moved = (int)content[1];
            //Update Grid call with data collected as parameters
            updateGrid(n_Cell, who_Moved);
            CheckWin();
            if(theresWinner() == false)
            {
                startTurn();
            }
        }
        if (obj.Code == WIN_EVENT_ID)
        {
            if (gameOverMenu.activeSelf == false) // To only send it once.
            {
                Debug.Log("WIN_EVENT received");
                object[] content = (object[])obj.CustomData;
                int victor = (int)content[0];
                gameOverScreen(victor);
            }
        }
        if (obj.Code == REMATCH_RQST_EVENT_ID)
        {
            Debug.Log("REMATCH_RQST_EVENT received");
            object[] content = (object[])obj.CustomData;
            int WhoAskRematch = (int)content[0];
            Debug.Log("Player #" + WhoAskRematch + " is asking for a rematch");
            
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextTime) //Time.timeSinceLevelLoaded
        {
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
            //Debug.Log("Checking for events");

            nextTime += interval;
        }
      
    }
    
}
