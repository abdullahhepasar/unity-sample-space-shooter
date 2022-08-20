using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class AsteroidSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _smallAsteroid = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _mediumAsteroid = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _bigAsteroid = NetworkPrefabRef.Empty;

    [SerializeField] private NetworkPrefabRef _EnemyAlien = NetworkPrefabRef.Empty;

    [SerializeField] private float minSpawnDelay = 3f;
    [SerializeField] private float maxSpawnDelay = 9f;

    [SerializeField] private int minAsteroidSplinters = 3;
    [SerializeField] private int maxAsteroidSplinters = 6;
    
    [Networked] private TickTimer _spawnDelay { get; set; }
    [Networked] private TickTimer _spawnDelayforAlien { get; set; }
    
    private float _screenBoundaryX = 0.0f;
    private float _screenBoundaryY = 0.0f;

    private List<NetworkId> _asteroids = new List<NetworkId>();

    private List<NetworkId> _aliens = new List<NetworkId>();
    
    public void StartAsteroidSpawner()
    {
        if (Object.HasStateAuthority == false) return;
        
        SetSpawnDelay();

        SetSpawnDelayForAlien();
        
        _screenBoundaryX = Camera.main.orthographicSize * Camera.main.aspect;
        _screenBoundaryY = Camera.main.orthographicSize;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false) return;

        SpawnAlien();

        SpawnAsteroid();

        CheckOutOfBoundsAsteroids();
    }

    private void SpawnAlien()
    {
        if (_spawnDelayforAlien.Expired(Runner) == false) return;

        Vector2 direction = Random.insideUnitCircle;
        Vector3 position = Vector3.zero;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            position = new Vector3(Mathf.Sign(direction.x) * _screenBoundaryX, 0, direction.y * _screenBoundaryY);
        else
            position = new Vector3(direction.x * _screenBoundaryX, 0, Mathf.Sign(direction.y) * _screenBoundaryY);

        position -= position.normalized * 0.1f;
        var rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        var aliens = Runner.Spawn(_EnemyAlien, position, rotation, PlayerRef.None, onBeforeSpawned: SpinAlien);
        _aliens.Add(aliens.Id);

        SetSpawnDelayForAlien();
    }

    private void SpawnAsteroid()
    {
        if (_spawnDelay.Expired(Runner) == false) return;

        Vector2 direction = Random.insideUnitCircle;
        Vector3 position = Vector3.zero;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            position = new Vector3(Mathf.Sign(direction.x) * _screenBoundaryX, 0, direction.y * _screenBoundaryY);
        else 
            position = new Vector3(direction.x * _screenBoundaryX, 0, Mathf.Sign(direction.y) * _screenBoundaryY);

        position -= position.normalized * 0.1f;        
        var rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        var asteroid = Runner.Spawn(_bigAsteroid, position, rotation, PlayerRef.None, onBeforeSpawned: SpinBigAsteroid); 
        _asteroids.Add(asteroid.Id);
        
        SetSpawnDelay();
    }
    
    private void SetSpawnDelay()
    {
        var time = Random.Range(minSpawnDelay, maxSpawnDelay);
        _spawnDelay = TickTimer.CreateFromSeconds(Runner, time);
    }

    private void SetSpawnDelayForAlien()
    {
        var time = Random.Range(minSpawnDelay, maxSpawnDelay);
        _spawnDelayforAlien = TickTimer.CreateFromSeconds(Runner, time);
    }

    private void CheckOutOfBoundsAsteroids()
    {
        for (int i = 0; i < _asteroids.Count; i++)
        {
            if (Runner.TryFindObject(_asteroids[i], out var asteroid) == false)
            {
                _asteroids.RemoveAt(i);
                i--;
                continue;
            }

            if (IsWithinScreenBoundary(asteroid.transform.position)) continue;
            
            Runner.Despawn(asteroid);
            i--;
        }
    }
    
    private bool IsWithinScreenBoundary(Vector3 asteroidPosition)
    {
        return Mathf.Abs(asteroidPosition.x) < _screenBoundaryX && Mathf.Abs(asteroidPosition.z) < _screenBoundaryY;
    }

    private void SpinAlien(NetworkRunner runner, NetworkObject asteroidNetworkObject)
    {
        Vector3 force = -asteroidNetworkObject.transform.position.normalized * 1000.0f;
        Vector3 torque = Random.insideUnitSphere * Random.Range(500.0f, 1500.0f);

        var rb = asteroidNetworkObject.GetComponent<Rigidbody>();
        rb.AddForce(force);
        rb.AddTorque(torque);

        var alienBehaviour = asteroidNetworkObject.GetComponent<AlienBehaviour>();
    }

    private void SpinBigAsteroid(NetworkRunner runner, NetworkObject asteroidNetworkObject)
    {
        Vector3 force = -asteroidNetworkObject.transform.position.normalized * 1000.0f;
        Vector3 torque = Random.insideUnitSphere * Random.Range(500.0f, 1500.0f);

        var rb = asteroidNetworkObject.GetComponent<Rigidbody>();
        rb.AddForce(force);
        rb.AddTorque(torque);

        var asteroidBehaviour = asteroidNetworkObject.GetComponent<AsteroidBehaviour>();
        asteroidBehaviour.IsMedium = false;
        asteroidBehaviour.IsBig = true;
    }

    private void SpinMediumAsteroid(NetworkRunner runner, NetworkObject asteroidNetworkObject, Vector3 force, Vector3 torque)
    {
        var rb = asteroidNetworkObject.GetComponent<Rigidbody>();
        rb.AddForce(force);
        rb.AddTorque(torque);

        var asteroidBehaviour = asteroidNetworkObject.GetComponent<AsteroidBehaviour>();
        asteroidBehaviour.IsMedium = true;
        asteroidBehaviour.IsBig = false;
    }

    private void SpinSmallAsteroid(NetworkRunner runner, NetworkObject asteroidNetworkObject, Vector3 force, Vector3 torque)
    {
        var rb = asteroidNetworkObject.GetComponent<Rigidbody>();
        rb.AddForce(force);
        rb.AddTorque(torque);
        
        var asteroidBehaviour = asteroidNetworkObject.GetComponent<AsteroidBehaviour>();
        asteroidBehaviour.IsMedium = false;
        asteroidBehaviour.IsBig = false;
    }

    public void BreakUpBigAsteroid(Vector3 position, bool isSmall)
    {
        int splintersToSpawn = Random.Range(minAsteroidSplinters, maxAsteroidSplinters);

        for (int counter = 0; counter < splintersToSpawn; ++counter)
        {
            float addSpeed = isSmall ? 1500f : 1000f;

            Vector3 force = Quaternion.Euler(0, counter * 360.0f / splintersToSpawn, 0) * Vector3.forward * Random.Range(0.5f, 1.5f) * addSpeed;
            Vector3 torque = Random.insideUnitSphere * Random.Range(500.0f, 1500.0f);
            Quaternion rotation = Quaternion.Euler(0, Random.value * 180.0f, 0);

            if (isSmall)
            {
                Runner.Spawn(_smallAsteroid, position + force.normalized * 10.0f, rotation, PlayerRef.None,
                    (networkRunner, asteroidNetworkObject) => SpinSmallAsteroid(networkRunner, asteroidNetworkObject, force, torque));
            }
            else
            {
                Runner.Spawn(_mediumAsteroid, position + force.normalized * 12.0f, rotation, PlayerRef.None,
                    (networkRunner, asteroidNetworkObject) => SpinMediumAsteroid(networkRunner, asteroidNetworkObject, force, torque));
            }
        }
    }
}
