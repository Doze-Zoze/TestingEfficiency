using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TestingEfficiency.DamageStats
{
    public class ProjectileSourceManager : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public DataStructures.DamageSourceData sourceData;
        public IEntitySource source = null;
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            sourceData = new DataStructures.DamageSourceData(projectile.owner, projectile.type, DataStructures.DamageSourceType.Projectile);
            if (!FightStatsConfig.Instance.detailedDmgStats)
            {
                this.source = source;
                NestedSourceCheck(projectile, source);
            }
        }

        public void NestedSourceCheck(Projectile projectile, IEntitySource source, int nest = 0)
        {
            if (source is IEntitySource_WithStatsFromItem)
            {
                sourceData.SourceType = DataStructures.DamageSourceType.Item;
                sourceData.SourceID = ((EntitySource_ItemUse)source).Item.type;
            }
            if (source is EntitySource_ItemUse)
            {
                sourceData.SourceType = DataStructures.DamageSourceType.Item;
                sourceData.SourceID = ((EntitySource_ItemUse)source).Item.type;
            }
            if (source is EntitySource_Parent)
            {
                var s = ((EntitySource_Parent)source).Entity;
                if (s is Projectile && (s as Projectile).GetGlobalProjectile<ProjectileSourceManager>().source != null && nest < 10)
                {
                    NestedSourceCheck((Projectile)s, (s as Projectile).GetGlobalProjectile<ProjectileSourceManager>().source, nest + 1);
                }
                if (s is Player)
                {
                    sourceData.OwnerID = s.whoAmI;
                }
                if (s is NPC)
                {
                    sourceData.OwnerType = DataStructures.DamageSourceOwner.NPC;
                    sourceData.OwnerID = (s as NPC).type;
                }
            }
        }
    }
}
