using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sample.DedicatedServer;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;

    NetworkRunner networkRunner;


    async void Start()
    {
        if (CommandLineUtils.IsHeadlessMode() == false)
        {
            SceneManager.LoadScene((int)SceneDefs.MENU, LoadSceneMode.Single);
            return;
        }

        var config = DedicatedServerConfig.Resolve();
        Debug.Log(config);

        var runner = networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network runner";

        // Start the Server
        var result = await StartSimulation(
            runner,
            config.SessionName,
            config.SessionProperties,
            config.Port,
            config.Lobby,
            config.Region,
            config.PublicIP,
            config.PublicPort
        );

        // Check if all went fine
        if (result.Ok)
        {
            Log.Debug($"Runner Start DONE");
        }
        else
        {
            // Quit the application if startup fails
            Log.Debug($"Error while starting Server: {result.ShutdownReason}");

            // it can be used any error code that can be read by an external application
            // using 0 means all went fine
            Application.Quit(1);
        }
        // var clientTask = InitializeNetworkRunner(networkRunner, gameMode: GameMode.AutoHostOrClient, NetAddress.Any(),
        //     SceneManager.GetActiveScene().buildIndex, null);

        Debug.Log($"Server NetworkRunner started.");
    }


    private Task<StartGameResult> StartSimulation(
        NetworkRunner runner,
        string SessionName,
        Dictionary<string, SessionProperty> customProps,
        ushort port,
        string customLobby,
        string customRegion,
        string customPublicIP = null,
        ushort customPublicPort = 0
    )
    {
        // Build Custom Photon Config
        var photonSettings = PhotonAppSettings.Instance.AppSettings.GetCopy();

        if (string.IsNullOrEmpty(customRegion) == false)
        {
            photonSettings.FixedRegion = customRegion.ToLower();
        }

        // Build Custom External Addr
        NetAddress? externalAddr = null;

        if (string.IsNullOrEmpty(customPublicIP) == false && customPublicPort > 0)
        {
            if (IPAddress.TryParse(customPublicIP, out var _))
            {
                externalAddr = NetAddress.CreateFromIpPort(customPublicIP, customPublicPort);
            }
            else
            {
                Log.Warn("Unable to parse 'Custom Public IP'");
            }
        }


        //

        // Start Runner
        return runner.StartGame(new StartGameArgs()
        {
            SessionName = SessionName,
            GameMode = GameMode.Server,
            SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
            Scene = (int)SceneDefs.GAME,
            SessionProperties = customProps,
            Address = NetAddress.Any(port),
            CustomPublicAddress = externalAddr,
            CustomLobbyName = customLobby,
            CustomPhotonAppSettings = photonSettings,
        });
    }


    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address,
        SceneRef scene, Action<NetworkRunner> initialized)
    {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneManager == null)
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();


        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = "test",
            Initialized = initialized,
            SceneManager = sceneManager
        });
    }
}