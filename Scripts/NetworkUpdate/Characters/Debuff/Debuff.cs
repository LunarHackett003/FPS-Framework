using UnityEngine;

public enum DebuffType : int
{
    none = 0,
    burn = 1,
    stun = 2,
    poision = 3
}



[System.Serializable]
public class Debuff
{
    public string name;


    public float timeRemaining;
    public bool tickDownTime;

    public NetEntity entity;

    public DebuffType type;

    public bool useDamageOverTime;
    public float damagePerTick = 10;
    public bool increaseDamageOverTime = false;
    public float damageAddPerTick = 1f;
    public float damageMultPerTick = 1.011f;
    public float damageInterval = 0.5f;
    protected float currentInterval;

    public bool useStun;
    public float moveSpeedModifier = 0.5f, lookSpeedModifier = 0.5f;
    public bool canJump = false, canSprint = false;
    public bool restrictSlotsWhileDebuffed;
    public bool[] slotAllowed = new bool[]
    {
        true, true, true, true
    };

    public virtual void InitialiseDebuff(DebuffStats statsIn)
    {
        //If DebuffStats.addDuration is true, we'll extend the time of this by n seconds.
        timeRemaining = statsIn.addDuration ? (timeRemaining + statsIn.duration) : statsIn.duration;
        if (timeRemaining > 0)
            tickDownTime = true;
    }

    public virtual void UpdateDebuff(bool isServer)
    {
        if (tickDownTime)
        {
            timeRemaining -= Time.fixedDeltaTime;
            currentInterval += Time.fixedDeltaTime;
            if(currentInterval > damageInterval)
            {
                currentInterval %= damageInterval;
                if(isServer)
                    DamageOverTime();
            }
            if (isServer)
                ProcessEffects();

            if (timeRemaining < 0)
                tickDownTime = false;
        }

    }
    protected virtual void DamageOverTime()
    {
        entity.ModifyHealth(-damagePerTick);
        if (increaseDamageOverTime)
        {
            damagePerTick += damageAddPerTick;
            damagePerTick *= damageMultPerTick;
        }
    }

    protected virtual void ProcessEffects()
    {
        for (int i = 0; i < slotAllowed.Length; i++)
        {
            entity.slotsAllowed[i] &= slotAllowed[i];
        }
        entity.rotationModifier += lookSpeedModifier;
        entity.movementModifier += moveSpeedModifier;
        entity.canJump &= canJump;
        entity.canSprint &= canSprint;
    }
}