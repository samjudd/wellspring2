using Godot;

public class MeleeCarry1 : Player
{
  public Spatial _sword;
  private AnimationNodeStateMachinePlayback _attackSM;
  private Timer _attackTimer;
  private float _attackDuration = 0.666f;

  public override void _Ready()
  {
    base._Ready();
    _attackSM = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/attack_sm/playback");
    _attackTimer = GetNode<Timer>("AttackTimer");

    // load sword scene 
    ResourceLoader.Load("res://MeleeCarry1/MeleeCarry1_weapon.tscn");
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

  private void ThrowSword()
  {
      // create sword as child of tree root
      // GetTree().Root.AddChild()
  }
}