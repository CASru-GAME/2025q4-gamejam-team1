using UnityEngine;

public class Stamina 
{
    private int maxStamina;
    private int currentStamina;
    
    public Stamina(int initializedStamina)
    {
        this.maxStamina= initializedStamina;
        this.currentStamina = initializedStamina;
    }
    //参照用変数
    public int MaxStamina => maxStamina;
    public int CurrentStamina => currentStamina;
    
    //staminaを減らす関数
    public void Consume(int amount)
    {
        if(amount < 0)
        {
            Debug.LogWarning("amountがマイナスです");
        }
        currentStamina -= amount;
        if(currentStamina < 0)
        {
            currentStamina = 0;
        }
    }
    //staminaを増やす関数

    public void Heal(int heal)
    {
        currentStamina += heal;
        if(heal < 0)
        {
            Debug.LogWarning("healがマイナスです");
        }
    }

}
