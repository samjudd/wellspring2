using Godot;

public class Player : KinematicBody
{
  [Export]
  public float _gravity = -9.8f;
  [Export]
  public float _maxSpeed = 5.0f;
  [Export]
  public float _jumpSpeed = 4.0f;
  [Export]
  public float _accel = 2.0f;
  [Export]
  public float _deaccel = 8.0f;
  [Export]
  public float _maxSlopeAngle = 40.0f;
  [Export]
  public float _mouseSensitivity = 0.05f;
  [Export]
  public float _maxSprintSpeed = 8.0f;
  [Export]
  public float _sprintAccel = 10.0f;
  private bool _isSprinting = false;
  private Vector3 _vel = new Vector3();
  private Vector3 _dir = new Vector3();
  private Camera _camera;
  private Spatial _rotationHelper;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _camera = GetNode<Camera>("RotationHelper/Camera");
    _rotationHelper = GetNode<Spatial>("RotationHelper");

    Input.SetMouseMode(Input.MouseMode.Captured);
  }

  public override void _PhysicsProcess(float delta)
  {
    ProcessInput(delta);
    ProcessMovement(delta);
  }

  private void ProcessInput(float delta)
  {
    //  -------------------------------------------------------------------
    //  Walking
    _dir = new Vector3();
    Transform camXform = _camera.GlobalTransform;

    Vector2 inputMovementVector = new Vector2();

    if (Input.IsActionPressed("movement_forward"))
      inputMovementVector.y += 1;
    if (Input.IsActionPressed("movement_backward"))
      inputMovementVector.y -= 1;
    if (Input.IsActionPressed("movement_left"))
      inputMovementVector.x -= 1;
    if (Input.IsActionPressed("movement_right"))
      inputMovementVector.x += 1;

    inputMovementVector = inputMovementVector.Normalized();

    // Basis vectors are already normalized.
    _dir += -camXform.basis.z * inputMovementVector.y;
    _dir += camXform.basis.x * inputMovementVector.x;
    //  -------------------------------------------------------------------

    //  -------------------------------------------------------------------
    //  Jumping
    if (IsOnFloor())
    {
      if (Input.IsActionJustPressed("movement_jump"))
        _vel.y = _jumpSpeed;
    }
    //  -------------------------------------------------------------------

    //  -------------------------------------------------------------------
    //  Sprinting
    if (Input.IsActionPressed("movement_sprint") && IsOnFloor())
      _isSprinting = true;
    else
      _isSprinting = false;
    //  -------------------------------------------------------------------

    //  -------------------------------------------------------------------
    //  Capturing/Freeing the cursor
    if (Input.IsActionJustPressed("ui_cancel"))
    {
      if (Input.GetMouseMode() == Input.MouseMode.Visible)
        Input.SetMouseMode(Input.MouseMode.Captured);
      else
        Input.SetMouseMode(Input.MouseMode.Visible);
    }
    //  -------------------------------------------------------------------
  }

  private void ProcessMovement(float delta)
  {
    _dir.y = 0;
    _dir = _dir.Normalized();

    _vel.y += delta * _gravity;

    Vector3 hvel = _vel;
    hvel.y = 0;

    Vector3 target = _dir;

    if (_isSprinting)
      target *= _maxSprintSpeed;
    else
      target *= _maxSpeed;

    float accel;
    if (_dir.Dot(hvel) > 0)
      if (_isSprinting)
        accel = _sprintAccel;
      else
        accel = _accel;
    else
      accel = _deaccel;

    hvel = hvel.LinearInterpolate(target, accel * delta);
    _vel.x = hvel.x;
    _vel.z = hvel.z;
    _vel = MoveAndSlide(_vel, new Vector3(0, 1, 0), false, 4, Mathf.Deg2Rad(_maxSlopeAngle));
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
    {
      InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
      _rotationHelper.RotateX(Mathf.Deg2Rad(mouseEvent.Relative.y * _mouseSensitivity));
      RotateY(Mathf.Deg2Rad(-mouseEvent.Relative.x * _mouseSensitivity));

      Vector3 cameraRot = _rotationHelper.RotationDegrees;
      cameraRot.x = Mathf.Clamp(cameraRot.x, -70, 70);
      _rotationHelper.RotationDegrees = cameraRot;
    }
  }
}
