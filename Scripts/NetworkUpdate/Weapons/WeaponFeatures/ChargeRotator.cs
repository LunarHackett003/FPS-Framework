using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeRotator : LunarScript
{
    public BaseNetWeapon weapon;

    public float targetSpinLerpSpeed = 4;
    public float spinSpeed = 5;
    float targetSpeed;
    public Vector3 axis = Vector3.up;

    private void Start()
    {
        if(weapon == null)
        {
            weapon = GetComponentInParent<BaseNetWeapon>();
        }
    }

    public override void LPostUpdate()
    {
        base.LPostUpdate();

        if (weapon)
        {
            targetSpeed = Mathf.Lerp(targetSpeed, weapon.chargeAmount, Time.deltaTime * targetSpinLerpSpeed * targetSpinLerpSpeed);
            transform.Rotate(axis, targetSpeed * Time.deltaTime * spinSpeed, Space.Self);
        }
    }
}
