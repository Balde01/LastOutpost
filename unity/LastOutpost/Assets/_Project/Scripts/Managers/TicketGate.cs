using UnityEngine;
[RequireComponent(typeof(Animator))]
public class TicketGate : MonoBehaviour
{
    [Header("Ticket Gate Settings")]
    [SerializeField] private bool _isOpen = false;
    [SerializeField] private Animator _gateAnimator;
    [SerializeField] private string _openAnimationTrigger = "Open";
    [SerializeField] private string _closeAnimationTrigger = "Close";
    [SerializeField] private Collider _gateCollider; // Collider to block passage when gate is closed
    public bool IsOpen => _isOpen;
    public void OpenGate()
    {
        if (!_isOpen)
        {
            _isOpen = true;
            if (_gateAnimator != null)
            {
                _gateCollider.enabled = false; // Disable collider to allow passage
                _gateAnimator.SetTrigger(_openAnimationTrigger);
            }
        }
    }
    public void CloseGate()
    {
        if (_isOpen)
        {
            _isOpen = false;
            if (_gateAnimator != null)
            {
                _gateCollider.enabled = true; // Enable collider to block passage
                _gateAnimator.SetTrigger(_closeAnimationTrigger);
            }
        }
    }
}