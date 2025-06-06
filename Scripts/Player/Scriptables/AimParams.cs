using UnityEngine;

[CreateAssetMenu(fileName = "AimParams", menuName = "Scriptable Objects/AimParams")]
public class AimParams : ScriptableObject
{
    [Tooltip("This value is subtracted from the Base FOV when it is in effect.")] public float aimFOV = -10, altAimFOV = -5;
    [Tooltip("How quickly, per degree of FOV in the transition, your view moves towards the target fov.")] public float aimSpeed = 15;

    [Tooltip("The linear offset to apply to the rweapon when aiming")] public Vector3 baseAimPositionOffset = Vector3.zero;
    [Tooltip("The scale for the linear offset when aiming")] public Vector3 aimedWeaponPositionScale = Vector3.one;
    [Tooltip("The linear offset to apply to the rweapon when aiming")] public Vector3 aimPositionOffsetAngled = Vector3.zero;
    [Tooltip("How much of the rweapon's rotation to remove, from 0 to 1"), Range(0, 1)] public float aimRotationReduction = 0f;
    [Tooltip("How much to rotate the rweapon by AFTER APPLY THE ROTATION REDUCTION when aiming")] public Quaternion aimRotationOffset = Quaternion.identity;
    [Tooltip("The linear offset to apply to the rweapon when crouching")] public Vector3 crouchPositionOffset = Vector3.zero;
    [Tooltip("The curve to sample when offsetting the rweapon for crouching")] public AnimationCurve crouchLerpCurve;
    [Tooltip("The curve to sample when offsetting the rweapon for aiming")] public AnimationCurve aimLerpCurve;
    [Tooltip("The angular offset to apply to the rweapon when crouching and not aiming")] public Quaternion crouchRotationOffset = Quaternion.identity;
    [Tooltip("The additive fov of the viewmodel camera when aiming")] public float viewmodelFOV = -20;
}
