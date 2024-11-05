﻿using System;
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
        
        // Floating animation
        context.Target.Projectile.position.Y += MathF.Cos(context.Target.LifeTime * idleAnimationFrequency) * idleAnimationAmplitude;
        
        // Spawn dust
        if (Main.rand.NextBool(6)) {
            var dust = Dust.NewDustDirect(
                context.Target.Projectile.position, 
                context.Target.Projectile.width, 
                context.Target.Projectile.height, 
                DustID.PinkFairy, 
                0f, 0f,
                200,
                default,
                0.8f
            );

            dust.velocity *= 0.3f;
        }

        context.Target.Projectile.velocity *= inertia;
    }

    public void Exit(StateExitContext<FallenHumanProjectile> context)
    {
        
    }
}