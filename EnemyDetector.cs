using Microsoft.Xna.Framework;
using Terraria;

namespace FallenHuman;

public class EnemyDetector
{
    private readonly float rangeSquared;

    public EnemyDetector(float range)
    {
        this.rangeSquared = range * range;
    }

    public NPC GetEnemyInRange(Vector2 position)
    {
        foreach (NPC npc in Main.ActiveNPCs) {
            if (!npc.CanBeChasedBy()) {
            	continue;
            }

            float distanceSquared = position.DistanceSQ(npc.Center);
            if (distanceSquared >= rangeSquared) {
            	continue;
            }

            return npc;
        }

        return null;
    }
}