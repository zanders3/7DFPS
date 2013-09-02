using UnityEngine;

public class Weapon
{
    private WeaponType type = WeaponType.None;
    private Transform playerHead;
    private GameObject weapon = null;

    public Weapon(Transform playerHead)
    {
        this.playerHead = playerHead;
    }

    public WeaponType Type 
    { 
        get { return type; } 
        set 
        {
            if (type != value)
            {
                if (weapon != null)
                {
                    GameObject.Destroy(weapon);
                    weapon = null;
                }

                if (value != WeaponType.None)
                {
                    weapon = (GameObject)GameObject.Instantiate(Resources.Load("Weapons/" + value));
                    weapon.transform.parent = playerHead;

                    weapon.transform.localPosition = new Vector3(0.3f, -0.3f, 0.0f);
                    weapon.transform.localRotation = Quaternion.identity;
                }

                type = value;
            }
        }
    }

    public void Fire()
    {
        switch (Type)
        {
             case WeaponType.BoxingGloveGun:

                RaycastHit hit;
                if (Physics.Raycast(new Ray(playerHead.transform.position, playerHead.transform.forward), out hit))
                {
                    if (hit.collider.GetComponent<PlayerScript>() != null && hit.distance < 20.0f)
                        hit.collider.GetComponent<PlayerScript>().rigidbody.AddForce(playerHead.transform.forward * 10000.0f);
                }

                break;
        }
    }
}
