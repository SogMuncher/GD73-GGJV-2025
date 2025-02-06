using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIComponents
{
    public class SetCursorLockMode : MonoBehaviour
    {
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void SetModeNone()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        public void SetModeConfined()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void SetModeLocked()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void SetVisibilityOn()
        {
            Cursor.visible = true;
        }

        public void SetVisibilityOff()
        {
            Cursor.visible = false;
        }
    }
}