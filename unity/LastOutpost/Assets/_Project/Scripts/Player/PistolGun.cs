using UnityEngine;

public class PistolGun : Gun
{
    public override void Reload()
    {
        throw new System.NotImplementedException();
    }

    public override void Shoot()
    {
        // Implémentation concrète ici, car il n'y a pas de méthode de base à appeler
        // Exemple : décrémenter les munitions et déclencher un tir
        if (_ammo > 0)
        {
            _ammo--;
            // Logique de tir (effets, projectiles, etc.)
            Debug.Log("Pistolet : tir effectué. Munitions restantes : " + _ammo);
        }
        else
        {
            Debug.Log("Pistolet : plus de munitions, rechargez !");
        }
    }

}
