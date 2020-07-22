using Godot;
using System.Collections.Generic;

public class MeleeCarry1Weapon : KinematicBody
{
  [Export]
  public float _spinVelocityRPS = 1.5f;
  [Export]
  public float _throwVelocity = 15.0f;
  [Export]
  public float _minDetectorWeight = 0.25f;
  public bool isRightWeapon;
  private Vector3 _velocity;
  private bool _processMovement = true;
  private AnimationPlayer _animationPlayer;
  private Area _pickupHitbox;
  private List<RayCast> _detectorList = new List<RayCast>();

  public override void _Ready()
  {
    _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

    // get ref to pickup hitbox and turn it off
    _pickupHitbox = GetNode<Area>("PickupArea");
    _pickupHitbox.Connect("area_entered", this, "PickupCallback");
    _pickupHitbox.Monitorable = false;
    _pickupHitbox.Monitoring = false;

    // Get list of all detector raycasts for later use
    MakeDetectorList(GetNode<Spatial>("Detectors"));
  }

  public override void _PhysicsProcess(float delta)
  {
    if (_processMovement)
    {
      ProcessMovement(delta);
      KinematicCollision collision = MoveAndCollide(_velocity * delta);
      if (collision != null)
        CollisionHandler(collision);
    }
  }

  private void ProcessMovement(float delta)
  {
    // no drag at the moment, do gravity acceleration here
    _velocity += delta * (Vector3)PhysicsServer.AreaGetParam(GetWorld().Space, PhysicsServer.AreaParameter.GravityVector);
    // do rotation here, RotateX doesn't work for some reason but this does... 
    RotateObjectLocal(Vector3.Left, -_spinVelocityRPS * 2 * Mathf.Pi * delta);
  }

  private void CollisionHandler(KinematicCollision collision)
  {
    // stop updating movement
    _processMovement = false;

    // get surface normal
    RayCast normalFinder = GetNode<RayCast>("NormalFinder");
    normalFinder.GlobalTransform = normalFinder.GlobalTransform.LookingAt(collision.Position, Vector3.Up);
    normalFinder.ForceRaycastUpdate();
    Vector3 surfaceNormal = normalFinder.GetCollisionNormal();
    GD.Print(surfaceNormal);

    // need to rotate weapon so +y axis is opposite parallel to surface normal and move it into body to be "stuck in"
    GlobalTransform = Normal2Basis(GlobalTransform, -surfaceNormal);
    Vector3 localDelta = ToLocal(collision.Position) - ToLocal(GetNode<Position3D>("PenetrationPoint").GlobalTransform.origin);
    TranslateObjectLocal(localDelta);

    // turn on pickup hitbox
    _pickupHitbox.Monitoring = true;
    _pickupHitbox.Monitorable = true;

    // visualize collision location
    // MeshInstance debugTeleportLocation = GetNode<MeshInstance>("DebugTeleportLocation");
    // debugTeleportLocation.Visible = true;
    // Transform placeholder = debugTeleportLocation.GlobalTransform;
    // placeholder.origin = collision.Position;
    // debugTeleportLocation.GlobalTransform = placeholder;
  }

  public void Throw(Vector3 globalThrowDirection)
  {
    _velocity += globalThrowDirection * _throwVelocity;
  }

  private void PickupCallback(Area area)
  {
    QueueFree();
  }

  public Vector3 GetTeleportLocation()
  {
    // get weighted sum of all 
    Vector3 resultant = Vector3.Zero;
    Spatial detector = GetNode<Spatial>("Detectors");
    GD.Print("new throw with detector position", detector.GlobalTransform.origin.ToString());
    foreach (RayCast cast in _detectorList)
    {
      cast.ForceRaycastUpdate();
      if (cast.IsColliding())
      {
        // get collision point in local coordinates to detector spatial in middle of sword
        Vector3 collisionPoint = detector.GlobalTransform.XformInv(cast.GetCollisionPoint());
        DrawSphere(collisionPoint);
        GD.Print(cast.Name, " activated with length ", collisionPoint.Length().ToString());
        resultant += collisionPoint.Normalized() * (1 - Mathf.Min(collisionPoint.Length(), _minDetectorWeight));
      }
    }
    // return 1m in opposite direction to weighted average, should be free location
    return detector.Transform.Xform(-resultant.Normalized());
  }

  private void MakeDetectorList(Node node)
  {
    int numChildren = node.GetChildCount();
    for (int i = 0; i < numChildren; i++)
    {
      _detectorList.Add((RayCast)node.GetChild(i));
    }
  }

  private void DrawSphere(Vector3 location)
  {
    MeshInstance sphere = new MeshInstance();
    SphereMesh shape = new SphereMesh();
    GD.Print("sphere id", sphere.GetInstanceId().ToString());
    GetNode<Spatial>("Detectors").AddChild(sphere);
    sphere.Owner = GetNode<Spatial>("Detectors");
    sphere.Transform.Translated(location);
    shape.Radius = 0.05f;
    shape.Height = shape.Radius * 2.0f;
    sphere.Mesh = shape;
    sphere.Visible = true;
  }

  private Transform Normal2Basis(Transform xform, Vector3 normal)
  {
    // cross each unit global basis vector with the normal to get a second perpendicular vector
    Vector3 v2 = Vector3.Zero;
    if (Vector3.Forward.Cross(normal).Length() >= 1e-3)
      v2 = Vector3.Forward.Cross(normal);
    else if (Vector3.Up.Cross(normal).Length() >= 1e-3)
      v2 = Vector3.Up.Cross(normal);
    else if (Vector3.Right.Cross(normal).Length() >= 1e-3)
      v2 = Vector3.Right.Cross(normal);
    else 
      GD.PrintErr("No secondary perpendicular vector could be found, something is wrong.");
    v2 = v2.Normalized();

    // cross the first 2 vectors to get a third
    Vector3 v3 = normal.Cross(v2);

    // make basis with 3 vectors
    xform.basis = new Basis(v3, normal, v2);
    xform.basis = xform.basis.Orthonormalized();
    return xform;
  }
}
