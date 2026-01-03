using UnityEngine.InputSystem;
using static PlayerInput;
using UnityEngine.Events;
using UnityEngine;

namespace LifeHMA.Player
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "LifeHMA/Input/InputReader")]
    public class InputReader : ScriptableObject, PlayerInput.IPlayerActions
    {
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction Interact = delegate { };

        private PlayerInput inputActions;

        public Vector3 Direction => inputActions.Player.Movement.ReadValue<Vector2>();

        private void OnEnable()
        {
            if(inputActions == null)
            {
                inputActions = new PlayerInput();
                inputActions.Player.SetCallbacks(this);
            }
            inputActions.Enable();
        }
        private void OnDisable()
        {
            inputActions.Disable();
        }

        public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Jump.Invoke(true);
            }
            if (context.canceled)
            {
                Jump.Invoke(false);
            }
        }

        public void OnMovement(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            //----
        }

        public void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Interact.Invoke();
            }
        }
    }
}
