using Godot;

public class Util
{
  public enum CollisionLayers
  {
    ENVIRONMENT,
    PLAYER,
    FRIENDLY,
    ENEMY,
    INTERACTIVE,
    PLAYER_ATTACK,
    FRIENDLY_ATTACK,
    ENEMY_ATTACK,
    GROUND
  }

  public static Transform Normal2Basis(Transform xform, Vector3 normal)
  {
    // cross each unit global basis vector with the normal to get a second vector perpendicular to the normal vector
    Vector3 v2 = Vector3.Zero;
    if (Vector3.Forward.Cross(normal).Length() >= 1e-3)
      v2 = Vector3.Forward.Cross(normal);
    else if (Vector3.Up.Cross(normal).Length() >= 1e-3)
      v2 = Vector3.Up.Cross(normal);
    else if (Vector3.Right.Cross(normal).Length() >= 1e-3)
      v2 = Vector3.Right.Cross(normal);
    else
    {
      GD.PrintErr("No secondary perpendicular vector could be found, something is wrong.");
      // need to return some sort of thing that will throw an error 
    }
    v2 = v2.Normalized();

    // cross the input normal vector with our calculated vector above to get a third vector perpendicular to both 
    Vector3 v3 = normal.Cross(v2);

    // we can then create an orthonormal basis with the 3 vectors that has a y basis component parallel ot the normal vector
    xform.basis = new Basis(v3, normal, v2);
    xform.basis = xform.basis.Orthonormalized();
    return xform;
  }

  public static void DrawSphere(Vector3 globalLocation, Spatial parent, float diameter = 0.05f)
  {
    MeshInstance sphere = new MeshInstance();
    SphereMesh shape = new SphereMesh();
    parent.AddChild(sphere);
    Transform placeholder = sphere.GlobalTransform;
    placeholder.origin = globalLocation;
    sphere.GlobalTransform = placeholder;
    shape.Radius = diameter / 2.0f;
    shape.Height = diameter;
    sphere.Mesh = shape;
    sphere.Visible = true;
  }
}