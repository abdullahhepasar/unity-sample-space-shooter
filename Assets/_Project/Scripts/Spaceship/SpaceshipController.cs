using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SpaceshipController : NetworkBehaviour
{
    [SerializeField] private float respawnDelay = 3.0f;
    [SerializeField] private float spaceshipDamageRadius = 3.8f;
    [SerializeField] private LayerMask CollisionLayers;
    
    private Rigidbody rigidbody = null;
    private PlayerDataNetworked playerDataNetworked = null;
    private SpaceshipVisualController visualController = null;

    private List<LagCompensatedHit> lagCompensatedHits = new List<LagCompensatedHit>();

    public bool AcceptInput => _isAlive && Object.IsValid;
    [Networked(OnChanged = nameof(OnAliveStateChanged))] private NetworkBool _isAlive { get; set; }
    [Networked] private TickTimer _respawnTimer { get; set; }

    private Collider colliderPlayer = null;

    [SerializeField] private GameObject Shield;
    [Networked] private TickTimer BonusInvulnerabilityTimer { get; set; }

    private bool BonusAutoShooterActive = false;
    [Networked] private TickTimer BonusAutoShooterTimer { get; set; }
    [SerializeField] private LayerMask BonusAutoShooterLayers;

    public override void Spawned()
    {
        rigidbody = GetComponent<Rigidbody>();
        playerDataNetworked = GetComponent<PlayerDataNetworked>();
        visualController = GetComponent<SpaceshipVisualController>();
        colliderPlayer = GetComponent<Collider>();
        Shield.SetActive(false);

        if (Object.HasStateAuthority == false) return;
        _isAlive = true;
    }

    private static void OnAliveStateChanged (Changed<SpaceshipController> spaceshipController)
    {
        spaceshipController.LoadOld();
        var wasAlive = spaceshipController.Behaviour._isAlive;
        
        spaceshipController.LoadNew();
        var isAlive = spaceshipController.Behaviour._isAlive;
        
        spaceshipController.Behaviour.ToggleVisuals(wasAlive, isAlive);
    }

    private void ToggleVisuals(bool wasAlive, bool isAlive)
    {
        if (wasAlive == false && isAlive == true)
        {
            visualController.TriggerSpawn();
        }
        else if (wasAlive == true && isAlive == false)
        {
            visualController.TriggerDestruction();
            colliderPlayer.enabled = false;
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        if (_respawnTimer.Expired(Runner))
        {
            _isAlive = true;
            _respawnTimer = default;
        }

        if (!_isAlive)
            return;

        if (HitCollision())
            ShipWasHit();

        if (BonusAutoShooterActive)
            BonusAutoShooter();
    }

    private bool HitCollision()
    {
        if (BonusInvulnerabilityTimer.ExpiredOrNotRunning(Runner) == false) return false;
        else Shield.SetActive(false);

        lagCompensatedHits.Clear();
        
        var count = Runner.LagCompensation.OverlapSphere(rigidbody.position, spaceshipDamageRadius, Object.InputAuthority, lagCompensatedHits,
            CollisionLayers.value);

        if (count <= 0) return false;

        if (lagCompensatedHits.Count > 0)
        {
            Sort(lagCompensatedHits, count);

            if (lagCompensatedHits[0].GameObject.GetComponent<AsteroidBehaviour>())
                lagCompensatedHits[0].GameObject.GetComponent<AsteroidBehaviour>().HitAsteroid(PlayerRef.None);

            if (lagCompensatedHits[0].GameObject.GetComponent<AlienBehaviour>())
                lagCompensatedHits[0].GameObject.GetComponent<AlienBehaviour>().HitAlien(PlayerRef.None);

            //Set Bonus
            if (lagCompensatedHits[0].GameObject.GetComponent<BonusBehaviour>())
            {
                lagCompensatedHits[0].GameObject.GetComponent<BonusBehaviour>().HitBonus();
                SetBonus(lagCompensatedHits[0].GameObject.GetComponent<BonusBehaviour>().bonusType);

                return false;
            }
        }

        return true;
    }

    public void ShipWasHit()
    {
        if (BonusInvulnerabilityTimer.ExpiredOrNotRunning(Runner) == false) return;
        if (_respawnTimer.ExpiredOrNotRunning(Runner) == false) return;

        _isAlive = false;
        
        ResetShip();

        if (Object.HasStateAuthority == false) return;
        
        if (playerDataNetworked.Lives > 1) {
            _respawnTimer = TickTimer.CreateFromSeconds(Runner, respawnDelay);
        }
        else {
            _respawnTimer = default;
        }
        
        playerDataNetworked.SubtractLife();

        FindObjectOfType<GameStateController>().CheckIfGameHasEnded();
    }
    
    private void ResetShip()
    {
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
    
    private static void Sort(List<LagCompensatedHit> hits, int maxHits)
    {
        while (true)
        {
            bool swap = false;

            for (int i = 0; i < maxHits; ++i)
            {
                for (int j = i + 1; j < maxHits; ++j)
                {
                    if (hits[j].Distance >= hits[i].Distance)
                        continue;

                    (hits[i], hits[j]) = (hits[j], hits[i]);
                    swap = true;
                }
            }

            if (swap == false)
                return;
        }
    }

    #region SET BONUS

    private void SetBonus(BonusBehaviour.BonusType bonusType)
    {
        switch (bonusType)
        {
            case BonusBehaviour.BonusType.Invulnerability:

                Shield.SetActive(true);
                BonusInvulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, 10f);

                break;
            case BonusBehaviour.BonusType.AutoShooter:

                BonusAutoShooter();

                break;
            default:
                break;
        }
    }

    private void BonusAutoShooter()
    {
        BonusAutoShooterActive = true;

        if (BonusAutoShooterTimer.ExpiredOrNotRunning(Runner) == false) return;

        BonusAutoShooterTimer = TickTimer.CreateFromSeconds(Runner, 5f);

        Collider[] colliderPlayers = Physics.OverlapSphere(rigidbody.position, 100f, BonusAutoShooterLayers.value);

        if (colliderPlayers.Length <= 0) return;

        Runner.Spawn(GetComponent<SpaceshipFireController>()._bullet, rigidbody.position, Quaternion.LookRotation(colliderPlayers[0].transform.position - rigidbody.position), Object.InputAuthority);
    }

    #endregion
}
