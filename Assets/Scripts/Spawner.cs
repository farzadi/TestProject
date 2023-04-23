using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{

    public NetworkObject playerPrefab;
    CharacterInputHandler characterInputHandler;
    private readonly Dictionary<PlayerRef, NetworkObject> _playerMap = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

        if (runner.IsServer && playerPrefab != null) {
            
            var character = runner.Spawn(playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, inputAuthority: player);

            _playerMap[player] = character;

            Log.Info($"Spawn for Player: {player}");
        }else Debug.Log("OnPlayerJoined");
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
        if (characterInputHandler == null && NetworkPlayer.Local != null)
            characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();

        if (characterInputHandler != null)
            input.Set(characterInputHandler.GetNetworkInput());
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {

        if (_playerMap.TryGetValue(player, out var character)) {
            // Despawn Player
            runner.Despawn(character);

            // Remove player from mapping
            _playerMap.Remove(player);

            Log.Info($"Despawn for Player: {player}");
        }

        if (_playerMap.Count == 0) {
            Log.Info("Last player left, shutdown...");

            // Shutdown Server after the last player leaves
            //runner.Shutdown();
        }
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown");
        // Quit application after the Server Shutdown
        // Application.Quit(0);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("OnDisconnectedFromServer");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("OnConnectRequest");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OnConnectFailed");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("OnSessionListUpdated " +  sessionList.Count);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
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

}