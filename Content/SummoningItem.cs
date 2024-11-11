using FallenHuman.Assets.Sounds;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FallenHuman.Content;

public class SummoningItem : ModItem
{
    public static SoundStyle UseSoundStyle;

    public override void SetStaticDefaults()
    {
        UseSoundStyle = new SoundStyle(SoundAssetPath.Equip);
    }

    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 30;
        Item.useStyle = ItemUseStyleID.None;
        Item.useAnimation = 0;
        Item.useTime = 0;
        Item.rare = ItemRarityID.Red;
        Item.noMelee = true;
        Item.value = Item.sellPrice(0, 2);
        Item.buffType = ModContent.BuffType<FallenHumanBuff>();
        Item.shoot = ModContent.ProjectileType<FallenHumanProjectile>();
        Item.DamageType = DamageClass.Summon;
    }
    
    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer) {
            player.AddBuff(Item.buffType, 3600);
        }
        
        if (!Main.dedServ) {
            SoundEngine.PlaySound(UseSoundStyle, player.Center); // TODO fix the equip sound - this entire function isn't being called for some reason?
        }
        
        return true;
    }
    
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.GoldBar, 32)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}