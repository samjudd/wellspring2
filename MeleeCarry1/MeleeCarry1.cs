using Godot;

public class MeleeCarry1 : Player
{
  private PackedScene _weaponPS;
  private PackedScene _markWavePS;
  private PackedScene _totemPS;
  private Position3D _leftSwordSpawn;
  private Position3D _rightSwordSpawn;
  private Position3D _waveSpawn;
  private Timer _attackTimer;
  private float _attackDuration = 0.666f;
  private bool _hasRightWeapon = true;
  private MeleeCarry1Weapon _teleportWeapon;
  private Enemy _teleportEnemy;
  private TeleportationTotem _teleportTotem;
  private AnimationNodeStateMachinePlayback _stateMachineController;
  private AnimationStateMachine _stateMachine;


  public override void _Ready()
  {
    base._Ready();
    // get references to scene components to be used
    _attackTimer = GetNode<Timer>("AttackTimer");

    // load packed scenes 
    _weaponPS = (PackedScene)ResourceLoader.Load("res://MeleeCarry1/MeleeCarry1_weapon.tscn");
    _markWavePS = (PackedScene)ResourceLoader.Load("res://MeleeCarry1/MarkWave.tscn");
    _totemPS = (PackedScene)ResourceLoader.Load("res://MeleeCarry1/TeleportationTotem.tscn");

    // get spawn positions
    _leftSwordSpawn = GetNode<Position3D>("Armature/Skeleton/headAttachment/LeftSwordSpawnPoint");
    _rightSwordSpawn = GetNode<Position3D>("Armature/Skeleton/headAttachment/RightSwordSpawnPoint");
    _waveSpawn = GetNode<Position3D>("Armature/Skeleton/headAttachment/MarkWaveSpawnPoint");

    // connect interaction area to body for sword pickup
    GetNode<Area>("InteractionArea").Connect("area_entered", this, "InteractionCallback");

    // setup references to state machine
    _stateMachineController = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/stateMachine/playback");
    AnimationNodeBlendTree treeRoot = (AnimationNodeBlendTree)_animationTree.TreeRoot;
    AnimationNodeStateMachine stateMachineChild = (AnimationNodeStateMachine)treeRoot.GetNode("stateMachine");
    _stateMachine = new AnimationStateMachine(stateMachineChild, _stateMachineController);

  }

  protected override void ProcessInput(float delta)
  {
    base.ProcessInput(delta);
    string currentNode = _stateMachineController.GetCurrentNode();

    //  ----------------------- Attacking -----------------------
    if (Input.IsActionJustPressed("main_mouse"))
    {
      if (currentNode == "idle_top")
      {
        _stateMachineController.Travel("attack_1");
        _attackTimer.Start(_attackDuration);
      }
      else if (currentNode == "attack_1" && _attackTimer.TimeLeft < _attackDuration / 3.0f)
      {
        _stateMachineController.Travel("attack_2");
        _attackTimer.Start(_attackDuration);
      }
      else if (currentNode == "attack_2" && _attackTimer.TimeLeft < _attackDuration / 3.0f)
      {
        _stateMachineController.Travel("attack_3");
      }
      else if (currentNode == "idle_top_right_weapon")
      {
        _stateMachineController.Travel("attack_1_right_weapon");
      }
      else if (currentNode == "idle_top_left_weapon")
      {
        _stateMachineController.Travel("attack_2_left_weapon");
      }
    }

    //  ---------------------- Sword Throwing / Teleporting ----------------------
    else if (Input.IsActionJustPressed("secondary_mouse"))
    {
      if (_targetingRaycast.GetCollider() is CollisionObject collidedWith && (collidedWith.HasNode("SigilOfTeleportation") || collidedWith.GetParent().HasNode("SigilOfTeleportation")))
      {
        if (collidedWith.GetParent() is MeleeCarry1Weapon weapon)
          TeleportWeapon(weapon);
        else if (collidedWith is Enemy enemy)
          TeleportEnemy(enemy);
        else if (collidedWith is TeleportationTotem totem)
          TeleportTotem(totem);
      }
      else if (currentNode == "idle_top" || currentNode == "idle_top_left_weapon")
        _stateMachineController.Travel("weapon_throw_left_bt");
      else if (currentNode == "idle_top_right_weapon")
        _stateMachineController.Travel("weapon_throw_right_bt");
    }

    //  ---------------------- Mark Wave ----------------------
    else if (Input.IsActionJustPressed("ability_r") && currentNode == "idle_top")
    {
      _stateMachineController.Travel("mark_wave_bt");
    }

    //  ---------------------- Summon Totem ----------------------
    else if (Input.IsActionJustPressed("ability_e") && currentNode == "idle_top")
    {
      SummonTotem();
    }
  }

  private void TeleportWeapon(MeleeCarry1Weapon weapon)
  {
    _animationTree.Set("parameters/teleport_os/active", true);
    _teleportWeapon = weapon;
  }

  private void TeleportEnemy(Enemy enemy)
  {
    _animationTree.Set("parameters/teleport_os/active", true);
    _teleportEnemy = enemy;
  }

  private void TeleportTotem(TeleportationTotem totem)
  {
    _animationTree.Set("parameters/teleport_os/active", true);
    _teleportTotem = totem;
  }

  private void SummonTotem()
  {
    // if the raycast is colliding and the collider it is colliding with is the ground summon totem
    if (_targetingRaycast.IsColliding() && ((PhysicsBody)_targetingRaycast.GetCollider()).GetCollisionLayerBit((int)Util.CollisionLayers.GROUND))
    {
      TeleportationTotem summonTotem = (TeleportationTotem)_totemPS.Instance();
      GetTree().Root.AddChild(summonTotem);
      Transform placeholder = summonTotem.GlobalTransform;
      placeholder.origin = _targetingRaycast.GetCollisionPoint();
      summonTotem.GlobalTransform = placeholder;
      _stateMachineController.Travel("summon_totem_bt");
    }
  }

  private void TeleportCallback()
  {
    Transform placeholder = GlobalTransform;
    if (_teleportWeapon != null)
    {
      placeholder.origin = _teleportWeapon.GetTeleportLocation();
      InteractionCallback(_teleportWeapon.GetNode<Area>("PickupArea"));
      _teleportWeapon.PickupCallback(_teleportWeapon.GetNode<Area>("PickupArea"));
      _teleportWeapon = null;
      GlobalTransform = placeholder;
    }
    else if (_teleportEnemy != null)
    {
      placeholder.origin = _teleportEnemy.GetTeleportLocation();
      if (_teleportEnemy.HasNode("SigilOfTeleportation"))
        _teleportEnemy.GetNode("SigilOfTeleportation").QueueFree();
      placeholder = placeholder.LookingAt(_teleportEnemy.Transform.origin, Vector3.Up);
      _teleportEnemy = null;
      GlobalTransform = placeholder;
      RotationDegrees = new Vector3(0.0f, RotationDegrees.y + 180.0f, 0.0f);
    }
    else if (_teleportTotem != null)
    {
      placeholder.origin = _teleportTotem.GetTeleportLocation();
      _teleportTotem = null;
      GlobalTransform = placeholder;
    }
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
    // set transform and which weapon it is for pickup
    thrownWeapon.GlobalTransform = _leftSwordSpawn.GlobalTransform;
    thrownWeapon.isRightWeapon = false;
    // call throw to impart velocity
    thrownWeapon.Throw(-_camera.GlobalTransform.basis.z);
    if (!_hasRightWeapon)
      _stateMachineController.Travel("idle_top_no_weapons");
  }

  private void RightThrowCallback()
  {
    // instance weapon
    MeleeCarry1Weapon thrownWeapon = (MeleeCarry1Weapon)_weaponPS.Instance();
    // add weapon as child of main scene
    GetTree().Root.AddChild(thrownWeapon);
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
      string currentNode = _stateMachineController.GetCurrentNode();
      if (pickupWeapon.isRightWeapon)
      {
        if (currentNode == "idle_top_left_weapon")
          _stateMachineController.Travel("idle_top");
        else if (currentNode == "idle_top_no_weapons")
          _stateMachineController.Travel("idle_top_right_weapon");
        _hasRightWeapon = true;
      }
      else
      {
        if (currentNode == "idle_top_right_weapon")
          _stateMachineController.Travel("idle_top");
        else if (currentNode == "idle_top_no_weapons")
          _stateMachineController.Travel("idle_top_left_weapon");
      }
    }
  }

  private void MarkWaveCallback()
  {
    // instance weapon
    MarkWave wave = (MarkWave)_markWavePS.Instance();
    // add weapon as child of main scene
    GetTree().Root.AddChild(wave);
    // set transform 
    wave.GlobalTransform = _waveSpawn.GlobalTransform;
    // give wave velocity
    wave.Cast(-_camera.GlobalTransform.basis.z);
  }
}