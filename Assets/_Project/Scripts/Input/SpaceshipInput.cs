using Fusion;

enum SpaceshipButtons
{
    Fire = 0,
    Accelerate = 1
}

public struct SpaceshipInput : INetworkInput
{
    public float HorizontalInput;
    public float VerticalInput;
    public NetworkButtons Buttons;
}
