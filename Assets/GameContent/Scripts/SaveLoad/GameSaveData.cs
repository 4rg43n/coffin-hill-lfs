using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public string slotName;
    public string saveTimestamp;
    public string gameVersion = "0.1.0";
    public PlayerSaveData player;
    public SettingsSaveData settings;
}

[Serializable]
public class PlayerSaveData
{
    public string playerName = "Player";
    public int money;
    public string currentMapScene = "Overworld";
    public Vector2Int gridPosition;
    public List<PokemonInstanceSaveData> party   = new List<PokemonInstanceSaveData>();
    public List<PokemonInstanceSaveData> pcBox   = new List<PokemonInstanceSaveData>();
    public List<ItemSaveData> bag                = new List<ItemSaveData>();
    public bool[] pokedexSeen    = new bool[152];
    public bool[] pokedexCaught  = new bool[152];
    public MapRunData activeMapRun;   // null = no run in progress
}

[Serializable]
public class PokemonInstanceSaveData
{
    public int pokedexNumber;
    public string nickname;
    public int level;
    public int currentHP;
    public int totalExp;
    public int[] ivs = new int[6];
    public int[] evs = new int[6];
    public StatusCondition status;
    public int sleepCounter;
    public int[] movePPCurrent = new int[4];
    public int[] moveIndices   = new int[4]; // indices into PokemonDatabase's move list
}

[Serializable]
public class ItemSaveData
{
    public string itemName;
    public int quantity;
}

[Serializable]
public class SettingsSaveData
{
    public float musicVolume = 1f;
    public float sfxVolume   = 1f;
}
