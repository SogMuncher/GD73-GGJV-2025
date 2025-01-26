using System.Collections;
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
    private int[] playerRoundsWon;

    // PlayerInputManager reference
    private PlayerInputManager playerInputManager;
    private PlayerInput playerInput;

    public TextMeshProUGUI[] playerScoreTexts;
    public TextMeshProUGUI[] playerRoundsWonTexts;
    public TextMeshProUGUI P1WinText;
    public TextMeshProUGUI P2WinText;
    public TextMeshProUGUI RoundStartText;

    [SerializeField] private float _roundStartTimer = 1f;

    public UnityEvent OnP1Win;
    public UnityEvent OnP2Win;

    public UnityEvent OnRoundEnd;

    
    [SerializeField]private float _maxScore = 2;

    private void Awake()
    {
        instance = this;
        // Get the PlayerInputManager component
        playerInputManager = GetComponent<PlayerInputManager>();

        // Initialize player scores array 
        playerScores = new int[playerInputManager.playerCount];
        playerRoundsWon = new int[playerInputManager.playerCount];

        if (playerScoreTexts.Length != playerInputManager.playerCount)
        {
            Debug.LogError(" Number of score text fields does not match the number of platers");
            return;
        }

        
    }
    private void Start()
    {
        StartCoroutine(RoundStart());
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

                IncrementPlayerRoundsWon(playerIndex);
                playerScores[0] = 0;
                UpdateUIScore();
                OnRoundEnd.Invoke();
                StartCoroutine(RoundStart());
            }
            if (playerScores[1] == 2)
            {
                
                IncrementPlayerRoundsWon(playerIndex);
                playerScores[1] = 0;
                UpdateUIScore();
                OnRoundEnd.Invoke();
                StartCoroutine(RoundStart());



            }

            Debug.Log(playerScores[0]);
            Debug.Log(playerScores[1]);

        }
    }

    public void IncrementPlayerRoundsWon(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerScores.Length)
        {
            playerRoundsWon[playerIndex]++;
            UpdateUIScore();
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

        for (int i = 0; i < playerRoundsWon.Length; i++)
        {
            playerRoundsWonTexts[i].text = "Rounds Won: " + playerRoundsWon[i];
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
    
    public IEnumerator RoundStart()
    {
        Time.timeScale = 0f;
        RoundStartText.text = "Round Starting in";

        yield return new WaitForSecondsRealtime(_roundStartTimer + 1f);

        RoundStartText.text = "3";

        yield return new WaitForSecondsRealtime(_roundStartTimer);


        RoundStartText.text = "2";

        yield return new WaitForSecondsRealtime(_roundStartTimer);



        RoundStartText.text = "1";

        yield return new WaitForSecondsRealtime(_roundStartTimer);


        RoundStartText.text = "Fight";

        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(_roundStartTimer);

        RoundStartText.text = "";


        yield break;

    }
}
