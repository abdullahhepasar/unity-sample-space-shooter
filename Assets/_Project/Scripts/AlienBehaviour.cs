using Fusion;
using UnityEngine;

public class AlienBehaviour : NetworkBehaviour
{
    [SerializeField] private int _points = 5;

    [SerializeField] private NetworkPrefabRef _bullet = NetworkPrefabRef.Empty;
    [SerializeField] private NetworkPrefabRef _bonus = NetworkPrefabRef.Empty;

    private Rigidbody _rigidbody = null;

    [SerializeField] private float minSpawnDelay = 2.0f;
    [SerializeField] private float maxSpawnDelay = 5.0f;

    [SerializeField] private float DamageRadius = 200f;

    [Networked] private TickTimer _shootCooldown { get; set; }

    [SerializeField] private LayerMask targetLayer;

    public override void Spawned()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void FixedUpdateNetwork()
    {
        SpawnBullet();
    }

    public void HitAlien(PlayerRef player)
    {
        if (Object == null) return;
        if (Object.HasStateAuthority == false) return;

        var rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        Runner.Spawn(_bonus, _rigidbody.position, rotation, Object.InputAuthority);

        if (Runner.TryGetPlayerObject(player, out var playerNetworkObject))
            playerNetworkObject.GetComponent<PlayerDataNetworked>().AddToScore(_points);

        Runner.Despawn(Object);
    }

    private void SpawnBullet()
    {
        if (_shootCooldown.ExpiredOrNotRunning(Runner) == false) return;

        Collider[] colliderPlayers = Physics.OverlapSphere(transform.position, DamageRadius, targetLayer.value);
        Physics.SyncTransforms();
        if (colliderPlayers.Length <= 0) return;

        Runner.Spawn(_bullet, _rigidbody.position, Quaternion.LookRotation(colliderPlayers[0].transform.position - _rigidbody.position), Object.InputAuthority);

        _shootCooldown = TickTimer.CreateFromSeconds(Runner, Random.Range(minSpawnDelay, maxSpawnDelay));
    }
}
