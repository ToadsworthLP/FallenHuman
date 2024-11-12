using System;
using Terraria;
using Terraria.ID;

namespace FallenHuman.Content.States;

public class CooldownState : IState<FallenHumanProjectile>
{
    private readonly float idleAnimationFrequency;
    private readonly float idleAnimationAmplitude;
    private readonly float cooldown;
    private readonly float inertia;

    public CooldownState(float idleAnimationFrequency, float idleAnimationAmplitude, float cooldown, float inertia)
    {
        this.cooldown = cooldown;
        this.inertia = inertia;
        this.idleAnimationFrequency = idleAnimationFrequency;
        this.idleAnimationAmplitude = idleAnimationAmplitude;
    }

    public void Enter(StateEnterContext<FallenHumanProjectile> context)
    {
        
    }

    public void Update(StateUpdateContext<FallenHumanProjectile> context)
    {
        if (context.Target.StateTime > cooldown)
        {
            context.StateMachine.ChangeState(FallenHumanProjectile.IdleState, context.Target);
            return;
        }
        
        float playerVelocitySquared = context.Target.Projectile.velocity.LengthSquared();

        // Floating animation
        context.Target.Projectile.position.Y += MathF.Cos(context.Target.LifeTime * idleAnimationFrequency) * idleAnimationAmplitude;
        
        // Glow while moving
        if (playerVelocitySquared > 0.1f)
        {
            if (!Main.dedServ) {
                Lighting.AddLight(context.Target.Projectile.Center, context.Target.Projectile.Opacity * 0.9f, context.Target.Projectile.Opacity * 0.1f, context.Target.Projectile.Opacity * 0.3f);
            }
        }

        // Deal damage while moving
        context.Target.Projectile.friendly = playerVelocitySquared > 0.1f;
        
        // Spawn dust while moving quickly
        if (playerVelocitySquared > 20.0f)
        {
            float speedMultiplier = Main.rand.NextFloat(0.05f, 0.1f);
            var dust = Dust.NewDustDirect(
                context.Target.Projectile.position, 
                context.Target.Projectile.width, 
                context.Target.Projectile.height, 
                DustID.PinkFairy, 
                context.Target.Projectile.velocity.X * speedMultiplier, context.Target.Projectile.velocity.Y * speedMultiplier,
                0,
                default,
                0.8f
            );
        }

        context.Target.Projectile.velocity *= inertia;
    }

    public void Exit(StateExitContext<FallenHumanProjectile> context)
    {
        context.Target.Projectile.friendly = false;
    }
}