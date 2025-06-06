using UnityEngine;

public class Debuff
{
    public float timeRemaining;
    public bool tickDownTime;
    public NetEntity entity;
    public virtual void UpdateDebuff()
    {
        if (tickDownTime)
        {
            timeRemaining -= Time.fixedDeltaTime;
        }
    }
}

public class DamageOverTime : Debuff
{
    public float damagePerTick = 10;
    public bool increaseDamageOverTime = false;
    public float damageAddPerTick = 1f;
    public float damageMultPertick = 1.011f;
    public float damageInterval = 0.5f;
    protected float currentInterval;
    public override void UpdateDebuff()
    {
        base.UpdateDebuff();
        currentInterval += Time.fixedDeltaTime;
        if(currentInterval > damageInterval)
        {
            currentInterval = 0;
            entity.currentHealth.Value -= damagePerTick;
            if (increaseDamageOverTime)
            {
                damagePerTick += damageAddPerTick;
                damagePerTick *= damageMultPertick;
            }
        }
    }
}