using UnityEngine;
using System.Collections;

public class FrontEnd : MonoBehaviour 
{	
	public static FrontEnd Instance;
	private Rect mRect;
	private int mBlockWidth;
	private int mBlockHeight;
	
	public enum eFrontEndState
	{
		title,
		host,
		join,
		results,
        ingame
	}
	public eFrontEndState mState;
	
	public GUISkin mSkin;
	public string mPlayerName;
	public string mGameName;
	
	private HostData[] mHostList;

	// Use this for initialization
	void Start() 
    {	
		Instance = this;
		mBlockWidth = Screen.width / 20;
		mBlockHeight = Screen.height / 10;
		mRect = new Rect(0,0,mBlockWidth, mBlockHeight);
		mPlayerName = "Player";
		mGameName = "7DFPSAwesomeness";
		MasterServer.RequestHostList("7DFPSAwesomeness");
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
		case eFrontEndState.results:
			DrawResults();
			break;
        case eFrontEndState.ingame:
            DrawScoreboard();
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

    void DrawScoreboard()
    {
        if (LevelState.Instance != null)
        {
            GUILayout.BeginVertical();
            foreach (Player player in LevelState.Instance.Players)
                GUILayout.Label(player.Name);
            GUILayout.EndVertical();
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
		else
		{
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
	}
	
	void DrawResults()
	{
	}
	
	void HostGame()
	{
		Network.InitializeServer(32, 25002, !Network.HavePublicAddress());
		MasterServer.RegisterHost("7DFPSAwesomeness", mGameName); 
	}
	
	void JoinGame()
	{
	}
	
	public void SetState(eFrontEndState zState)
	{
        if (mState == zState)
            return;

		mState = zState;

        if (zState == eFrontEndState.ingame)
            LevelLoader.Load("TestLevel");
        else
            LevelLoader.Unload();
	}

    // Network events

    void OnServerInitialized()
    {
        Debug.Log("Server up and running!");
        FrontEnd.Instance.SetState(FrontEnd.eFrontEndState.ingame);
    }
    
    // client
    void OnConnectedToServer()
    {
        Debug.Log("Connected to server");
        FrontEnd.Instance.SetState(FrontEnd.eFrontEndState.ingame);
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
