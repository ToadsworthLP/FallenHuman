using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace FallenHuman.Content.States;

public class FollowPlayerState : IState<FallenHumanProjectile>
{
    private readonly float idleAnimationFrequency;
    private readonly float idleAnimationAmplitude;
    private readonly float minFollowSpeed;
    private readonly float idleTransitionDistanceSquared;
    private readonly float teleportDistanceSquared;
    private readonly float accelerationTime;
    private readonly float movementModeSwitchDistanceSquared;

    public FollowPlayerState(float idleAnimationFrequency, float idleAnimationAmplitude, float idleTransitionDistance, float teleportDistance, float minFollowSpeed, float accelerationTime, float movementModeSwitchDistance)
    {
        this.idleAnimationFrequency = idleAnimationFrequency;
        this.idleAnimationAmplitude = idleAnimationAmplitude;
        this.minFollowSpeed = minFollowSpeed;
        this.accelerationTime = accelerationTime;

        this.teleportDistanceSquared = teleportDistance * teleportDistance;
        this.idleTransitionDistanceSquared = idleTransitionDistance * idleTransitionDistance;
        this.movementModeSwitchDistanceSquared = movementModeSwitchDistance * movementModeSwitchDistance;
    }

    public void Enter(StateEnterContext<FallenHumanProjectile> context)
    {
        
    }

    public void Update(StateUpdateContext<FallenHumanProjectile> context)
    {
        Player player = Main.player[context.Target.Projectile.owner];
        float playerDistanceSquared = player.Center.DistanceSQ(context.Target.Projectile.Center);
        
        // If there's an enemy in range, attack
        if (FallenHumanProjectile.EnemyDetector.GetEnemyInRange(player.Center) != null)
        {
            context.StateMachine.ChangeState(FallenHumanProjectile.AttackState, context.Target);
            return;
        }
        
        // If the player is close enough, transition to the idle state
        if (playerDistanceSquared < idleTransitionDistanceSquared)
        {
            context.StateMachine.ChangeState(FallenHumanProjectile.IdleState, context.Target);
            return;
        }
        
        // If the player is very far away, teleport
        if (playerDistanceSquared > teleportDistanceSquared)
        {
            context.Target.Projectile.Center = player.Center;
        }
        
        // Floating animation
        context.Target.Projectile.position.Y += MathF.Cos(context.Target.LifeTime * idleAnimationFrequency) * idleAnimationAmplitude;

        // Move towards the player
        Vector2 toPlayer = (player.Center - context.Target.Projectile.Center).SafeNormalize(Vector2.Zero);
        float velocityMultiplier = MathF.Min(context.Target.StateTime / accelerationTime, 1f);

        if (playerDistanceSquared < movementModeSwitchDistanceSquared)
        {
            float velocityX = MathF.Max(minFollowSpeed, MathF.Abs(player.velocity.X) * velocityMultiplier);
            float velocityY = MathF.Max(minFollowSpeed, MathF.Abs(player.velocity.Y) * velocityMultiplier);
            context.Target.Projectile.velocity = new Vector2(toPlayer.X * velocityX, toPlayer.Y * velocityY);
        }
        else
        {
            context.Target.Projectile.velocity = toPlayer * MathF.Max(minFollowSpeed, toPlayer.Length() * 10f * velocityMultiplier);
        }
    }

    public void Exit(StateExitContext<FallenHumanProjectile> context)
    {
        
    }
}