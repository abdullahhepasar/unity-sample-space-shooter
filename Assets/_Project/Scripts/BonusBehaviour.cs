using System;
using Fusion;
using UnityEngine;

using Random = UnityEngine.Random;

public class BonusBehaviour : NetworkBehaviour
{
    public enum BonusType { Invulnerability, AutoShooter }
    [HideInInspector] public BonusType bonusType;

    [Networked] public int bonusIndex { get; set; }

    [SerializeField] private float _maxLifetime = 7.0f;

    [Networked] private TickTimer _currentLifetime { get; set; }

    public override void Spawned()
    {
        _currentLifetime = TickTimer.CreateFromSeconds(Runner, _maxLifetime);

        bonusType = (BonusType)Enum.ToObject(typeof(BonusType), Random.Range(0, Enum.GetValues(typeof(BonusType)).Length));

        if (Object.HasInputAuthority)
            RPC_Configure(((int)bonusType).ToString());
    }

    public override void FixedUpdateNetwork()
    {
        CheckLifetime();
    }

    private void CheckLifetime()
    {
        if (_currentLifetime.Expired(Runner) == false) return;

        HitBonus();
    }

    public void HitBonus()
    {
        Runner.Despawn(Object);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Configure(string index)
    {
        bonusIndex = int.Parse(index);

        bonusType = (BonusType)bonusIndex;
    }
}
