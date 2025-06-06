using UnityEngine;

public class CameraLayerSetter : MonoBehaviour
{
    public Camera cam;

    public LayerMask mask;


    private void OnValidate()
    {
        if(cam == null)
            cam = GetComponent<Camera>();
    }


    private void Awake()
    {
        if (cam)
        {
            cam.cullingMask = mask;
        }
    }
}
