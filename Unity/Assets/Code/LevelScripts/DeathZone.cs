using System;
using UnityEngine;

/// <summary>
/// Insta-kills any player that moves into the zone.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class DeathZone : MonoBehaviour
{
    void Start()
    {
        this.collider.isTrigger = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
        Gizmos.DrawCube(collider.bounds.center, collider.bounds.extents);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.extents);
    }

    void OnTriggerEnter(Collider other)
    {
        /*if (!ServerBase.IsClient && other.GetComponent<PlayerMovement>() != null)
        {
            Frontend.GetServer().KillPlayer(other.GetComponent<PlayerMovement>().Info);
        }*/
    }
}
