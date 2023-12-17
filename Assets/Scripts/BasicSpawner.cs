using Fusion.Sockets;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.Photon.Realtime;
using System.Linq;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{

    private NetworkRunner _runner;

    [SerializeField] private NetworkPrefabRef _playerPrefab;

    private static BasicSpawner _Instance;

    public static BasicSpawner Instance => _Instance;

    [Serializable]
    public struct SpawnPoint
    {
        public Vector3 Position;
        public Vector3 Rotation;

        public SpawnPoint(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
        }

    }

    public GameObject[] SpawnPointLocators;

    SpawnPoint[] SpawnPoints;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    void Awake()
    {
        _Instance = this;
        if(SpawnPointLocators == null) 
        {
            throw new Exception("No spawn point locators!");
        }
        SpawnPoints = new SpawnPoint[SpawnPointLocators.Length];
        for(int i = 0; i < SpawnPointLocators.Length; i++)
        {
            SpawnPoints[i] = new SpawnPoint(SpawnPointLocators[i].transform.position, SpawnPointLocators[i].transform.eulerAngles);
        }
    }

    void OnDrawGizmos()
    {
        if(SpawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            for(int i = 0; i < SpawnPoints.Length; i++)
            {
                Gizmos.DrawSphere(SpawnPoints[i].Position, 0.5f);
            }
        }
    }

    private void OnGUI()
    {
        if (_runner != null) return;
        if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
        {
            StartGame(GameMode.Host);
        }
        if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
        {
            StartGame(GameMode.Client);
        }
    }

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoomm",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        //print("Game started");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //print("On player joined!");
        if (runner.IsServer)
        {
            // Create a unique position for the player
            var point = GetSpawnPointInit(player.RawEncoded);
            Vector3 spawnPosition = point.Position;
            NetworkObject networkPlayerObject = runner.Spawn(
                _playerPrefab, spawnPosition,
                Quaternion.Euler(point.Rotation), player,
                (runner, o) => {
                    //if (player == runner.LocalPlayer)
                    //{
                    //    PlayerCamera.Instance.Target = o.transform;
                    //} else
                    //{
                    //    // tell other player to get his camera attached to this object - done in PlayerTankController
                    //}
                    //print("Player spawned");
                }
            );
            // Keep track of the player avatars so we can remove it when they disconnect
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Find and remove the players avatar
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) 
    {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W))
            data.SetButton(NetworkInputData.FORWARD_BUTTON);
        if (Input.GetKey(KeyCode.S))
            data.SetButton(NetworkInputData.BACK_BUTTON);
        if (Input.GetKey(KeyCode.A))
            data.SetButton(NetworkInputData.LEFT_BUTTON);
        if (Input.GetKey(KeyCode.D))
            data.SetButton(NetworkInputData.RIGHT_BUTTON);

        if (Input.GetKey(KeyCode.Mouse0))
            data.SetButton(NetworkInputData.FIRE_BUTTON);
        if (Input.GetKey(KeyCode.Space))
            data.SetButton(NetworkInputData.SECONDARY_FIRE_BUTTON);

        if (Input.GetKey(KeyCode.J))
            data.SetButton(NetworkInputData.SUICIDE_BUTTON);

        if (!Input.GetKey(KeyCode.C))
        {
            data.MX = PlayerCamera.Instance.mx;
            data.MY = PlayerCamera.Instance.my;
        } else
        {
            data.MX = data.MY = float.NaN;
        }

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public SpawnPoint GetSpawnPointInit(int playerId)
    {
        return SpawnPoints[playerId % SpawnPoints.Length];
    }

    public SpawnPoint GetSpawnPointRespawn()
    {
        // TODO
        float[] Dists = new float[SpawnPoints.Length];
        for(int i = 0; i < Dists.Length; i++)
        {
            Dists[i] = float.MaxValue;
            foreach(var player in _spawnedCharacters.Values)
            {
                float d = Vector3.Distance(SpawnPoints[i].Position, player.transform.position);
                if (d < Dists[i])
                    Dists[i] = d;
            }
        }
        int inx = Array.IndexOf(Dists, Dists.Max());
        return SpawnPoints[inx];
    }

}