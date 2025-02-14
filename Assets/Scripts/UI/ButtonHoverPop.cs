using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIComponents
{
    public class ButtonHoverPop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {

        private Button _button;
        [SerializeField] private float _buttonHoverScaleXZ = 1.05f;
        [SerializeField] private float _buttonHoverScaleY = 1.2f;
        [SerializeField] private float _buttonShake = 0.75f;
        [SerializeField] private float _shakeTime = 0.25f;
        private Quaternion _originalRotation;
        private bool _isSelected;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Start()
        {
            _originalRotation = _button.GetComponent<RectTransform>().transform.rotation;
  
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isSelected) { return; }
            Selected();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isSelected) { return; }
            UnSelected();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_isSelected) { return; }
            Selected();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!_isSelected) { return; }
            UnSelected();
        }

        private void Selected()
        {
            _isSelected = !_isSelected;
            Vector3 scaleZero = _button.GetComponent<RectTransform>().localScale;
            _button.GetComponent<RectTransform>().localScale = new Vector3(scaleZero.x * _buttonHoverScaleXZ, scaleZero.y * _buttonHoverScaleY, scaleZero.z * _buttonHoverScaleXZ);
            StartCoroutine(ShakeCorroutine());
        }

        private void UnSelected()
        {
            _isSelected = !_isSelected;
            StopAllCoroutines();
            Vector3 newScale = _button.GetComponent<RectTransform>().localScale;
            _button.GetComponent<RectTransform>().localScale = new Vector3(newScale.x / _buttonHoverScaleXZ, newScale.y / _buttonHoverScaleY, newScale.z / _buttonHoverScaleXZ);
            _button.transform.rotation = Quaternion.Euler(Vector4.zero);
        }

        private IEnumerator ShakeCorroutine()
        {
            float timer = 0f;
            while (timer < _shakeTime)
            {
                _button.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-_buttonShake, _buttonShake));
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            yield break;
        }

    }

}

