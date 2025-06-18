using UnityEngine;

[CreateAssetMenu(fileName = "NewMonster", menuName = "Monsters/MonsterSO")]
public class MonsterSO : ScriptableObject
{
    // Basic info
    public string monsterName;
    public string nickName;
    public bool gender;

    // Stats
    public int maxHealth = 100;
    public int str = 5;
    public int def = 5;
    public int spd = 5;
    public int magStr = 5;
    public int magDef = 5;
    public int luck = 5;
} 