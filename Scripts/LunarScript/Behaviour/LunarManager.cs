using UnityEngine;

public class LunarManager : MonoBehaviour
{
    public static LunarManager Instance { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void IntialiseOnLoad()
    {
        Instance = new GameObject("Lunar Manager").AddComponent<LunarManager>();
        DontDestroyOnLoad(Instance.gameObject);
    }

    public delegate void LUpdate(); 
    public delegate void LTimeStep();
    public delegate void LPostUpdate();

    public LUpdate lUpdate;
    public LPostUpdate lPostUpdate;
    public LTimeStep lTimeStep;

    // Update is called once per frame
    private void Update()
    {
        lUpdate?.Invoke();
    }
    private void FixedUpdate()
    {
        lTimeStep?.Invoke();
    }
    private void LateUpdate()
    {
        lPostUpdate?.Invoke();
    }
}
