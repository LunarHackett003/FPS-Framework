using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentChargeOnWeapon : LunarNetScript
{
    public BaseNetWeapon weapon;
    public Image fillImage;

    public override void LTimestep()
    {
        base.LTimestep();

        if (!weapon.useEquipmentRecharge)
        {
            return;
        }

        float lerp = weapon.currentEquipmentRechargeTime == 0 ? 1 : Mathf.InverseLerp(0, weapon.equipmentRechargeTime, weapon.currentEquipmentRechargeTime);
        fillImage.fillAmount = lerp;
    }
}
