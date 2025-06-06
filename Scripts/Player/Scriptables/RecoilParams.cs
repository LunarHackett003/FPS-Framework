using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Recoil Params")]
public class RecoilParams : ScriptableObject
{
    public AnimationCurve recoilMultiplierCurve = AnimationCurve.Linear(0, 1, 1, 0);
    public float maxRecoilShots = 10;
    public float recoilShotClearTime = 1;
    public float recoilDecay = 5;
    public Vector3 minLinearRecoil, maxLinearRecoil;
    public Vector3 minAngularRecoil, maxAngularRecoil;
    public Vector3 aimedLinearRecoilScale;
    public Vector3 aimedAngularRecoilScale;
    public float recoilSnappiness = 5;
    public float recoilSpeed = 5;

    public float tempAimPitchPerShot = 0.1f;
    public float permanentAimPitchPerShot = 0.02f;

    public float tempAimPitchDecay = 2f;
    public float tempAimPitchSnappiness = 2f;

    public bool chargeAffectsRecoil = false;
    public AnimationCurve recoilChargeInfluence;
}
