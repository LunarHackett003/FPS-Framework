using UnityEngine;

[CreateAssetMenu(fileName = "DashParams", menuName = "Scriptable Objects/DashParams")]
public class DashParams : ScriptableObject
{
    [Tooltip("How much force should be applied while dashing? X is forward force and y is vertical force.")]
    public Vector2 dashForce;
    [Tooltip("How much damping should be applied while dashing?")]
    public float dashForceDamping;
    [Tooltip("How much of the players aim direction in each axis, from 0 to 1, should be used to calculate the dash direction?\n" +
        "Higher values will use a greater portion of the player's pitch angle - at 0, dashing will ignore pitch. At 1, the player dashes in exactly the direction they are looking in.")]
    public float dashDirectionPitchContribution;
    [Tooltip("How long does the dash last for?")]
    public float dashDuration;
    [Tooltip("How long after a dash ends must you wait to use another dash?")]
    public float dashDelayTime;
    [Tooltip("The maximum FOV change when dashing")]
    public float dashMaxFOV;
    [Tooltip("How the FOV changes over time while dashing")]
    public AnimationCurve dashFOVCurve;
    [Tooltip("How much the player can steer while dashing")]
    public float dashSteerSpeed;
    [Tooltip("How much vertical force to apply regardless of pitch.")]
    public float dashVerticalAmount;
}
