
public class Skirmisher : Player
{
  // need this here so that godot can see it to have it as a callback
  protected override void JumpCallback()
  {
    _vel.y = _jumpSpeed;
  }
}
