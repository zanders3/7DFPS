using UnityEngine;

public enum WeaponType
{
    None,
    BoxingGloveGun
}

[RequireComponent(typeof(BoxCollider))]
public class WeaponPickup : MonoBehaviour
{
    public WeaponType WeaponType = WeaponType.BoxingGloveGun;
    private GameObject currentWeapon;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }

    void Start()
    {
        collider.isTrigger = true;

        currentWeapon = (GameObject)GameObject.Instantiate(Resources.Load("Weapons/" + WeaponType), transform.position, Quaternion.identity);
        currentWeapon.transform.parent = transform;
        foreach (var c in currentWeapon.GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    void Update()
    {
        currentWeapon.transform.Rotate(Vector3.up, Time.deltaTime * 90.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.IsServer && other.GetComponent<PlayerScript>() != null)
        {
            other.GetComponent<PlayerScript>().Player.SetWeapon(WeaponType);
        }
    }
}
