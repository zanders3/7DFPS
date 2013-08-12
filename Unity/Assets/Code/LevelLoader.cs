using UnityEngine;
using System.Collections;

/// <summary>
/// Loads and unloads game state.
/// </summary>
public static class LevelLoader
{
    private static GameObject level = null;
    private static GameObject levelState = null;

    /// <summary>
    /// Loads the level prefab. Will unload the current level if needed.
    /// </summary>
	public static void Load(string levelName)
    {
        if (level != null)
            return;

        level = (GameObject)GameObject.Instantiate(Resources.Load("Levels/" + levelName), Vector3.zero, Quaternion.identity);

        Network.Instantiate(Resources.Load("Player"), Vector3.zero, Quaternion.identity, 0);

        if (Network.isServer)
        {
            levelState = (GameObject)Network.Instantiate(Resources.Load("LevelState"), Vector3.zero, Quaternion.identity, 0);
        }
    }

    /// <summary>
    /// Unloads the game scene.
    /// </summary>
    public static void Unload()
    {
        if (level == null)
            return;

        GameObject.Destroy(level);

        if (Network.isServer)
            Network.Destroy(levelState);
    }
}
