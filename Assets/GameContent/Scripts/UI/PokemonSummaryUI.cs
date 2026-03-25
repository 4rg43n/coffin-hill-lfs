using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PokemonSummaryUI : MonoBehaviour
{
    [SerializeField] private Image spriteImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI typesText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI[] movesText;
    [SerializeField] private TextMeshProUGUI[] movesPPText;

    public void ShowST(PokemonInstance inst)
    {
        if (inst == null) return;
        if (spriteImage && inst.data.frontSprite) spriteImage.sprite = inst.data.frontSprite;
        if (nameText)  nameText.text  = inst.nickname;
        if (levelText) levelText.text = $"Lv.{inst.level}";
        if (typesText)
        {
            string types = inst.data.primaryType.ToString();
            if (inst.data.secondaryType != PokemonType.None)
                types += $" / {inst.data.secondaryType}";
            typesText.text = types;
        }
        if (statsText)
        {
            statsText.text =
                $"HP: {inst.currentHP}/{PokemonInstance.GetMaxHPST(inst)}\n" +
                $"Atk: {PokemonInstance.GetStatST(inst, PokemonInstance.StatAtk)}\n" +
                $"Def: {PokemonInstance.GetStatST(inst, PokemonInstance.StatDef)}\n" +
                $"Spd: {PokemonInstance.GetStatST(inst, PokemonInstance.StatSpd)}\n" +
                $"Spc: {PokemonInstance.GetStatST(inst, PokemonInstance.StatSpc)}";
        }
        for (int i = 0; i < 4; i++)
        {
            if (movesText.Length > i)
                movesText[i].text = inst.moves[i] != null ? inst.moves[i].moveName : "-";
            if (movesPPText.Length > i)
                movesPPText[i].text = inst.moves[i] != null
                    ? $"{inst.currentPP[i]}/{inst.moves[i].maxPP}" : "-";
        }
    }
}
