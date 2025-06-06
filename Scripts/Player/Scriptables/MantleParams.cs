using UnityEngine;

[CreateAssetMenu(fileName = "MantleParams", menuName = "Scriptable Objects/MantleParams")]
public class MantleParams : ScriptableObject
{
    public AnimationCurve mantleVerticalPath, mantleLateralPath;
    public float mantleCheckDistance, mantleMaxHeight,
        mantleMinEjectTime, mantleMaxEjectTime, mantleRearEjectForce,
        mantleUpEjectForce, mantleEnableTime, mantleSpeed, mantlePointForwardOffset;
    public Vector3 mantleCheckOffset, mantleCheckBounds;
    public Vector3 mantleHeightRayOffset;
    public bool mantleFollowsTransform;
    public float mantlePointOffset;
    public float mantleDismountSpeed;
}
