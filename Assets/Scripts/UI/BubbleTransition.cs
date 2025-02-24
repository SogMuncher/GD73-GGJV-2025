using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BubbleTransition : MonoBehaviour
{
    [SerializeField] private float _playbacktime = 2.2f;
    [SerializeField] private float _blackScreenDuration = 2f;
    [SerializeField] Image _blackScreen;
    private ParticleSystem _particleSystem;

    public UnityEvent<string> OnTransitionEnd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        //_playbacktime = (_particleSystem.duration + _particleSystem.startLifetime) - _blackScreenDuration;
    }

    public void PlayBoobleTransition(string sceneName)
    {
        StartCoroutine(PlayBubbleTransitionCorroutine(sceneName));
    }
    
    private IEnumerator PlayBubbleTransitionCorroutine(string sceneName)
    {
        _particleSystem.Play();
        yield return new WaitForSeconds(_playbacktime);

        float timer = 0f;
        while (timer < _blackScreenDuration)
        {
            _blackScreen.color = new Color(_blackScreen.color.r, _blackScreen.color.g, _blackScreen.color.b, (timer / _blackScreenDuration) + 0.15f);
            timer += Time.deltaTime;
            yield return null;
        }

        OnTransitionEnd.Invoke(sceneName);
        yield break;
    }
}
