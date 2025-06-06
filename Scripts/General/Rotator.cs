using UnityEngine;

public class Rotator : LunarScript
{

    public Vector3 axis;
    public float speed;

    Rigidbody rb;

    private void Start()
    {
        if(rb == null) 
            rb = GetComponent<Rigidbody>();
    }

    public override void LTimestep()
    {
        base.LTimestep();

        rb.MoveRotation(rb.rotation * Quaternion.Euler(axis * speed * Time.fixedDeltaTime));
    }
}
