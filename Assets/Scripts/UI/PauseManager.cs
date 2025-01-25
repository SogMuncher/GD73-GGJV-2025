using UnityEngine;
using UnityEngine.Events;

public class PauseManager : MonoBehaviour
{
    public UnityEvent OnResume;
 
    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        OnResume.Invoke();

    }

    public void Quit()
    {
        Application.Quit();
    }
}
