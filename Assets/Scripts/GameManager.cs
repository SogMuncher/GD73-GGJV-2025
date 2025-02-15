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
using Unity.VisualScripting;

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
    private PlayerController[] _playerControllers;
    private List<ThrownWeapon> thrownWeapon = new List<ThrownWeapon>();
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
    [SerializeField] private Health _healthP1;
    [SerializeField] private GameObject[] _heartsP1;
    [SerializeField] private Health _healthP2;
    [SerializeField] private GameObject[] _heartsP2;

    [Header("Controls")]
    [SerializeField] Image _keyboard;
    [SerializeField] Image _gamepad;
    [SerializeField] private float _fadeTime = 3f;

    [Header("Score HUD")]
    [SerializeField] private PlayerController _p1;
    [SerializeField] private PlayerController _p2;
    [SerializeField] private RectTransform _p1HUD;
    [SerializeField] private RectTransform _p2HUD;
    [SerializeField] private Image[] _heartIconsP1;
    [SerializeField] private Image[] _heartIconsP2;
    [SerializeField] private Image _scorePanelP1;
    [SerializeField] private Image _scorePanelP2;
    [SerializeField] private float _range = 10f;
    public TextMeshProUGUI[] playerScoreTexts;
    public TextMeshProUGUI[] playerRoundsWonTexts;
    private Vector3 _p1ScreenPosition;
    private Vector3 _p2ScreenPosition;

    public UnityEvent RoundStarting;
    public UnityEvent RoundStarted;

    public bool IsPaused = false;


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

        _playerControllers = FindObjectsOfType<PlayerController>();
        IsPaused = false;
    }

    private void Start()
    {
        StartCoroutine(RoundStart());
        ScoreIntro();
        IsPaused = false;
    }

    private void Update()
    {
        FadeOutScoreOnDistance();
    }

    public void UpdatePlayerHealth(int playerIndex)
    {
        if (playerIndex >= 0)
        {
            if (playerIndex == 0)
            {
                if (_heartsP1 != null)
                {
                    if (_healthP1.GetCurrentHealth() >= 0)
                    {
                        _heartsP1[_healthP1.GetCurrentHealth()].SetActive(false);
                    }
                    if (_healthP1.GetCurrentHealth() <= 0)
                    {
                        StartCoroutine(SlowTime(1));
                    }
                }
            }

            if (playerIndex == 1)
            {
                if (_heartsP2 != null)
                {
                    if (_healthP2.GetCurrentHealth() >= 0)
                    {
                        _heartsP2[_healthP2.GetCurrentHealth()].SetActive(false);
                    }
                    if (_healthP2.GetCurrentHealth() <= 0)
                    {
                        StartCoroutine(SlowTime(0));
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

    private IEnumerator SlowTime(int playerIndex)
    {
        Time.timeScale = 0.15f;

        yield return new WaitForSeconds(0.75f);

        Time.timeScale = 1f;
        StartCoroutine(RoundEnd(playerIndex)); 
    }

    private IEnumerator RoundEnd(int playerIndex)
    {
        RuntimeManager.PlayOneShot(_roundEndSFX, transform.position); //Play win sound
        IncrementPlayerRoundsWon(playerIndex);
        OnRoundEnd.Invoke();
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
        yield break;
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
                Win("P1");
                _winPanelP1.SetActive(true);
            }
            if (playerRoundsWon[1] == 3f)
            {
                Win("P2");
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
            playerRoundsWonTexts[i].text = playerRoundsWon[i] + " / 3";
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
        Cursor.visible = false;
        RoundStarting.Invoke();

        for (int i = 0; i < _playersInput.Length; i++)
        {
            PlayerInput playerInput = _playersInput[i];
            playerInput.DeactivateInput();
        }

        for (int j = 0; j < _playerControllers.Length; j++)
        {
            PlayerController playerController = _playerControllers[j];
            playerController.ResetAmmo();
        }

        StartCoroutine(ControlsFadeIn());

        RuntimeManager.PlayOneShot(_roundStartSFX, transform.position); //Play round start sound
        yield return new WaitForSeconds(0.5f);
        Transform CountDownTransform = RoundStartText.transform;
        //Time.timeScale = 0f;

        RoundStartText.text = "Best of 5 Rounds";
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


        RoundStartText.text = "Fight!";
        CountDownTransform.transform.DOPunchScale(new Vector3(2f, 2f, 2f), 0.2f, 0, 0.1f).SetUpdate(true);


        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(_roundStartTimer);

        RoundStartText.text = "";

        StartCoroutine(ControlsFadeOut());

        //SwitchMap("Gameplay");
        for (int i = 0; i < _playersInput.Length; i++)
        {
            PlayerInput playerInput = _playersInput[i];
            playerInput.ActivateInput();
        }

        RoundStarted.Invoke();

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

    private IEnumerator ControlsFadeOut()
    {
        float timer = _fadeTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            _keyboard.color = new Color(_keyboard.color.r, _keyboard.color.g, _keyboard.color.b, timer / _fadeTime);
            _gamepad.color = new Color(_gamepad.color.r, _gamepad.color.g, _gamepad.color.b, timer / _fadeTime);
            yield return null;
        }
        yield break;
    }

    private IEnumerator ControlsFadeIn()
    {
        float timer = 0;
        while (timer < _fadeTime)
        {
            timer += Time.deltaTime;
            _keyboard.color = new Color(_keyboard.color.r, _keyboard.color.g, _keyboard.color.b,  timer / _fadeTime);
            _gamepad.color = new Color(_gamepad.color.r, _gamepad.color.g, _gamepad.color.b, timer / _fadeTime);
            yield return null;
        }
        yield break;
    }

    public void SwitchIsPaused()
    {
        IsPaused = !IsPaused;
    }

    private void FadeOutScoreOnDistance()
    {
        _p1ScreenPosition = new Vector3(Camera.main.WorldToScreenPoint(_p1.transform.position).x, Camera.main.WorldToScreenPoint(_p1.transform.position).y, 0);
        _p2ScreenPosition = new Vector3(Camera.main.WorldToScreenPoint(_p2.transform.position).x, Camera.main.WorldToScreenPoint(_p2.transform.position).y, 0);

        float distanceP1ToP1HUD = Vector3.Distance(_p1HUD.position, _p1ScreenPosition);
        float distanceP2ToP1HUD = Vector3.Distance(_p1HUD.position, _p2ScreenPosition);

        float closestPlayerDistanceToP1HUD = Mathf.Min(distanceP1ToP1HUD, distanceP2ToP1HUD);

        float lerpedDistanceToP1HUD = Mathf.Lerp(1, 0, closestPlayerDistanceToP1HUD / _range);

        _scorePanelP1.color = 
            Color.Lerp(new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 0.8f), 
            new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 0.1f), _range / closestPlayerDistanceToP1HUD);

        playerScoreTexts[0].color = Color.Lerp(new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 1f),
            new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 0.1f), _range / closestPlayerDistanceToP1HUD);

        playerRoundsWonTexts[0].color = Color.Lerp(new Color(_scorePanelP2.color.r, _scorePanelP2.color.g, _scorePanelP2.color.b, 1f),
            new Color(_scorePanelP2.color.r, _scorePanelP2.color.g, _scorePanelP2.color.b, 0.1f), _range / closestPlayerDistanceToP1HUD);

        for (int i = 0; i < _heartIconsP1.Length; i++)
        {
            _heartIconsP1[i].color =
                Color.Lerp(new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 1f),
            new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 0.1f), _range / closestPlayerDistanceToP1HUD);
        }


        float distanceP1ToP2HUD = Vector3.Distance(_p2HUD.position, _p1ScreenPosition);
        float distanceP2ToP2HUD = Vector3.Distance(_p2HUD.position, _p2ScreenPosition);

        float closestPlayerDistanceToP2HUD = Mathf.Min(distanceP1ToP2HUD, distanceP2ToP2HUD);

        float lerpedDistanceToP2HUD = Mathf.Lerp(1, 0, closestPlayerDistanceToP2HUD / _range);

        _scorePanelP2.color = 
            Color.Lerp(new Color(_scorePanelP2.color.r, _scorePanelP2.color.g, _scorePanelP2.color.b, 0.8f), 
            new Color(_scorePanelP2.color.r, _scorePanelP2.color.g, _scorePanelP2.color.b, 0.1f), _range / closestPlayerDistanceToP2HUD);

        playerScoreTexts[1].color = Color.Lerp(new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 1f),
            new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 0.1f), _range / closestPlayerDistanceToP2HUD);

        playerRoundsWonTexts[1].color = Color.Lerp(new Color(_scorePanelP2.color.r, _scorePanelP2.color.g, _scorePanelP2.color.b, 1f),
            new Color(_scorePanelP2.color.r, _scorePanelP2.color.g, _scorePanelP2.color.b, 0.1f), _range / closestPlayerDistanceToP2HUD);

        for (int i = 0; i < _heartIconsP2.Length; i++)
        {
            _heartIconsP2[i].color =
                Color.Lerp(new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 1f),
            new Color(_scorePanelP1.color.r, _scorePanelP1.color.g, _scorePanelP1.color.b, 0.1f), _range / closestPlayerDistanceToP2HUD);
        }
    }
}
