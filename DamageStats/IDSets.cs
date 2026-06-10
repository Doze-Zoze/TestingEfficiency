using System;
using Terraria;
using Terraria.ID;

namespace TestingEfficiency.DamageStats
{
    public static class IDSets
    {
        public static int[] NpcToCountAs = NPCID.Sets.Factory.CreateNamedSet("TestingEfficiency/NpcToCountAs")
            .Description("NPC types that should be counted as if they're another NPC type for tracking, such as worm segments")
            .RegisterIntSet(-1,
            NPCID.WallofFleshEye, NPCID.WallofFlesh,
            NPCID.TheHungryII, NPCID.TheHungry,
            NPCID.GolemFistLeft, NPCID.Golem,
            NPCID.GolemFistRight, NPCID.Golem,
            NPCID.GolemHead, NPCID.Golem,
            NPCID.PrimeCannon, NPCID.SkeletronPrime,
            NPCID.PrimeSaw, NPCID.SkeletronPrime,
            NPCID.PrimeLaser, NPCID.SkeletronPrime,
            NPCID.PrimeVice, NPCID.SkeletronPrime,
            NPCID.SkeletronHand, NPCID.SkeletronHead,
            NPCID.TheDestroyerBody, NPCID.TheDestroyer,
            NPCID.TheDestroyerTail, NPCID.TheDestroyer,
            NPCID.EaterofWorldsBody, NPCID.EaterofWorldsHead,
            NPCID.EaterofWorldsTail, NPCID.EaterofWorldsHead
            );


        public static bool[] ShouldMergeInstances = NPCID.Sets.Factory.CreateNamedSet("TestingEfficiency/ShouldMergeInstances")
            .Description("NPC types that should count multiple instances of the same NPC as one boss, such as Eater of Worlds")
            .RegisterBoolSet(
                NPCID.Creeper,
                NPCID.TheHungry,
                NPCID.ServantofCthulhu,
                NPCID.EaterofWorldsHead,
                NPCID.PrimeCannon
            );

        public static bool[] ShouldBlacklist = NPCID.Sets.Factory.CreateNamedSet("TestingEfficiency/ShouldBlacklist")
            .Description("NPC types that should be blacklisted from counting as a boss")
            .RegisterBoolSet(
            );

        public static bool[] ShouldTrackAsABoss = NPCID.Sets.Factory.CreateNamedSet("TestingEfficiency/ShouldTrackAsABoss")
            .Description("NPC types that should be whitelisted to count as a boss")
            .RegisterBoolSet(
                NPCID.Creeper,
                NPCID.TheHungry,
                NPCID.ServantofCthulhu,
                NPCID.EaterofWorldsHead,
                NPCID.PrimeCannon
            );

        public static int[] BossKillTimes = NPCID.Sets.Factory.CreateNamedSet("CalamityMod/BossKillTimes")
            .RegisterIntSet(0);

    }
}
