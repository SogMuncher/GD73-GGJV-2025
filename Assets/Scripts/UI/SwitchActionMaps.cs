using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UIComponents
{
    public class SwitchActionMaps : MonoBehaviour
    {
        //[SerializeField] private bool _allPlayers = true;
        [SerializeField] PlayerInput playerInput;

        public void SwitchMap(string newMap)
        {
            playerInput.SwitchCurrentActionMap(newMap);
        }
    }
}