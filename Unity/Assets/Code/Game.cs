using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	
	public static Game Instance;
	
	public bool mbInGame;

	// Use this for initialization
	void Start () {
		Instance = this;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void StartGame()
	{
		mbInGame = true;
	}
	
	public void EndGame()
	{
		mbInGame = false;
	}
}
