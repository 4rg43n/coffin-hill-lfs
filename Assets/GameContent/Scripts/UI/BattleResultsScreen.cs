using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Placeholder results panel shown at the end of every battle.
/// Wire the SerializeField refs via CoffinHill/Wire Battle Scene UI.
/// Fill this out later with XP gains, caught pokemon, item drops, etc.
/// </summary>
public class BattleResultsScreen : MonoBehaviour
{
    [SerializeField] private GameObject     panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Button          continueButton;

    private bool _continued;

    /// <summary>Shows the panel and waits until the player taps Continue.</summary>
    public IEnumerator ShowST(BattleResult result)
    {
        _continued = false;

        titleText.text = result switch
        {
            BattleResult.Win    => "VICTORY!",
            BattleResult.Lose   => "DEFEATED...",
            BattleResult.Fled   => "ESCAPED!",
            BattleResult.Caught => "CAUGHT!",
            _                   => "BATTLE OVER"
        };

        bodyText.text = result switch
        {
            BattleResult.Win    => "You won the battle!\n\n(Rewards coming soon)",
            BattleResult.Lose   => "You were defeated...\n\n(Continue?)",
            BattleResult.Fled   => "You got away safely.",
            BattleResult.Caught => "You caught a new monster!\n\n(Party system coming soon)",
            _                   => ""
        };

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() => _continued = true);

        panel.SetActive(true);

        yield return new WaitUntil(() => _continued);

        panel.SetActive(false);
    }
}
