using UnityEngine;

public class ReloadAnimationManager : MonoBehaviour
{
    [SerializeField] private GameObject _ammunitionObject;
    [SerializeField] private Animator _animator;
    [SerializeField] private string _reloadAnimationTriggerName = "Reload";
    [SerializeField] private string _idleAnimationBoolName = "Idle";

    private void Start()
    {
        if (_ammunitionObject == null)
        {
            Debug.LogError("[ReloadAnimationManager] Ammunition object reference is missing!");
        }
        if (_animator == null)
        {
            Debug.LogError("[ReloadAnimationManager] Animator reference is missing!");
        }
        this.DesactiveAmmunition(); 
    }

    public void ActiveAmmunition()
    {
        if (_ammunitionObject != null)
        {
            _ammunitionObject.SetActive(true);
        }
    }

    public void DesactiveAmmunition()
    {
        if (_ammunitionObject != null)
        {
            _ammunitionObject.SetActive(false);
        }
    }

    public void PlayReloadAnimation()
    {
        if (_animator != null)
        {
            _animator.SetBool(_idleAnimationBoolName, false);
            _animator.SetTrigger(_reloadAnimationTriggerName);
        }
    }
}
