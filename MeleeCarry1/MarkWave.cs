using Godot;

public class MarkWave : KinematicBody
{
  [Export]
  public float _waveVelocity = 15.0f;

  private Vector3 _velocity;
  private PackedScene _sigilOfTeleportationPS;

  public override void _Ready()
  {
    _sigilOfTeleportationPS = (PackedScene)ResourceLoader.Load("res://MeleeCarry1/SigilOfTeleportation.tscn");
    GetNode<Area>("MarkWave/MarkWave").Connect("body_entered", this, "MarkEnemy");
  }

  public override void _PhysicsProcess(float delta)
  {
    KinematicCollision collision = MoveAndCollide(_velocity * delta);
  }

  public void Cast(Vector3 direction)
  {
    _velocity = direction * _waveVelocity;
    GetNode<AnimationPlayer>("AnimationPlayer").Play("expand");
  }

  private void Dissipate()
  {
    QueueFree();
  }

  private void MarkEnemy(Node body)
  {
    if (body is Enemy enemy && !enemy.HasNode("SigilOfTeleportation"))
    {
      // instance sigil 
      Spatial sigilOfTeleportation = (Spatial)_sigilOfTeleportationPS.Instance();
      // add sigil as child of enemy
      enemy.AddChild(sigilOfTeleportation);
      // set transform
      sigilOfTeleportation.Transform = enemy._markLocation.Transform;
    }
  }
}
