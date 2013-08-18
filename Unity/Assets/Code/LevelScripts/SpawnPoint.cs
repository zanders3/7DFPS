using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines a player starting position.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    private static List<Vector3> spawnPoints = new List<Vector3>();

    public static Vector3 GetSpawnPoint()
    {
        if (spawnPoints.Count > 0)
            return spawnPoints[Random.Range(0, spawnPoints.Count)];
        else
            return Vector3.zero;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }

    void Start()
    {
        spawnPoints.Add(transform.position);
    }

    void OnDisable()
    {
        spawnPoints.Remove(transform.position);
    }
}
