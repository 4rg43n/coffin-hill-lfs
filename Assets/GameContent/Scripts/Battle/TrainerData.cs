using UnityEngine;

[CreateAssetMenu(fileName = "NewTrainer", menuName = "CoffinHill/Trainer Data")]
public class TrainerData : ScriptableObject
{
    public string trainerName;
    [Header("Visuals")]
    public Sprite trainerSprite;   // leave null to use generated placeholder
    [TextArea] public string preBattleDialogue;
    [TextArea] public string postBattleDialogue;
    public PokemonInstance[] party;
    public int rewardMoney;
}
