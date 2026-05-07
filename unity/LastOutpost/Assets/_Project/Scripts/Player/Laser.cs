using NaughtyAttributes;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [Foldout("Laser References"), SerializeField] private GameObject _laser;
    [Foldout("Laser References"), SerializeField] private float _laserMaxDistance = 200f;
    [Foldout("Raycast Settings"), SerializeField] private float _raycastDistance = 100f;

    private void Start()
    {
        if (ValidatorReferences.Validate(this, (_laser, nameof(_laser))))
        {
            enabled = false;
            return;
        }
    }
    void Update()
    {
        bool hit = Physics.Raycast(_laser.transform.position, _laser.transform.forward, out RaycastHit hitCast, _raycastDistance);

         if (hit)
         {
              _laser.transform.localScale = new Vector3(_laser.transform.localScale.x, hitCast.distance, _laser.transform.localScale.z);
         }
         else
         {
              _laser.transform.localScale = new Vector3(_laser.transform.localScale.x, _laserMaxDistance, _laser.transform.localScale.z);
        }
    }
}
