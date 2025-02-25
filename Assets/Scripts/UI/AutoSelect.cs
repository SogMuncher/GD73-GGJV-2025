using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            }
        }

        private void OnEnable()
        {
            if (TryGetComponent(out Selectable button))
            {
                EventSystem.current.SetSelectedGameObject(null);
                button.Select();
                OnAutoSelected?.Invoke();
            }
        }
    }
}