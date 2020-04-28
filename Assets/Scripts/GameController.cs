using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{

    public int turn_who; // 0 = x, 1 = O
    public int turnCounter; // Number of turns played
    public GameObject[] turnIcons; // Displays who's turn it is
    public GameObject gameMenu; //Game object that contains all the main menu buttons and UI
    public GameObject gameOverMenu; //Game object that contains the menu when a match has ended and the win message
    public Sprite[] playerIcons; // 0 = x icon, 1 = O icon
    public Button[] ticTacToe_Spaces; //the grid buttons
    public int[] markedSpaces; //Id's which space was marked by which player
    public TextMeshProUGUI[] txt; //score texts
    public TextMeshProUGUI winMsg_text;
    public int[] scores;
    public GameObject tie_msg; //Text box indicating there was a tie
    public Image[] winLines; //Contains the different lines that show when there's a winner
    public GameObject winnerPanel; //To prevent interaction with board in win transitions
    public int nRounds = 3;
    public GameObject roundSelectionScreen;
    public GameObject onlineScreen;
    public Button quitButton;
    public GameObject quitMessage;

    // Start is called before the first frame update
    void Start()
    {
        GameMenu();
    }

    public void GameMenu()
    {
        if(roundSelectionScreen != null)
        {
            roundSelectionScreen.SetActive(false);
        }
        if(winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }
        if(gameMenu != null)
        {
            gameMenu.SetActive(true);
        }
        if (onlineScreen != null)
        {
            onlineScreen.SetActive(false);
        }
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);
        }
    }

    public void showRoundSelectionScreen()
    {
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);
        }
        gameMenu.SetActive(false);
        roundSelectionScreen.SetActive(true);
    }

    void GameSetup()
    {
        turnCounter = 0;
        var col = ticTacToe_Spaces[0].colors;
        col.normalColor = Color.clear;
        col.disabledColor = Color.white;

        for(int i = 0; i < ticTacToe_Spaces.Length; i++)
        {
            ticTacToe_Spaces[i].colors = col;
            ticTacToe_Spaces[i].interactable = true;
            ticTacToe_Spaces[i].GetComponent<Image>().sprite = null;
        }
        for(int i = 0; i < markedSpaces.Length; i++){

            markedSpaces[i] = -1;

        }
    }

    public void newGame()
    {
        winnerPanel.SetActive(false);
        tie_msg.SetActive(false);
        quitButton.gameObject.SetActive(true);
        if (gameMenu != null)
        {
            gameMenu.SetActive(false);
        }

        gameOverMenu.SetActive(false);
        turn_who = 0;
        turnIcons[0].SetActive(true);
        turnIcons[1].SetActive(false);
        scores[0] = 0;
        scores[1] = 0;
        txt[0].text = scores[0].ToString();
        txt[1].text = scores[1].ToString();
        GameSetup();
    }

    public void getRounds(int n)
    {
        nRounds = n;
        roundSelectionScreen.SetActive(false);
        newGame();
    }

    public void TicTacToe_Button(int nCell)
    {
        
        ticTacToe_Spaces[nCell].image.sprite = playerIcons[turn_who];
        var col = ticTacToe_Spaces[nCell].colors;
        col.normalColor = Color.white;
        ticTacToe_Spaces[nCell].colors = col;
        markedSpaces[nCell] = turn_who;
        turnCounter++;
        ticTacToe_Spaces[nCell].interactable = false;
        CheckWin();

    }

    void switchTurns()
    {
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
       for(int i = 0; i < 3; i++)
        {
            int counter = 0;
            // TO CHECK HORIZONTALLY
            for (int j = 0; j < 3; j++)
            {
                if (ticTacToe_Spaces[(i*3)+j].image.sprite == playerIcons[turn_who]) { counter++; }
            }
            if (counter == 3)
            {
                // *something cool*
                StartCoroutine(showLine(GetLine(2, i), isWin)); //line animation
                isWin = true;
            }
            else
            {
                counter = 0;
                // TO CHECK VERTICALLY
                for (int k = 0; k < 3; k++)
                {
                    int vIndex = i + 3 * k;  
                    if (ticTacToe_Spaces[vIndex].image.sprite == playerIcons[turn_who]) {
                        counter++;
                    }
                }
                if (counter == 3)
                {
                    // *something cool*
                    StartCoroutine(showLine(GetLine(1, i), isWin)); //line animation
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
            //REGULAR EVENT: NO ROUND WON OR A TIE 
            switchTurns();
        }
        
        if (turnCounter == 9 & isWin == false)
        {
            // There was a tie!
            StartCoroutine(tieMsg(0.65f));
        }
    }

    // To get the line corresponding to the type of win
    Image GetLine(int nOrientation,int i) //nOrientation: 1 = Vertical; 2 = Horizontal; 3 = Diagonal
    {
        if(nOrientation == 1)
        {
            if(i == 0)
            {
                return winLines[0];
            }
            if(i == 1)
            {
                return winLines[1];
            }
            if(i == 2)
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
            if(i == 0)
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
        winnerPanel.SetActive(true);
        yield return new WaitForSeconds(1.25f);
        winnerPanel.SetActive(false);
        line.gameObject.SetActive(false);
        if(isWin == false)
        {
            theresWinner();
        }
    }

    public void theresWinner()
    {
        scores[turn_who]++;
        txt[turn_who].text = scores[turn_who].ToString();

        if (scores[turn_who] >= nRounds)
        {
            //*[player] won this game!!*
            gameOverScreen();
        }
        else{
            // *[player] won this round!!*
            switchTurns();
            GameSetup();
        }
    }

    public void gameOverScreen()
    {
        string victor = (turn_who == 0) ? ("X") : ("O");
        winMsg_text.text = victor + " WON THIS MATCH!";
        winnerPanel.SetActive(true);
        gameOverMenu.SetActive(true);
    }

    public void OnOnlineButtonClicked()
    {
        gameMenu.SetActive(false);
        onlineScreen.SetActive(true);
    }

    public void OnQuitButtonClicked()
    {
        quitMessage.SetActive(true);
        quitButton.gameObject.SetActive(false);
    }

    public void OnYesButtonClicked()
    {
        quitMessage.SetActive(false);
        gameMenu.SetActive(true);
    }

    public void OnNoButtonClicked()
    {
        quitMessage.SetActive(false);
        quitButton.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
