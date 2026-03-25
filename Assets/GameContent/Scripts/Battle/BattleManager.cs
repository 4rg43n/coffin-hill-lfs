using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private static BattleManager _instance;
    public static BattleManager GetInstanceST() => _instance;

    private BattleStateMachine _stateMachine;
    private PokemonInstance _playerPokemon;
    private PokemonInstance _enemyPokemon;
    private TrainerData _currentTrainer;
    private bool _isWildBattle;
    private BattleUI _battleUI;

    public PokemonInstance PlayerPokemon => _playerPokemon;
    public PokemonInstance EnemyPokemon  => _enemyPokemon;

    private void Awake()
    {
        _instance = this;
        _stateMachine = new BattleStateMachine();
        _stateMachine.OnStateChanged += OnBattleStateChanged;
        _battleUI = FindAnyObjectByType<BattleUI>();
    }

    public void StartWildEncounterST(PokemonInstance enemy)
    {
        _isWildBattle = true;
        _enemyPokemon = enemy;

        SaveLoadManager slm = SaveLoadManager.GetInstanceST();
        if (slm?.ActiveSaveData?.player?.party != null
            && slm.ActiveSaveData.player.party.Count > 0)
        {
            // Use first alive party member; simplified — party is list of save data
            // For now use a placeholder until party system is complete
        }

        GameManager.GetInstanceST()?.SceneTransitionManager
            .LoadSceneAdditiveST("Battle", OnBattleSceneLoaded);
    }

    public void StartTrainerBattleST(TrainerData trainer)
    {
        _isWildBattle = false;
        _currentTrainer = trainer;
        _enemyPokemon = trainer.party != null && trainer.party.Length > 0
            ? trainer.party[0] : null;

        GameManager.GetInstanceST()?.SceneTransitionManager
            .LoadSceneAdditiveST("Battle", OnBattleSceneLoaded);
    }

    private void OnBattleSceneLoaded()
    {
        GameManager.GetInstanceST()?.ChangeStateST(GameState.Battle);
        _stateMachine.TransitionToST(BattleState.Intro);
    }

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
            case BattleState.PlayerAction:
                break;
            case BattleState.EnemyAction:
                StartCoroutine(EnemyTurnRoutine());
                break;
            case BattleState.StatusTick:
                StartCoroutine(StatusTickRoutine());
                break;
            case BattleState.CheckFainted:
                CheckFaintedST();
                break;
            case BattleState.BattleEnd:
                break;
        }
    }

    private IEnumerator IntroRoutine()
    {
        _battleUI?.AppendLogST(_isWildBattle
            ? $"A wild {_enemyPokemon?.data?.speciesName} appeared!"
            : $"{_currentTrainer?.trainerName} wants to battle!");
        yield return new WaitForSeconds(1.5f);
        _stateMachine.TransitionToST(BattleState.PlayerChoose);
    }

    public void OnPlayerSelectedMoveST(MoveData move)
    {
        if (_stateMachine.CurrentState != BattleState.PlayerChoose) return;
        _stateMachine.TransitionToST(BattleState.PlayerAction);
        StartCoroutine(PlayerActionRoutine(move));
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

    private IEnumerator PlayerActionRoutine(MoveData move)
    {
        yield return ExecuteMoveST(_playerPokemon, _enemyPokemon, move);
        if (!_enemyPokemon.IsAlive)
        {
            _stateMachine.TransitionToST(BattleState.CheckFainted);
            yield break;
        }
        _stateMachine.TransitionToST(BattleState.EnemyAction);
    }

    private IEnumerator EnemyTurnRoutine()
    {
        MoveData enemyMove = EnemyAI.ChooseMoveST(_enemyPokemon, _playerPokemon);
        if (enemyMove == null)
        {
            _stateMachine.TransitionToST(BattleState.StatusTick);
            yield break;
        }
        yield return ExecuteMoveST(_enemyPokemon, _playerPokemon, enemyMove);
        _stateMachine.TransitionToST(BattleState.StatusTick);
    }

    private IEnumerator ExecuteMoveST(PokemonInstance attacker, PokemonInstance defender, MoveData move)
    {
        // Paralysis check
        if (attacker.status == StatusCondition.Paralyzed && Random.value < 0.25f)
        {
            _battleUI?.AppendLogST($"{attacker.nickname} is fully paralyzed!");
            yield break;
        }
        // Sleep check
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
        // Freeze check
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

        // Find PP slot
        int ppSlot = System.Array.IndexOf(attacker.moves, move);
        if (ppSlot >= 0 && attacker.currentPP[ppSlot] > 0)
            attacker.currentPP[ppSlot]--;

        _battleUI?.AppendLogST($"{attacker.nickname} used {move.moveName}!");
        yield return new WaitForSeconds(0.5f);

        // Accuracy check
        if (Random.Range(0, 100) >= move.accuracy)
        {
            _battleUI?.AppendLogST("But it missed!");
            yield break;
        }

        bool isCrit = BattleFormulas.IsCritST(attacker, move);
        int damage = BattleFormulas.CalculateDamageST(attacker, defender, move, isCrit);

        if (damage > 0)
        {
            if (isCrit) _battleUI?.AppendLogST("A critical hit!");
            defender.currentHP = Mathf.Max(0, defender.currentHP - damage);
            _battleUI?.UpdateHPST(defender == _enemyPokemon, defender.currentHP,
                PokemonInstance.GetMaxHPST(defender));
        }

        // Status effect
        if (move.statusEffect != StatusCondition.None && defender.status == StatusCondition.None
            && Random.value < move.statusChance)
        {
            defender.status = move.statusEffect;
            if (move.statusEffect == StatusCondition.Asleep)
                defender.sleepCounter = Random.Range(1, 8);
            _battleUI?.AppendLogST($"{defender.nickname} became {defender.status}!");
        }

        yield return new WaitForSeconds(0.3f);
    }

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

    private void CheckFaintedST()
    {
        if (_enemyPokemon != null && !_enemyPokemon.IsAlive)
        {
            _battleUI?.AppendLogST($"{_enemyPokemon.nickname} fainted!");
            EndBattleST(BattleResult.Win);
            return;
        }
        if (_playerPokemon != null && !_playerPokemon.IsAlive)
        {
            _battleUI?.AppendLogST($"{_playerPokemon.nickname} fainted!");
            EndBattleST(BattleResult.Lose);
            return;
        }
        _stateMachine.TransitionToST(BattleState.PlayerChoose);
    }

    public void EndBattleST(BattleResult result)
    {
        _stateMachine.TransitionToST(BattleState.BattleEnd);
        StartCoroutine(EndBattleRoutine(result));
    }

    private IEnumerator EndBattleRoutine(BattleResult result)
    {
        yield return new WaitForSeconds(1f);
        GameManager.GetInstanceST()?.SceneTransitionManager
            .UnloadSceneST("Battle", () =>
            {
                GameManager.GetInstanceST()?.ChangeStateST(GameState.Overworld);
            });
    }
}
