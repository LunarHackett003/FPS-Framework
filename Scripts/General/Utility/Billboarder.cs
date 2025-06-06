using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboarder : LunarScript
{
    public override void LPostUpdate()
    {
        base.LPostUpdate();

        transform.LookAt(Camera.main.transform, Vector3.up);
        transform.localEulerAngles += new Vector3(0, 180, 0);
    }
}
