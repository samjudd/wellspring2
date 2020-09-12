using Godot;

public class SigilOfTeleportation : Spatial
{
  [Export]
  public float _sigilDuration = 5.0f;

  private Timer _sigilTimer;

  public override void _Ready()
  {
    _sigilTimer = GetNode<Timer>("SigilTimer");
    _sigilTimer.Connect("timeout", this, "Timeout");
    if (_sigilDuration > 0.0f)
      _sigilTimer.Start(_sigilDuration);
  }

  private void Timeout()
  {
    // delete from scene tree at end of scene
    QueueFree();
  }
}
