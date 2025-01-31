using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public UnityEvent OnP1Resume;
    public UnityEvent OnP2Resume;
    public UnityEvent OnP1OutroEnd;
    public UnityEvent OnP2OutroEnd;

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
        StartCoroutine(PauseOutroP1Routine());

    }

    private IEnumerator PauseOutroP1Routine()
    {
        _pause1RectTransform.DOAnchorPosX(SidePosXp1, tweenDuration).SetUpdate(true);
        yield return new WaitForSeconds(tweenDuration);
        StartCoroutine(SetP1ActiveFalse());
    }

    private IEnumerator SetP1ActiveFalse()
    {
        OnP1OutroEnd.Invoke();
        yield break;
    }

    public void PauseIntroP2()
    {
        _pause2RectTransform.DOAnchorPosX(finalPosXp2, tweenDuration).SetUpdate(true);

    }
    public void PauseOutroP2()
    {
        StartCoroutine(PauseOutroP2Routine());
    }

    private IEnumerator PauseOutroP2Routine()
    {
        _pause2RectTransform.DOAnchorPosX(SidePosXp2, tweenDuration).SetUpdate(true);
        yield return new WaitForSeconds(tweenDuration);
        StartCoroutine(SetP2ActiveFalse());
    }

    private IEnumerator SetP2ActiveFalse()
    {
        OnP2OutroEnd.Invoke();
        yield break;
    }
}
