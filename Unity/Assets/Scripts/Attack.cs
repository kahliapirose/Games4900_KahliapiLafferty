using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    private InputSystem_Actions input;

    private void Awake() => input = new InputSystem_Actions();

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Attack.performed += AttackPressed;
    }

    private void OnDisable()
    {
        input.Player.Attack.performed -= AttackPressed;
        input.Player.Disable();
    }

    private void AttackPressed(InputAction.CallbackContext _) => Debug.Log("Attack!!");
}