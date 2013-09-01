using UnityEngine;

public class BoxingGloveWeapon : Weapon
{
    public override WeaponType Type { get { return WeaponType.BoxingGloveGun; } }
    
    public float MaxDistance = 5.0f, HitForce = 300.0f;
    
    protected override void DoFire(Ray fireDirection)
    {
        /*RaycastHit hitInfo;
        if (Physics.Raycast(fireDirection, out hitInfo) && hitInfo.distance <= MaxDistance && hitInfo.collider.GetComponent<PlayerMovement>() != null)
        {
            hitInfo.rigidbody.AddForce(fireDirection.direction * HitForce);
        }*/
    }
}

