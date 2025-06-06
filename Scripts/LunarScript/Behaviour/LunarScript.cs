using UnityEngine;

public class LunarScript : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        LunarManager.Instance.lPostUpdate += LPostUpdate;
        LunarManager.Instance.lUpdate += LUpdate;
        LunarManager.Instance.lTimeStep += LTimestep;
    }
    protected virtual void OnDisable()
    {
        LunarManager.Instance.lPostUpdate -= LPostUpdate;
        LunarManager.Instance.lUpdate -= LUpdate;
        LunarManager.Instance.lTimeStep -= LTimestep;
    }


    public virtual void LUpdate()
    {

    }
    public virtual void LTimestep()
    {

    }
    public virtual void LPostUpdate()
    {

    }
}
