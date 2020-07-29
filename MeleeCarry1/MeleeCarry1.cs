using Godot;

public class MeleeCarry1 : Player
{
  private PackedScene _weaponPS;
  private Position3D _leftSwordSpawn;
  private Position3D _rightSwordSpawn;
  private AnimationNodeStateMachinePlayback _attackSM;
  private Timer _attackTimer;
  private float _attackDuration = 0.666f;
  private bool _hasRightWeapon = true;
  private RayCast _targetingRaycast;

  public override void _Ready()
  {
    base._Ready();
    _attackSM = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/attack_sm/playback");
    _attackTimer = GetNode<Timer>("AttackTimer");
    // load sword scene 
    _weaponPS = (PackedScene)ResourceLoader.Load("res://MeleeCarry1/MeleeCarry1_weapon.tscn");
    // get spawn positions
    _leftSwordSpawn = GetNode<Position3D>("Armature/Skeleton/headAttachment/LeftSwordSpawnPoint");
    _rightSwordSpawn = GetNode<Position3D>("Armature/Skeleton/headAttachment/RightSwordSpawnPoint");
    // connect interaction area to body for sword pickup
    GetNode<Area>("InteractionArea").Connect("area_entered", this, "InteractionCallback");
    // get reference to targeting raycast for teleporting
    _targetingRaycast = GetNode<RayCast>("Camera/TargetingRaycast");
  }
  
  protected override void ProcessInput(float delta)
  {
    base.ProcessInput(delta);

    //  ----------------------- Attacking -----------------------
    if (Input.IsActionJustPressed("main_mouse"))
    {
      string currentNode = _attackSM.GetCurrentNode();
      if (currentNode == "idle_top")
      {
        _attackSM.Travel("attack_1");
        _attackTimer.Start(_attackDuration);
      }
      else if (currentNode == "attack_1" && _attackTimer.TimeLeft < _attackDuration/3.0f)
      {
        _attackSM.Travel("attack_2");
        _attackTimer.Start(_attackDuration);
      }
      else if (currentNode == "attack_2" && _attackTimer.TimeLeft < _attackDuration/3.0f)
      {
        _attackSM.Travel("attack_3");
      }
      else if (currentNode == "idle_top_right_weapon")
      {
        _attackSM.Travel("attack_1_right_weapon");
      }
      else if (currentNode == "idle_top_left_weapon")
      {
        _attackSM.Travel("attack_2_left_weapon");
      }
    }

    //  ---------------------- Sword Throwing ----------------------
    else if (Input.IsActionJustPressed("secondary_mouse"))
    {
      string currentNode = _attackSM.GetCurrentNode();
      if (_targetingRaycast.IsColliding() && ((Area)_targetingRaycast.GetCollider()).GetParent() is MeleeCarry1Weapon weapon)
        Teleport(weapon);
      else if (currentNode == "idle_top" || currentNode == "idle_top_left_weapon")
        _attackSM.Travel("weapon_throw_left_bt");
      else if (currentNode == "idle_top_right_weapon")
        _attackSM.Travel("weapon_throw_right_bt");
    }
  }

  private void Teleport(MeleeCarry1Weapon weapon)
  {
    Transform placeholder = Transform;
    placeholder.origin = weapon.GetTeleportLocation();
    Transform = placeholder;
    InteractionCallback(weapon.GetNode<Area>("PickupArea"));
    weapon.PickupCallback(weapon.GetNode<Area>("PickupArea"));
  }

  // need this here so that godot can see it to have it as a callback
  protected override void JumpCallback()
  {
    _vel.y = _jumpSpeed;
  }

  private void LeftThrowCallback()
  {
    // instance weapon
    MeleeCarry1Weapon thrownWeapon = (MeleeCarry1Weapon)_weaponPS.Instance();
    // add weapon as child of main scene (maybe not needed?)
    GetTree().Root.AddChild(thrownWeapon);
    thrownWeapon.Owner = GetTree().Root;
    // set transform and which weapon it is for pickup
    thrownWeapon.GlobalTransform = _leftSwordSpawn.GlobalTransform;
    thrownWeapon.isRightWeapon = false;
    // call throw to impart velocity
    thrownWeapon.Throw(-_camera.GlobalTransform.basis.z);
    if (!_hasRightWeapon)
      _attackSM.Travel("idle_top_no_weapons");
  } 

  private void RightThrowCallback()
  {
    // instance weapon
    MeleeCarry1Weapon thrownWeapon = (MeleeCarry1Weapon)_weaponPS.Instance();
    // add weapon as child of main scene (maybe not needed?)
    GetTree().Root.AddChild(thrownWeapon);
    thrownWeapon.Owner = GetTree().Root;
    // set transform and which weapon it is for pickup
    thrownWeapon.GlobalTransform = _rightSwordSpawn.GlobalTransform;
    thrownWeapon.isRightWeapon = true;
    // call throw to impart velocity
    thrownWeapon.Throw(-_camera.GlobalTransform.basis.z);
    _hasRightWeapon = false;
  }

  private void InteractionCallback(Area area)
  {
    if (area.GetParent() is MeleeCarry1Weapon pickupWeapon)
    {
      string currentNode = _attackSM.GetCurrentNode();
      if (pickupWeapon.isRightWeapon)
      {
        if (currentNode == "idle_top_left_weapon")
          _attackSM.Travel("idle_top");
        else if (currentNode == "idle_top_no_weapons")
          _attackSM.Travel("idle_top_right_weapon");
        _hasRightWeapon = true;
      }
      else
      {
        if (currentNode == "idle_top_right_weapon")
          _attackSM.Travel("idle_top"); 
        else if (currentNode == "idle_top_no_weapons")
          _attackSM.Travel("idle_top_left_weapon");
      }
    }
  }
}