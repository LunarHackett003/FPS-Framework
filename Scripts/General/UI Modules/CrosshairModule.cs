using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairModule : UIModule
{
    public CanvasGroup crosshairGroup;
    public float currentCrosshairSize = 1f;
    public float crosshairTargetSize = 0;
    public float crosshairLerpSpeed = 3f;
    public float baseCrosshairSize = 100f;

    public RectTransform crosshairRect;
    public Image weaponChargeImage;
    public RectTransform weaponChargeRoot;
    public float weaponChargeFillAmount;

    public BaseNetWeapon Weapon => Player.weaponController.CurrentWeapon;

    public BaseNetWeapon lastWeapon;
    bool weaponUsesCharge;
    public override void UpdateModule()
    {
        if (Weapon == null || Weapon.controller == null)
            return;

        crosshairGroup.alpha = Weapon.hideCrosshairWhenAiming ? 1 - Weapon.controller.aimAmount : 1;

        crosshairTargetSize = 1 + Weapon.controller.Spread(Weapon.baseAttackSpread + Weapon.attackSpreadAmount);

        currentCrosshairSize = Mathf.Lerp(currentCrosshairSize, Weapon.crosshairSpreadBase * (crosshairTargetSize * Weapon.crosshairSpreadMax), Time.deltaTime * crosshairLerpSpeed);
        crosshairRect.sizeDelta = new Vector2(currentCrosshairSize, currentCrosshairSize) * baseCrosshairSize;

        if(lastWeapon != Weapon)
        {
            weaponUsesCharge = Weapon.primaryUsesCharge || Weapon.secondaryUsesCharge;
            weaponChargeRoot.gameObject.SetActive(weaponUsesCharge);
        }
        if (weaponUsesCharge)
        {
            weaponChargeFillAmount = Weapon.chargeAmount;
            weaponChargeImage.fillAmount = weaponChargeFillAmount;
        }
    }
}
