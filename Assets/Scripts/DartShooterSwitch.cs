using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class DartShooterSwitch : MonoBehaviour
{
    private bool _isOn = true;

    [SerializeField] private GameObject _baseOff;
    [SerializeField] private GameObject _leverOff;
    [SerializeField] private GameObject _baseOn;
    [SerializeField] private GameObject _leverOn;

    [HideInInspector] public UnityEvent OnSwitchOn;
    [HideInInspector] public UnityEvent OnSwitchOff;
    

    public void SwitchOnOff()
    {
        if (_isOn == false )
        {
            _baseOff.gameObject.SetActive(false);
            _leverOff.gameObject.SetActive(false);
            _baseOn.gameObject.SetActive(true);
            _leverOn.gameObject.SetActive(true);
            OnSwitchOn?.Invoke();
            Debug.Log("Switch ON!");
            _isOn = true;
            return;
        }

        if (_isOn == true)
        {
            _baseOff.gameObject.SetActive(true);
            _leverOff.gameObject.SetActive(true);
            _baseOn.gameObject.SetActive(false);
            _leverOn.gameObject.SetActive(false);
            OnSwitchOff?.Invoke();
            Debug.Log("Switch OFF!");
            _isOn = false;
            return;
        }
    }
}
