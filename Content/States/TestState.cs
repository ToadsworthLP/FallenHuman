using System;

namespace FallenHuman.Content.States;

public class TestState : IState<FallenHumanProjectile>
{
    public IState<FallenHumanProjectile> NextState;

    public void Enter(StateEnterContext<FallenHumanProjectile> context)
    {
        
    }

    public void Update(StateUpdateContext<FallenHumanProjectile> context)
    {
        if(context.Target.StateTime > 60) context.StateMachine.ChangeState(NextState, context.Target);
    }

    public void Exit(StateExitContext<FallenHumanProjectile> context)
    {
        
    }
}