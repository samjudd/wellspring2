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
  private Area _swordHitbox;
  private Area _pickupHitbox;

  public override void _Ready()
  {
    _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    _swordHitbox = GetNode<Area>("Armature/Skeleton/SwordAttachment/Hitbox");
    _swordHitbox.Connect("body_entered", this, "CollisionCallback");
    _pickupHitbox = GetNode<Area>("PickupArea");
    _pickupHitbox.Connect("area_entered", this, "PickupCallback");
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

  private void CollisionCallback(Node body)
  {
    // stop animation, turn off sword hitbox, turn on pickup hitbox and set rigidbody to static so it won't move
    _animationPlayer.Stop();
    _swordHitbox.SetDeferred("Monitorable", false);
    _swordHitbox.SetDeferred("Monitoring", false);
    _pickupHitbox.Monitoring = true;
    _pickupHitbox.Monitorable = true;
    Mode = ModeEnum.Static;
    // still need to sorta "bury" sword in object or ground
  }

  private void PickupCallback(Area area)
  {
    QueueFree();
  }
}
