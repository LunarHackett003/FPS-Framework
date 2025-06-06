
[System.Serializable]
public class NetworkTimer
{
    float timer;
    public float MinTimeBetweenTicks {  get;}
    public int CurrentTick { get; private set; }
    public bool ShouldTick { get; private set; }

    public NetworkTimer(float serverTickRate)
    {
        MinTimeBetweenTicks = 1f / serverTickRate;
    }

    public void Update(float deltaTime)
    {
        timer += deltaTime;
        ShouldTick = SetShouldTick();
    }

    public bool SetShouldTick()
    {
        if(timer >= MinTimeBetweenTicks)
        {
            timer -= MinTimeBetweenTicks;
            CurrentTick++;
            return true;
        }
        return false;
    }
}
