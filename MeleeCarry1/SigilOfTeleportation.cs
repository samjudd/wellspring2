using Godot;
using System;

public class SigilOfTeleportation : Spatial
{
  [Export]
  public float _sigilTime = 5.0f; // better name plz.

  private Timer _sigilTimer;

    public override void _Ready()
  {
    _sigilTimer = GetNode<Timer>("SigilTimer");
    _sigilTimer.Connect("timeout", this, "Timeout"); // is it possible to not have to handle these as strings. Maybe get handles to the function.
    if (_sigilTime > 0.0f)  _sigilTimer.Start(_sigilTime);
  }

  private void Timeout()
  {
    QueueFree(); // What is this?
  }
}
