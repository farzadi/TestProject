using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion.Sample.DedicatedServer
{
    public class ClientManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public MatchmakerUI matchmakerUI;
        [SerializeField] private NetworkRunner _runnerPrefab;

        private string _sessionName = "defaultSession";
        private string _lobbyName = "defaultLobby";
        private NetworkRunner _instanceRunner;
        private CharacterInputHandler characterInputHandler;

        private enum State
        {
            SelectMode,
            StartClient,
            JoinLobby,
            LobbyJoined,
            Started,
        }

        private State _currentState;
        private List<SessionInfo> _currentSessionList;

        void Awake()
        {
            Application.targetFrameRate = 60;

            _currentState = State.JoinLobby;
        }



        public async void JoinLobby()
        {
            _instanceRunner = GetRunner("Client");

            _currentState = State.LobbyJoined;

            var result = await JoinLobby(_instanceRunner);

            if (result.Ok == false)
            {
                Debug.LogWarning(result.ShutdownReason);

                _currentState = State.SelectMode;
            }
            else
            {
                Debug.Log("JoinLobby Done");
            }
        }

        private NetworkRunner GetRunner(string name)
        {
            Debug.Log("StartGame 1 NetworkRunner");
            var runner = Instantiate(_runnerPrefab);
            runner.name = name;
            runner.ProvideInput = true;
            runner.AddCallbacks(this);

            return runner;
        }

        public Task<StartGameResult> StartSimulation(
            NetworkRunner runner,
            GameMode gameMode,
            string sessionName
        )
        {
            Debug.Log("StartGame 2 StartSimulation " + SceneManager.GetActiveScene().buildIndex 
                                                     + " " + matchmakerUI.GetMatchId() + " " +matchmakerUI.GetServerAddress()
                                                     + " " +matchmakerUI.GetServerPort());

            runner.PushHostMigrationSnapshot();
            return runner.StartGame(new StartGameArgs()
            {
                CustomPublicAddress =
                    NetAddress.CreateFromIpPort(matchmakerUI.GetServerAddress(), matchmakerUI.GetServerPort()),
                SessionName = sessionName,
                GameMode = gameMode,
                SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                Scene = SceneManager.GetActiveScene().buildIndex,
            });
        }

        public Task<StartGameResult> JoinLobby(NetworkRunner runner)
        {
            return runner.JoinSessionLobby(SessionLobby.ClientServer, matchmakerUI.GetMatchId());
        }

        // ------------ RUNNER CALLBACKS ------------------------------------------------------------------------------------

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _currentSessionList = null;
            _currentState = State.SelectMode;

            // Reload scene after shutdown

            if (Application.isPlaying)
            {
                SceneManager.LoadScene((byte)SceneDefs.MENU);
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.Log("OnDisconnectedFromServer");
            //runner.Shutdown();
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log("OnConnectFailed");
            // runner.Shutdown();
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Log.Debug($"Received: {sessionList.Count}");

            _currentSessionList = sessionList;

            foreach (var session in _currentSessionList.ToArray())
            {
                var props = "";
                foreach (var item in session.Properties)
                {
                    props += $"{item.Key}={item.Value.PropertyValue}, ";
                }

                Debug.Log($"Session: {session.Name} ({props})");
                StartSimulation(_instanceRunner, GameMode.Client, session.Name);

                _currentState = State.Started;
            }
        }

        // Other callbacks
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (characterInputHandler == null && NetworkPlayer.Local != null)
                characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();

            if (characterInputHandler != null)
                input.Set(characterInputHandler.GetNetworkInput());
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }
    }
}