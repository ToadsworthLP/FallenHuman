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
	public const int Damage = 15;
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
			.WithOffset(-10, 0f)
			.WithSpriteDirection(-1)
			.WithCode(PreviewAnimation);

		EnemyDetector = new EnemyDetector(500f);

		SlashSoundStyle = new SoundStyle(SoundAssetPath.Slash);
		
		// State machine setup
		IdleState = new IdleState(0.05f, 0.5f, 120f, 0.95f);
		FollowPlayerState = new FollowPlayerState(0.05f, 0.5f, 100f, 1000f, 3f, 30f, 500f);
		AttackState = new AttackState(0.25f, 70f, 10);
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
		
		proj.scale = 0.8f;
	}

	public override void SetDefaults() {
		Projectile.width = 38;
		Projectile.height = 54;
		Projectile.penetrate = -1;
		Projectile.netImportant = true;
		Projectile.timeLeft *= 5;
		Projectile.friendly = false;
		Projectile.minion = true;
		Projectile.ignoreWater = true;
		Projectile.scale = 1f;
		Projectile.tileCollide = false;
		Projectile.DamageType = DamageClass.Summon;
		Projectile.penetrate = -1;
	}
	
	public override bool? CanCutTiles() {
		return false;
	}

	public override bool MinionContactDamage() {
		return true;
	}

	public override void AI() {
		Player player = Main.player[Projectile.owner];

		// If the player is no longer active (online) - deactivate (remove) the projectile.
		if (!player.active) {
			Projectile.active = false;
			return;
		}

		// Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
		if (!player.dead && player.HasBuff(ModContent.BuffType<FallenHumanBuff>())) {
			Projectile.timeLeft = 2;
		}
		
		StateMachine.Update(this);

		if (MathF.Abs(Projectile.velocity.X) > 0.1f)
		{
			Projectile.direction = Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
		}
		
		// Lights up area around it.
		if (!Main.dedServ) {
			Lighting.AddLight(Projectile.Center, Projectile.Opacity * 0.9f, Projectile.Opacity * 0.1f, Projectile.Opacity * 0.3f);
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

	// private void UpdateDash(Player player) {
	// 	// The following code makes our pet dash at enemies when certain conditions are met
	//
	// 	AIDashCharge++;
	//
	// 	if (AIDashCharge <= DashCooldown || (int)AIFadeProgress % 100 != 0) {
	// 		return;
	// 	}
	//
	// 	// Enumerate
	// 	foreach (var npc in Main.ActiveNPCs) {
	// 		// Ignore this npc if it's friendly.
	// 		if (npc.friendly) {
	// 			continue;
	// 		}
	//
	// 		// Ignore this npc if it's too far away. Note that we're using squared values for our checks, to avoid square root calculations as a small, but effective optimization.
	// 		if (player.DistanceSQ(npc.Center) >= RangeHypotenuseSquared) {
	// 			continue;
	// 		}
	//
	// 		Projectile.velocity += Vector2.Normalize(npc.Center - Projectile.Center) * DashSpeed; // Fling the projectile towards the npc.
	// 		AIDashCharge = 0f; // Reset the charge.
	//
	// 		// Play a sound.
	// 		if (!Main.dedServ) {
	// 			SoundEngine.PlaySound(SoundID.Item42, Projectile.Center);
	// 		}
	//
	// 		break;
	// 	}
	// }

	// private void UpdateFading(Player player) {
	// 	//TODO: Comment and clean this up more.
	//
	// 	var playerCenter = player.Center; // Cache the player's center vector to avoid recalculations.
	//
	// 	AIFadeProgress++;
	//
	// 	if (AIFadeProgress < FadeInTicks) {
	// 		Projectile.alpha = (int)(255 - 255 * AIFadeProgress / FadeInTicks);
	// 	}
	// 	else if (AIFadeProgress < FadeInTicks + FullBrightTicks) {
	// 		Projectile.alpha = 0;
	//
	// 		if (Main.rand.NextBool(6)) {
	// 			var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkFairy, 0f, 0f, 200, default, 0.8f);
	//
	// 			dust.velocity *= 0.3f;
	// 		}
	// 	}
	// 	else if (AIFadeProgress < FadeInTicks + FullBrightTicks + FadeOutTicks) {
	// 		Projectile.alpha = (int)(255 * (AIFadeProgress - FadeInTicks - FullBrightTicks) / FadeOutTicks);
	// 	}
	// 	else {
	// 		Projectile.Center = playerCenter + Main.rand.NextVector2Circular(Range, Range);
	// 		AIFadeProgress = 0f;
	//
	// 		Projectile.velocity = 2f * Vector2.Normalize(playerCenter - Projectile.Center);
	// 	}
	//
	// 	if (Vector2.Distance(playerCenter, Projectile.Center) > RangeHypotenuse) {
	// 		Projectile.Center = playerCenter + Main.rand.NextVector2Circular(Range, Range);
	// 		AIFadeProgress = 0f;
	//
	// 		Projectile.velocity += 2f * Vector2.Normalize(playerCenter - Projectile.Center);
	// 	}
	//
	// 	if ((int)AIFadeProgress % 100 == 0) {
	// 		Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(90));
	// 	}
	// }

	// private void UpdateExtraMovement() {
	// 	// Adds some friction to the pet's movement as long as its speed is above 1
	// 	if (Projectile.velocity.Length() > 1f) {
	// 		Projectile.velocity *= 0.98f;
	// 	}
	//
	// 	// If the pet stops - launch it into a random direction at a low speed.
	// 	if (Projectile.velocity == Vector2.Zero) {
	// 		Projectile.velocity = Vector2.UnitX.RotatedBy(Main.rand.NextFloat() * MathHelper.TwoPi) * 2f;
	// 	}
	// }
}