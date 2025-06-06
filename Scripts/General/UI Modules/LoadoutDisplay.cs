using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutDisplay : MonoBehaviour
{
    public Image iconBase, iconChargeIndicator;
    public TMP_Text chargeNumber;
    bool usesCharge;
    internal BaseNetWeapon weapon;

    public Color selectedColour, unselectedColour;

    public void UpdateDisplay(BaseNetWeapon weapon, int index)
    {
        if(this.weapon == null || weapon != this.weapon)
        {
            iconBase.sprite = weapon.weaponIcon;
            iconChargeIndicator.sprite = weapon.weaponIcon;

            chargeNumber.gameObject.SetActive(weapon.useEquipmentRecharge);
            iconChargeIndicator.enabled = weapon.useEquipmentRecharge;

            this.weapon = weapon;
        }
        if (weapon.useEquipmentRecharge)
        {
            iconChargeIndicator.fillAmount = Mathf.InverseLerp(0, weapon.equipmentRechargeTime, weapon.currentEquipmentRechargeTime);
            chargeNumber.text = weapon.equipmentCharges.Value.ToString();
        }

        iconBase.color = index == GameplayCanvas.player.weaponController.weaponIndex.Value ? selectedColour : unselectedColour;
    }


}
