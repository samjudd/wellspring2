using Godot;

public class Enemy : KinematicBody
{
  public Position3D _markLocation;

  public override void _Ready()
  {
    _markLocation = GetNode<Position3D>("MarkLocation");
  }

  public Vector3 GetTeleportLocation()
  {
    // teleport 1m behind the origin of the enemy
    return ToGlobal(Vector3.Forward * 1.5f);
  }
}
