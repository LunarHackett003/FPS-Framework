using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIModule : MonoBehaviour
{

    public NetPlayerEntity Player => GameplayCanvas.player;

    public abstract void UpdateModule();
}
