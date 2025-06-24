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
    public float damageMultPertick = 1.011f;
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
    public virtual void UpdateDebuff()
    {
        if (tickDownTime)
        {
            timeRemaining -= Time.fixedDeltaTime;
        }
    }
}