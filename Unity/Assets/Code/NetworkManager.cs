using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
	
	public static NetworkManager Instance;

	// Use this for initialization
	void Start () 
    {
		Instance = this;
	}

	// server
	void OnPlayerConnected(NetworkPlayer zPlayer)
	{
		Debug.Log("Player " + zPlayer + " connected");
	}
	
	void OnPlayerDisconnected(NetworkPlayer zPlayer)
	{
		Debug.Log("Player " + zPlayer + " disconnected");
	}
	
	void OnServerInitialized()
	{
		Debug.Log("Server up and running!");
		FrontEnd.Instance.SetState(FrontEnd.eFrontEndState.lobby);
	}
	
	// client
	void OnConnectedToServer()
	{
		Debug.Log("Connected to server");
		FrontEnd.Instance.SetState(FrontEnd.eFrontEndState.lobby);
	}
	
	void OnFailedToConnect(NetworkConnectionError zError)
	{
		Debug.Log ("Failed to connect to server: " + zError);
		FrontEnd.Instance.SetState(FrontEnd.eFrontEndState.title);
	}
	
	// both
	void OnDisconnectedFromServer(NetworkDisconnection zInfo)
	{
		if (Network.isServer)
		{
			Debug.Log("Local server disconnection");
		}
		else
		{
			if (zInfo == NetworkDisconnection.LostConnection)
				Debug.Log("Lost connection to server");
			else
				Debug.Log ("Successfully disconnected from server");
		}
		FrontEnd.Instance.SetState(FrontEnd.eFrontEndState.title);
	}
	
}
