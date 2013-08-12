using UnityEngine;
using System.Collections;

/// <summary>
/// Loads and unloads game state.
/// </summary>
public static class Game
{
    private static GameObject level = null;

    /// <summary>
    /// Loads the level prefab and spawns the player characters, etc.
    /// </summary>
	public static void LoadLevel(string levelName)
    {
        level = (GameObject)GameObject.Instantiate(Resources.Load("Levels/" + levelName), Vector3.zero, Quaternion.identity);
    }

    public static void UnloadLevel()
    {
        GameObject.Destroy(level);
    }
}
