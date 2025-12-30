using CalamityMod.DataStructures;
using CalamityMod.Events;
using CalamityMod.NPCs;
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
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestingEfficiency;


public class dpsSystem : ModSystem
{
    public struct NPCData //this is a way for me to store the NPC's Type, index in the Main.NPC Array (aka the npc.whoAmI value), and the NPC's name all in one variable. Useful for ensuring it's the same NPC and not a new one with the same type or index.
    {
        public NPCData(NPC npc) //Sets the type/index/name automatically from a passed NPC
        {
            Type = npc.type;
            Index = npc.whoAmI;
            Name = npc.TypeName;
        }
        public NPCData(int type, int index) //Automatically gets the name from the npc type
        {
            Type = type;
            Index = index;
            if (!damagecalcGlobalNPC.merge.Contains(type))
            {
                Name = Main.npc[index].TypeName;
            }
            else
            {
                Name = ContentSamples.NpcsByNetId[type].TypeName;
            }
        }

        public NPCData(int type, int index, string name) //Manually define all 3 values
        {
            Type = type;
            Index = index;
            Name = name;
        }

        public string Name { get; }
        public int Type { get; }
        public int Index { get; }
    }

    public static int bossTimeIGT;

    public static DateTime bossTimeRTA;

    public static bool isBossAlive = false;

    public static int bossRushTimeIGT = 0;

    public static DateTime bossRushTimeRTA;

    public static Dictionary<NPCData, Dictionary<string, int>> dmgdata = new Dictionary<NPCData, Dictionary<string, int>>(); //This is where I store the damage data. the NPCData refers to the NPC, the String refers to the damage source, and the int is the damage amount.

    public static Dictionary<NPCData, Dictionary<string, int>> dotdata = new Dictionary<NPCData, Dictionary<string, int>>();

    public static List<int> HPGraphList = new List<int>();

    public static Texture2D lastPrint = null;

    public static string lastIGT = null;
    public override void PostUpdateEverything()
    {
        if (Main.npc.Any(x => x.active && x.boss))
        {
            if (!isBossAlive)
            {
                isBossAlive = true;
                bossTimeIGT = 0;
                bossTimeRTA = DateTime.UtcNow;
                HPGraphList = new();
            }
            if (bossTimeIGT % 15 == 0)
            {
                int bossHPTotal = 0;
                foreach (var item in Main.ActiveNPCs)
                {
                    if (item.boss && !damagecalcGlobalNPC.blacklist.Contains(item.type))
                    {
                        bossHPTotal += item.life;
                    }
                }

                HPGraphList.Add(bossHPTotal);
            }
            bossTimeIGT++;
        }
        else
        {
            int goalTime = 0;
            var webhook = new webhookmanager.Webhook(Embeds: new List<webhookmanager.Embed> { });
            var dotWebhook = new webhookmanager.Webhook(Embeds: new List<webhookmanager.Embed> { });
            string rta = "";
            string igt = "";
            bool printed = false;
            foreach (var npc in dmgdata)
                if ((npc.Key.Index == -1) ? true : !Main.npc[npc.Key.Index].active || (Main.npc[npc.Key.Index].type != npc.Key.Type))
                //^ if there's no active bosses and this NPC isn't active anymore, we'll print out the damage results. We only do this if there's no active bosses so that bosses such as Moon Lord get all the data printed at the end of the fight and not throughout.
                {
                    if (CalamityGlobalNPC.BossKillTimes.Keys.Contains(npc.Key.Type))
                        goalTime = (int)MathHelper.Max(goalTime, CalamityGlobalNPC.BossKillTimes[npc.Key.Type]);
                    int totaldmg = 0;
                    foreach (KeyValuePair<string, int> i in npc.Value.OrderBy(key => key.Value)) //for each damage source, add that damage amount to the total damage counter
                    {
                        //OLD CODE IGNORE output += $"\n{i.Key}: {((float)(i.Value * 100) / npc.lifeMax).ToString("0.00")}% ({i.Value} dmg)";
                        totaldmg += i.Value;
                    }
                    string output = $"[c/78ffa3:{npc.Key.Name}] [c/ababab:({totaldmg} dmg dealt)]"; //Prepare to print out the total damage dealt
                    var fieldvar = new List<webhookmanager.WebhookField> { }; //this is for discord webhook integration, ignore it
                    foreach (KeyValuePair<string, int> i in npc.Value.OrderBy(key => -key.Value)) //Runs for each damage source, sorted from highest damage dealt to lowest damage deal
                    {
                        output += $"\n{i.Key}: [c/f7d57e:{((float)(i.Value * 100) / totaldmg).ToString("0.00")}%] [c/ababab:({i.Value} dmg)]"; //Prepare print out the damage source, the % of the total damage from that source, and the actual value of damage from that source
                                                                                                                                               //totaldmg += i.Value;
                        fieldvar.Add(new webhookmanager.WebhookField(i.Key, $"{((float)(i.Value * 100) / totaldmg).ToString("0.00")} % ({i.Value} dmg)", false)); //adds that same info to the webhook
                    }

                    //DoT
                    if (dotdata.Any(x => x.Key.Index == npc.Key.Index))
                    {
                        int totalRegen = 0;
                        var dotData = dotdata.First(x => x.Key.Index == npc.Key.Index);
                        foreach (var dot in dotData.Value)
                        {
                            totalRegen += dot.Value;
                        }
                        output += $"\n[c/78ffa3:DoT Estimates] [c/ababab:({-totalRegen / 120} dmg dealt)]";

                        foreach (var dot in dotData.Value)
                        {
                            if (dot.Value == 0)
                                continue;
                            output += $"\n{dot.Key}: [c/f7d57e:{((float)(dot.Value * 100) / totalRegen).ToString("0.00")}%] [c/ababab:({-dot.Value / 120} dmg)]";
                        }

                        dotdata.Remove(dotData.Key);
                    }
                    /*
                    OLD CODE IGNORE
                    if (totaldmg < npc.lifeMax)
                    {
                        output += $"\nOther: {((float)(npc.lifeMax-totaldmg)*100 / npc.lifeMax).ToString("0.00")}% ({npc.lifeMax - totaldmg} dmg)";
                    }*/
                    if (FightStatsConfig.Instance.dmg) Main.NewText(output); //print all the data saved before in one message
                    if (DiscordConfig.Instance.autowebhook && DiscordConfig.Instance.dmgwebhook) //this sends the webhook message if enabled in config
                    {
                        // webhook.embeds.Add(new webhookmanager.Embed(Title: "Boss Fight Length", Fields: new List<webhookmanager.WebhookField> { new webhookmanager.WebhookField("In-Game Time", igt, true), new webhookmanager.WebhookField("Real Time", rta, true) }, Description: ""));
                        webhook.embeds.Add(new webhookmanager.Embed(Title: npc.Key.Name, Fields: fieldvar, Description: $"({totaldmg} dmg dealt)"));
                        //webhook.embeds.Add(new webhookmanager.Embed(Title: "Equipment", ))
                        //webhook.Publish();
                    }
                    dmgdata.Remove(npc.Key); //removes that NPC from the damage list, now that the info has been printed out
                    printed = true;
                }
            if (printed)
            {
                var enumerate = dotdata.ToArray();
                foreach (var dotData in enumerate)
                {
                    if (!Main.npc.Any(x => x.active && dotData.Key.Type == x.type && (dotData.Key.Index == -1 || dotData.Key.Index == x.whoAmI)))
                    {
                        dotdata.Remove(dotData.Key);
                    }
                }
                if (HPGraphList.Count() > 0)
                {
                    int maxHP = HPGraphList.Max();

                    lastPrint = new Texture2D(Main.graphics.GraphicsDevice, HPGraphList.Count(), 100);

                    var texData = new Color[100 * HPGraphList.Count()];
                    for (var i = 0; i < texData.Length; i++)
                    {
                        int x = i % HPGraphList.Count();
                        int y = i / HPGraphList.Count();
                        //new Color(33, 42, 78, 197);
                        texData[i] = (1 - (y / 100f) > HPGraphList[x] / (float)maxHP) ? new Color(52, 66, 119) : Color.White;
                    }
                    lastPrint.SetData(texData);
                    //using (FileStream fs = new FileStream("dmgGraph.png", FileMode.Create))
                    //{
                        //lastPrint.SaveAsPng(fs, lastPrint.Width, lastPrint.Height);
                    //}
                }
            }
            if (isBossAlive)
            {
                isBossAlive = false;

                if (Math.Floor((double)bossTimeIGT / 60 / 60) > 0)
                {
                    igt = $"{Math.Floor((double)bossTimeIGT / 60 / 60)}:{(bossTimeIGT / 60d % 60 >= 10 ? Math.Truncate(bossTimeIGT / 60d % 60 * 100) / 100 : "0" + Math.Truncate(bossTimeIGT / 60d % 60 * 100) / 100)}";
                }
                else
                {
                    igt = $"{Math.Truncate(bossTimeIGT / 60d % 60 * 100) / 100} seconds";
                }
                if (Math.Floor(DateTime.UtcNow.Subtract(bossTimeRTA).TotalSeconds / 60) > 0)
                {
                    rta = $"{(Math.Floor(DateTime.UtcNow.Subtract(bossTimeRTA).TotalSeconds / 60))}:{(DateTime.UtcNow.Subtract(bossTimeRTA).TotalSeconds % 60 >= 10 ? Math.Truncate(DateTime.UtcNow.Subtract(bossTimeRTA).TotalSeconds % 60 * 100) / 100 : "0" + Math.Truncate(DateTime.UtcNow.Subtract(bossTimeRTA).TotalSeconds % 60 * 100) / 100)}";
                }
                else
                {

                    rta = $"{(Math.Truncate(DateTime.UtcNow.Subtract(bossTimeRTA).TotalSeconds % 60 * 100) / 100)} seconds";
                }
                string goalText = "";
                if (Math.Floor((double)goalTime / 60 / 60) > 0)
                {
                    goalText = $"{Math.Floor((double)goalTime / 60 / 60)}:{(goalTime / 60d % 60 >= 10 ? Math.Truncate(goalTime / 60d % 60 * 100) / 100 : "0" + Math.Truncate(goalTime / 60d % 60 * 100) / 100)}";
                }
                else
                {
                    goalText = $"{Math.Truncate(goalTime / 60d % 60 * 100) / 100} seconds";
                }
                lastIGT = $"[c/fab698:Boss Fight Length]\n{goalText} Goal Time\n{igt} IGT\n{rta} RTA";
                if (FightStatsConfig.Instance.time) Main.NewText($"[c/773241:Boss Fight Length] ({goalText} goal time)\n{igt} IGT\n{rta} RTA");

                if (DiscordConfig.Instance.autowebhook && (DiscordConfig.Instance.timewebhook || DiscordConfig.Instance.accessorywebhook || DiscordConfig.Instance.dmgwebhook)) //this sends the webhook message if enabled in config
                {
                    if (DiscordConfig.Instance.timewebhook) webhook.embeds.Add(new webhookmanager.Embed(Title: "Boss Fight Length", Fields: new List<webhookmanager.WebhookField> { new webhookmanager.WebhookField("In-Game Time", igt, true), new webhookmanager.WebhookField("Real Time", rta, true) }, Description: ""));
                    //webhook.embeds.Add(new webhookmanager.Embed(Title: npc.Key.Name, Fields: fieldvar, Description: $"({totaldmg} dmg dealt)"));
                    string equip = "";
                    for (var i = 0; i <= 9; i++)
                    {
                        equip += Main.LocalPlayer.armor[i].HoverName + "\\n";
                    }

                    if (DiscordConfig.Instance.accessorywebhook) webhook.embeds.Add(new webhookmanager.Embed("Equipment", equip));
                    webhook.Publish();
                }
            }
        }
        if (BossRushEvent.BossRushActive)
        {
            if (bossRushTimeIGT < 1)
            {
                bossRushTimeRTA = DateTime.UtcNow;
            }
            bossRushTimeIGT++;
        }
        else
        {
            if (bossRushTimeIGT > 0)
            {
                string rta = "";
                string igt = "";
                if (Math.Floor((double)bossRushTimeIGT / 60 / 60) > 0)
                {
                    igt = $"{Math.Floor((double)bossRushTimeIGT / 60 / 60)}:{(bossRushTimeIGT / 60d % 60 >= 10 ? Math.Truncate(bossRushTimeIGT / 60d % 60 * 100) / 100 : "0" + Math.Truncate(bossRushTimeIGT / 60d % 60 * 100) / 100)}";
                }
                else
                {
                    igt = $"{Math.Truncate(bossRushTimeIGT / 60d % 60 * 100) / 100} seconds";
                }
                if (Math.Floor(DateTime.UtcNow.Subtract(bossRushTimeRTA).TotalSeconds / 60) > 0)
                {
                    rta = $"{(Math.Floor(DateTime.UtcNow.Subtract(bossRushTimeRTA).TotalSeconds / 60))}:{(DateTime.UtcNow.Subtract(bossRushTimeRTA).TotalSeconds % 60 >= 10 ? Math.Truncate(DateTime.UtcNow.Subtract(bossRushTimeRTA).TotalSeconds % 60 * 100) / 100 : "0" + Math.Truncate(DateTime.UtcNow.Subtract(bossRushTimeRTA).TotalSeconds % 60 * 100) / 100)}";
                }
                else
                {

                    rta = $"{(Math.Truncate(DateTime.UtcNow.Subtract(bossRushTimeRTA).TotalSeconds % 60 * 100) / 100)} seconds";
                }

                if (FightStatsConfig.Instance.time) Main.NewText($"[c/773241:Boss Rush Length]\n{igt} IGT\n{rta} RTA");
                bossRushTimeIGT = 0;
            }
        }
    }
}

public class ProjectileSourceManager : GlobalProjectile
{
    public override bool InstancePerEntity => true;
    public string ItemName = "";
    public string ParentName = "";
    public IEntitySource source = null;
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        this.source = source;
        NestedSourceCheck(projectile, source);
    }

    public void NestedSourceCheck(Projectile projectile, IEntitySource source, int nest = 0)
    {
        if (source is IEntitySource_WithStatsFromItem)
        {
            ItemName = ((IEntitySource_WithStatsFromItem)source).Item.Name;
        }
        if (source is EntitySource_ItemUse)
        {
            ItemName = ((EntitySource_ItemUse)source).Item.Name;
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
                ParentName = (s as Player).name;
            }
            if (s is NPC)
            {
                ParentName = (s as NPC).FullName;
            }
        }
    }
}

public class DamageCalcPlayer : ModPlayer
{
    //This is to allow refering direct NPC hits as "Misc" damage insteaad of "DoT & Environment". This is because Tmod doesn't support EntitySources for those.
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.GetGlobalNPC<damagecalcGlobalNPC>().PlayerMiscDamage[Player.whoAmI] += damageDone;
    }
}
public class damagecalcGlobalNPC : GlobalNPC
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
         ModContent.NPCType < PerforatorTailSmall >(),
         //ModContent.NPCType<FalseBrain>()
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

    public int MiscDamage = 0;

    public int[] PlayerMiscDamage = new int[Main.maxPlayers];
    string GetOwnerName(Projectile projectile)
    {
        if (projectile.GetGlobalProjectile<ProjectileSourceManager>().ParentName != "")
            return projectile.GetGlobalProjectile<ProjectileSourceManager>().ParentName;
        return Main.player[projectile.owner].name;
    }

    string GetSourceName(Projectile projectile)
    {
        if (!FightStatsConfig.Instance.detailedDmgStats && projectile.GetGlobalProjectile<ProjectileSourceManager>().ItemName != "")
            return projectile.GetGlobalProjectile<ProjectileSourceManager>().ItemName;
        return projectile.Name;
    }
    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
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
                            npc = basenpc;
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
        //CombatText.NewText(npc.Hitbox, Color.AliceBlue, "!");
        if (merge.Contains(npc.type) && Main.npc.Any(x => x.boss && x.active))
        {
            int x = 0; //this will be used to store already-existing damage from the same damage source
            if (!dpsSystem.dmgdata.TryGetValue(new dpsSystem.NPCData(npc.type, -1), out var y)) //check if NPC is already in the array. if it is, "y" becomes the Dictonary<string,int> that stores the damage sources for that NPC. if it isn't in the array, we'll create a new one below.
            {
                y = new Dictionary<string, int> { };
            }
            if (y.ContainsKey($"{GetOwnerName(projectile)}'s {GetSourceName(projectile)}")) //Checks if the damage source is already in the damage sources dictionary for that NPC.
            {
                y.TryGetValue($"{GetOwnerName(projectile)}'s {GetSourceName(projectile)}", out x); //gets the amount of damage done from this damage source, then...
                y.Remove($"{GetOwnerName(projectile)}'s {GetSourceName(projectile)}"); // it clears that damage source from the dictionary.
            }
            y.Add($"{GetOwnerName(projectile)}'s {GetSourceName(projectile)}", x + (npc.life < 0 ? damageDone + npc.life : damageDone)); //Adds this damage source, in the format of "PlayerName's ProjectileName", to the dict. don't forget to add "x" to the value to keep previous damage from the same source
            dpsSystem.dmgdata.Remove(new dpsSystem.NPCData(npc.type, -1)); //removes this NPC from the damage data list...
            dpsSystem.dmgdata.Add(new dpsSystem.NPCData(npc.type, -1), y); //... so we can add it back with the update damage values

            npc.GetGlobalNPC<damagecalcGlobalNPC>().previousHP -= damageDone; //updates this NPC's previousHP variable, which I used to calculate debuff/environmental damage sources. if there's a better way to calculate debuffs, tell me plz
            npc.GetGlobalNPC<damagecalcGlobalNPC>().PlayerMiscDamage[Main.player[projectile.owner].whoAmI] -= damageDone; //Also updates the misc dmage

        }
        else if ((npc.boss) && !blacklist.Contains(npc.type))
        {
            int x = 0; //this will be used to store already-existing damage from the same damage source
            if (!dpsSystem.dmgdata.TryGetValue(new dpsSystem.NPCData(npc), out var y)) //check if NPC is already in the array. if it is, "y" becomes the Dictonary<string,int> that stores the damage sources for that NPC. if it isn't in the array, we'll create a new one below.
            {
                y = new Dictionary<string, int> { };
            }
            if (y.ContainsKey($"{GetOwnerName(projectile)}'s {GetSourceName(projectile)}")) //Checks if the damage source is already in the damage sources dictionary for that NPC.
            {
                y.TryGetValue($"{GetOwnerName(projectile)}'s {GetSourceName(projectile)}", out x); //gets the amount of damage done from this damage source, then...
                y.Remove($"{GetOwnerName(projectile)}'s {GetSourceName(projectile)}"); // it clears that damage source from the dictionary.
            }
            y.Add($"{GetOwnerName(projectile)}'s {GetSourceName(projectile)}", x + (npc.life < 0 ? damageDone + npc.life : damageDone)); //Adds this damage source, in the format of "PlayerName's ProjectileName", to the dict. don't forget to add "x" to the value to keep previous damage from the same source
            dpsSystem.dmgdata.Remove(new dpsSystem.NPCData(npc)); //removes this NPC from the damage data list...
            dpsSystem.dmgdata.Add(new dpsSystem.NPCData(npc), y); //... so we can add it back with the update damage values
            npc.GetGlobalNPC<damagecalcGlobalNPC>().previousHP -= damageDone; //updates this NPC's previousHP variable, which I used to calculate debuff/environmental damage sources. if there's a better way to calculate debuffs, tell me plz
            npc.GetGlobalNPC<damagecalcGlobalNPC>().PlayerMiscDamage[Main.player[projectile.owner].whoAmI] -= damageDone; //Also updates the misc dmage

        }

    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) //this is the same as the projectile one but for melee weapons. i'm not commenting it all again.
    {
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
                            npc = basenpc;
                            break;
                        }
                    }
                }
            }
        }
        if (replaceList.Keys.Contains(npc.type))
        {
            //if (merge.Contains(replaceList[npc.type]))
            //{
            npc = ContentSamples.NpcsByNetId[npc.type];
            /*}
            else
                foreach (var NPC in Main.npc)
            {
                if (NPC.type == replaceList[npc.type] && NPC.active)
                {
                    npc = NPC;
                    break;
                }
            }*/
        }
        if (merge.Contains(npc.type))
        {
            int x = 0; //this will be used to store already-existing damage from the same damage source
            if (!dpsSystem.dmgdata.TryGetValue(new dpsSystem.NPCData(npc.type, -1), out var y)) //check if NPC is already in the array. if it is, "y" becomes the Dictonary<string,int> that stores the damage sources for that NPC. if it isn't in the array, we'll create a new one below.
            {
                y = new Dictionary<string, int> { };
            }
            if (y.ContainsKey($"{player.name}'s {item.Name}"))
            {
                y.TryGetValue($"{player.name}'s {item.Name}", out x);
                y.Remove($"{player.name}'s {item.Name}");
            }
            y.Add($"{player.name}'s {item.Name}", x + (damageDone > npc.life ? npc.life : damageDone)); //Adds this damage source, in the format of "PlayerName's ProjectileName", to the dict. don't forget to add "x" to the value to keep previous damage from the same source
            dpsSystem.dmgdata.Remove(new dpsSystem.NPCData(npc.type, -1)); //removes this NPC from the damage data list...
            dpsSystem.dmgdata.Add(new dpsSystem.NPCData(npc.type, -1), y); //... so we can add it back with the update damage values
            npc.GetGlobalNPC<damagecalcGlobalNPC>().previousHP -= damageDone; //updates this NPC's previousHP variable, which I used to calculate debuff/environmental damage sources. if there's a better way to calculate debuffs, tell me plz
            npc.GetGlobalNPC<damagecalcGlobalNPC>().PlayerMiscDamage[player.whoAmI] -= damageDone; //Also updates the misc dmage

        }
        else
        if ((npc.boss) && !blacklist.Contains(npc.type))
        {
            int x = 0;
            if (!dpsSystem.dmgdata.TryGetValue(new dpsSystem.NPCData(npc), out var y))
            {
                y = new Dictionary<string, int> { };
            }
            if (y.ContainsKey($"{player.name}'s {item.Name}"))
            {
                y.TryGetValue($"{player.name}'s {item.Name}", out x);
                y.Remove($"{player.name}'s {item.Name}");
            }
            y.Add($"{player.name}'s {item.Name}", x + (damageDone > npc.life ? npc.life : damageDone));
            dpsSystem.dmgdata.Remove(new dpsSystem.NPCData(npc));
            dpsSystem.dmgdata.Add(new dpsSystem.NPCData(npc), y);
            npc.GetGlobalNPC<damagecalcGlobalNPC>().previousHP = npc.life;
        }
    }

    public Dictionary<string, int> dmg = new Dictionary<string, int>(); //this dict i actually don't think I use anymore. I used this back when I stored all damage data on each NPC instead of in the ModSystem.
    public int previousHP = -1; // each NPC has a default value of -1 here so that I can tell they were newly spawned, not just healing from 0 hp to full.
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


        if (isFirst && (npc.boss || merge.Contains(npc.type)) && (!blacklist.Contains(npc.type) || !replaceList.Keys.Contains(npc.type)))
        {
            //Calculate dots
            if (Main.npc.Any(x => x.boss && x.active))
            {
                if (!dpsSystem.dotdata.TryGetValue(new dpsSystem.NPCData(npc.type, merge.Contains(npc.type) ? -1 : npc.whoAmI), out var y)) //check if NPC is already in the array. if it is, "y" becomes the Dictonary<string,int> that stores the damage sources for that NPC. if it isn't in the array, we'll create a new one below.
                {
                    y = new Dictionary<string, int> { };
                    dpsSystem.dotdata.Add(new dpsSystem.NPCData(npc.type, merge.Contains(npc.type) ? -1 : npc.whoAmI), y);
                }
                foreach (var type in npc.buffType)
                {
                    string name = Lang.GetBuffName(type);
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
                    var x = 0;
                    if (y.ContainsKey("DoT: " + name)) //instead of adjusting the damage source based on weapon, we're setting a fixed damage here since most of the time this damage is from DoT or traps. some things, like Shield of Cthulhu, do fall in here too tho
                    {
                        y.TryGetValue("DoT: " + name, out x);
                        y.Remove("DoT: " + name);
                    }
                    y.Add("DoT: " + name, x + (npc.lifeRegen));
                    npc.lifeRegen = oldRegen;
                    npc.lifeRegenCount = oldCount;
                }
                dpsSystem.dotdata.Remove(new dpsSystem.NPCData(npc.type, merge.Contains(npc.type) ? -1 : npc.whoAmI)); //removes this NPC from the damage data list...
                dpsSystem.dotdata.Add(new dpsSystem.NPCData(npc.type, merge.Contains(npc.type) ? -1 : npc.whoAmI), y); //... so we can add it back with the update damage values
            }

            if (npc.life != previousHP && previousHP != -1) //if their HP is different than it was last tick and last tick it wasn't -1 (aka newly spawned), we'll run this code to log the damage as a "DoT & Environment" damage source. Everything in this IF statement works the same as adding the projectile damage to the Dict, but with a fixed Damage Source string.
            {
                int x = 0;
                if (!dpsSystem.dmgdata.TryGetValue(new dpsSystem.NPCData(npc.type, (merge.Contains(npc.type) ? -1 : npc.whoAmI)), out var y))
                {
                    y = new Dictionary<string, int> { };
                }
                if (PlayerMiscDamage.Sum() > 0)
                {
                    for (int i = 0; i < PlayerMiscDamage.Length; i++)
                    {
                        if (PlayerMiscDamage[i] == 0)
                            continue;
                        var damageDone = Math.Min(PlayerMiscDamage[i], Math.Max(0, previousHP - npc.life));
                        var player = Main.player[i];
                        if (y.ContainsKey($"{player.name}'s Misc.")) //instead of adjusting the damage source based on weapon, we're setting a fixed damage here since most of the time this damage is from DoT or traps. some things, like Shield of Cthulhu, do fall in here too tho
                        {
                            y.TryGetValue($"{player.name}'s Misc.", out x);
                            y.Remove($"{player.name}'s Misc.");
                        }
                        y.Add($"{player.name}'s Misc.", x + damageDone);
                        dpsSystem.dmgdata.Remove(new dpsSystem.NPCData(npc.type, (npc.type == ModContent.NPCType<AstrumDeusHead>() ? -1 : npc.whoAmI)));
                        dpsSystem.dmgdata.Add(new dpsSystem.NPCData(npc.type, (npc.type == ModContent.NPCType<AstrumDeusHead>() ? -1 : npc.whoAmI)), y);
                        previousHP -= damageDone;
                        PlayerMiscDamage[i] = 0;
                    }
                }
                var misc = Math.Min(MiscDamage, Math.Max(0, previousHP - npc.life));
                if (misc > 0)
                {
                    if (y.ContainsKey("Misc.")) //instead of adjusting the damage source based on weapon, we're setting a fixed damage here since most of the time this damage is from DoT or traps. some things, like Shield of Cthulhu, do fall in here too tho
                    {
                        y.TryGetValue("Misc.", out x);
                        y.Remove("Misc.");
                    }
                    y.Add("Misc.", x + misc);
                    dpsSystem.dmgdata.Remove(new dpsSystem.NPCData(npc.type, (npc.type == ModContent.NPCType<AstrumDeusHead>() ? -1 : npc.whoAmI)));
                    dpsSystem.dmgdata.Add(new dpsSystem.NPCData(npc.type, (npc.type == ModContent.NPCType<AstrumDeusHead>() ? -1 : npc.whoAmI)), y);
                    previousHP -= misc;
                    MiscDamage = 0;
                }

                x = 0;
                if (npc.life != previousHP)
                {
                    if (y.ContainsKey("DoT & Environment")) //instead of adjusting the damage source based on weapon, we're setting a fixed damage here since most of the time this damage is from DoT or traps. some things, like Shield of Cthulhu, do fall in here too tho
                    {
                        y.TryGetValue("DoT & Environment", out x);
                        y.Remove("DoT & Environment");
                    }
                    y.Add("DoT & Environment", x + (previousHP - npc.life));
                    dpsSystem.dmgdata.Remove(new dpsSystem.NPCData(npc.type, (npc.type == ModContent.NPCType<AstrumDeusHead>() ? -1 : npc.whoAmI)));
                    dpsSystem.dmgdata.Add(new dpsSystem.NPCData(npc.type, (npc.type == ModContent.NPCType<AstrumDeusHead>() ? -1 : npc.whoAmI)), y);
                }

            }
            previousHP = npc.life; //update the previousHP for next frame
            /*
            OLD DEBUG CODE IGNORE
            if (Main.time % 60 == 0)
            {
                string output = npc.FullName;
                foreach (KeyValuePair<string, int> i in dmg)
                {
                    output += $"\n{i.Key}: {i.Value}";
                }
                Main.NewText(output);
            }*/
        }
    }
}
