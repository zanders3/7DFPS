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

    void Start()
    {
        instance = this;

        playerName = playerNames[Random.Range(0, playerNames.Length)];
        gameName = playerName + "'s Game";
    }

    void ResetNetwork()
    {
        NetworkManager.Stop();
    }

    void OnDisable()
    {
        ResetNetwork();
    }

    void OnConnected(long playerID)
    {
    }

    void OnDisconnected(long playerID)
    {
        //TODO: easy/simple player object destruction (SHOULD be automatic)
    }

    public static void SetState(FrontendState state)
    {
        if (instance.state == state)
            return;

        instance.state = state;

        if (state == FrontendState.Title)
        {
            instance.ResetNetwork();
        }
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

                    bool startNetwork = false;
                    bool isServer = false;

                    if (GUILayout.Button("Host"))
                    {
                        startNetwork = true;
                        isServer = true;
                    }

                    if (GUILayout.Button("Join"))
                    {
                        startNetwork = true;
                        isServer = false;
                    }

                    if (startNetwork)
                    {
                        NetworkManager.Start(isServer, gameName, OnConnected, OnDisconnected);
                        SetState(isServer ? FrontendState.InGame : FrontendState.Lobby);

                        NetworkManager.Replicator.Register<Player>();
                        NetworkManager.Replicator.Register<PlayerList>();

                        PlayerList.SetPlayerName(playerName);

                        if (isServer)
                            NetworkManager.Replicator.Create<PlayerList>();
                    }
                }
                GUILayout.EndVertical();
                break;

            case FrontendState.Lobby:
                GUILayout.BeginVertical();
                {
                    foreach (GameServer host in NetworkManager.DiscoverGames())
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(host.GameName);
                            if (GUILayout.Button("Join"))
                            {
                                NetworkManager.Join(host);
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

