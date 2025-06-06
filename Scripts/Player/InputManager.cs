using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SingletonBehaviour<InputManager>
{
    public PlayerInput playerInput;
    public PauseMenu pauseMenu;
    public GameplayCanvas gameplayCanvas;

    private Vector2 lookInput;
    private bool crouchInput;
    private Vector2 moveInput;
    private bool fireInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool aimInput;
    private bool fireSwitchInput;
    private bool slowWalkInput;
    private bool dashInput;
    private bool reloadInput;
    private Vector2 weaponSwitchInput;
    private bool interactInput;
    private bool grabInput;

    public static Vector2 MoveInput { get => PauseMenu.GamePaused ? Vector2.zero : Instance.moveInput; set => Instance.moveInput = value; }
    public static Vector2 LookInput { get => PauseMenu.GamePaused ? Vector2.zero : Instance.lookInput; set => Instance.lookInput = value; }
    public static bool PrimaryInput { get => Instance.fireInput && !PauseMenu.GamePaused; set => Instance.fireInput = value; }
    public static bool JumpInput { get => Instance.jumpInput && !PauseMenu.GamePaused; set => Instance.jumpInput = value; }
    public static bool SprintInput { get => Instance.sprintInput && !PauseMenu.GamePaused; set => Instance.sprintInput = value; }
    public static bool CrouchInput { get => Instance.crouchInput; set => Instance.crouchInput = value; }
    public static bool SecondaryInput { get => Instance.aimInput && !PauseMenu.GamePaused; set => Instance.aimInput = value; }
    public static bool AltAimInput { get => Instance.fireSwitchInput; set => Instance.fireSwitchInput = value; }
    public static bool SlowWalkInput { get => Instance.slowWalkInput && !PauseMenu.GamePaused; set => Instance.slowWalkInput = value; }
    public static bool DashInput { get => Instance.dashInput && !PauseMenu.GamePaused; set => Instance.dashInput = value; }
    public static bool ReloadInput { get => Instance.reloadInput && !PauseMenu.GamePaused; set => Instance.reloadInput = value; }
    public static bool FireSwitchInput { get => Instance.fireSwitchInput && !PauseMenu.GamePaused; set => Instance.fireSwitchInput = value; }
    public static Vector2 WeaponSwitchInput { get => PauseMenu.GamePaused ? Vector2.zero : Instance.weaponSwitchInput; set => Instance.weaponSwitchInput = value; }
    public static bool InteractInput { get => Instance.interactInput && !PauseMenu.GamePaused; set => Instance.interactInput = value; }
    public static bool GrabInput { get => Instance.grabInput && !PauseMenu.GamePaused; set => Instance.grabInput = value; }

    public bool gamepadCrouchToggle = true;
    public bool altAimToggle = true;

    #region Input
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void GetLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    public void GetFireInput(InputAction.CallbackContext context)
    {
        fireInput = context.ReadValueAsButton();
    }
    public void GetJumpInput(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValueAsButton();
    }
    public void GetSprintInput(InputAction.CallbackContext context)
    {
        sprintInput = context.ReadValueAsButton();
    }
    public void GetCrouchInput(InputAction.CallbackContext context)
    {
        crouchInput = context.ReadValueAsButton();
    }
    public void GetAimInput(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValueAsButton();
    }
    public void GetCrouchToggleInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            crouchInput = !crouchInput;
    }
    public void GetFireSwitchInput(InputAction.CallbackContext context)
    {
        fireSwitchInput = context.ReadValueAsButton();
    }
    public void GetSlowWalkToggleInput(InputAction.CallbackContext context)
    {
        if(context.performed)
            slowWalkInput = !slowWalkInput;
    }
    public void GetSlowWalkInput(InputAction.CallbackContext context)
    {
        slowWalkInput = context.ReadValueAsButton();
    }
    public void GetCrouchGamepad(InputAction.CallbackContext context)
    {
        if (gamepadCrouchToggle)
        {
            if (context.performed)
                crouchInput = !crouchInput;
        }
        else
        {
            crouchInput = context.ReadValueAsButton();
        }
    }

    public void GetDashInput(InputAction.CallbackContext context)
    {
        dashInput = context.ReadValueAsButton();
    }

    public void GetPauseInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            pauseMenu.TogglePause();
    }

    public void GetReloadInput(InputAction.CallbackContext context)
    {
        reloadInput = context.ReadValueAsButton();
    }
    public void GetWeaponSwitchInput(InputAction.CallbackContext context)
    {
        weaponSwitchInput = context.ReadValue<Vector2>();
    }
    public void GetIneractInput(InputAction.CallbackContext context)
    {
        interactInput = context.ReadValueAsButton();
    }
    public void GetGrabInput(InputAction.CallbackContext context)
    {
        grabInput = context.ReadValueAsButton();
    }
    #endregion

    private void Awake()
    {
        if(Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Debug.Log("Input Manager Destroyed!");
            Destroy(gameObject);
            return;
        }
        Debug.Log("Input Manager Created!");
        pauseMenu.SetPaused(true);
        gameplayCanvas.Initialise();

    }
    public void PauseGame(bool paused)
    {
        playerInput.DeactivateInput();
    }
}
