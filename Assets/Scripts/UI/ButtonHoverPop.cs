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
    public class ButtonHoverPop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private Button _button;
        private bool _isSelected;

        //[Header("Scale")]
        //[SerializeField] private float _buttonHoverScaleXZ = 1.05f;
        //[SerializeField] private float _buttonHoverScaleY = 1.2f;
        //[SerializeField] private float _scaleDuration = 0.5f;
        //[SerializeField] private float _scaleSpringDamping = 0.15f;
        //[SerializeField] private float _scaleSpringFrequency = 2.0f;
        //[SerializeField] private float _scaleDampingRatio = 0.2f;
        private Vector3 _originalScale;

        [Header("Shake")]
        [SerializeField] private float _shakeAmplitud = 0.75f;
        [SerializeField] private float _shakeDuration = 0.25f;
        [SerializeField] private float _shakeSpringDamping = 0.15f;
        [SerializeField] private float _shakeSpringFrequency = 1.0f;
        [SerializeField] private float _shakeDampingRatio = 0.2f;

        public UnityEvent OnCursorHover;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = _button.GetComponent<RectTransform>().localScale;
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
            OnCursorHover?.Invoke();
            _isSelected = !_isSelected;
            //Vector3 scaleZero = _button.GetComponent<RectTransform>().localScale;
            //_button.GetComponent<RectTransform>().localScale = new Vector3(scaleZero.x * _buttonHoverScaleXZ, scaleZero.y * _buttonHoverScaleY, scaleZero.z * _buttonHoverScaleXZ);
            StartCoroutine(ShakeCoroutine());

            //StartCoroutine(ScaleCoroutine(targetScale, _scaleDuration, _scaleSpringDamping, _scaleSpringFrequency, _scaleDampingRatio));
        }

        private void UnSelected()
        {
            _isSelected = !_isSelected;
            StopAllCoroutines();
            //_button.transform.rotation = Quaternion.Euler(Vector4.zero);
            //_button.GetComponent<RectTransform>().localScale = _originalScale;
        }

        private IEnumerator ShakeCoroutine()
        {
            float timer = 0f;

            while (timer < _shakeDuration)
            {
                float t = timer / _shakeDuration;
                float damping = Mathf.Exp(-_shakeSpringDamping * t);
                float frequency = Mathf.Cos(_shakeSpringFrequency * 2 * Mathf.PI * t) * (1 - _shakeDampingRatio);
                float shakeAmount = _shakeAmplitud * damping * frequency;
                _button.transform.rotation = Quaternion.Euler(0, 0, shakeAmount);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            _button.transform.rotation = Quaternion.Euler(Vector4.zero);
            yield break;

            //float timer = 0f;
            //while (timer < _shakeTime)
            //{
            //    _button.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-_buttonShake, _buttonShake));
            //    timer += Time.unscaledDeltaTime;
            //    yield return null;
            //}
            //yield break;
        }

        private IEnumerator ScaleCoroutine(Vector3 targetScale, float duration, float springDamping, float springFrequency, float dampingRatio)
        {
            float timer = 0f;
            Vector3 initialScale = _originalScale;
            Vector3 velocity = Vector3.zero;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / duration;
                float damping = Mathf.Exp(-springDamping * t);
                float frequency = Mathf.Cos(springFrequency * 2 * Mathf.PI * t) * (1 - dampingRatio);
                float interpolation = damping + frequency;

                Vector3 newScale = Vector3.SmoothDamp(initialScale, targetScale, ref velocity, interpolation, Mathf.Infinity, Time.unscaledDeltaTime);
                _button.GetComponent<RectTransform>().localScale = newScale;

                //_button.GetComponent<RectTransform>().localScale = Vector3.Lerp(initialScale, targetScale, interpolation);
                yield return null;
            }

            _button.GetComponent<RectTransform>().localScale = targetScale;
            yield break;
        }

    }

}

