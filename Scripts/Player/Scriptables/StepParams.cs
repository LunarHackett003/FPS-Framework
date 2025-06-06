using UnityEngine;

[CreateAssetMenu(fileName = "StepParams", menuName = "Scriptable Objects/StepParams")]
public class StepParams : ScriptableObject
{
    [Tooltip("How high can the player step?")] public float stepHeight = 0.3f;
    [Tooltip("How far forwards we check for steps")] public float stepDistance = 0.5f;
    [Tooltip("How much to smooth moving up steps")] public float stepSpeed = 3f;
    [Tooltip("Which layers to cast against to check for steps")] public LayerMask stepLayermask;
    [Tooltip("What size box to use when checking for steps")] public Vector3 stepBoxSize;
}
