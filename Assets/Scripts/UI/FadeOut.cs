using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeOut : MonoBehaviour
{
    [SerializeField] private float _blackScreenDuration;
    [SerializeField] private Image _blackScreen;
    
    private void Start()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        float timer = _blackScreenDuration;
        while (timer > 0)
        {
            _blackScreen.color = new Color(_blackScreen.color.r, _blackScreen.color.g, _blackScreen.color.b, (timer / _blackScreenDuration));
            timer -= Time.deltaTime;
            yield return null;
        }

        yield break;
    }
}
