using Godot;

public class MeleeCarry1 : Player
{
  private PackedScene _weaponPS;
  private Position3D _leftSwordSpawn;
  private Position3D _rightSwordSpawn;
  private AnimationNodeStateMachinePlayback _attackSM;
  private Timer _attackTimer;
  private float _attackDuration = 0.666f;

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
      else if (currentNode == "attack1" && _attackTimer.TimeLeft < _attackDuration/3.0f)
      {
        _attackSM.Travel("attack_2");
        _attackTimer.Start(_attackDuration);
      }
      else if (currentNode == "attack2" && _attackTimer.TimeLeft < _attackDuration/3.0f)
      {
        _attackSM.Travel("attack_3");
      }
      else if (currentNode == "idle_top_no_left_sword")
      {
        _attackSM.Travel("attack_1_no_left");
      }
      else if (currentNode == "idle_top_no_right_sword")
      {
        _attackSM.Travel("attack_2_no_right");
      }
    }

    //  ---------------------- Sword Throwing ----------------------
    else if (Input.IsActionJustPressed("secondary_mouse"))
    {
      string currentNode = _attackSM.GetCurrentNode();
      if (currentNode == "idle_top" || currentNode == "idle_top_no_right_sword")
      {
        _attackSM.Travel("sword_throw_left_bt");
      }
      else if (currentNode == "idle_top_no_left_sword")
      {
        _attackSM.Travel("sword_throw_right_bt");
      }
    }
  }

  // need this here so that godot can see it to have it as a callback
  protected override void JumpCallback()
  {
    _vel.y = _jumpSpeed;
  }

  private void ThrowSwordCallback()
  {
    // instance node and reparent to the scene root
    MeleeCarry1Weapon thrownWeapon = (MeleeCarry1Weapon)_weaponPS.Instance();
    GetTree().Root.AddChild(thrownWeapon);
    thrownWeapon.Owner = GetTree().Root;
    if (_attackSM.GetCurrentNode() == "sword_throw_left_bt")
    {
      thrownWeapon.GlobalTransform = _leftSwordSpawn.GlobalTransform;
      thrownWeapon.isRightWeapon = false;
    }
    else
    {
      thrownWeapon.GlobalTransform = _rightSwordSpawn.GlobalTransform;
      thrownWeapon.isRightWeapon = true;
    }
    thrownWeapon.Throw(-_camera.GlobalTransform.basis.z);
  } 
}