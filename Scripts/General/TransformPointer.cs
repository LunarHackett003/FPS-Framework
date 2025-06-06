using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformPointer : LunarScript
{
    public bool useUp, useLateUpdate;
    public Transform target;
    public override void LUpdate()
    {
        if (useLateUpdate)
            return;
        Point();
    }
    public override void LPostUpdate()
    {
        if (useLateUpdate)
            Point();
    }
    void Point()
    {
        if (useUp)
        {
            transform.LookAt(target, target.up);
        }
        else
        {
            transform.LookAt(target);
        }
    }
}
