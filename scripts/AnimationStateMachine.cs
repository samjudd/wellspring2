using Godot;

public class AnimationStateMachine
{
  private AnimationNodeStateMachinePlayback _stateMachinePlayback;
  private AnimationNodeStateMachine _stateMachine;

  public AnimationStateMachine(AnimationNodeStateMachine stateMachine, AnimationNodeStateMachinePlayback stateMachinePlayback)
  {
    _stateMachine = stateMachine;
    _stateMachinePlayback = stateMachinePlayback;
  }

  public bool Transition(string newState, bool secondaryCondition = true)
  {
    string currentNodeName = _stateMachinePlayback.GetCurrentNode();
    string[] travelPath = _stateMachinePlayback.GetTravelPath();
    // if there is no travel path just put new state there so it passes
    if (travelPath.Length == 0)
      travelPath = new string[1] { "" };
    if (_stateMachine.HasTransition(currentNodeName, newState) && travelPath[0] != newState && secondaryCondition)
    {
      _stateMachinePlayback.Travel(newState);
      return true;
    }
    return false;
  }

  public bool Travel(string newState, bool secondaryCondition = true)
  {
    if (_stateMachine.HasNode(newState) && secondaryCondition)
    {
      _stateMachinePlayback.Travel(newState);
      return true;
    }
    return false;
  }

  public string CurrentState() { return _stateMachinePlayback.GetCurrentNode(); }

  public bool IsInState(string state) { return _stateMachinePlayback.GetCurrentNode() == state; }
}