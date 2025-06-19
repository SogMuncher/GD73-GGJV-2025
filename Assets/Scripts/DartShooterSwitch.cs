using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class DartShooterSwitch : MonoBehaviour
{
    public bool IsOn { get; private set; }  = true;

    [SerializeField] private GameObject _baseOff;
    [SerializeField] private GameObject _leverOff;
    [SerializeField] private GameObject _baseOn;
    [SerializeField] private GameObject _leverOn;

    [HideInInspector] public UnityEvent OnSwitchOn;
    [HideInInspector] public UnityEvent OnSwitchOff;
    

    public void SwitchOnOff()
    {
        if (IsOn == false )
        {
            _baseOff.gameObject.SetActive(false);
            _leverOff.gameObject.SetActive(false);
            _baseOn.gameObject.SetActive(true);
            _leverOn.gameObject.SetActive(true);
            OnSwitchOn?.Invoke();
            Debug.Log("Switch ON!");
            IsOn = true;
            return;
        }

        if (IsOn == true)
        {
            _baseOff.gameObject.SetActive(true);
            _leverOff.gameObject.SetActive(true);
            _baseOn.gameObject.SetActive(false);
            _leverOn.gameObject.SetActive(false);
            OnSwitchOff?.Invoke();
            Debug.Log("Switch OFF!");
            IsOn = false;
            return;
        }
    }
}
