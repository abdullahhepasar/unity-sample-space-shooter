using UnityEngine;
using Fusion;

public class SpaceshipMovementController : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float movementSpeed = 2000f;
    [SerializeField] private float maxSpeed = 200f;
    
    private Rigidbody rigidbody = null;
    private SpaceshipController SpaceshipController = null;
    
    [Networked] private float screenBoundaryX {get; set;}
    [Networked] private float screenBoundaryY {get; set;}

    [Networked] private NetworkButtons buttonsPrev { get; set; }

    public override void Spawned()
    {
        rigidbody = GetComponent<Rigidbody>();
        SpaceshipController = GetComponent<SpaceshipController>();

        if (Object.HasStateAuthority == false) return;
        
        screenBoundaryX = Camera.main.orthographicSize * Camera.main.aspect;
        screenBoundaryY = Camera.main.orthographicSize;
    }
    
    public override void FixedUpdateNetwork()
    {
        if (SpaceshipController.AcceptInput == false) return;

        if (Runner.TryGetInputForPlayer<SpaceshipInput>(Object.InputAuthority, out var input))
            Move(input, false);

        if (GetInput<SpaceshipInput>(out var inputButton))
        {
            var pressed = inputButton.Buttons.GetPressed(buttonsPrev);
            if (pressed.WasPressed(buttonsPrev, SpaceshipButtons.Accelerate))
                Move(input, true);
        }

        CheckExitScreen();
    }

    private void Move(SpaceshipInput input, bool accelerate)
    {
        Quaternion rot = rigidbody.rotation * Quaternion.Euler(0, input.HorizontalInput * rotationSpeed * Runner.DeltaTime, 0);
        rigidbody.MoveRotation(rot);

        Vector3 force = (rot * Vector3.forward) * (accelerate ? 1f : input.VerticalInput) * movementSpeed * Runner.DeltaTime;
        rigidbody.AddForce(force);

        if (rigidbody.velocity.magnitude > maxSpeed)
        {
            rigidbody.velocity = rigidbody.velocity.normalized * maxSpeed;
        }
    }
    
    private void CheckExitScreen()
    {
        var position = rigidbody.position;

        if (Mathf.Abs(position.x) < screenBoundaryX && Mathf.Abs(position.z) < screenBoundaryY) return;
        
        if (Mathf.Abs(position.x) > screenBoundaryX)
            position = new Vector3(-Mathf.Sign(position.x) * screenBoundaryX, 0, position.z);

        if (Mathf.Abs(position.z) > screenBoundaryY)
            position = new Vector3(position.x, 0, -Mathf.Sign(position.z) * screenBoundaryY);

        position -= position.normalized * 0.1f;
        GetComponent<NetworkRigidbody>().TeleportToPosition(position);
    }
}
