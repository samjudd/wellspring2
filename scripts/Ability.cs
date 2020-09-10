using Godot;

public class Ability
{
  public string _abilityName;
  public float _abilityCooldown;
  private float _lastUsed;
  public float _currentCooldown
  {
    get { return Mathf.Max(_abilityCooldown - (OS.GetUnixTime() - _lastUsed), 0.0f); }
    set { _lastUsed = OS.GetUnixTime() - value; }
  }

  public Ability(string name, float cooldown)
  {
    _abilityName = name;
    _abilityCooldown = cooldown;
  }

  public bool Use()
  {
    if (_currentCooldown == 0.0f)
    {
      _lastUsed = OS.GetUnixTime();
      return true;
    }
    return false;
  }
}