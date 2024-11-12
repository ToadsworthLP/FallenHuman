using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace FallenHuman.Content.States;

public class AttackState : IState<FallenHumanProjectile>
{
    private readonly float distanceDependentAttackVelocityMultiplier;
    private readonly float minAttackVelocity;
    
    public AttackState(float distanceDependentAttackVelocityMultiplier, float minAttackVelocity)
    {
        this.distanceDependentAttackVelocityMultiplier = distanceDependentAttackVelocityMultiplier;
        this.minAttackVelocity = minAttackVelocity;
    }

    public void Enter(StateEnterContext<FallenHumanProjectile> context)
    {
        
    }

    public void Update(StateUpdateContext<FallenHumanProjectile> context)
    {
        Player player = Main.player[context.Target.Projectile.owner];
        NPC target = FallenHumanProjectile.EnemyDetector.GetEnemyInRange(player.Center);

        if (target == null)
        {
            context.StateMachine.ChangeState(FallenHumanProjectile.IdleState, context.Target);
            return;
        }

        int attackDamage = GetAttackDamage(player);
        context.Target.Projectile.originalDamage = attackDamage;

        Vector2 toTarget = (target.Center - context.Target.Projectile.Center).SafeNormalize(Vector2.Zero);
        float distanceToTarget = (target.Center - context.Target.Projectile.Center).Length();
        context.Target.Projectile.velocity = toTarget * MathF.Max(distanceToTarget * distanceDependentAttackVelocityMultiplier, minAttackVelocity);

        if (!Main.dedServ) {
        	SoundEngine.PlaySound(FallenHumanProjectile.SlashSoundStyle, context.Target.Projectile.Center);
            Lighting.AddLight(context.Target.Projectile.Center, context.Target.Projectile.Opacity * 0.9f, context.Target.Projectile.Opacity * 0.1f, context.Target.Projectile.Opacity * 0.3f);
        }
        
        context.Target.Projectile.friendly = true;
        
        context.StateMachine.ChangeState(FallenHumanProjectile.CooldownState, context.Target);
    }

    public void Exit(StateExitContext<FallenHumanProjectile> context)
    {
        
    }

    private int GetAttackDamage(Player player)
    {
        return Math.Max(FallenHumanProjectile.MinDamage, (int)(player.GetWeaponDamage(player.HeldItem) * 0.5f));
    }
}