using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerTest : MonoBehaviour
{
    private void OnEnable()
    {
        //PlayerInputManager.instance.playerJoinedEvent.AddListener(OnPlayerJoined);
    }

    private void OnDisable()
    {
        //PlayerInputManager.instance.playerJoinedEvent.RemoveListener(OnPlayerJoined);
    }

    protected void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("Player Joined: " + playerInput.playerIndex);
        // Customize player setup if needed
    }
}
