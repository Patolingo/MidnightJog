using UnityEngine;

public class InputManager : PersistentSingleton<InputManager>
{

    private InputSystem_Actions inputActions;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
        }
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }


    public void SubscribeToGameplay(InputSystem_Actions.IPlayerActions callback)
    {
        inputActions.Player.AddCallbacks(callback);
    }
    public void UnsubscribeFromGameplay(InputSystem_Actions.IPlayerActions callback)
    {
        inputActions.Player.RemoveCallbacks(callback);
    }

}
