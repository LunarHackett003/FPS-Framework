using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The GameplayCanvas is a script that controls the parts of the UI that the player can see when they're not in a menu.<br></br>
/// This is everything from ammo counters and weapon names, to crosshairs and leaderboards.
/// </summary>
public class GameplayCanvas : LunarScript
{
    public static GameplayCanvas Instance { get; private set; }
    public static NetPlayerEntity player;

    bool playerAliveLast;

    private void Awake()
    {
        Initialise();
    }
    public void Initialise()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public CanvasGroup hud, deathUI;

    public UIModule[] uiModules;

    private void OnGUI()
    {
        GUILayout.Space(100);
        if (Instance == null)
        {
            GUILayout.Label("Gameplay Canvas Not Working!");
        }
    }

    public override void LPostUpdate()
    {
        if (Instance == null)
            Instance = this;


        base.LPostUpdate();

        if(player == null)
        {
            return;
        }

        if(playerAliveLast != player.isDead.Value)
        {
            ProcessPlayerDeath(player.isDead.Value);
            playerAliveLast = player.isDead.Value;
        }

        for (int i = 0; i < uiModules.Length; i++)
        {
            UpdateModule(uiModules[i]);
        }
    }
    void ProcessPlayerDeath(bool died)
    {
        hud.alpha = died ? 0 : 1;
        deathUI.alpha = died ? 1 : 0;
    }


    public void UpdateModule(UIModule module)
    {
        if(module != null)
            module.UpdateModule();
    }
}
