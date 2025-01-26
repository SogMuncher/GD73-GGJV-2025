using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public UnityEvent OnP1Resume;
    public UnityEvent OnP2Resume;

    [Header("Tween")]
    [SerializeField] private RectTransform _pause1RectTransform;
    [SerializeField] private RectTransform _pause2RectTransform;
    [SerializeField] private float SidePosXp1, finalPosXp1;
    [SerializeField] private float SidePosXp2, finalPosXp2;
    [SerializeField] private float tweenDuration;

    private void Start()
    {
        //SidePosXp1 = _pause1RectTransform.transform.position.x;
        //SidePosXp2 = _pause2RectTransform.transform.position.x;
    }
    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void ResumeP1()
    {
        Time.timeScale = 1f;
        OnP1Resume.Invoke();

    }
    public void ResumeP2()
    {
        Time.timeScale = 1f;
        OnP2Resume.Invoke();


    }

    public void Quit()
    {
        Application.Quit();
    }

    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void PauseIntroP1()
    {
        _pause1RectTransform.DOAnchorPosX(finalPosXp1, tweenDuration).SetUpdate(true);


    }
    public void PauseOutroP1()
    {
        _pause1RectTransform.DOAnchorPosX(SidePosXp1, tweenDuration).SetUpdate(true);


    }

    public void PauseIntroP2()
    {
        _pause2RectTransform.DOAnchorPosX(finalPosXp2, tweenDuration).SetUpdate(true);


    }
    public void PauseOutroP2()
    {
        _pause2RectTransform.DOAnchorPosX(SidePosXp2, tweenDuration).SetUpdate(true);


    }

}
