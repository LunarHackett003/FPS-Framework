using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutModule : UIModule
{
    public LoadoutDisplay[] loadoutDisplays;
    int weaponCount;


    public override void UpdateModule()
    {
        //Return if the player or their weapon controller does not exist
        if (Player == null || Player.weaponController == null)
            return;

        weaponCount = Player.weaponController.weapons.Count;

        //Exit early if our player has no weapons
        if (weaponCount == 0)
            return;
        //Check all weapons to see if they use equipment Charges
        for (int i = 0; i < weaponCount; i++)
        {
            BaseNetWeapon w = Player.weaponController.weapons[i];
            if(w != null)
            {
                UpdateEquipmentCharges(w,  i);
            }
        }
    }
    void UpdateEquipmentCharges(BaseNetWeapon weapon, int slotIndex)
    {
        loadoutDisplays[slotIndex].UpdateDisplay(weapon, slotIndex);
    }
}
