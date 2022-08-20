using UnityEngine;
using Fusion;

public class SpaceshipVisualController : SimulationBehaviour, ISpawned
{
    [SerializeField] private SpriteRenderer playerShip = null;
    [SerializeField] private ParticleSystem _destructionVFX = null;
    [SerializeField] private ParticleSystem _engineTrailVFX = null;
    
    public void Spawned()
    {
        var playerRef = Object.InputAuthority;
        playerShip.color = GetColor(playerRef);
    }

    public void TriggerSpawn()
    {
        playerShip.enabled = true;
        _engineTrailVFX.Play();
        _destructionVFX.Stop();
    }
    
    public void TriggerDestruction()
    {
        playerShip.enabled = false;
        _engineTrailVFX.Stop();
        _destructionVFX.Play();
    }

    public static Color GetColor(int player)
    {
        switch (player)
        {
            case 0: return Color.cyan;
            case 1: return Color.red;
            case 2: return Color.green;
            case 3: return Color.yellow;
            case 4: return Color.magenta;
            case 5: return Color.white;
            case 6: return Color.blue;
            case 7: return Color.grey;
        }

        return Color.black;
    }
}
