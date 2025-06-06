using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class ProjectileHitEffect : LunarNetScript
{
    public NetworkBehaviourReference source;
}
