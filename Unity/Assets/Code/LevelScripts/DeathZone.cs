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

    void OnTriggerEnter(Collider other)
    {
        if (!ServerBase.IsClient && other.GetComponent<PlayerMovement>() != null)
        {
            Frontend.GetServer().KillPlayer(other.GetComponent<PlayerMovement>().Info);
        }
    }
}
