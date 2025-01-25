using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerTest : MonoBehaviour
{
    protected void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("Player Joined: " + playerInput.playerIndex);
        // Customize player setup if needed
    }
}
