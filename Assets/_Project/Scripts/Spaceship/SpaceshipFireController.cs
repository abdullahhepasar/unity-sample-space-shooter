using UnityEngine;
using Fusion;

public class SpaceshipFireController : NetworkBehaviour
{
    [SerializeField] private float _delayBetweenShots = 0.2f;
    [SerializeField] public NetworkPrefabRef _bullet = NetworkPrefabRef.Empty;
    
    private Rigidbody _rigidbody = null;
    private SpaceshipController _spaceshipController = null;
    
    [Networked] private NetworkButtons _buttonsPrevious { get; set; }
    [Networked] private TickTimer _shootCooldown { get; set; }

    public override void Spawned()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _spaceshipController = GetComponent<SpaceshipController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (_spaceshipController.AcceptInput == false) return;

        if (GetInput<SpaceshipInput>(out var input) == false) return;
        
        Fire(input);
    }

    private void Fire(SpaceshipInput input)
    {
        var pressed = input.Buttons.GetPressed(_buttonsPrevious);

        if (pressed.WasPressed(_buttonsPrevious, SpaceshipButtons.Fire))
            SpawnBullet();
        
        _buttonsPrevious = input.Buttons;
    }

    private void SpawnBullet()
    {
        if (_shootCooldown.ExpiredOrNotRunning(Runner) == false) return;
        
        Runner.Spawn(_bullet, _rigidbody.position, _rigidbody.rotation, Object.InputAuthority);
        
        _shootCooldown = TickTimer.CreateFromSeconds(Runner, _delayBetweenShots);
    }
}
