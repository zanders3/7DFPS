using UnityEngine;
using System.Collections;

public class FrontEnd : MonoBehaviour {
	
	public static FrontEnd Instance;
	private Rect mRect;
	private int mBlockWidth;
	private int mBlockHeight;
	
	public enum eFrontEndState
	{
		title,
		host,
		join,
		lobby,
		results,
	}
	public eFrontEndState mState;
	
	public GUISkin mSkin;
	public string mPlayerName;
	public string mGameName;
	
	private HostData[] mHostList;

	// Use this for initialization
	void Start () {
		
		Instance = this;
		mBlockWidth = Screen.width / 20;
		mBlockHeight = Screen.height / 10;
		mRect = new Rect(0,0,mBlockWidth, mBlockHeight);
		mPlayerName = "Player";
		mGameName = "7DFPSAwesomeness";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		if (mSkin != null)
			GUI.skin = mSkin;
		switch(mState)
		{
		case eFrontEndState.title:
			DrawTitle();
			break;
		case eFrontEndState.host:
			DrawHost();
			break;
		case eFrontEndState.join:
			DrawJoin();
			break;
		case eFrontEndState.lobby:
			DrawLobby();
			break;
		case eFrontEndState.results:
			DrawResults();
			break;
		}
	}
	
	void DrawTitle()
	{
		mRect.Set(mBlockWidth, mBlockHeight, mBlockWidth * 4, mBlockHeight);
		GUI.Label(mRect, "Player name:");
		mRect.x += mRect.width;
		mPlayerName = GUI.TextArea(mRect, mPlayerName);
		mRect.Set(mBlockWidth, mRect.y + mRect.height, mRect.width, mRect.height);
		if (GUI.Button(mRect, "Host"))
		{
			SetState(eFrontEndState.host);
		}
		mRect.y += mRect.height;
		if (GUI.Button(mRect, "Join"))
		{
			MasterServer.RequestHostList("7DFPSAwesomeness");
			mHostList = MasterServer.PollHostList();
			SetState(eFrontEndState.join);
		}
	}
	
	void DrawHost()
	{
		mRect.Set(mBlockWidth, mBlockHeight, mBlockWidth * 4, mBlockHeight);
		GUI.Label(mRect, "Game name: ");
		mRect.x += mRect.width;
		mGameName = GUI.TextArea(mRect, mGameName);
		mRect.Set(mBlockWidth, mRect.y + mRect.height, mRect.width, mRect.height);
		if (GUI.Button(mRect, "Host"))
		{
			HostGame();
		}
		mRect.y += mRect.height;
		if (GUI.Button(mRect, "Back"))
		{
			SetState(eFrontEndState.title);
		}
	}
	
	void DrawJoin()
	{
		mRect.Set(mBlockWidth, mBlockHeight, mBlockWidth * 4, mBlockHeight);
		if (mHostList.Length == 0)
		{
			if (GUI.Button(mRect, "Refresh"))
			{
				MasterServer.PollHostList();
			}
		}
		foreach (HostData lGame in mHostList)
		{
			GUI.Label(mRect,lGame.gameName);
			mRect.x += mRect.width;
			if (GUI.Button(mRect, "Join"))
			{
				Network.Connect(lGame);
			}
		}
	}
	
	void DrawLobby()
	{
		mRect.Set(mBlockWidth, mBlockHeight, mBlockWidth * 4, mBlockHeight);
		GUI.Label(mRect, "Lobby");
		mRect.y += mRect.height;
		if (Network.isServer)
		{
			if (GUI.Button(mRect, "Start"))
			{
				StartGame();
			}
		}
		mRect.x += mRect.width;
		if (GUI.Button(mRect, "Back"))
		{
			Network.Disconnect();
		}
	}
	
	void DrawResults()
	{
	}
	
	void HostGame()
	{
		Network.InitializeServer(32, 25002, !Network.HavePublicAddress());
		MasterServer.RegisterHost("7DFPSAwesomeness", mGameName); 
	}
	
	void StartGame()
	{
		Debug.Log("Starting Game");
	}
	
	void JoinGame()
	{
	}
	
	public void SetState(eFrontEndState zState)
	{
		mState = zState;
	}
}
