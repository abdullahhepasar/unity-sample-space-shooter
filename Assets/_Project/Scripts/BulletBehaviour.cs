using Fusion;
using UnityEngine;

public class BulletBehaviour : NetworkBehaviour
{
    [SerializeField] private float _maxLifetime = 3.0f;
    [SerializeField] private float _speed = 200.0f;
    [SerializeField] private LayerMask targetLayer;
    
    [Networked] private Vector3 _direction { get; set; }
    
    [Networked] private TickTimer _currentLifetime { get; set; }
    
    public override void Spawned()
    {
        if (Object.HasStateAuthority == false) return;

        _direction = transform.forward;
        _currentLifetime = TickTimer.CreateFromSeconds(Runner, _maxLifetime);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasHit() == false)
            transform.Translate(_direction * _speed * Runner.DeltaTime, Space.World);
        else {
            Runner.Despawn(Object);
            return;
        }
        
        CheckLifetime();
    }

    private void CheckLifetime()
    {
        if (_currentLifetime.Expired(Runner) == false) return;

        Runner.Despawn(Object);
    }

    private bool HasHit()
    {
        var hitAsteroid = Runner.LagCompensation.Raycast(transform.position, _direction, _speed * Runner.DeltaTime,
            Object.InputAuthority, out var hit, targetLayer);

        if (hitAsteroid == false) return false;

        if (hit.GameObject.GetComponent<AsteroidBehaviour>())
            hit.GameObject.GetComponent<AsteroidBehaviour>().HitAsteroid(Object.InputAuthority);

        if (hit.GameObject.GetComponent<AlienBehaviour>())
            hit.GameObject.GetComponent<AlienBehaviour>().HitAlien(Object.InputAuthority);

        if (hit.GameObject.GetComponent<SpaceshipController>())
            hit.GameObject.GetComponent<SpaceshipController>().ShipWasHit();

        return true;
    }
}
