using UnityEngine;

[CreateAssetMenu(fileName = "NewTrainer", menuName = "CoffinHill/Trainer Data")]
public class TrainerData : ScriptableObject
{
    public string trainerName;
    [TextArea] public string preBattleDialogue;
    [TextArea] public string postBattleDialogue;
    public PokemonInstance[] party;
    public int rewardMoney;
}
