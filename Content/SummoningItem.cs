using FallenHuman.Assets.Sounds;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FallenHuman.Content;

public class SummoningItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 30;
        Item.damage = 0;
        Item.useStyle = ItemUseStyleID.None;
        Item.UseSound = new SoundStyle(SoundAssetPath.Equip);
        Item.useAnimation = 0;
        Item.useTime = 0;
        Item.rare = ItemRarityID.Red;
        Item.noMelee = true;
        Item.value = Item.sellPrice(0, 2);
        Item.buffType = ModContent.BuffType<FallenHumanBuff>();
        Item.shoot = ModContent.ProjectileType<FallenHumanProjectile>();
    }
    
    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer) {
            player.AddBuff(Item.buffType, 3600);
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