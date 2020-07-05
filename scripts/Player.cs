using Godot;

public class Player : KinematicBody
{
  [Export]
  public float _gravity = -9.8f;
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

  private bool _isSprinting = false;
  // Current velocity in world coordinates
  private Vector3 _vel = new Vector3();
  // Player input direction in world coordinates
  private Vector3 _dir = new Vector3();
  private Camera _camera;
  private AnimationPlayer _animationPlayer;
  private Skeleton _skeleton;
  private int _headBoneIndex;
  private Transform _initialHeadTransform;
  private Camera _debugCamera;
  private bool _jumping = false;
  private bool _isOnFloorLast = true;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    _camera = GetNode<Camera>("Camera");
    _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    _skeleton = GetNode<Skeleton>("Armature/Skeleton");
    _headBoneIndex = _skeleton.FindBone("head");
    _initialHeadTransform = _skeleton.GetBonePose(_headBoneIndex);
    _debugCamera = GetNode<Camera>("../DebugCamera");
    Input.SetMouseMode(Input.MouseMode.Captured);
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

  private void ProcessInput(float delta)
  {
    //  ----------------------- Jumping -----------------------
    if (!_jumping && Input.IsActionJustPressed("movement_jump"))
    {
      _animationPlayer.Play("jump", 0.1f, 3.0f);
      _jumping = true;
    }
    
    //  ----------------------- Walking -----------------------
    Vector2 inputMovementVector = new Vector2();

    if (Input.IsActionPressed("movement_forward"))
      inputMovementVector.y += 1;
    if (Input.IsActionPressed("movement_backward"))
      inputMovementVector.y += -1;
    if (Input.IsActionPressed("movement_left"))
      inputMovementVector.x += -1;
    if (Input.IsActionPressed("movement_right"))
      inputMovementVector.x += 1;

    // if you're jumping ignore directional input
    if (!_jumping)
    {
      SetAnimation(inputMovementVector);
      inputMovementVector = inputMovementVector.Normalized();
    }
    else
    {
      inputMovementVector = Vector2.Zero;
    }

    _dir = new Vector3();
    Transform camXform = _camera.GlobalTransform;
    // Basis vectors are already normalized.
    _dir += -camXform.basis.z * inputMovementVector.y;
    _dir += camXform.basis.x * inputMovementVector.x;

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
    if (_dir.Dot(hvel) > 0.0f)
      if (_isSprinting)
        accel = _sprintAccel;
      else
        accel = _accel;
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
      _camera.RotateX(Mathf.Deg2Rad(-mouseEvent.Relative.y * _mouseSensitivity));
      RotateY(Mathf.Deg2Rad(-mouseEvent.Relative.x * _mouseSensitivity));

      Vector3 cameraRot = _camera.RotationDegrees;
      cameraRot.x = Mathf.Clamp(cameraRot.x, -70, 70);
      _camera.RotationDegrees = cameraRot;
    }
  }

  // plays correct animation 
  // !(_animationPlayer.CurrentAnimation == "running_front" && _animationPlayer.GetPlayingSpeed() > 0.0f) means 
  // if animation is playing the same one and is in the same play direction don't play it again
  private void SetAnimation(Vector2 inputMovementVector)
  {
    if (inputMovementVector == new Vector2(0, 0) && _animationPlayer.CurrentAnimation != "idle")
      _animationPlayer.Play("idle", 0.1f);
    else if (inputMovementVector == new Vector2(0, 1) && !(_animationPlayer.CurrentAnimation == "running_front" && _animationPlayer.GetPlayingSpeed() > 0.0f))
      _animationPlayer.Play("running_front", 0.1f);
    else if (inputMovementVector == new Vector2(-1, 1) && !(_animationPlayer.CurrentAnimation != "running_frontleft" && _animationPlayer.GetPlayingSpeed() > 0.0f))
      _animationPlayer.Play("running_frontleft", 0.1f);
    else if (inputMovementVector == new Vector2(1, 1) && !(_animationPlayer.CurrentAnimation != "running_frontright" && _animationPlayer.GetPlayingSpeed() > 0.0f))
      _animationPlayer.Play("running_frontright", 0.1f);
    else if (inputMovementVector == new Vector2(-1, 0) && _animationPlayer.CurrentAnimation != "running_left")
      _animationPlayer.Play("running_left", 0.1f);
    else if (inputMovementVector == new Vector2(1, 0) && _animationPlayer.CurrentAnimation != "running_right")
      _animationPlayer.Play("running_right", 0.1f);
    else if (inputMovementVector == new Vector2(0, -1) && !(_animationPlayer.CurrentAnimation != "running_front" && _animationPlayer.GetPlayingSpeed() < 0.0f))
      _animationPlayer.PlayBackwards("running_front", 0.1f);
    else if (inputMovementVector == new Vector2(-1, -1) && !(_animationPlayer.CurrentAnimation != "running_frontright" && _animationPlayer.GetPlayingSpeed() < 0.0f))
      _animationPlayer.PlayBackwards("running_frontright", 0.1f);
    else if (inputMovementVector == new Vector2(1, -1) && !(_animationPlayer.CurrentAnimation != "running_frontright" && _animationPlayer.GetPlayingSpeed() < 0.0f))
      _animationPlayer.PlayBackwards("running_frontleft", 0.1f);
  }

  private void JumpCallback()
  {
    _vel.y = _jumpSpeed;
  }
}
