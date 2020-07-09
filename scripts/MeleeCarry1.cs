using Godot;

public class MeleeCarry1 : Player
{
  private AnimationNodeStateMachinePlayback _attackSM;
  private Timer _attackTimer;
  private float _attackDuration = 0.666f;

  public override void _Ready()
  {
    base._Ready();
    _attackSM = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/attack_sm/playback");
    _attackTimer = GetNode<Timer>("AttackTimer");
  }

  public override void _Process(float delta)
  {
    base._Process(delta);
  }

  protected override void ProcessInput(float delta)
  {
    base.ProcessInput(delta);

    //  ----------------------- Attacking -----------------------
    if (Input.IsActionJustPressed("main_mouse"))
    {
      if (_attackSM.GetCurrentNode() == "idle_top")
      {
        _attackSM.Travel("attack1");
        _attackTimer.Start(_attackDuration);
      }
      if (_attackSM.GetCurrentNode() == "attack1" && _attackTimer.TimeLeft < _attackDuration/3.0f)
      {
        _attackSM.Travel("attack2");
        _attackTimer.Start(_attackDuration);
      }
      if (_attackSM.GetCurrentNode() == "attack2" && _attackTimer.TimeLeft < _attackDuration/3.0f)
      {
        _attackSM.Travel("attack3");
      }

    }
  }

  // need this here so that godot can see it to have it as a callback
  protected override void JumpCallback()
  {
    _vel.y = _jumpSpeed;
  }
}