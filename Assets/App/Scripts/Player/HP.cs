using UnityEngine;

public class HP 
{
    private int maxHP;
    private int currentHP;
    
    public HP(int initializedHP)
    {
        this.maxHP= initializedHP;
        this.currentHP = initializedHP;
    }
    //参照用変数
    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    
    //hpを減らす関数
    public void Damage(int damage)
    {
        if(damage < 0)
        {
            Debug.LogWarning("damageがマイナスです");
            return;
        }
        currentHP -= damage;
        if(currentHP < 0)
        {
            currentHP = 0;
        }

        /*if(maxHP < currentHP)
        {
            currentHP = maxHP;
        }*/
    }
    //hpを増やす関数

    public void Heal(int heal)
    {
        currentHP += heal;
        if(heal < 0)
        {
            Debug.LogWarning("healがマイナスです");
            return;
        }
    }

}
