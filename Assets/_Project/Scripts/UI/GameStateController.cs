using System;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class GameStateController : NetworkBehaviour
{
    enum GameState
    {
        Starting,
        Running,
        Ending
    }

    [SerializeField] private float _startDelay = 4.0f;
    [SerializeField] private float _endDelay = 4.0f;
    [SerializeField] private float _gameSessionLength = 180.0f;

    [SerializeField] private TextMeshProUGUI _startEndDisplay = null;
    [SerializeField] private TextMeshProUGUI _ingameTimerDisplay = null;

    [Networked] private TickTimer _timer { get; set; }
    [Networked] private GameState _gameState { get; set; }

    [Networked] private NetworkBehaviourId _winner { get; set; }

    private List<NetworkBehaviourId> _playerDataNetworkedIds = new List<NetworkBehaviourId>();

    public override void Spawned()
    {
        _startEndDisplay.gameObject.SetActive(true);
        _ingameTimerDisplay.gameObject.SetActive(false);

        if (_gameState != GameState.Starting)
        {
            foreach (var player in Runner.ActivePlayers)
            {
                if (Runner.TryGetPlayerObject(player, out var playerObject) == false) continue;
                TrackNewPlayer(playerObject.GetComponent<PlayerDataNetworked>().Id);
            }
        }

        if (Object.HasStateAuthority == false) return;

        // Initialize the game state on the host
        _gameState = GameState.Starting;
        _timer = TickTimer.CreateFromSeconds(Runner, _startDelay);
    }

    public override void FixedUpdateNetwork()
    {
        switch (_gameState)
        {
            case GameState.Starting:
                UpdateStartingDisplay();
                break;
            case GameState.Running:
                UpdateRunningDisplay();
                if (_timer.ExpiredOrNotRunning(Runner))
                    GameHasEnded();
                break;
            case GameState.Ending:
                UpdateEndingDisplay();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateStartingDisplay()
    {
        _startEndDisplay.text = $"Game Starts In {Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0)}";
        
        // --- Host
        if (Object.HasStateAuthority == false) return;
        if (_timer.ExpiredOrNotRunning(Runner) == false) return;
        
        FindObjectOfType<SpaceshipSpawner>().StartSpaceshipSpawner(this);
        FindObjectOfType<AsteroidSpawner>().StartAsteroidSpawner();

        _gameState = GameState.Running;
        _timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);
    }
    
    private void UpdateRunningDisplay()
    {
        _startEndDisplay.gameObject.SetActive(false);
        _ingameTimerDisplay.gameObject.SetActive(true);
        _ingameTimerDisplay.text = $"{Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0).ToString("000")} seconds left";
    }

    private void UpdateEndingDisplay()
    {        
        if (Runner.TryFindBehaviour(_winner, out PlayerDataNetworked playerData) == false) return;
        
        _startEndDisplay.gameObject.SetActive(true);
        _ingameTimerDisplay.gameObject.SetActive(false);
        _startEndDisplay.text = $"{playerData.NickName} won with {playerData.Score} points. Disconnecting in {Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0)}";
        _startEndDisplay.color = SpaceshipVisualController.GetColor(playerData.Object.InputAuthority);

        if (_timer.ExpiredOrNotRunning(Runner) == false) return;

        if (Object.HasStateAuthority) {
            Runner.Shutdown();
        }
    }

    public void CheckIfGameHasEnded()
    {
        if (Object.HasStateAuthority == false) return;

        int playersAlive = 0;
        
        for(int i = 0; i <_playerDataNetworkedIds.Count; i++)
        {
            if (Runner.TryFindBehaviour(_playerDataNetworkedIds[i], out PlayerDataNetworked playerDataNetworkedComponent) == false)
            {
                _playerDataNetworkedIds.RemoveAt(i);
                i--;
                continue;
            }

            if (playerDataNetworkedComponent.Lives > 0) playersAlive++;
        }
        
        if (playersAlive > 1) return;

        foreach (var playerDataNetworkedId in _playerDataNetworkedIds)
        {
            if (Runner.TryFindBehaviour(playerDataNetworkedId, out PlayerDataNetworked playerDataNetworkedComponent) ==
                false) continue;

            if (playerDataNetworkedComponent.Lives > 0 == false) continue;

            _winner = playerDataNetworkedId;
        }
        
        GameHasEnded();
    }

    private void GameHasEnded()
    {
        _timer = TickTimer.CreateFromSeconds(Runner, _endDelay);
        _gameState = GameState.Ending;
    }

    public void TrackNewPlayer(NetworkBehaviourId playerDataNetworkedId)
    {
        _playerDataNetworkedIds.Add(playerDataNetworkedId);
    }
}
