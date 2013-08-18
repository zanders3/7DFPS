using UnityEngine;

public enum WeaponType
{
    BoxingGloveGun
}

[RequireComponent(typeof(BoxCollider))]
public class WeaponPickup : MonoBehaviour
{
    public WeaponType WeaponType = WeaponType.BoxingGloveGun;

    private Weapon currentWeapon;
    private float weaponSpawnTimer = 0.0f;

    public float WeaponSpawnTime = 3.0f;

    void SpawnWeapon()
    {
        currentWeapon = ((GameObject)GameObject.Instantiate(Resources.Load("Weapons/" + WeaponType), transform.position, Quaternion.identity)).GetComponent<Weapon>();
        currentWeapon.transform.parent = transform;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }

    void Start()
    {
        collider.isTrigger = true;
        SpawnWeapon();
    }

    void Update()
    {
        if (currentWeapon == null)
        {
            weaponSpawnTimer += Time.deltaTime;
            if (weaponSpawnTimer >= WeaponSpawnTime)
                SpawnWeapon();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ServerBase.IsClient && other.GetComponent<PlayerMovement>() != null)
        {
            Frontend.GetServer().SetPlayerWeapon(other.GetComponent<PlayerMovement>().Info, WeaponType);

            Destroy(currentWeapon);
            currentWeapon = null;
            weaponSpawnTimer = 0.0f;
        }
    }
}
