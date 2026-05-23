using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Menu;

public class PersistentData : MonoBehaviourSingletonPersistent<PersistentData>
{
    public bool HasACharacterBeenChosen;
    public int IndexOfChosenCharacter;
    public CharacterData CurrentCharacter;
    public D_LevelConfig CurrentLevelConfig;
    public string CurrentBossName;
    public string BossToUnlockOnDefeat;
    public bool DebugInfiniteLife;
}
