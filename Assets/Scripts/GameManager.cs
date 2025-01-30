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
using System;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    // Array to store player scores
    private int[] playerScores;
    private int[] playerRoundsWon;

    // PlayerInputManager reference
    private PlayerInputManager playerInputManager;
    [SerializeField]
    private PlayerInput[] _playersInput;
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

    [SerializeField] EventReference _roundStartSFX;

    [SerializeField] EventReference _roundEndSFX;


    [SerializeField] private float _maxScore = 5;

    [Header("Tween")]
    [SerializeField] private RectTransform _scoreRectTransform;
    [SerializeField] private RectTransform _pause1RectTransform;
    [SerializeField] private RectTransform _pause2RectTransform;
    [SerializeField] private float topPosY, finalPosY;
    [SerializeField] private float tweenDuration;

    [SerializeField] private GameObject _winPanelP1;
    [SerializeField] private GameObject _winPanelP2;

    [Header("Health Track")]
    [SerializeField] private Health _p1;
    [SerializeField] private GameObject[] _heartsP1;  
    [SerializeField] private Health _p2;
    [SerializeField] private GameObject[] _heartsP2;

    [Header("Controls")]
    [SerializeField] GameObject _controlsObject;
    [SerializeField] Image _keyboard;
    [SerializeField] Image _gamepad;
    [SerializeField] private float _fadeTime = 3f;


    private void Awake()
    {
        instance = this;
        // Get the PlayerInputManager component
        playerInputManager = GetComponent<PlayerInputManager>();

        // Initialize player scores array 
        playerScores = new int[2];
        playerRoundsWon = new int[2];
        Debug.Log(playerInputManager.playerCount);

        //if (playerScoreTexts.Length != 2)
        //{
        //    Debug.LogError(" Number of score text fields does not match the number of platers");
        //    return;
        //}

    }
    private void Start()
    {
        StartCoroutine(RoundStart());
        ScoreIntro();        
    }

    public void UpdatePlayerHealth(int playerIndex)
    {
        if (playerIndex >= 0)
        {
            if (playerIndex == 0)
            {
                _heartsP1[_p1.GetCurrentHealth()].SetActive(false);
                if (_p1.GetCurrentHealth() <= 0)
                {
                    RuntimeManager.PlayOneShot(_roundEndSFX, transform.position); //Play win sound
                    OnRoundEnd.Invoke();
                    IncrementPlayerRoundsWon(1);
                    StartCoroutine(CallDestroyWeapons());
                    StartCoroutine(RoundStart());
                    foreach (GameObject heart in _heartsP1)
                    {
                        if (heart != null)
                        {
                            heart.SetActive(true);
                        }
                    }

                    foreach (GameObject heart in _heartsP2)
                    {
                        if (heart != null)
                        {
                            heart.SetActive(true);
                        }
                    }
                }
            }

            if (playerIndex == 1)
            {
                _heartsP2[_p2.GetCurrentHealth()].SetActive(false);

                if (_p2.GetCurrentHealth() <= 0)
                {
                    RuntimeManager.PlayOneShot(_roundEndSFX, transform.position); //Play win sound
                    OnRoundEnd.Invoke();
                    IncrementPlayerRoundsWon(0);
                    StartCoroutine(CallDestroyWeapons());
                    StartCoroutine(RoundStart());
                    foreach (GameObject heart in _heartsP1)
                    {
                        if (heart != null)
                        {
                            heart.SetActive(true);
                        }
                    }

                    foreach (GameObject heart in _heartsP2)
                    {
                        if (heart != null)
                        {
                            heart.SetActive(true);
                        }
                    }
                }
            }
            

            ////playerScores[playerIndex]++;
            //UpdateUIScore();

            //if (playerScores[0] == _maxScore)
            //{
            //    OnRoundEnd.Invoke();

            //    IncrementPlayerRoundsWon(1);
            //    playerScores[0] = 0;
            //    UpdateUIScore();
            //    StartCoroutine(CallDestroyWeapons());
            //    StartCoroutine(RoundStart());
            //}
            //if (playerScores[1] == _maxScore)
            //{
            //    OnRoundEnd.Invoke();

            //    IncrementPlayerRoundsWon(0);
            //    playerScores[1] = 0;
            //    UpdateUIScore();
            //    StartCoroutine(CallDestroyWeapons());
            //    StartCoroutine(RoundStart());
            //}

            //Debug.Log(playerScores[0]);
            //Debug.Log(playerScores[1]);
        }
    }


    
    // Function to increment player score based on player index
    public void IncrementPlayerScore(int playerIndex)
    {
        
        if (playerIndex >= 0)
        {
            playerScores[playerIndex]++;
            UpdateUIScore();

            if (playerScores[0] == _maxScore)
            {
                OnRoundEnd.Invoke();

                IncrementPlayerRoundsWon(1);
                playerScores[0] = 0;
                UpdateUIScore();
                StartCoroutine(CallDestroyWeapons());
                StartCoroutine(RoundStart());
            }
            if (playerScores[1] == _maxScore)
            {
                OnRoundEnd.Invoke();
                
                IncrementPlayerRoundsWon(0);
                playerScores[1] = 0;
                UpdateUIScore();
                StartCoroutine(CallDestroyWeapons());
                StartCoroutine(RoundStart());
            }

            Debug.Log(playerScores[0]);
            Debug.Log(playerScores[1]);
        }
    }


    public void IncrementPlayerRoundsWon(int playerIndex)
    {
        
        if (playerIndex >= 0)
        {
            playerRoundsWon[playerIndex]++;
            UpdateUIScore();
            

            if (playerRoundsWon[0] == 3f)
            {
                Win("Player 1 is the GOAT!");
                _winPanelP1.SetActive(true);
            }
            if (playerRoundsWon[1] == 3f)
            {
                Win("Player 2's really LIKE that");
                _winPanelP2.SetActive(true);
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
        /*for (int i = 0; i < playerScores.Length; i++)
        {
            //if (i  == 0)
            //{
            //    if (_heartsP1.Length > 0)
            //    {
            //        _heartsP1[(_heartsP1.Length - playerScores[i])].SetActive(false);
            //    }
            //}

            //if (i == 1)
            //{
            //    if (_heartsP2.Length > 0)
            //    {
            //        _heartsP2[(_heartsP2.Length - playerScores[i])].SetActive(false);
            //    }
            //}

            
            playerScoreTexts[i].text = "Player " + (i + 1) + " HP: " + (5 - playerScores[i]);
            Transform ScoreTransform = playerScoreTexts[i].transform;
            //ScoreTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);
        }*/

        for (int i = 0; i < playerRoundsWon.Length; i++)
        {
            playerRoundsWonTexts[i].text = "Rounds Won: " + playerRoundsWon[i] + " / 3";
            Transform RoundsTransform = playerRoundsWonTexts[i].transform;
            //RoundsTransform.transform.DOPunchScale(new Vector3(2f,2f,2f), 0.2f, 0, 0.1f).SetUpdate(true);
        }
    }

    public void ScoreIntro()
    {
        StartCoroutine(ScoreIntroRoutine());        
    }

    private IEnumerator ScoreIntroRoutine()
    {
        yield return new WaitForSeconds(_roundStartTimer);
        _scoreRectTransform.DOAnchorPosY(finalPosY, tweenDuration).SetUpdate(true);
    }

    public IEnumerator RoundStart()
    {
        //SwitchMap("Idle");
        for (int i = 0; i < _playersInput.Length; i++)
        {
            PlayerInput playerInput = _playersInput[i];
            playerInput.DeactivateInput();
        }

        RuntimeManager.PlayOneShot(_roundStartSFX, transform.position); //Play round start sound
        yield return new WaitForSeconds(0.5f);
        Transform CountDownTransform = RoundStartText.transform;
        //Time.timeScale = 0f;

        RoundStartText.text = "Best to 5 Rounds";
        CountDownTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(_roundStartTimer + 1.25f);

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

        StartCoroutine(FadeOut());

        //SwitchMap("Gameplay");
        for (int i = 0; i < _playersInput.Length; i++)
        {
            PlayerInput playerInput = _playersInput[i];
            playerInput.ActivateInput();
        }

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

    public void SwitchMap(string newMap)
    {
        for (int i = 0; i < _playersInput.Length; i++)
        {
            PlayerInput playerInput = _playersInput[i];
            playerInput.SwitchCurrentActionMap(newMap);
        }
    }

    private IEnumerator FadeOut()
    {
        float timer = _fadeTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            _keyboard.color = new Color(_keyboard.color.r, _keyboard.color.g, _keyboard.color.b, timer / _fadeTime);
            _gamepad.color = new Color(_gamepad.color.r, _gamepad.color.g, _gamepad.color.b, timer / _fadeTime);
            yield return null;
            //_controlsObject.SetActive(false);
        }
        yield break;
    }
}
