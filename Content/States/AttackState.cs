using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FallenHuman.Content.States;

public class AttackState : IState<FallenHumanProjectile>
{
    private readonly float distanceDependentAttackVelocityMultiplier;
    private readonly float minAttackVelocity;
    private readonly int attackDamage;
    
    public AttackState(float distanceDependentAttackVelocityMultiplier, float minAttackVelocity, int attackDamage)
    {
        this.distanceDependentAttackVelocityMultiplier = distanceDependentAttackVelocityMultiplier;
        this.attackDamage = attackDamage;
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

        Vector2 toTarget = (target.Center - context.Target.Projectile.Center).SafeNormalize(Vector2.Zero);
        float distanceToTarget = (target.Center - context.Target.Projectile.Center).Length();
        context.Target.Projectile.velocity = toTarget * MathF.Max(distanceToTarget * distanceDependentAttackVelocityMultiplier, minAttackVelocity);

        if (!Main.dedServ) {
        	SoundEngine.PlaySound(FallenHumanProjectile.SlashSoundStyle, context.Target.Projectile.Center);
        }
        
        //target.life -= attackDamage;

        context.Target.Projectile.friendly = true;
        
        // Spawn dust
        if (context.Target.Projectile.velocity.LengthSquared() > 0.01f)
        {
            for (int i = 0; i < 3; i++)
            {
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

                dust.velocity *= 0.5f;
            }
        }
        
        context.StateMachine.ChangeState(FallenHumanProjectile.CooldownState, context.Target);
    }

    public void Exit(StateExitContext<FallenHumanProjectile> context)
    {
        
    }
}