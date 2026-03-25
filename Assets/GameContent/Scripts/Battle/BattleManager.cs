using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private static BattleManager _instance;
    public static BattleManager GetInstanceST() => _instance;

    // ── Static pending encounter ──────────────────────────────────────
    private static PokemonInstance _pendingEnemy;
    private static TrainerData     _pendingTrainer;
    private static bool            _pendingIsWild;

    public static void QueueWildEncounterST(PokemonInstance enemy)
    {
        _pendingEnemy   = enemy;
        _pendingIsWild  = true;
        _pendingTrainer = null;
        GameManager.GetInstanceST()?.SceneTransitionManager
            .LoadSceneAdditiveST("Battle", null);
    }

    public static void QueueTrainerBattleST(TrainerData trainer)
    {
        _pendingTrainer = trainer;
        _pendingIsWild  = false;
        _pendingEnemy   = trainer?.party != null && trainer.party.Length > 0
            ? trainer.party[0] : null;
        GameManager.GetInstanceST()?.SceneTransitionManager
            .LoadSceneAdditiveST("Battle", null);
    }

    // ── Debug ─────────────────────────────────────────────────────────
    [Header("Debug")]
    [SerializeField] public bool debugMode = false;

    // ── Instance state ────────────────────────────────────────────────
    private BattleStateMachine  _stateMachine;
    private PokemonInstance     _playerPokemon;
    private PokemonInstance     _enemyPokemon;
    private TrainerData         _currentTrainer;
    private bool                _isWildBattle;
    private BattleUI            _battleUI;
    private BattleVisuals       _battleVisuals;
    private BattleResultsScreen _resultsScreen;

    public PokemonInstance PlayerPokemon => _playerPokemon;
    public PokemonInstance EnemyPokemon  => _enemyPokemon;

    private void Awake()
    {
        _instance = this;

        _enemyPokemon   = _pendingEnemy;
        _currentTrainer = _pendingTrainer;
        _isWildBattle   = _pendingIsWild;
        _pendingEnemy   = null;
        _pendingTrainer = null;

        _stateMachine = new BattleStateMachine();
        _stateMachine.OnStateChanged += OnBattleStateChanged;

        _battleUI      = FindAnyObjectByType<BattleUI>();
        _battleVisuals = FindAnyObjectByType<BattleVisuals>();
        _resultsScreen = FindAnyObjectByType<BattleResultsScreen>(FindObjectsInactive.Include);

        if (_enemyPokemon != null)
        {
            _playerPokemon = ResolvePlayerPokemonST();
            GameManager.GetInstanceST()?.ChangeStateST(GameState.Battle);
            _stateMachine.TransitionToST(BattleState.Intro);
        }
        // else: BattleTestBootstrap.Start() will call InitBattleST()
    }

    private PokemonInstance ResolvePlayerPokemonST()
    {
        var slm = SaveLoadManager.GetInstanceST();
        if (slm?.ActiveSaveData?.player?.party != null)
        {
            foreach (var save in slm.ActiveSaveData.player.party)
            {
                var inst = PokemonHydrator.HydrateST(save);
                if (inst != null && inst.IsAlive) return inst;
            }
        }
        var db = PokemonDatabase.GetInstanceST();
        PokemonData fallback = db?.GetByNumberST(1);
        if (fallback != null) return PokemonInstance.CreateST(fallback, 5);
        Debug.LogWarning("BattleManager: no player pokemon resolved.");
        return null;
    }

    // ── Test / direct-call entry points ──────────────────────────────
    public void InitBattleST(PokemonInstance player, PokemonInstance enemy,
                              bool isWild = true, TrainerData trainer = null)
    {
        _playerPokemon  = player;
        _enemyPokemon   = enemy;
        _isWildBattle   = isWild;
        _currentTrainer = trainer;   // null is fine — visuals use a placeholder silhouette
        _stateMachine.TransitionToST(BattleState.Intro);
    }

    public void StartWildEncounterST(PokemonInstance enemy)  => QueueWildEncounterST(enemy);
    public void StartTrainerBattleST(TrainerData trainer)    => QueueTrainerBattleST(trainer);

    // ── State machine ─────────────────────────────────────────────────
    private void OnBattleStateChanged(BattleState state)
    {
        switch (state)
        {
            case BattleState.Intro:
                StartCoroutine(IntroRoutine());
                break;
            case BattleState.PlayerChoose:
                _battleUI?.ShowActionMenuST(true);
                break;
            case BattleState.CheckFainted:
                StartCoroutine(CheckFaintedRoutine());
                break;
        }
    }

    // ── Intro ─────────────────────────────────────────────────────────
    private IEnumerator IntroRoutine()
    {
        // Initialise visuals (creates sprites) then run the intro sequence
        _battleVisuals?.InitST(_playerPokemon, _enemyPokemon, _isWildBattle, _currentTrainer);
        _battleUI?.SetupBattleST(_enemyPokemon, _playerPokemon);

        if (_battleVisuals != null)
        {
            yield return _battleVisuals.PlayIntroSequenceST();
        }
        else
        {
            // Fallback: text only
            _battleUI?.AppendLogST(_isWildBattle
                ? $"A wild {_enemyPokemon?.data?.speciesName} appeared!"
                : $"{_currentTrainer?.trainerName} wants to battle!");
            yield return new WaitForSeconds(1.5f);
        }

        _stateMachine.TransitionToST(BattleState.PlayerChoose);
    }

    // ── Player input ──────────────────────────────────────────────────
    public void OnPlayerSelectedMoveST(MoveData move)
    {
        if (_stateMachine.CurrentState != BattleState.PlayerChoose) return;
        _stateMachine.TransitionToST(BattleState.PlayerAction);
        StartCoroutine(TurnRoutine(move));
    }

    // ── Debug shortcuts ───────────────────────────────────────────────
    public void DebugWinST()
    {
        if (_enemyPokemon == null) return;
        _enemyPokemon.currentHP = 0;
        _battleUI?.UpdateHPST(true, 0, PokemonInstance.GetMaxHPST(_enemyPokemon));
        _stateMachine.TransitionToST(BattleState.CheckFainted);
    }

    public void DebugLoseST()
    {
        if (_playerPokemon == null) return;
        _playerPokemon.currentHP = 0;
        _battleUI?.UpdateHPST(false, 0, PokemonInstance.GetMaxHPST(_playerPokemon));
        _stateMachine.TransitionToST(BattleState.CheckFainted);
    }

    public void OnRunSelectedST()
    {
        if (_stateMachine.CurrentState != BattleState.PlayerChoose) return;
        if (_isWildBattle)
        {
            _battleUI?.AppendLogST("Got away safely!");
            EndBattleST(BattleResult.Fled);
        }
        else
        {
            _battleUI?.AppendLogST("Can't flee from a trainer battle!");
            _stateMachine.TransitionToST(BattleState.PlayerChoose);
        }
    }

    // ── Turn resolution (speed order) ────────────────────────────────
    private IEnumerator TurnRoutine(MoveData playerMove)
    {
        MoveData enemyMove = EnemyAI.ChooseMoveST(_enemyPokemon, _playerPokemon);

        int playerSpd = PokemonInstance.GetStatST(_playerPokemon, PokemonInstance.StatSpd);
        int enemySpd  = PokemonInstance.GetStatST(_enemyPokemon,  PokemonInstance.StatSpd);
        bool playerFirst = playerSpd > enemySpd ||
                           (playerSpd == enemySpd && Random.value < 0.5f);

        if (playerFirst)
        {
            yield return ExecuteMoveST(_playerPokemon, _enemyPokemon, playerMove);
            if (!_enemyPokemon.IsAlive) { _stateMachine.TransitionToST(BattleState.CheckFainted); yield break; }
            if (enemyMove != null)
                yield return ExecuteMoveST(_enemyPokemon, _playerPokemon, enemyMove);
        }
        else
        {
            if (enemyMove != null)
                yield return ExecuteMoveST(_enemyPokemon, _playerPokemon, enemyMove);
            if (!_playerPokemon.IsAlive) { _stateMachine.TransitionToST(BattleState.CheckFainted); yield break; }
            yield return ExecuteMoveST(_playerPokemon, _enemyPokemon, playerMove);
        }

        _stateMachine.TransitionToST(BattleState.StatusTick);
        yield return StatusTickRoutine();
    }

    // ── Move execution ────────────────────────────────────────────────
    private IEnumerator ExecuteMoveST(PokemonInstance attacker, PokemonInstance defender, MoveData move)
    {
        // Status blocks
        if (attacker.status == StatusCondition.Paralyzed && Random.value < 0.25f)
        {
            _battleUI?.AppendLogST($"{attacker.nickname} is fully paralyzed!");
            yield break;
        }
        if (attacker.status == StatusCondition.Asleep)
        {
            attacker.sleepCounter--;
            if (attacker.sleepCounter <= 0)
            {
                attacker.status = StatusCondition.None;
                _battleUI?.AppendLogST($"{attacker.nickname} woke up!");
            }
            else
            {
                _battleUI?.AppendLogST($"{attacker.nickname} is fast asleep!");
                yield break;
            }
        }
        if (attacker.status == StatusCondition.Frozen)
        {
            if (Random.value < 0.1f)
            {
                attacker.status = StatusCondition.None;
                _battleUI?.AppendLogST($"{attacker.nickname} thawed out!");
            }
            else
            {
                _battleUI?.AppendLogST($"{attacker.nickname} is frozen solid!");
                yield break;
            }
        }

        // PP deduction
        int ppSlot = System.Array.IndexOf(attacker.moves, move);
        if (ppSlot >= 0 && attacker.currentPP[ppSlot] > 0)
            attacker.currentPP[ppSlot]--;

        // ── Move name text, then skill animation ──────────────────────
        _battleUI?.AppendLogST($"{attacker.nickname} used {move.moveName}!");

        bool hitEnemy = defender == _enemyPokemon;
        if (_battleVisuals != null)
            yield return _battleVisuals.PlaySkillAnimationST(hitEnemy, move);
        else
            yield return new WaitForSeconds(0.5f);

        // Accuracy
        if (Random.Range(0, 100) >= move.accuracy)
        {
            _battleUI?.AppendLogST("But it missed!");
            yield break;
        }

        // Damage
        bool isCrit = BattleFormulas.IsCritST(attacker, move);
        int  damage = BattleFormulas.CalculateDamageST(attacker, defender, move, isCrit);
        if (damage > 0)
        {
            if (isCrit) _battleUI?.AppendLogST("A critical hit!");
            defender.currentHP = Mathf.Max(0, defender.currentHP - damage);
            _battleUI?.UpdateHPST(hitEnemy, defender.currentHP,
                PokemonInstance.GetMaxHPST(defender));
        }

        // Status application
        if (move.statusEffect != StatusCondition.None
            && defender.status == StatusCondition.None
            && Random.value < move.statusChance)
        {
            defender.status = move.statusEffect;
            if (move.statusEffect == StatusCondition.Asleep)
                defender.sleepCounter = Random.Range(1, 8);
            _battleUI?.AppendLogST($"{defender.nickname} became {defender.status}!");
        }

        yield return new WaitForSeconds(0.3f);
    }

    // ── Status tick ───────────────────────────────────────────────────
    private IEnumerator StatusTickRoutine()
    {
        ProcessStatusEffectsST(_playerPokemon);
        ProcessStatusEffectsST(_enemyPokemon);
        yield return new WaitForSeconds(0.5f);
        _stateMachine.TransitionToST(BattleState.CheckFainted);
    }

    public void ProcessStatusEffectsST(PokemonInstance inst)
    {
        if (inst == null || !inst.IsAlive) return;
        int maxHP = PokemonInstance.GetMaxHPST(inst);
        switch (inst.status)
        {
            case StatusCondition.Burned:
                inst.currentHP = Mathf.Max(1, inst.currentHP - maxHP / 8);
                _battleUI?.AppendLogST($"{inst.nickname} is hurt by its burn!");
                break;
            case StatusCondition.Poisoned:
                inst.currentHP = Mathf.Max(1, inst.currentHP - maxHP / 16);
                _battleUI?.AppendLogST($"{inst.nickname} is hurt by poison!");
                break;
        }
        _battleUI?.UpdateHPST(inst == _enemyPokemon, inst.currentHP, maxHP);
    }

    // ── Faint check (coroutine for animation hooks) ───────────────────
    private IEnumerator CheckFaintedRoutine()
    {
        if (_enemyPokemon != null && !_enemyPokemon.IsAlive)
        {
            _battleUI?.AppendLogST($"{_enemyPokemon.nickname} fainted!");
            if (_battleVisuals != null)
            {
                yield return _battleVisuals.PlayFaintAnimationST(isEnemy: true);
                yield return _battleVisuals.PlayVictoryAnimationST(isEnemy: false);
            }
            EndBattleST(BattleResult.Win);
            yield break;
        }
        if (_playerPokemon != null && !_playerPokemon.IsAlive)
        {
            _battleUI?.AppendLogST($"{_playerPokemon.nickname} fainted!");
            if (_battleVisuals != null)
            {
                yield return _battleVisuals.PlayFaintAnimationST(isEnemy: false);
                yield return _battleVisuals.PlayVictoryAnimationST(isEnemy: true);
            }
            EndBattleST(BattleResult.Lose);
            yield break;
        }
        _stateMachine.TransitionToST(BattleState.PlayerChoose);
    }

    // ── Battle end ────────────────────────────────────────────────────
    public void EndBattleST(BattleResult result)
    {
        _stateMachine.TransitionToST(BattleState.BattleEnd);
        StartCoroutine(EndBattleRoutine(result));
    }

    private IEnumerator EndBattleRoutine(BattleResult result)
    {
        yield return new WaitForSeconds(0.5f);

        // Trainer outro (trainer battles only)
        if (_battleVisuals != null)
            yield return _battleVisuals.PlayOutroSequenceST(result);

        // Results screen — waits for player to tap Continue
        if (_resultsScreen != null)
            yield return _resultsScreen.ShowST(result);
        else
            yield return new WaitForSeconds(2f);

        // Return to overworld
        GameManager.GetInstanceST()?.SceneTransitionManager
            .UnloadSceneST("Battle", () =>
                GameManager.GetInstanceST()?.ChangeStateST(GameState.Overworld));
    }
}
