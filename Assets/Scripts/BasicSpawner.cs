using Fusion.Sockets;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    SpawnPoint[] SpawnPoints;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    void Awake()
    {
        _Instance = this;
        var SpawnPointLocators = RootGameManager.Instance.SpawnPointLocators;
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

    #region UI_METHODS

    public void StartHost()
    {
        StartGame(GameMode.Host);
    }

    public void JoinGame()
    {
        StartGame(GameMode.Client);
    }

    #endregion

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        var d = _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = UIManager.GetGameRoomNameInput(),
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        await d;
        if (d.Result.Ok)
        {
            UIManager.SetMPGameWindowActive(false);
            UIManager.SetNetworkStateText($"Network: CONN");
        } else
        {
            Destroy(_runner);
            transform.SetParent(null);
            string msg = "Network: ";
            switch (d.Result.ShutdownReason)
            {
                case ShutdownReason.Ok:
                    break;
                case ShutdownReason.Error:
                    msg += "INT_ERR";
                    break;
                case ShutdownReason.IncompatibleConfiguration:
                    msg += "INC_CONFIG";
                    break;
                case ShutdownReason.ServerInRoom:
                    msg += "DUPL_HOST";
                    break;
                case ShutdownReason.DisconnectedByPluginLogic:
                    msg += "P_ERR";
                    break;
                case ShutdownReason.GameClosed:
                    msg += "CLOSED";
                    break;
                case ShutdownReason.GameNotFound:
                    msg += "NOT_FOUND";
                    break;
                case ShutdownReason.MaxCcuReached:
                    msg += "NO_SPACE";
                    break;
                case ShutdownReason.GameIdAlreadyExists:
                    break;
                case ShutdownReason.GameIsFull:
                    msg += "GAME_FULL";
                    break;
                case ShutdownReason.InvalidRegion:
                case ShutdownReason.InvalidAuthentication:
                case ShutdownReason.CustomAuthenticationFailed:
                case ShutdownReason.AuthenticationTicketExpired:
                case ShutdownReason.PhotonCloudTimeout:
                case ShutdownReason.AlreadyRunning:
                case ShutdownReason.InvalidArguments:
                case ShutdownReason.HostMigration:
                case ShutdownReason.ConnectionTimeout:
                case ShutdownReason.ConnectionRefused:
                default:
                    msg += "ERR";
                    break;
            }
            UIManager.SetNetworkStateText(msg);
            Instantiate(RootGameManager.Instance.BasicSpawnerPrefab);
        }
        //print("d");
        //print("Game started");
    }

    public void QuitPlaying()
    {
        if(_runner)
        {
            _runner.Shutdown(true, ShutdownReason.Ok);
        }
        
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

        if (Input.GetKey(KeyCode.W) || UIManager.ConstantForward)
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

    public SpawnPoint GetRespawnPoint(PlayerRef playerId)
    {
        // TODO
        float[] Dists = new float[SpawnPoints.Length];
        for(int i = 0; i < Dists.Length; i++)
        {
            Dists[i] = float.MaxValue;
            foreach(var player in _spawnedCharacters.Keys)
            {
                if(player == playerId)
                    continue;
                float d = Vector3.Distance(SpawnPoints[i].Position, _spawnedCharacters[player].transform.position);
                if (d < Dists[i])
                    Dists[i] = d;
            }
        }
        int inx = Array.IndexOf(Dists, Dists.Max());
        return SpawnPoints[inx];
    }

}