using CalamityMod.DataStructures;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static TestingEfficiency.DataStructures;

namespace TestingEfficiency.DamageStats
{

    public class DamageStatsRecorder : GlobalNPC
    {
        public override bool InstancePerEntity => true; //allow storing entity-specific data here instead of data general to all NPCs

        public static Dictionary<int, int> replaceList = new Dictionary<int, int> {
        {ModContent.NPCType<DevourerofGodsBody>(),ModContent.NPCType<DevourerofGodsHead>()},
        {ModContent.NPCType<DevourerofGodsTail>(),ModContent.NPCType<DevourerofGodsHead>()},
        {ModContent.NPCType<DesertScourgeBody>(),ModContent.NPCType<DesertScourgeHead>()},
        {ModContent.NPCType<DesertScourgeTail>(),ModContent.NPCType<DesertScourgeHead>()},
        {ModContent.NPCType<AstrumDeusBody>(),ModContent.NPCType<AstrumDeusHead>()},
        {ModContent.NPCType<AstrumDeusTail>(),ModContent.NPCType<AstrumDeusHead>()},
        {ModContent.NPCType<AquaticScourgeBody>(),ModContent.NPCType<AquaticScourgeHead>()},
        {ModContent.NPCType<AquaticScourgeBodyAlt>(),ModContent.NPCType<AquaticScourgeHead>()},
        {ModContent.NPCType<AquaticScourgeTail>(),ModContent.NPCType<AquaticScourgeHead>()},
        {ModContent.NPCType<ThanatosBody1>(),ModContent.NPCType<ThanatosHead>()},
        {ModContent.NPCType<ThanatosBody2>(),ModContent.NPCType<ThanatosHead>()},
        { ModContent.NPCType < ThanatosTail >(), ModContent.NPCType < ThanatosHead >() },
        {ModContent.NPCType<StormWeaverBody>(),ModContent.NPCType<StormWeaverHead>()},
        {ModContent.NPCType<StormWeaverTail>(),ModContent.NPCType<StormWeaverHead>()},
        { ModContent.NPCType < AresGaussNuke >(), ModContent.NPCType < AresBody >() },
        { ModContent.NPCType < AresLaserCannon >(), ModContent.NPCType < AresBody >() },
        { ModContent.NPCType < AresPlasmaFlamethrower >(), ModContent.NPCType < AresBody >() },
        { ModContent.NPCType < AresTeslaCannon >(), ModContent.NPCType < AresBody >() },
        { ModContent.NPCType < Apollo >(), ModContent.NPCType < Artemis >() },
        { ModContent.NPCType < PerforatorBodyLarge >(), ModContent.NPCType < PerforatorHeadLarge >() },
        { ModContent.NPCType < PerforatorBodyMedium >(), ModContent.NPCType < PerforatorHeadMedium >() },
        { ModContent.NPCType < PerforatorBodySmall >(), ModContent.NPCType < PerforatorHeadSmall >() },
        { ModContent.NPCType < PerforatorTailLarge >(), ModContent.NPCType < PerforatorHeadLarge >() },
        { ModContent.NPCType < PerforatorTailMedium >(), ModContent.NPCType < PerforatorHeadMedium >() },
        { ModContent.NPCType < PerforatorTailSmall >(), ModContent.NPCType < PerforatorHeadSmall >() },
        { NPCID.WallofFleshEye, NPCID.WallofFlesh},
        {NPCID.TheHungryII, NPCID.TheHungry },
        {NPCID.GolemFistLeft,NPCID.Golem },
        {NPCID.GolemFistRight,NPCID.Golem },
        {NPCID.GolemHead,NPCID.Golem },
        {NPCID.PrimeCannon, NPCID.SkeletronPrime },
        {NPCID.PrimeSaw, NPCID.SkeletronPrime },
        {NPCID.PrimeLaser, NPCID.SkeletronPrime },
        {NPCID.PrimeVice, NPCID.SkeletronPrime },
        {NPCID.TheDestroyerBody,NPCID.TheDestroyer },
        {NPCID.TheDestroyerTail,NPCID.TheDestroyer },
        {NPCID.EaterofWorldsBody,NPCID.EaterofWorldsHead },
        {NPCID.EaterofWorldsTail,NPCID.EaterofWorldsHead },
        { ModContent.NPCType<RavagerClawLeft>(), ModContent.NPCType<RavagerBody>()},
        { ModContent.NPCType<RavagerClawRight>(), ModContent.NPCType<RavagerBody>()},
        { ModContent.NPCType<RavagerHead>(), ModContent.NPCType<RavagerBody>()},
        { ModContent.NPCType<RavagerLegLeft>(), ModContent.NPCType<RavagerBody>()},
        { ModContent.NPCType<RavagerLegRight>(), ModContent.NPCType<RavagerBody>()},
        { ModContent.NPCType<SplitEbonianPaladin>(), ModContent.NPCType<EbonianPaladin>()},
        { ModContent.NPCType<SplitCrimulanPaladin>(), ModContent.NPCType<CrimulanPaladin>()}
    };
        public static List<int> blacklist = new List<int> {  //NPCs to blacklist
        ModContent.NPCType<DevourerofGodsBody>(),
        ModContent.NPCType<DevourerofGodsTail>(),
        ModContent.NPCType<DesertScourgeBody>(),
        ModContent.NPCType<DesertScourgeTail>(),
        ModContent.NPCType<AstrumDeusBody>(),
        ModContent.NPCType<AstrumDeusTail>(),
        ModContent.NPCType<AquaticScourgeBody>(),
        ModContent.NPCType<AquaticScourgeBodyAlt>(),
        ModContent.NPCType<AquaticScourgeTail>(),
        ModContent.NPCType<ThanatosBody1>(),
        ModContent.NPCType<ThanatosBody2>(),
        ModContent.NPCType < ThanatosTail >(),
        ModContent.NPCType<StormWeaverBody>(),
        ModContent.NPCType<StormWeaverTail>(),
        ModContent.NPCType < AresGaussNuke >(),
        ModContent.NPCType < AresLaserCannon >(),
        ModContent.NPCType < AresPlasmaFlamethrower >(),
        ModContent.NPCType < AresTeslaCannon >(),
        ModContent.NPCType < Apollo >(),
         ModContent.NPCType < RavagerClawLeft >(),
         ModContent.NPCType < RavagerClawRight>(),
         ModContent.NPCType < RavagerHead>(),
         ModContent.NPCType < RavagerLegLeft >(),
         ModContent.NPCType < RavagerLegRight>(),
        ModContent.NPCType < PerforatorBodyLarge >(),
        ModContent.NPCType < PerforatorBodyMedium >(),
         ModContent.NPCType < PerforatorBodySmall >(),
         ModContent.NPCType < PerforatorTailLarge >(),
         ModContent.NPCType < PerforatorTailMedium >(),
         ModContent.NPCType < PerforatorTailMedium >(),
         ModContent.NPCType < PerforatorTailSmall >()
    };
        public static int[] merge =
        {
        NPCID.Creeper,
        ModContent.NPCType<AstrumDeusHead>(),
        NPCID.TheHungry,
        NPCID.ServantofCthulhu,
        NPCID.EaterofWorldsHead,
        NPCID.GolemHeadFree,
        ModContent.NPCType<Catastrophe>(),
        ModContent.NPCType<Cataclysm>(),
        ModContent.NPCType<SoulSeeker>(),
        ModContent.NPCType<SupremeCatastrophe>(),
        ModContent.NPCType<SoulSeekerSupreme>(),
        ModContent.NPCType<SupremeCataclysm>(),
        ModContent.NPCType<DarkEnergy>(),
        ModContent.NPCType<CryogenShield>(),
        ModContent.NPCType<AnahitasIceShield>(),
        ModContent.NPCType<PerforatorHeadSmall>(),
        ModContent.NPCType<PerforatorHeadMedium>(),
        ModContent.NPCType<PerforatorHeadLarge>(),
        ModContent.NPCType<PolterPhantom>(),
        ModContent.NPCType<RavagerHead2>(),
        ModContent.NPCType<SplitCrimulanPaladin>(),
        ModContent.NPCType<SplitEbonianPaladin>(),
    };

        public static int[] countsAsBoss =
        {
            ModContent.NPCType<ProfanedGuardianCommander>(),
            ModContent.NPCType<ProfanedGuardianDefender>(),
            ModContent.NPCType<ProfanedGuardianHealer>()
        };

        public static bool ShouldRecord(NPC n) => (n.boss || countsAsBoss.Contains(n.type) || replaceList.ContainsKey(n.type));

        public int MiscDamage = 0;
        public int mergedDamage = 0;
        public int previousHP = -1; // each NPC has a default value of -1 here so that I can tell they were newly spawned, not just healing from 0 hp to full.

        public int[] PlayerMiscDamage = new int[Main.maxPlayers];
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            var sourceData = projectile.GetGlobalProjectile<ProjectileSourceManager>().sourceData;
            RecordDamage(npc, damageDone, sourceData);

        }

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            var sourceData = new DamageSourceData(player.whoAmI, item.type, DamageSourceType.Item);
            RecordDamage(npc, damageDone, sourceData);
        }

        void RecordDamage(NPC npc, int damageDone, DamageSourceData sourceData)
        {
            if (!((Main.npc.Any(x => ShouldRecord(x) && x.active) && merge.Contains(npc.type)) || (ShouldRecord(npc) )))
                return;

            foreach (var i in merge)
            {
                NPC basenpc = null;
                if (NPC.CountNPCS(i) > 1 && npc.type == i)
                {
                    foreach (var NPC in Main.npc)
                    {
                        if (NPC.type == i)
                        {
                            if (basenpc is null) 
                                basenpc = npc;
                            if (npc.whoAmI == NPC.whoAmI)
                            {
                                npc = basenpc;
                                break;
                            }
                        }
                    }
                }
            }
            bool merged = false;
            if (replaceList.Keys.Contains(npc.type))
            {
                if (merge.Contains(replaceList[npc.type]))
                {
                    npc = ContentSamples.NpcsByNetId[npc.type];
                }
                else
                    foreach (var NPC in Main.npc)
                    {
                        if (NPC.type == replaceList[npc.type] && NPC.active)
                        {
                            npc = NPC;
                            merged = true;
                            break;
                        }
                    }
            }

            var npcData = new NPCData(npc);
            if (merge.Contains(npc.type))
                npcData = new NPCData(npc.type, -1);

            //Try to get the existing damage dictionary for this NPC. if it doesn't exist, create a new one.
            if (!DamageStatsSystem.DamageData.TryGetValue(npcData, out var NpcDamageDict))
                NpcDamageDict = new() { };

            int damageToWrite = (npc.life < 0 ? damageDone + npc.life : damageDone);

            //Add it to the NPC's dictionary
            if (NpcDamageDict.ContainsKey(sourceData))
                NpcDamageDict[sourceData] += damageToWrite;
            else
                NpcDamageDict[sourceData] = damageToWrite;

            //update the parent dictionary
            DamageStatsSystem.DamageData[npcData] = NpcDamageDict;

            //updates this NPC's previousHP variable, which I used to calculate debuff/environmental damage sources. if there's a better way to calculate debuffs, tell me plz
            npc.GetGlobalNPC<DamageStatsRecorder>().previousHP -= damageDone;
            if (merged) //Use to make sure segment dmg doesn't count as "healing"
                npc.GetGlobalNPC<DamageStatsRecorder>().mergedDamage += damageDone;

            ////Also updates the misc dmage
            if (sourceData.OwnerType == DamageSourceOwner.Player)
                npc.GetGlobalNPC<DamageStatsRecorder>().PlayerMiscDamage[sourceData.OwnerID] -= damageDone;

        }

        public override void PostAI(NPC npc)
        {
            var isFirst = true;
            foreach (var i in merge)
            {
                var basenpc = new NPC();
                if (NPC.CountNPCS(i) > 1 && npc.type == i)
                {
                    foreach (var NPC in Main.npc)
                    {
                        if (NPC.type == i)
                        {
                            if (basenpc == new NPC()) basenpc = npc;
                            if (npc.whoAmI == NPC.whoAmI)
                            {
                                isFirst = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (replaceList.Keys.Contains(npc.type))
            {
                if (merge.Contains(replaceList[npc.type]))
                {
                    npc = ContentSamples.NpcsByNetId[npc.type];
                }
                else
                    foreach (var NPC in Main.npc)
                    {
                        if (NPC.type == replaceList[npc.type] && NPC.active)
                        {
                            npc = NPC;
                            break;
                        }
                    }
            }


            if (isFirst && (npc.boss || countsAsBoss.Contains(npc.type) || merge.Contains(npc.type)) && (!blacklist.Contains(npc.type) || !replaceList.Keys.Contains(npc.type)))
            {
                //Calculate dots
                //This entire system should be redone to match the main damage system.
                if (Main.npc.Any(x => (x.boss || countsAsBoss.Contains(x.type)) && x.active))
                {
                    foreach (var type in npc.buffType)
                    {
                        var oldRegen = npc.lifeRegen;
                        var oldCount = npc.lifeRegenCount;
                        npc.lifeRegenCount = 0;
                        npc.lifeRegen = 0;
                        if (BuffDatasets.DebuffDataset[type] is not null)
                        {
                            var index = npc.FindBuffIndex(type);
                            var debuffData = BuffDatasets.DebuffDataset[type];
                            if (debuffData == null || debuffData == DebuffData.Oiled) //Oiled is done after
                                continue;
                            int dmg = 1;
                            debuffData.NPCLifeRegenMethod(npc, type, ref index, ref dmg);
                        }


                        var npcData = new NPCData(npc);
                        if (merge.Contains(npc.type))
                            npcData = new NPCData(npc.type, -1);

                        var sourceData = new DamageSourceData(-1, type,DamageSourceType.DoT);

                        //Try to get the existing damage dictionary for this NPC. if it doesn't exist, create a new one.
                        if (!DamageStatsSystem.DoTData.TryGetValue(npcData, out var NpcDamageDict))
                            NpcDamageDict = new() { };


                        //Add it to the NPC's dictionary
                        if (NpcDamageDict.ContainsKey(sourceData))
                            NpcDamageDict[sourceData] += npc.lifeRegen;
                        else
                            NpcDamageDict[sourceData] = npc.lifeRegen;

                        //update the parent dictionary
                        DamageStatsSystem.DoTData[npcData] = NpcDamageDict;

                        npc.lifeRegen = oldRegen;
                        npc.lifeRegenCount = oldCount;
                    }
                }
                if (npc.life != previousHP && previousHP != -1) //if their HP is different than it was last tick and last tick it wasn't -1 (aka newly spawned), we'll run this code to log the damage as a "DoT & Environment" damage source. Everything in this IF statement works the same as adding the projectile damage to the Dict, but with a fixed Damage Source string.
                {
                    if (!DamageStatsSystem.DamageData.TryGetValue(new NPCData(npc.type, (merge.Contains(npc.type) ? -1 : npc.whoAmI)), out var NpcDamageDict))
                        NpcDamageDict = new() { };

                    if (PlayerMiscDamage.Sum() > 0)
                    {
                        for (int i = 0; i < PlayerMiscDamage.Length; i++)
                        {
                            if (PlayerMiscDamage[i] == 0)
                                continue;
                            var damageDone = Math.Min(PlayerMiscDamage[i], Math.Max(0, previousHP - npc.life));
                            var player = Main.player[i];
                            var sourceData = new DamageSourceData(player.whoAmI, -1, DamageSourceType.Misc);
                            int damageToWrite = (npc.life < 0 ? damageDone + npc.life : damageDone);
                            if (!NpcDamageDict.ContainsKey(sourceData))
                                NpcDamageDict[sourceData] = damageToWrite;
                            else
                                NpcDamageDict[sourceData] += damageToWrite;
                            DamageStatsSystem.DamageData[new NPCData(npc.type, (merge.Contains(npc.type) ? -1 : npc.whoAmI))] = NpcDamageDict;
                            previousHP -= damageDone;
                            PlayerMiscDamage[i] = 0;
                        }
                    }
                    var misc = Math.Min(MiscDamage, Math.Max(0, previousHP - npc.life));
                    if (misc > 0)
                    {
                        var sourceData = new DamageSourceData(-1, -1, DamageSourceType.Misc);
                        if (!NpcDamageDict.ContainsKey(sourceData))
                            NpcDamageDict[sourceData] = misc;
                        else
                            NpcDamageDict[sourceData] += misc;
                        previousHP -= misc;
                        MiscDamage = 0;
                    }
                    
                    if (npc.life - mergedDamage != previousHP)
                    {
                        var sourceData = new DamageSourceData(-1, -1, DamageSourceType.Environment);
                        if (!NpcDamageDict.ContainsKey(sourceData))
                            NpcDamageDict[sourceData] = previousHP - (npc.life - mergedDamage);
                        else
                            NpcDamageDict[sourceData] += previousHP - (npc.life - mergedDamage);
                    }

                }
                previousHP = npc.life;
            }
        }
    }

    public class PlayerMiscDmgRecorder : ModPlayer
    {
        //This is to allow refering direct NPC hits as "Misc" damage insteaad of "DoT & Environment". This is because Tmod doesn't support EntitySources for those.
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<DamageStatsRecorder>().PlayerMiscDamage[Player.whoAmI] += damageDone;
        }
    }
}
