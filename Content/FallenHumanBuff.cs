using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace FallenHuman.Content;

public class FallenHumanBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }
    
    public override void Update(Player player, ref int buffIndex) {
        bool unused = false;
        BuffHandle_SpawnPetIfNeededAndSetTime(player, buffIndex, ref unused, ModContent.ProjectileType<FallenHumanProjectile>());
        
        //Console.WriteLine(player.buffTime[buffIndex]);
    }

    private static void BuffHandle_SpawnPetIfNeededAndSetTime(Player player, int buffIndex, ref bool petBool, int petProjID, int buffTimeToGive = 3600)
    {
        //player.buffTime[buffIndex] = buffTimeToGive;
        petBool = true;
        bool flag = !(player.ownedProjectileCounts[petProjID] > 0);

        Vector2 center = player.Center;

        if (flag && player.whoAmI == Main.myPlayer)
            Projectile.NewProjectile(
                GetProjectileSource_Buff(player, buffIndex),
                center.X, center.Y, 
                0f, 0f,
                petProjID,
                FallenHumanProjectile.MinDamage,
                FallenHumanProjectile.Knockback,
                player.whoAmI
            );
    }

    private static IEntitySource GetProjectileSource_Buff(Player player, int buffIndex)
    {
        int buffId = player.buffType[buffIndex];
        return new EntitySource_Buff(player, buffId, buffIndex);
    }
}