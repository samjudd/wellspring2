using Godot;

public class MeleeCarry1 : Player
{
  private PackedScene _weaponPS;
  private PackedScene _markWavePS;
  private PackedScene _totemPS;
  private Position3D _leftSwordSpawn;
  private Position3D _rightSwordSpawn;
  private Position3D _waveSpawn;
  private AnimationNodeStateMachinePlayback _attackSM;
  private Timer _attackTimer;
  private float _attackDuration = 0.666f; // Extract this to config.
  private bool _hasRightWeapon = true;
  private RayCast _targetingRaycast;
  private MeleeCarry1Weapon _teleportWeapon;
  private Enemy _teleportEnemy;
  private TeleportationTotem _teleportTotem;

  public override void _Ready() // Why does it have the leading underscore if it is public.
  {
    base._Ready();
    // get references to scene components to be used
    _attackSM = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/attack_sm/playback");
    _attackTimer = GetNode<Timer>("AttackTimer");
    _targetingRaycast = GetNode<RayCast>("Camera/TargetingRaycast");

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
  }

  protected override void ProcessInput(float delta)
  {
    base.ProcessInput(delta);
    string currentNode = _attackSM.GetCurrentNode();

    //  ----------------------- Attacking -----------------------
    // I thought these animation graphs were handled w/o code?
    if (Input.IsActionJustPressed("main_mouse"))
    {
      if (currentNode == "idle_top")
      {
        _attackSM.Travel("attack_1");
        _attackTimer.Start(_attackDuration);
      }
      else if (currentNode == "attack_1" && _attackTimer.TimeLeft < _attackDuration / 3.0f)
      {
        _attackSM.Travel("attack_2");
        _attackTimer.Start(_attackDuration);
      }
      else if (currentNode == "attack_2" && _attackTimer.TimeLeft < _attackDuration / 3.0f)
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
        _attackSM.Travel("weapon_throw_left_bt");
      else if (currentNode == "idle_top_right_weapon")
        _attackSM.Travel("weapon_throw_right_bt");
    }

    //  ---------------------- Mark Wave ----------------------
    else if (Input.IsActionJustPressed("ability_3") && currentNode == "idle_top")
    {
      _attackSM.Travel("mark_wave_bt");
    }

    //  ---------------------- Summon Totem ----------------------
    else if (Input.IsActionJustPressed("ability_2") && currentNode == "idle_top")
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
      _attackSM.Travel("summon_totem_bt");
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

  // Maybe a single throw callback would be preferable that takes an enum Weapon { LEFT_SWORD, RIGHT_SWORD} or smth like that.
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
      _attackSM.Travel("idle_top_no_weapons");
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