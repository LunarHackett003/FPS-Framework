using UnityEngine;

[CreateAssetMenu(fileName = "ViewParams", menuName = "Scriptable Objects/ViewParams")]
public class ViewParams : ScriptableObject
{
    //Aiming
    [Header("Aiming")] public Vector2 lookSpeed = Vector2.one * 10;
    public float lookPitchClamp = 89f;
    public float aimLookModifier = 0.8f;
    public float aimModifierPerDegree = 0.99f;
    public float sprintFOV = 5;

    //View
    [Header("View")] public float baseFOV = 80;
    public float crouchedHeadHeight = 0.1f;
    public float standingHeadHeight = 0.55f;
    public float strafeHeadTiltAngle = 2;
    public float slideHeadTiltAngle = 5;
    public float slideFOV = 5;
    public float headTiltSpeed = 12;
    public float viewmodelBaseFOV = 45;
    public float fovMoveSpeed = 15;
}
