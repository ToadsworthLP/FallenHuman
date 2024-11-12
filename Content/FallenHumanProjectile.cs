using System;
using System.Runtime.InteropServices;
using FallenHuman.Assets.Sounds;
using FallenHuman.Content.States;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FallenHuman.Content;

public class FallenHumanProjectile : ModProjectile, IStateMachineTarget
{
	public const int MinDamage = 1;
	public const float Knockback = 5.0f;
	
	public static StateMachine<FallenHumanProjectile> StateMachine;
	public static IdleState IdleState;
	public static FollowPlayerState FollowPlayerState;
	public static AttackState AttackState;
	public static CooldownState CooldownState;

	public static EnemyDetector EnemyDetector;

	public static SoundStyle SlashSoundStyle;

	public float LifeTime
	{
		get => Projectile.ai[0];
		set => Projectile.ai[0] = value;
	}
	
	public float StateTime 
	{ 
		get => Projectile.ai[1];
		set => Projectile.ai[1] = value;
	}
	
	public int CurrentStateId 
	{ 
		get => new PackedState(Projectile.ai[2]).CurrentStateId;
		set {
			var packed = new PackedState(Projectile.ai[2])
			{
				CurrentStateId = (ushort)value
			};
			Projectile.ai[2] = packed.EncodedValue;
		}
	}
	
	public int LastStateId 
	{ 
		get => new PackedState(Projectile.ai[2]).LastStateId;
		set {
			var packed = new PackedState(Projectile.ai[2])
			{
				LastStateId = (ushort)value
			};
			Projectile.ai[2] = packed.EncodedValue;
		}
	}

	public override void SetStaticDefaults() {
		Main.projFrames[Projectile.type] = 1;
		Main.projPet[Projectile.type] = true;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		
		ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
			.WithOffset(0, -6f)
			.WithSpriteDirection(1)
			.WithCode(PreviewAnimation);

		EnemyDetector = new EnemyDetector(500f);

		SlashSoundStyle = new SoundStyle(SoundAssetPath.Slash);
		
		// State machine setup
		IdleState = new IdleState(0.05f, 0.5f, 120f, 0.95f);
		FollowPlayerState = new FollowPlayerState(0.05f, 0.5f, 100f, 1000f, 3f, 30f, 500f);
		AttackState = new AttackState(0.25f, 70f);
		CooldownState = new CooldownState(0.05f, 0.5f, 60f, 0.8f);
		
		StateMachine = new StateMachine<FallenHumanProjectile>();
		StateMachine.RegisterStates(IdleState, FollowPlayerState, AttackState, CooldownState);
	}
	
	public static void PreviewAnimation(Projectile proj, bool walking)
	{
		if (walking)
		{
			float num = 1f;
			float num2 = (float)Main.timeForVisualEffects % 60f / 60f;
			proj.position.Y += 0f - num + (float)(Math.Cos(num2 * ((float)Math.PI * 2f) * 2f * 0.5f) * (num * 2f));
		}
		
		proj.scale = 1.0f;
	}

	public override void SetDefaults() {
		Projectile.width = 24;
		Projectile.height = 40;
		Projectile.penetrate = -1;
		Projectile.netImportant = true;
		Projectile.timeLeft *= 5;
		Projectile.friendly = false;
		Projectile.minion = true;
		Projectile.ignoreWater = true;
		Projectile.scale = 1f;
		Projectile.tileCollide = false;
		Projectile.DamageType = DamageClass.Summon;
		Projectile.ContinuouslyUpdateDamageStats = true;
	}
	
	public override bool? CanCutTiles() {
		return false;
	}

	public override bool MinionContactDamage() {
		return true;
	}

	public override void AI() {
		Player player = Main.player[Projectile.owner];
		
		if (!player.active) {
			Projectile.active = false;
			return;
		}

		if (!player.dead && player.HasBuff(ModContent.BuffType<FallenHumanBuff>())) {
			Projectile.timeLeft = 2;
		}
		
		StateMachine.Update(this);

		if (MathF.Abs(Projectile.velocity.X) > 0.1f)
		{
			Projectile.direction = Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct PackedState
	{
		[FieldOffset(0)] public float EncodedValue;
		[FieldOffset(sizeof(ushort) * 0)] public ushort CurrentStateId;
		[FieldOffset(sizeof(ushort) * 1)] public ushort LastStateId;

		public PackedState(float encodedValue)
		{
			EncodedValue = encodedValue;
		}

		public PackedState(ushort currentStateId, ushort lastStateId)
		{
			CurrentStateId = currentStateId;
			LastStateId = lastStateId;
		}
	}
}