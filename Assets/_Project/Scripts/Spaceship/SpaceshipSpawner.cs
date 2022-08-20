using UnityEngine;
using Fusion;

public class SpaceshipSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft, ISpawned
{
    [SerializeField] private NetworkPrefabRef _spaceshipNetworkPrefab = NetworkPrefabRef.Empty;
    
    private bool _gameIsReady = false;
    private GameStateController _gameStateController = null;

    private SpawnPoint[] _spawnPoints = null;
    
    public void Spawned()
    {
        if (Object.HasStateAuthority == false) return;

        _spawnPoints = FindObjectsOfType<SpawnPoint>();
    }
    
    public void StartSpaceshipSpawner(GameStateController gameStateController)
    {
        _gameIsReady = true;
        _gameStateController = gameStateController;
        foreach (var player in Runner.ActivePlayers)
        {
            SpawnSpaceship(player);
        }
    }
    
    public void PlayerJoined(PlayerRef player)
    {
        if (_gameIsReady == false) return;

        SpawnSpaceship(player);   
    }
    
    private void SpawnSpaceship(PlayerRef player)
    {
        int index = player % _spawnPoints.Length;
        var spawnPosition = _spawnPoints[index].transform.position; 
        
        var playerObject = Runner.Spawn(_spaceshipNetworkPrefab, spawnPosition, Quaternion.identity, player);
        Runner.SetPlayerObject(player, playerObject);
        
        _gameStateController.TrackNewPlayer(playerObject.GetComponent<PlayerDataNetworked>().Id);
    }
    
    public void PlayerLeft(PlayerRef player)
    {
        DespawnSpaceship(player);
    }
    
    private void DespawnSpaceship(PlayerRef player)
    {
        if (Runner.TryGetPlayerObject(player, out var spaceshipNetworkObject))
            Runner.Despawn(spaceshipNetworkObject);

        Runner.SetPlayerObject(player, null);
    }
}
