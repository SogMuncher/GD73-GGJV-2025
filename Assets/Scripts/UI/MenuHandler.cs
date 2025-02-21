using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    //[SerializeField] private string _gameScene = "InputTestScene";

    public void Play(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ResumeTime() 
    {
       Time.timeScale = 1f;
    }
}
