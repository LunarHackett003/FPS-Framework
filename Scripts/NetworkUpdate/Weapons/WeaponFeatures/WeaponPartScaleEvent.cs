using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPartScaleEvent : MonoBehaviour
{
    public Vector3[] scales;

    public void SetScaleByIndex(int index)
    {
        transform.localScale = scales[index];
    }
}
