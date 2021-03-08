using Godot;

public class Skirmisher : Player
{
  [Export] // bullets before needing to reload
  public int _gunClipSize = 6;

  private int _leftGunAmmo;
  private int _rightGunAmmo;
  private AnimationStateMachine _leftMainStateMachine;
  private AnimationStateMachine _rightStateMachine;

  public override void _Ready()
  {
    base._Ready();

    // initialize gun clips
    _leftGunAmmo = _gunClipSize;
    _rightGunAmmo = _gunClipSize;

    // setup references for left and right state machines
    AnimationNodeBlendTree treeRoot = (AnimationNodeBlendTree)_animationTree.TreeRoot;
    AnimationNodeStateMachine leftMainStateMachine = (AnimationNodeStateMachine)treeRoot.GetNode("leftMainStateMachine");
    _leftMainStateMachine = new AnimationStateMachine(leftMainStateMachine, (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/leftMainStateMachine/playback"));

    AnimationNodeStateMachine rightStateMachine = (AnimationNodeStateMachine)treeRoot.GetNode("rightStateMachine");
    _rightStateMachine = new AnimationStateMachine(rightStateMachine, (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/rightStateMachine/playback"));
  }

  protected override void ProcessClassSpecific(float delta)
  {
    BasicAttack(delta);
    ThrowJavelin(delta);
  }

  protected void BasicAttack(float delta)
  {
    if (Input.IsActionJustPressed("main_mouse"))
    {
      if (_leftMainStateMachine.Transition("fire_left"))
      {
        _leftGunAmmo -= 1;
        GD.Print("Left Gun Ammo: ", _leftGunAmmo);
      }
      else if (_rightStateMachine.Transition("fire_right"))
      {
        _rightGunAmmo -= 1;
        GD.Print("Right Gun Ammo: ", _rightGunAmmo);
      }
    }
  }

  protected void ThrowJavelin(float delta)
  {
    if (Input.IsActionJustPressed("ability_q"))
    {
      // secondary condition to check if right arm is shooting or reloading, only throw if idle
      GD.Print(_leftMainStateMachine.CurrentState(), _rightStateMachine.CurrentState());
      _leftMainStateMachine.Transition("throw_javelin", _rightStateMachine.IsInState("idle_right"));
    }
  }

  public void RightReloadCallback() { ReloadCallback(Util.WeaponHands.RIGHT_WEAPON); }
  public void LeftReloadCallback() { ReloadCallback(Util.WeaponHands.LEFT_WEAPON); }
  public void ReloadCallback(Util.WeaponHands hand)
  {
    if (hand == Util.WeaponHands.LEFT_WEAPON && _leftGunAmmo == 0)
    {
      _leftMainStateMachine.Transition("reload_left");
      _leftGunAmmo = _gunClipSize;
    }
    else if (hand == Util.WeaponHands.RIGHT_WEAPON && _rightGunAmmo == 0)
    {
      _rightStateMachine.Transition("reload_right");
      _rightGunAmmo = _gunClipSize;
    }
  }

  // need this here so that godot can see it to have it as a callback
  protected override void JumpCallback()
  {
    _vel.y = _jumpSpeed;
  }
}
