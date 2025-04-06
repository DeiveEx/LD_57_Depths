using UnityEngine;
using UnityEngine.InputSystem;

public struct InputState
{
    public Vector2 MoveInput;
    public bool HasJumped;
    public bool IsDiggingForward;
    public bool IsDiggingDown;
    public bool IsDiggingUp;
    public bool HasInteracted;
}

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerController _controller;

    private Vector2 _lastMoveInput;
    private InputState _currentInputState;

    private void Update()
    {
        _controller.SetInput(_currentInputState);
        ResetInputState();
    }

    public void OnMove(InputValue value)
    {
        _lastMoveInput = value.Get<Vector2>();
    }
    
    public void OnDigForward(InputValue value)
    {
        _currentInputState.IsDiggingForward = value.Get<float>() > 0;
    }
    
    public void OnDigDown(InputValue value)
    {
        _currentInputState.IsDiggingDown = value.Get<float>() > 0;
    }
    
    public void OnDigUp(InputValue value)
    {
        _currentInputState.IsDiggingUp = value.Get<float>() > 0;
    }
    
    public void OnInteract(InputValue value)
    {
        _currentInputState.HasInteracted = true;
    }
    
    public void OnJump(InputValue value)
    {
        _currentInputState.HasJumped = true;
    }

    private void ResetInputState()
    {
        _currentInputState.MoveInput = _lastMoveInput;
        _currentInputState.HasJumped = false;
        _currentInputState.HasInteracted = false;
    }
}
