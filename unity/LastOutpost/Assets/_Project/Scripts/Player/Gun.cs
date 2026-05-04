using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [SerializeField] protected int _ammo;
    [SerializeField] protected float _fireRate;

    public abstract void Shoot();
    public abstract void Reload();
}
