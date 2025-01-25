using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public UnityEvent OnP1Resume;
    public UnityEvent OnP2Resume;
 
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

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
