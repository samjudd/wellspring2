using Godot;
using System;

public class SigilOfTeleportation : Spatial
{
  [Export]
  public float _sigilTime = 5.0f;

  private Timer _sigilTimer;

    public override void _Ready()
  {
    _sigilTimer = GetNode<Timer>("SigilTimer");
    _sigilTimer.Connect("timeout", this, "Timeout");
    _sigilTimer.Start(_sigilTime);
  }

  private void Timeout()
  {
    QueueFree();
  }
}
