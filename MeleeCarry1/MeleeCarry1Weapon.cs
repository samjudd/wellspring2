using Godot;
using System;

public class MeleeCarry1Weapon : RigidBody
{
  [Export]
  public float _spinVelocityScaling = 3.0f;
  [Export]
  public float _throwVelocity = 12.0f;
  public bool isRightWeapon;
  private AnimationPlayer _animationPlayer;
  public override void _Ready()
  {
    _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    Connect("body_entered", this, "CollisionCallback");
    AngularDamp = 0.0f;
  }

  public void Throw(Vector3 globalThrowDirection)
  {
    _animationPlayer.Play("spin", 0.1f, _spinVelocityScaling);
    AxisLockAngularY = true;
    AxisLockAngularZ = true;
    AxisLockAngularX = true;
    LinearVelocity = globalThrowDirection * _throwVelocity;
  }

  private void CollisionCallback()
  {

  }
}
