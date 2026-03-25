using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("Enemy Info")]
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI enemyLevelText;
    [SerializeField] private Image enemyHPBar;

    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private Image playerHPBar;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private Image playerEXPBar;

    [Header("Action Menu")]
    [SerializeField] private GameObject actionMenu;
    [SerializeField] private Button fightButton;
    [SerializeField] private Button bagButton;
    [SerializeField] private Button partyButton;
    [SerializeField] private Button runButton;

    [Header("Move Menu")]
    [SerializeField] private GameObject moveMenu;
    [SerializeField] private Button[] moveButtons;
    [SerializeField] private TextMeshProUGUI[] moveNameTexts;
    [SerializeField] private TextMeshProUGUI[] movePPTexts;

    [Header("Battle Log")]
    [SerializeField] private TextMeshProUGUI battleLogText;
    [SerializeField] private ScrollRect battleLogScroll;

    public event Action<MoveData> OnMoveSelected;
    public event Action OnRunSelected;
    public event Action OnBagSelected;
    public event Action OnPartySelected;

    private BattleManager _battleManager;

    private void Start()
    {
        _battleManager = BattleManager.GetInstanceST();

        if (fightButton) fightButton.onClick.AddListener(OnFightClicked);
        if (bagButton)   bagButton.onClick.AddListener(() => OnBagSelected?.Invoke());
        if (partyButton) partyButton.onClick.AddListener(() => OnPartySelected?.Invoke());
        if (runButton)   runButton.onClick.AddListener(() =>
        {
            OnRunSelected?.Invoke();
            _battleManager?.OnRunSelectedST();
        });

        ShowActionMenuST(false);
        if (moveMenu) moveMenu.SetActive(false);
    }

    public void ShowActionMenuST(bool show)
    {
        if (actionMenu) actionMenu.SetActive(show);
        if (moveMenu)   moveMenu.SetActive(false);
    }

    private void OnFightClicked()
    {
        ShowActionMenuST(false);
        PopulateMoveMenuST();
        if (moveMenu) moveMenu.SetActive(true);
    }

    private void PopulateMoveMenuST()
    {
        PokemonInstance player = _battleManager?.PlayerPokemon;
        if (player == null) return;

        for (int i = 0; i < moveButtons.Length; i++)
        {
            if (i < player.moves.Length && player.moves[i] != null)
            {
                MoveData move = player.moves[i];
                int pp = player.currentPP[i];
                if (moveNameTexts.Length > i) moveNameTexts[i].text = move.moveName;
                if (movePPTexts.Length > i)   movePPTexts[i].text   = $"{pp}/{move.maxPP}";

                int capturedIndex = i;
                moveButtons[i].interactable = pp > 0;
                moveButtons[i].onClick.RemoveAllListeners();
                moveButtons[i].onClick.AddListener(() =>
                {
                    MoveData selectedMove = player.moves[capturedIndex];
                    OnMoveSelected?.Invoke(selectedMove);
                    _battleManager?.OnPlayerSelectedMoveST(selectedMove);
                    if (moveMenu) moveMenu.SetActive(false);
                });
            }
            else
            {
                if (moveNameTexts.Length > i) moveNameTexts[i].text = "-";
                if (movePPTexts.Length > i)   movePPTexts[i].text   = "-";
                moveButtons[i].interactable = false;
            }
        }
    }

    public void AppendLogST(string message)
    {
        if (battleLogText == null) return;
        battleLogText.text += (battleLogText.text.Length > 0 ? "\n" : "") + message;
        Canvas.ForceUpdateCanvases();
        if (battleLogScroll != null)
            battleLogScroll.verticalNormalizedPosition = 0f;
    }

    public void UpdateHPST(bool isEnemy, int current, int max)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        if (isEnemy)
        {
            if (enemyHPBar) StartCoroutine(AnimateHPBarST(enemyHPBar, enemyHPBar.fillAmount, ratio, 0.4f));
        }
        else
        {
            if (playerHPBar) StartCoroutine(AnimateHPBarST(playerHPBar, playerHPBar.fillAmount, ratio, 0.4f));
            if (playerHPText) playerHPText.text = $"{current}/{max}";
        }
    }

    public IEnumerator AnimateHPBarST(Image bar, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bar.fillAmount = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        bar.fillAmount = to;
    }

    public void SetupBattleST(PokemonInstance enemy, PokemonInstance player)
    {
        if (enemy != null)
        {
            if (enemyNameText)  enemyNameText.text  = enemy.nickname;
            if (enemyLevelText) enemyLevelText.text = $"Lv.{enemy.level}";
            if (enemyHPBar)     enemyHPBar.fillAmount = 1f;
        }
        if (player != null)
        {
            if (playerNameText)  playerNameText.text  = player.nickname;
            if (playerLevelText) playerLevelText.text = $"Lv.{player.level}";
            int maxHP = PokemonInstance.GetMaxHPST(player);
            if (playerHPBar)  playerHPBar.fillAmount = (float)player.currentHP / maxHP;
            if (playerHPText) playerHPText.text = $"{player.currentHP}/{maxHP}";
        }
    }
}
