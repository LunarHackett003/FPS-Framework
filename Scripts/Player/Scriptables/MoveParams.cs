using UnityEngine;

[CreateAssetMenu(fileName = "MoveParams", menuName = "Scriptable Objects/MoveParams")]
public class MoveParams : ScriptableObject
{
     [Header("Movement On Foot")] public float baseMoveForce = 15;
     public float walkDamping = 8;
     public float sprintForceMultiply = 1.5f;
     public float slowWalkForceMultiply = 0.7f;
     public float aimWalkMoveMultiply = 0.7f;
     public float groundPushForce = 2;

     [Header("Movement On Foot - Crouching")]
     public float sideAimWalkMoveMultiply = 0.5f;
     public float crouchWalkForceMultiply = 0.8f;
     public float crouchedCapsuleHeight = 1.2f;
     public float standingCapsuleHeight = 1.9f;
     public float crouchHeadSpeed = 5;
     public Vector3 crouchedCapsuleCentre = new(0, -0.4f, 0), standingCapsuleCentre = Vector3.zero;
     public float crouchObstructionDistance = 0.5f;
     public float crouchObstructionVerticalOffset = -0.2f;
     public Vector3 uncrouchCheckPosition;
     public bool canUncrouch;

     [Header("Movement On Foot - Platforms")] public bool followPlatformPosition;
     public bool followPlatformRotation;

     [Header("Movement On Foot - Sliding")]
     public float slideDamping = 0.1f;
     public float slidePushOffForce = 15f;
     public float slideSteerForce = 2f;

     [Header("Movement In Air")] public float jumpForce = 7f;
     public float airborneDamping = 0.02f;
     public float airMoveForce = 2f;
     [Header("Movement In Air - Extras")] public bool canMultiJump = false;
    public int multiJumps = 1;

    public float spectateMoveSpeed = 5, spectateFastMoveMultiplier = 2;
}
