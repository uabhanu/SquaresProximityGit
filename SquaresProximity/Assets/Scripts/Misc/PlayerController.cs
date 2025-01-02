namespace Misc
{
    using Event = Managers.Event;
    using Managers;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float mouseThreshold;

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL

                if(Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    ProcessEvent(Event.KeyboardTabPressed);
                }

                if(Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if(EventSystem.current.IsPointerOverGameObject()) return;

                    ProcessEvent(Event.MouseLeftClicked);
                }

                if(Mouse.current.delta.ReadValue().magnitude > mouseThreshold)
                {
                    ProcessEvent(Event.MouseMoved);
                }

                if(Gamepad.current != null)
                {
                    if(Gamepad.current.buttonSouth.wasPressedThisFrame) ProcessEvent(Event.JoystickXPressed);
                    if(Gamepad.current.dpad.down.wasPressedThisFrame) ProcessEvent(Event.JoystickDownPressed);
                    if(Gamepad.current.dpad.left.wasPressedThisFrame) ProcessEvent(Event.JoystickLeftPressed);
                    if(Gamepad.current.dpad.right.wasPressedThisFrame) ProcessEvent(Event.JoystickRightPressed);
                    if(Gamepad.current.dpad.up.wasPressedThisFrame) ProcessEvent(Event.JoystickUpPressed);
                }

            #endif

            #if UNITY_IOS || UNITY_ANDROID

                if(Touchscreen.current.primaryTouch.press.isPressed)
                {
                    Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                    ProcessEvent(Event.TouchscreenTapped , touchPosition);
                }

            #endif
        }

        private bool IsLocalPlayerTurn()
        {
            // Implement turn-checking logic
            return true;
        }

        private void ProcessEvent(Event eventType , Vector2? touchPosition = null)
        {
            if(touchPosition.HasValue)
            {
                EventsManager.Invoke(eventType , touchPosition.Value);
            }
            else
            {
                EventsManager.Invoke(eventType);
            }
        }

        private void SendRPC(Event eventType , Vector2? touchPosition = null)
        {
            Debug.Log($"Sending RPC for event: {eventType} , position: {touchPosition}");
            // TODO Implement RPC calls here
        }
    }
}