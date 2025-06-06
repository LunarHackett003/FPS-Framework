using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Loadout/Weapon Collection")]
public class TestLoadoutWeaponCollection : ScriptableObject
{
    public List<BaseNetWeapon> weapons;
}
