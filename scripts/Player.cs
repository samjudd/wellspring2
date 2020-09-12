using Godot;

public class Player : KinematicBody
{
  [Export]
  public float _maxSpeed = 5.0f;
  [Export]
  public float _jumpSpeed = 6.0f;
  [Export]
  public float _accel = 8.0f;
  [Export]
  public float _deaccel = 12.0f;
  [Export]
  public float _maxSlopeAngle = 40.0f;
  [Export]
  public float _mouseSensitivity = 0.05f;
  [Export]
  public float _maxSprintSpeed = 8.0f;
  [Export]
  public float _sprintAccel = 12.0f;
  [Export]
  public bool _lockXRotation = false;
  [Export]
  public bool _lockYRotation = false;
  [Export]
  public bool _lockXMovement = false;
  [Export]
  public bool _lockZMovement = false;
  [Export]
  public float _fieldOfView = 85.0f;
  [Export]
  public float _ability1Cooldown = 10.0f;
  [Export]
  public float _ability2Cooldown = 10.0f;
  [Export]
  public float _ability3Cooldown = 10.0f;
  [Export]
  public float _ability4Cooldown = 10.0f;


  // Current velocity in world coordinates
  protected Vector3 _vel = new Vector3(0, 0, 0);
  protected AnimationTree _animationTree;
  protected Camera _camera;
  protected Ability _ability1;
  protected Ability _ability2;
  protected Ability _ability3;
  protected Ability _ability4;
  protected float _health = 100.0f;
  protected float _energy = 200.0f;

  // Player input direction in global coordinates
  private Vector3 _gravity;
  private Vector3 _dir = new Vector3();
  private Skeleton _skeleton;
  private bool _isSprinting = false;
  private int _headBoneIndex;
  private Transform _initialHeadTransform;
  private Camera _debugCamera;
  private bool _jumping = false;
  private bool _isOnFloorLast = true;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _camera = GetNode<Camera>("Camera");
    _animationTree = GetNode<AnimationTree>("AnimationTree");
    _skeleton = GetNode<Skeleton>("Armature/Skeleton");
    _headBoneIndex = _skeleton.FindBone("head");
    _initialHeadTransform = _skeleton.GetBonePose(_headBoneIndex);
    _debugCamera = GetNode<Camera>("../DebugCamera");
    Input.SetMouseMode(Input.MouseMode.Captured);

    // activate animationTree in case it's not
    _animationTree.Active = true;

    // define gravity from params in editor
    _gravity = (Vector3)PhysicsServer.AreaGetParam(GetWorld().Space, PhysicsServer.AreaParameter.GravityVector) * (float)PhysicsServer.AreaGetParam(GetWorld().Space, PhysicsServer.AreaParameter.Gravity);
  }

  public override void _Process(float delta)
  {
    // rotate head and arms with camera
    Transform currentHeadTransform = _initialHeadTransform.Rotated(Vector3.Right, _camera.Rotation.x);
    _skeleton.SetBonePose(_headBoneIndex, currentHeadTransform);

    // set camera position to same as head bone position 
    _camera.Translation = _skeleton.GetBoneGlobalPose(_headBoneIndex).origin;
  }

  public override void _PhysicsProcess(float delta)
  {
    ProcessStateUpdates(delta);
    ProcessInput(delta);
    ProcessMovement(delta);
  }

  private void ProcessStateUpdates(float delta)
  {
    if (_isOnFloorLast == false && IsOnFloor())
    {
      _jumping = false;
    }
    _isOnFloorLast = IsOnFloor();
  }

  protected virtual void ProcessInput(float delta)
  {
    //  ----------------------- Jumping -----------------------
    if (!_jumping && Input.IsActionJustPressed("movement_jump"))
    {
      _animationTree.Set("parameters/jump_os/active", true);
      _jumping = true;
    }

    //  ----------------------- Walking -----------------------
    Vector2 inputMovementVector = new Vector2();

    if (Input.IsActionPressed("movement_forward") && !_lockZMovement)
      inputMovementVector.y += 1;
    if (Input.IsActionPressed("movement_backward") && !_lockZMovement)
      inputMovementVector.y += -1;
    if (Input.IsActionPressed("movement_left") && !_lockXMovement)
      inputMovementVector.x += 1;
    if (Input.IsActionPressed("movement_right") && !_lockXMovement)
      inputMovementVector.x += -1;

    // if you're jumping ignore directional input
    if (!_jumping)
    {
      // set running animation
      _animationTree.Set("parameters/run_bs2d/blend_position", inputMovementVector);
      inputMovementVector = inputMovementVector.Normalized();
    }
    else
    {
      inputMovementVector = Vector2.Zero;
    }
    // convert local movement vectors to global movement vectors
    _dir = Vector3.Zero;
    _dir += GlobalTransform.basis.x * inputMovementVector.x;
    _dir += GlobalTransform.basis.z * inputMovementVector.y;

    //  ----------------------- Sprinting ----------------------- 
    _isSprinting = Input.IsActionPressed("movement_sprint") && !_jumping;

    // -------------- Capturing/Freeing the cursor -------------- 
    if (Input.IsActionJustPressed("ui_cancel"))
    {
      if (Input.GetMouseMode() == Input.MouseMode.Visible)
        Input.SetMouseMode(Input.MouseMode.Captured);
      else
        Input.SetMouseMode(Input.MouseMode.Visible);
    }
    // ---------------- Change Camera for Debug ----------------
    if (Input.IsActionJustPressed("debug_swap_camera"))
    {
      if (_camera.Current)
        _debugCamera.Current = true;
      else if (_debugCamera.Current)
        _camera.Current = true;
    }
  }

  private void ProcessMovement(float delta)
  {
    // normalize player input direction
    _dir = _dir.Normalized();
    _vel += delta * _gravity;

    Vector3 hvel = _vel;
    hvel.y = 0;

    Vector3 target = _dir;

    if (_isSprinting)
      target *= _maxSprintSpeed;
    else
      target *= _maxSpeed;

    float accel;
    if (_dir.Dot(hvel) > 0.0f)
    {
      if (_isSprinting)
        accel = _sprintAccel;
      else
        accel = _accel;
    }
    else
      accel = _deaccel;

    if (_jumping)
      accel = _deaccel / 50.0f;

    hvel = hvel.LinearInterpolate(target, accel * delta);
    _vel.x = hvel.x;
    _vel.z = hvel.z;
    _vel = MoveAndSlide(_vel, Vector3.Up, false, 4, Mathf.Deg2Rad(_maxSlopeAngle));
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
    {
      InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
      if (!_lockYRotation)
        RotateY(Mathf.Deg2Rad(-mouseEvent.Relative.x * _mouseSensitivity));

      if (!_lockXRotation)
      {
        _camera.RotateX(Mathf.Deg2Rad(mouseEvent.Relative.y * _mouseSensitivity));
        Vector3 cameraRot = _camera.RotationDegrees;
        cameraRot.x = Mathf.Clamp(cameraRot.x, -_fieldOfView, _fieldOfView);
        _camera.RotationDegrees = cameraRot;
      }
    }
  }

  protected virtual void JumpCallback() { }
}
