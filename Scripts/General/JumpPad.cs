using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class JumpPad : LunarNetScript
{
    public VisualEffect[] vfx;

    public float jumpForce;

    public Transform direction;

    public bool setVelocity;

    private void OnTriggerEnter(Collider other)
    {
        if(other.attachedRigidbody != null)
        {
            if (setVelocity)
            {
                other.attachedRigidbody.velocity = direction.up * jumpForce;
            }
            else
            {
                other.attachedRigidbody.AddForce(direction.up * jumpForce, ForceMode.Impulse);
            }
        }
    }
    [Rpc(SendTo.Everyone)]
    public void PlayVFX_RPC()
    {
        vfx.PlayVFX(true);
    }

    private void OnDrawGizmos()
    {

        Vector3 velocity = direction.up * jumpForce;
        Vector3 pos = direction.position;
        for (int i = 0; i < 300; i++)
        {
            velocity += Time.fixedDeltaTime * Physics.gravity;
            pos += velocity * Time.fixedDeltaTime;
            Gizmos.DrawRay(pos, velocity * Time.fixedDeltaTime);
        }
    }

}
