using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestLoadoutMenu : LunarScript
{
    public static TestLoadoutMenu Instance { get; private set; }
    public TestLoadoutWeaponCollection weaponList;

    public List<int> selectedWeapons = new(4);

    public Button[] slotButtons;

    public TestLoadoutButton buttonPrefab;
    List<TestLoadoutButton> loadoutButtons;
    public RectTransform weaponListRoot;

    public enum SlotSelection
    {
        slot1 = 0,
        slot2 = 1,
        slot3 = 2,
        slot4 = 3,
    }
    public SlotSelection currentSlotSelection;

    private void Awake()
    {
        if (Instance != null)
        {
            gameObject.SetActive(false);
            return;
        }
        Instance = this;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            Debug.Log($"setting up listener for button {i}");
            slotButtons[i].onClick.AddListener(() =>
            {
                SetSelection(index);
            });
        }
        BuildUI();
        weaponListRoot.gameObject.SetActive(false);
    }

    void SetSelection(int slotIndex)
    {
        Debug.Log($"Selection enabled for slot {slotIndex}");
        currentSlotSelection = (SlotSelection)slotIndex;
        Debug.Log($"That translates to {(int)currentSlotSelection}");
        weaponListRoot.gameObject.SetActive(true);
    }

    public void SelectWeapon(int index)
    {
        selectedWeapons[(int)currentSlotSelection] = index;
        weaponListRoot.gameObject.SetActive(false);
    }

    public void BuildUI()
    {
        loadoutButtons = new();
        for (int i = 0; i < weaponList.weapons.Count; i++)
        {
            int index = i;
            var button = Instantiate(buttonPrefab, weaponListRoot);
            button.Initialise(weaponList.weapons[index], index);
            loadoutButtons.Add(button);

            button.button.onClick.AddListener(() => { SelectWeapon(index); });
        }
    }

}
