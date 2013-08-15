using UnityEngine;

public enum FrontendState
{
    Title,
    Lobby,
    InGame
}

public class Frontend : MonoBehaviour
{
    private static string[] playerNames = new string[]
    {
        "Antione",
        "Vergie",
        "Freddie",
        "Illa",
        "Melisa",
        "Jaqueline",
        "Ronny",
        "Numbers",
        "Horacio",
        "Renita",
        "Marvella",
        "Sherrill",
        "Hildred",
        "Lakia",
        "Genevive",
        "Luba",
        "Valery",
        "Mack",
        "Dewitt",
        "Coral"
    };

    private static Frontend instance = null;

    string playerName, gameName;
    FrontendState state = FrontendState.Title;
    ServerBase networkManager = null;

    void Start()
    {
        instance = this;

        playerName = playerNames[Random.Range(0, playerNames.Length)];
        gameName = playerName + "'s Game";
        System.Threading.Monitor.Enter(new object());
    }

    void ResetNetwork()
    {
        if (networkManager != null)
        {
            networkManager.Close();
            networkManager = null;
        }
    }

    void OnDisable()
    {
        ResetNetwork();
    }

    public static void SetState(FrontendState state)
    {
        if (instance.state == state)
            return;

        instance.state = state;

        if (state == FrontendState.Title)
        {
            instance.ResetNetwork();
            LevelManager.ClearLevel();
        }
    }

    void Update()
    {
        if (networkManager != null)
            networkManager.Poll();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 50.0f, 0.0f, 50.0f, 20.0f), "Console"))
            DebugConsole.IsOpen = !DebugConsole.IsOpen;

        switch (state)
        {
            case FrontendState.Title:
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("7DFPS");

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Player Name");
                        playerName = GUILayout.TextField(playerName);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Game Name");
                        gameName = GUILayout.TextField(gameName);
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Host"))
                    {
                        if (networkManager != null)
                            networkManager.Close();

                        networkManager = new Server(playerName, gameName);
                        SetState(FrontendState.InGame);
                    }

                    if (GUILayout.Button("Join"))
                    {
                        if (networkManager != null)
                            networkManager.Close();

                        networkManager = new Client(playerName);
                        SetState(FrontendState.Lobby);
                    }
                }
                GUILayout.EndVertical();
                break;

            case FrontendState.Lobby:
                GUILayout.BeginVertical();
                {
                    foreach (GameServer host in ((Client)networkManager).DiscoverClients())
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(host.GameName);
                            if (GUILayout.Button("Join"))
                            {
                                ((Client)networkManager).Join(host);
                                SetState(FrontendState.InGame);
                                break;
                            }
                        }
                        GUILayout.EndVertical();
                    }

                    if (GUILayout.Button("Back"))
                    {
                        SetState(FrontendState.Title);
                    }
                }
                GUILayout.EndVertical();
                break;

            case FrontendState.InGame:
                GUILayout.BeginVertical();
                if (GUILayout.Button("Quit"))
                    SetState(FrontendState.Title);
                break;
        }
    }
}

