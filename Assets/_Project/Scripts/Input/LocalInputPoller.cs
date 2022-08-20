using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class LocalInputPoller : MonoBehaviour, INetworkRunnerCallbacks
{
    private const string AXIS_HORIZONTAL = "Horizontal";
    private const string AXIS_VERTICAL = "Vertical";
    private const string BUTTON_FIRE1 = "Fire1";
    private const string BUTTON_JUMP = "Jump";
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        SpaceshipInput localInput = new SpaceshipInput();
        
        localInput.HorizontalInput = Input.GetAxis(AXIS_HORIZONTAL);
        localInput.VerticalInput = Input.GetAxis(AXIS_VERTICAL);
        localInput.Buttons.Set(SpaceshipButtons.Fire, Input.GetButton(BUTTON_FIRE1));
        localInput.Buttons.Set(SpaceshipButtons.Accelerate, Input.GetButton(BUTTON_JUMP));

        input.Set(localInput);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
}
