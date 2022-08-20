using UnityEngine;
using Fusion;

public class PlayerDataNetworked : NetworkBehaviour
{ 
    private const int STARTING_LIVES = 3;
    
    private PlayerOverviewPanel PanelOverview = null;

    [HideInInspector][Networked(OnChanged = nameof(OnNickNameChanged))] public NetworkString<_16> NickName { get; private set; }
    [HideInInspector][Networked(OnChanged = nameof(OnLivesChanged))] public int Lives { get; private set; }
    [HideInInspector][Networked(OnChanged = nameof(OnScoreChanged))] public int Score { get; private set; }
    
    public override void Spawned()
    {
        // Client
        if (Object.HasInputAuthority)
        {
            var nickName = FindObjectOfType<PlayerData>().GetNickName();
            RpcSetNickName(nickName);
        }
        
        // Host
        if (Object.HasStateAuthority)
        {
            Lives = STARTING_LIVES;
            Score = 0;
        }
        
        PanelOverview = FindObjectOfType<PlayerOverviewPanel>();
        PanelOverview.AddEntry(Object.InputAuthority, this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        PanelOverview.RemoveEntry(Object.InputAuthority);
    }

    // Update Score
    public void AddToScore(int points)
    {
        Score += points;
    }

    // Decrease the current Lives by 1
    public void SubtractLife()
    {
        Lives--;
    }
    
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    private void RpcSetNickName(string nickName)
    {
        if (string.IsNullOrEmpty(nickName)) return;
        NickName =nickName;
    }

    // Updates the player's nickname.
    public static void OnNickNameChanged(Changed<PlayerDataNetworked> playerInfo)
    {
        playerInfo.Behaviour.PanelOverview.UpdateNickName(playerInfo.Behaviour.Object.InputAuthority, playerInfo.Behaviour.NickName.ToString());
    }

    // Updates Player current Score.
    public static void OnScoreChanged(Changed<PlayerDataNetworked> playerInfo)
    {
        playerInfo.Behaviour.PanelOverview.UpdateScore(playerInfo.Behaviour.Object.InputAuthority, playerInfo.Behaviour.Score);
    }

    // Updates the current amount of Lives.
    public static void OnLivesChanged(Changed<PlayerDataNetworked> playerInfo)
    {
        playerInfo.Behaviour.PanelOverview.UpdateLives(playerInfo.Behaviour.Object.InputAuthority, playerInfo.Behaviour.Lives);
    }
}
