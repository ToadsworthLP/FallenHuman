using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace FallenHuman.Content.States;

public class IdleState : IState<FallenHumanProjectile>
{
    private readonly float idleAnimationFrequency;
    private readonly float idleAnimationAmplitude;
    private readonly float inertia;
    private readonly float followPlayerTransitionDistanceSquared;
    
    public IdleState(float idleAnimationFrequency, float idleAnimationAmplitude, float followPlayerTransitionDistance, float inertia)
    {
        this.idleAnimationFrequency = idleAnimationFrequency;
        this.idleAnimationAmplitude = idleAnimationAmplitude;
        this.inertia = inertia;
        this.followPlayerTransitionDistanceSquared = followPlayerTransitionDistance * followPlayerTransitionDistance;
    }

    public void Enter(StateEnterContext<FallenHumanProjectile> context)
    {
        
    }

    public void Update(StateUpdateContext<FallenHumanProjectile> context)
    {
        Player player = Main.player[context.Target.Projectile.owner];
        
        // If there's an enemy in range, attack
        if (FallenHumanProjectile.EnemyDetector.GetEnemyInRange(player.Center) != null)
        {
            context.StateMachine.ChangeState(FallenHumanProjectile.AttackState, context.Target);
            return;
        }
        
        // If the player is far enough away, transition to the follow state
        if (player.Center.DistanceSQ(context.Target.Projectile.Center) > followPlayerTransitionDistanceSquared)
        {
            context.StateMachine.ChangeState(FallenHumanProjectile.FollowPlayerState, context.Target);
        }
        
        // Floating animation
        context.Target.Projectile.position.Y += MathF.Cos(context.Target.LifeTime * idleAnimationFrequency) * idleAnimationAmplitude;
        
        // Add some drag
        if (context.Target.Projectile.velocity.Length() > 0.01f) {
            context.Target.Projectile.velocity *= inertia;
        }
        else
        {
            context.Target.Projectile.velocity = Vector2.Zero;
        }
    }

    public void Exit(StateExitContext<FallenHumanProjectile> context)
    {
        
    }
}