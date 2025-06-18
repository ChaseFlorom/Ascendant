using UnityEngine;

public class Monster
{
    //Basic info
    public string monsterName;
    public string nickName;
    public bool gender;




    //Stats
    public int maxHealth = 100;
    public int str = 5;
    public int def = 5;
    public int spd = 5;
    public int magStr = 5;
    public int magDef = 5;
    public int luck = 5;

    public Monster(MonsterSO so)
    {
        monsterName = so.monsterName;
        nickName = so.nickName;
        gender = so.gender;
        maxHealth = so.maxHealth;
        str = so.str;
        def = so.def;
        spd = so.spd;
        magStr = so.magStr;
        magDef = so.magDef;
        luck = so.luck;
    }
}
