using Godot;
using System;

public class TeleportationTotem : StaticBody {
  private int _totemNumber;

  public Vector3 GetTeleportLocation() {
    // teleport above the origin of the totem
    return ToGlobal(Vector3.Back);
  }
}
