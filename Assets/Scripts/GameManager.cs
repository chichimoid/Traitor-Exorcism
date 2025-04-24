using System;
using System.Collections.Generic;
using Maze.GameCycle;
using PlayerScripts;
using PlayerScripts.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Voting.GameCycle;

public enum GamePhase
{
    None,
    Phase1,
    Phase2,
    Phase3,
    Voting,
    Aftermath,
}
    
public class GameManager : NetworkBehaviour
{
    [SerializeField] private LayerMask spectatorIgnoreLayers;
    [SerializeField] private int spectatorLayer;
    
    private readonly NetworkVariable<GamePhase> _phase = new(GamePhase.None);

    public GamePhase Phase
    {
        get => _phase.Value;
        private set => _phase.Value = value;
    }
    
    public readonly NetworkList<ulong> AlivePlayersIds = new(new List<ulong>());
    
    private readonly NetworkVariable<bool> _isHardcore = new();

    private ulong _monsterId;

    public bool IsHardcore
    {
        get => _isHardcore.Value;
        private set => _isHardcore.Value = value;
    }
        
    private Phase1Initializer _phase1Initializer;
    private Phase1Ender _phase1Ender;
    private Phase2Initializer _phase2Initializer;
    private Phase2Ender _phase2Ender;
    private Phase3Initializer _phase3Initializer;
    private Phase3Ender _phase3Ender;
    private SuddenEndPhaseInitializer _suddenEndPhaseInitializer;
    private VotingPhaseInitializer _votingPhaseInitializer;
    private VotingPhaseEnder _votingPhaseEnder;
    private AftermathPhaseInitializer _aftermathPhaseInitializer;

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerDisconnected;
    }
    
    public void StartGame(bool isHardcore)
    {
        if (!IsServer) return;
        
        IsHardcore = isHardcore;
        
        foreach (var id in NetworkManager.ConnectedClientsIds)
        {
            AlivePlayersIds.Add(id);
            NetworkPlayer.GetInstance(id).GetComponent<NetworkPlayer>().OnPlayerDied += () => KillPlayer(id);
        }
        
        StartMazeScene();
    }

    [Rpc(SendTo.Everyone)]
    private void SetLoadingScreenRpc(bool show)
    {
        if (show) PlayerUI.Instance.LoadingScreenUI.Show();
        else PlayerUI.Instance.LoadingScreenUI.Hide();
    }
    
    private void StartMazeScene()
    {
        SetLoadingScreenRpc(true);
        
        SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Maze);
        ChangePlayerStatesRpc(PlayerState.InMaze);
    }
        
    public void OnMazeSceneStarted(Phase1Initializer p1Init, Phase1Ender p1End, Phase2Initializer p2Init, Phase2Ender p2End, 
        Phase3Initializer p3Init, Phase3Ender p3End, SuddenEndPhaseInitializer suddenInit)
    {
        if (!IsServer) return;
        
        _phase1Initializer = p1Init;
        _phase1Ender = p1End;
        _phase2Initializer = p2Init;
        _phase2Ender = p2End;
        _phase3Initializer = p3Init;
        _phase3Ender = p3End;
        _suddenEndPhaseInitializer = suddenInit;

        _phase1Ender.OnPhase1Ended += StartPhase2;
        _phase2Ender.OnPhase2Ended += StartPhase3;
        _phase3Ender.OnPhase3Ended += StartVotingScene;
        
        SetLoadingScreenRpc(false);
            
        StartPhase1();
    }

    /// <summary>
    /// The peaceful phase for preparation.
    /// Roles are not distributed yet.
    /// </summary>
    private void StartPhase1()
    {
        if (!IsServer) return;
            
        if (Phase != GamePhase.None) throw new Exception($"Phase1 can only start after None. Active phase is {Phase}");
        Phase = GamePhase.Phase1;
            
        if (_phase1Initializer == null) throw new Exception("Phase initializer not found");
        Debug.Log("Starting phase 1...");
        _phase1Initializer.Init();
            
        _phase1Ender.Subscribe();
    }

    /// <summary>
    /// The suspicion phase.
    /// Roles are given, survivors are surviving, monster player is building up power.
    /// </summary>
    private void StartPhase2()
    {
        if (!IsServer) return;

        if (Phase != GamePhase.Phase1) throw new Exception($"Phase2 can only start after Phase1. Active phase is {Phase}");
        Phase = GamePhase.Phase2;
        
        if (_phase2Initializer == null) throw new Exception("Phase initializer not found");
        Debug.Log("Starting phase 2...");
        _phase2Initializer.Init(out _monsterId);
        
        CheckSuddenEnd();
            
        _phase2Ender.Subscribe(NetworkPlayer.GetInstance(_monsterId).GetComponent<MonsterBar>());
    }
        
    /// <summary>
    /// The reveal phase.
    /// Monster player turns to monster and tries to kill everyone.
    /// Survivors are trying to find all the levers and escape.
    /// </summary>
    private void StartPhase3()
    {
        if (!IsServer) return;

        if (Phase != GamePhase.Phase2) throw new Exception($"Phase3 can only start after Phase2. Active phase is {Phase}");
        Phase = GamePhase.Phase3;
            
        if (_phase3Initializer == null) throw new Exception("Phase initializer not found");
        Debug.Log("Starting phase 3...");
        _phase3Initializer.Init();
            
        _phase3Ender.Subscribe();
    }
        
    private void StartVotingScene()
    {
        SetLoadingScreenRpc(true);
        
        SceneLoader.Instance.LoadSceneGlobal(SceneLoader.Scene.Voting);
        ChangePlayerStatesRpc(PlayerState.InVoting);
    }
        
    public void OnVotingSceneStarted(VotingPhaseInitializer pVotingInit, VotingPhaseEnder pVotingEnd, AftermathPhaseInitializer pAftermathInit)
    {
        if (!IsServer) return;

        _votingPhaseInitializer = pVotingInit;
        _votingPhaseEnder = pVotingEnd;
        _aftermathPhaseInitializer = pAftermathInit;
        _votingPhaseEnder.OnVotingPhaseEnded += StartAftermathPhase;
        
        SetLoadingScreenRpc(false);
            
        StartVotingPhase();
    }
        
    /// <summary>
    /// The voting phase.
    /// All players vote for who they think is the monster.
    /// The chosen player gets kicked and players see if they were right.
    /// </summary>
    private void StartVotingPhase()
    {
        if (!IsServer) return;
        
        if (Phase != GamePhase.Phase3) throw new Exception($"Voting can only start after Phase3. Active phase is {Phase}");
        Phase = GamePhase.Voting;
        
        if (_votingPhaseInitializer == null) throw new Exception("Phase initializer not found");
        Debug.Log("Starting voting phase...");
        _votingPhaseInitializer.Init();
        
        _votingPhaseEnder.Subscribe();
    }
    
    /// <summary>
    /// The aftermath phase.
    /// Some game over scene etc.
    /// </summary>
    private void StartAftermathPhase(ulong votedPlayer)
    {
        if (!IsServer) return;
        
        if (Phase != GamePhase.Voting) throw new Exception($"Aftermath can only start after Voting. Active phase is {Phase}");
        
        Phase = GamePhase.Aftermath;
        
        _aftermathPhaseInitializer.Init(votedPlayer);
        
        Debug.Log("Starting aftermath...");
        
        // TBA: Some aftermath.
    }

    public static ulong GetMonsterId()
    {
        foreach (var (id, client) in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.PlayerObject.GetComponent<NetworkPlayer>().Role == PlayerRole.Monster)
            {
                return id;
            }
        }
        throw new Exception("No monster found.");
    }
    
    public static NetworkPlayer GetMonsterNetworkPlayer()
    {
        foreach (var (id, client) in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.PlayerObject.GetComponent<NetworkPlayer>().Role == PlayerRole.Monster)
            {
                return client.PlayerObject.GetComponent<NetworkPlayer>();
            }
        }
        throw new Exception("No monster found.");
    }
    
    [Rpc(SendTo.Everyone)]
    private void ChangePlayerStatesRpc(PlayerState state)
    {
        NetworkPlayer.GetLocalInstance().State = state;
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    // This is just not called frequently.
    private void KillPlayer(ulong playerId)
    {
        Debug.Log($"Killing player {playerId}...");

        var networkPlayer = NetworkPlayer.GetLocalInstance();
        networkPlayer.State = PlayerState.Dead;
        AlivePlayersIds.Remove(playerId);
        
        CheckSuddenEnd();
        
        KillPlayerRpc(playerId);
    }
    
    [Rpc(SendTo.Everyone)]
    private void KillPlayerRpc(ulong playerId)
    {
        var networkPlayer = NetworkPlayer.GetInstance(playerId);
        networkPlayer.MeshObject.SetActive(false);
        networkPlayer.gameObject.layer = spectatorLayer;
        networkPlayer.GetComponent<Collider>().excludeLayers = spectatorIgnoreLayers;

        if (playerId == NetworkManager.Singleton.LocalClientId)
        {
            networkPlayer.GetComponent<PlayerInfection>().enabled = false;
            networkPlayer.GetComponent<PlayerHealth>().enabled = false;
        
            PlayerLocker.Instance.LockActions();
            
            networkPlayer.GetComponent<PlayerInteract>().ForceDrop();
        
            PlayerUI.Instance.EmoteWheelUI.Hide();
            PlayerUI.Instance.EmoteWheelUI.enabled = false;
            PlayerUI.Instance.ReachableObjectDisplayUI.Hide();
            PlayerUI.Instance.ReachableObjectDisplayUI.enabled = false;
        }
    }

    private void HandlePlayerDisconnected(ulong playerId)
    {
        if (!IsServer) return;
        
        AlivePlayersIds.Remove(playerId);
        CheckSuddenEnd();
    }
    
    private void CheckSuddenEnd()
    {
        if (AlivePlayersIds.Count == 0)
        {
            VerySuddenEndGame();
        }
        else if (!AlivePlayersIds.Contains(_monsterId))
        {
            SuddenEndGame(true);
        }
        else if (AlivePlayersIds.Count == 1)
        {
            SuddenEndGame(false);
        }
    }

    private void VerySuddenEndGame()
    {
        _suddenEndPhaseInitializer.Init(true, ulong.MaxValue);
    }
    
    private void SuddenEndGame(bool survivorsWon)
    {
        _suddenEndPhaseInitializer.Init(survivorsWon, _monsterId);
    }
}