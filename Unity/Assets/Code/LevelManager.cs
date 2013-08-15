using UnityEngine;

public class LevelManager
{
    private static GameObject currentLevel = null;

    public static void LoadLevel()
    {
        if (currentLevel != null)
            ClearLevel();

        currentLevel = (GameObject)GameObject.Instantiate(Resources.Load("Levels/TestLevel"), Vector3.zero, Quaternion.identity);
    }

    public static void ClearLevel()
    {
        if (currentLevel != null)
        {
            GameObject.Destroy(currentLevel);
            currentLevel = null;
        }
    }
}
