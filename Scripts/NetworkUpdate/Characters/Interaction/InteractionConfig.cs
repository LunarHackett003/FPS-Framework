using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Interaction Config")]
public class InteractionConfig : ScriptableObject
{
    public float maxInteractDistance = 8;
    public float interactThickness = 0.025f;

    public LayerMask interactLayerMask;

    public float interactedObjectMoveSpeed = 10;
    public float interactedObjectRotateSpeed = 10;

    public bool interactRotateUseSlerp = false;

    public bool canInteractThroughWalls = false;

    public Vector3 grabbedObjectOffsetFromWeaponPoint = (Vector3.right + Vector3.up) * 0.5f + (Vector3.forward * 0.3f);

    public bool canInteractWithEnemyItems = false;

    public float throwForce = 15;
}
