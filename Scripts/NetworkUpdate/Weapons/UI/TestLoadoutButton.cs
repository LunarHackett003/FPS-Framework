using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestLoadoutButton : MonoBehaviour
{
    public int weaponIndex;
    public Image iconDisplay;
    public TMP_Text nameDisplay;
    public Button button;
    public void Initialise(BaseNetWeapon weapon, int index)
    {
        weaponIndex = index;
        iconDisplay.sprite = weapon.weaponIcon;
        nameDisplay.text = weapon.displayName;
    }
}
