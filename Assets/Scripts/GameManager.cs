using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    // Array to store player scores
    private int[] playerScores;

    // PlayerInputManager reference
    private PlayerInputManager playerInputManager;
    private PlayerInput playerInput;

    public TextMeshProUGUI[] playerScoreTexts;
    public TextMeshProUGUI P1WinText;
    public TextMeshProUGUI P2WinText;

    public UnityEvent OnP1Win;
    public UnityEvent OnP2Win;

    
    [SerializeField]private float _maxScore = 2;

    private void Awake()
    {
        instance = this;
        // Get the PlayerInputManager component
        playerInputManager = GetComponent<PlayerInputManager>();

        // Initialize player scores array 
        playerScores = new int[playerInputManager.playerCount];

        if (playerScoreTexts.Length != playerInputManager.playerCount)
        {
            Debug.LogError(" Number of score text fields does not match the number of platers");
            return;
        }

        
    }

    // Function to increment player score based on player index
    public void IncrementPlayerScore(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerScores.Length)
        {
            playerScores[playerIndex]++;
            UpdateUIScore();

            if (playerScores[0] == 2)
            {
                
                P1Win("Player 1 wins!");
            }
            if (playerScores[1] == 2)
            {
                
                P2Win("Player 2 wins!");
            }

            Debug.Log(playerScores[0]);
            Debug.Log(playerScores[1]);

        }
    }

    // Function to get player score based on player index
    public int GetPlayerScore(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerScores.Length)
        {
            return playerScores[playerIndex];
        }
        return 0;
    }

    
    public void UpdateUIScore()
    {
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScoreTexts[i].text = "Player " + (i + 1) + ": " + playerScores[i];
        }
    }

    public void P1Win(string winnerString)
    {
        OnP1Win.Invoke();
        P1WinText.text = winnerString;
    }
    public void P2Win(string winnerString)
    {
        OnP2Win.Invoke();
        P2WinText.text = winnerString;
    }
}
