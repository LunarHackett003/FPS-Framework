using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartScaleEventProxy : MonoBehaviour
{
    public WeaponPartScaleEvent target;

    void SetScaleOnTarget(int index)
    {
        target.SetScaleByIndex(index);
    }
}
