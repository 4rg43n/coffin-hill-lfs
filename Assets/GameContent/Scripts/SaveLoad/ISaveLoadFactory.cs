public interface ISaveLoadFactory
{
    GameSaveData CreateNewSaveDataST(int slotIndex, string playerName);
    void SaveToSlotST(int slotIndex, GameSaveData data);
    GameSaveData LoadFromSlotST(int slotIndex);
    bool SlotExistsST(int slotIndex);
    void DeleteSlotST(int slotIndex);
}
