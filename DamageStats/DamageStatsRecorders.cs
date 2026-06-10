using System;
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

        public static bool ShouldRecord(NPC n) => (n.boss || IDSets.ShouldTrackAsABoss[n.type] || IDSets.NpcToCountAs[n.type] > -1);

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

        public static bool RunMerging(ref NPC npc)
        {
            int type = npc.type;
            if (IDSets.ShouldMergeInstances[npc.type])
            {
                npc = Main.npc.FirstOrDefault((x => x.type == type), npc);
            }

            bool merged = false;
            if (IDSets.NpcToCountAs[npc.type] != -1)
            {
                if (IDSets.ShouldMergeInstances[IDSets.NpcToCountAs[npc.type]])
                {
                    npc = ContentSamples.NpcsByNetId[npc.type];
                }
                else
                    foreach (var NPC in Main.npc)
                    {
                        if (NPC.type == IDSets.NpcToCountAs[npc.type] && NPC.active)
                        {
                            npc = NPC;
                            merged = true;
                            break;
                        }
                    }
            }
            return merged;
        }

        void RecordDamage(NPC npc, int damageDone, DamageSourceData sourceData)
        {

            if (!((Main.npc.Any(x => ShouldRecord(x) && x.active) && IDSets.ShouldMergeInstances[npc.type]) || (ShouldRecord(npc))))
                return;

            var origNPC = npc;
            bool merged = RunMerging(ref npc);

            var npcData = new NPCData(npc);
            if (IDSets.ShouldMergeInstances[npc.type])
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
            var a = npc.GetGlobalNPC<DamageStatsRecorder>().previousHP;
            //updates this NPC's previousHP variable, which I used to calculate debuff/environmental damage sources. if there's a better way to calculate debuffs, tell me plz
            npc.GetGlobalNPC<DamageStatsRecorder>().previousHP -= damageDone;
            if (merged) //Use to make sure segment dmg doesn't count as "healing"
                npc.GetGlobalNPC<DamageStatsRecorder>().mergedDamage += damageDone;

            ////Also updates the misc dmage
            if (sourceData.OwnerType == DamageSourceOwner.Player)
                npc.GetGlobalNPC<DamageStatsRecorder>().PlayerMiscDamage[sourceData.OwnerID] -= damageDone;

        }

        public static Func<NPC, int, int> DebuffFunc;

        public override void PostAI(NPC npc)
        {
            var isFirst = npc.whoAmI == (Main.npc.FirstOrDefault(x => x.whoAmI == npc.whoAmI, null)?.whoAmI ?? -1);

            if (isFirst && (npc.boss || IDSets.ShouldTrackAsABoss[npc.type] || IDSets.ShouldMergeInstances[npc.type]) && (!IDSets.ShouldBlacklist[npc.type]))
            {
                //Calculate dots when enabled.
                //Slightly laggy... but idk how to fix it.
                if (TestingEfficiency.CalamityLoaded && FightStatsConfig.Instance.calamityDebuffStats && Main.npc.Any(x => (x.boss || IDSets.ShouldTrackAsABoss[x.type]) && x.active))
                {
                    int countsAs = IDSets.NpcToCountAs[npc.type] >= 0 ? IDSets.NpcToCountAs[npc.type] : npc.type;
                    bool firstOfTheMergedOnes = Main.npc.FirstOrDefault(x => IDSets.NpcToCountAs[x.type] == countsAs || x.type == countsAs).whoAmI == npc.whoAmI;
                    if (firstOfTheMergedOnes)
                    { 

                    var allToMerge = Main.npc.Where(x => IDSets.NpcToCountAs[x.type] == npc.type || x.type == npc.type);
                        foreach (var NPC in allToMerge)
                        {
                            foreach (var type in NPC.buffType)
                            {

                                if (DebuffFunc is null)
                                    DebuffFunc = (Func<NPC, int, int>)TestingEfficiency.CalamityMod.Call("GetDebuffDamageFunction", npc, type);

                                int regen = DebuffFunc(npc, type);

                                var npcData = new NPCData(npc);
                                if (IDSets.ShouldMergeInstances[npc.type])
                                    npcData = new NPCData(NPC.type, -1);

                                var sourceData = new DamageSourceData(-1, type, DamageSourceType.DoT);

                                //Try to get the existing damage dictionary for this NPC. if it doesn't exist, create a new one.
                                if (!DamageStatsSystem.DoTData.TryGetValue(npcData, out var NpcDamageDict))
                                    NpcDamageDict = new() { };


                                //Add it to the NPC's dictionary
                                if (NpcDamageDict.ContainsKey(sourceData))
                                    NpcDamageDict[sourceData] += regen;
                                else
                                    NpcDamageDict[sourceData] = regen;

                                //update the parent dictionary
                                DamageStatsSystem.DoTData[npcData] = NpcDamageDict;
                            }
                        }
                    }
                }





                if (npc.life != previousHP && previousHP != -1) //if their HP is different than it was last tick and last tick it wasn't -1 (aka newly spawned), we'll run this code to log the damage as a "DoT & Environment" damage source. Everything in this IF statement works the same as adding the projectile damage to the Dict, but with a fixed Damage Source string.
                {
                    if (!DamageStatsSystem.DamageData.TryGetValue(new NPCData(npc.type, (IDSets.ShouldMergeInstances[npc.type] ? -1 : npc.whoAmI)), out var NpcDamageDict))
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
                            DamageStatsSystem.DamageData[new NPCData(npc.type, (IDSets.ShouldMergeInstances[npc.type] ? -1 : npc.whoAmI))] = NpcDamageDict;
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
                mergedDamage = 0;
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
