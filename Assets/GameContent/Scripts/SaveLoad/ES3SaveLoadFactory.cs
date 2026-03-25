using System;
using UnityEngine;

public class ES3SaveLoadFactory : ISaveLoadFactory
{
    private string GetPath(int slotIndex) => $"slots/slot_{slotIndex}.es3";

    public GameSaveData CreateNewSaveDataST(int slotIndex, string playerName)
    {
        return new GameSaveData
        {
            slotName      = $"Slot {slotIndex}",
            saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            gameVersion   = Application.version,
            player        = new PlayerSaveData { playerName = playerName },
            settings      = new SettingsSaveData()
        };
    }

    public void SaveToSlotST(int slotIndex, GameSaveData data)
    {
        data.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string path = GetPath(slotIndex);
        ES3File file = new ES3File(path);
        file.Save("gameData", data);
        file.Sync();
    }

    public GameSaveData LoadFromSlotST(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (!ES3.FileExists(path)) return null;
        ES3File file = new ES3File(path);
        return file.Load<GameSaveData>("gameData");
    }

    public bool SlotExistsST(int slotIndex)
    {
        return ES3.FileExists(GetPath(slotIndex));
    }

    public void DeleteSlotST(int slotIndex)
    {
        string path = GetPath(slotIndex);
        if (ES3.FileExists(path))
            ES3.DeleteFile(path);
    }
}
