using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LlamAcademy.Spring;
using LlamAcademy.Spring.Runtime;


namespace UIComponents
{
    public class AutoSelect : MonoBehaviour
    {
        public UnityEvent OnAutoSelected;

        private void Start()
        {
            if (TryGetComponent(out Selectable button))
            {
                EventSystem.current.SetSelectedGameObject(null);
                button.Select();
                //OnAutoSelected?.Invoke();
            }
        }

        private void OnEnable()
        {
            if (TryGetComponent(out Selectable button))
            {
                EventSystem.current.SetSelectedGameObject(null);
                button.Select();
                //OnAutoSelected?.Invoke();
            }
        }
    }
}