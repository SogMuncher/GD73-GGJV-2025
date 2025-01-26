using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;
using DG.Tweening;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    // Array to store player scores
    private int[] playerScores;
    private int[] playerRoundsWon;

    // PlayerInputManager reference
    private PlayerInputManager playerInputManager;
    private PlayerInput playerInput;
    private List<ThrownWeapon> thrownWeapon = new List<ThrownWeapon>();

    public TextMeshProUGUI[] playerScoreTexts;
    public TextMeshProUGUI[] playerRoundsWonTexts;
    public TextMeshProUGUI WonText;
    public TextMeshProUGUI RoundStartText;

    [SerializeField] private float _roundStartTimer = 1f;

    public UnityEvent OnWin;

    public UnityEvent OnRoundEnd;

    [SerializeField] EventReference _winSFX;
    [SerializeField] EventReference _countdownSFX;

    
    [SerializeField]private float _maxScore = 2;

    [Header ("Tween")]
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private float topPosY, finalPosY;
    [SerializeField] private float tweenDuration;

    private void Awake()
    {
        instance = this;
        // Get the PlayerInputManager component
        playerInputManager = GetComponent<PlayerInputManager>();

        // Initialize player scores array 
        playerScores = new int[2];
        playerRoundsWon = new int[2];
        Debug.Log(playerInputManager.playerCount);

        if (playerScoreTexts.Length != 2)
        {
            Debug.LogError(" Number of score text fields does not match the number of platers");
            return;
        }

        
    }
    private void Start()
    {
        StartCoroutine(RoundStart());
        ScoreIntro();

        
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
                StartCoroutine(CallDestroyWeapons());
                StartCoroutine(RoundStart());
            }
            if (playerScores[1] == 2)
            {
                
                IncrementPlayerRoundsWon(playerIndex);
                playerScores[1] = 0;
                UpdateUIScore();
                OnRoundEnd.Invoke();
                StartCoroutine(CallDestroyWeapons());
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
            

            if (playerRoundsWon[0] == 2f)
            {
                Win("Player 1 is the GOAT!");
            }
            if (playerRoundsWon[1] == 2f)
            {
                Win("Player 2's really LIKE that");
            }
        }
    }

    private IEnumerator CallDestroyWeapons()
    {
        
        DestroyWeapons();
        yield break;
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
            Transform ScoreTransform = playerScoreTexts[i].transform;
            ScoreTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);

        }

        for (int i = 0; i < playerRoundsWon.Length; i++)
        {
            playerRoundsWonTexts[i].text = "Rounds Won: " + playerRoundsWon[i];
            Transform RoundsTransform = playerRoundsWonTexts[i].transform;
            RoundsTransform.transform.DOPunchScale(new Vector3(2f,2f,2f), 0.2f, 0, 0.1f).SetUpdate(true);
        }
    }

    public void ScoreIntro()
    {
        _rectTransform.DOAnchorPosY(finalPosY, tweenDuration).SetUpdate(true);

        
    }
 
    
    public IEnumerator RoundStart()
    {
        Transform CountDownTransform = RoundStartText.transform;
        Time.timeScale = 0f;
        RoundStartText.text = "Round Starting in";
        CountDownTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);


        RuntimeManager.PlayOneShot(_countdownSFX, transform.position); //Play countdown sound

        yield return new WaitForSecondsRealtime(_roundStartTimer + 1f);


        RoundStartText.text = "3";
        CountDownTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);


        yield return new WaitForSecondsRealtime(_roundStartTimer);


        RoundStartText.text = "2";
        CountDownTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(_roundStartTimer);



        RoundStartText.text = "1";
        CountDownTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);


        yield return new WaitForSecondsRealtime(_roundStartTimer);


        RoundStartText.text = "Fight";
        CountDownTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);


        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(_roundStartTimer);

        RoundStartText.text = "";


        yield break;

    }
    public void Win(string winText)
    {
        OnWin.Invoke();
        WonText.text = winText;
        StartCoroutine(WinScreen());
    }
    public IEnumerator WinScreen()
    {
        RuntimeManager.PlayOneShot(_winSFX, transform.position); //Play win sound
        
        yield return new WaitForSeconds(2f);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(4f);

        SceneManager.LoadScene("MainMenu");

        yield break;
    }

    public void AddWeaponsToList(ThrownWeapon weapon)
    {
        thrownWeapon.Add(weapon);
    }

    private void DestroyWeapons()
    {
        foreach (ThrownWeapon weapon in thrownWeapon)
        {
            if (weapon != null)
            {
                weapon.DestroyObject();
            }
        }
        thrownWeapon.Clear();
    }
}
