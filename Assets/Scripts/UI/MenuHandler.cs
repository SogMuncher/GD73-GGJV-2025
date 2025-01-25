using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] private string _gameScene = "InputTestScene";

    public void Play()
    {
        SceneManager.LoadScene(_gameScene);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OnSelect(InputAction.CallbackContext context)
    {

    }
}
