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
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static TestingEfficiency.dpsSystem;
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

    public enum DamageSourceType
    {
        Projectile,
        Item,
        Misc,
        DoT,
        Environment
    }

    public enum DamageSourceOwner
    {
        Player,
        NPC
    }

    public struct DamageSourceData
    {

        public DamageSourceData(int playerid, int sourceid, DamageSourceType sourceType)
        {
            OwnerID = playerid;
            SourceID = sourceid;
            SourceType = sourceType;
        }

        /// <summary>
        /// Player.whoAmI
        /// </summary>
        public int OwnerID;
        /// <summary>
        /// Proctile.type/Item.type
        /// </summary>
        public int SourceID;
        public DamageSourceType SourceType;
        public DamageSourceOwner OwnerType = DamageSourceOwner.Player;
    }

    public static Dictionary<NPCData, Dictionary<DamageSourceData, int>> dmgdata = new Dictionary<NPCData, Dictionary<DamageSourceData, int>>(); //This is where I store the damage data. the NPCData refers to the NPC, the String refers to the damage source, and the int is the damage amount.

    public static Dictionary<NPCData, Dictionary<string, int>> dotdata = new Dictionary<NPCData, Dictionary<string, int>>();

    public static List<int> HPGraphList_Boss = new List<int>();
    public static List<int> HPGraphList_Player = new List<int>();
    public static int MaxHPGraph_Player = 0;

    public static Texture2D LastBossHPGraph = null;
    public static Texture2D LastPlayerHPGraph = null;

    public static string lastIGT = null;

    public static string lastSplits = null;

    (string, string, string) ConstructDamageLine(KeyValuePair<DamageSourceData, int> data, int totaldmg)
    {
        string owner = "";
        string source = "";
        if (data.Key.OwnerID >= 0)
        {
            if (data.Key.OwnerType == DamageSourceOwner.Player)
                owner = Main.player[data.Key.OwnerID].name + "'s ";
            if (data.Key.OwnerType == DamageSourceOwner.NPC)
                owner = ContentSamples.NpcsByNetId[data.Key.OwnerID].FullName + "'s ";
        }
        switch (data.Key.SourceType)
        {
            case DamageSourceType.Projectile:
                source = ContentSamples.ProjectilesByType[data.Key.SourceID].Name;
                break;

            case DamageSourceType.Item:
                source = ContentSamples.ItemsByType[data.Key.SourceID].Name;
                break;

            case DamageSourceType.DoT:
                source = "Damage over Time";
                break;

            case DamageSourceType.Misc:
                source = "Misc.";
                break;

            case DamageSourceType.Environment:
                source = "DoT & Environment";
                break;
        }
        return (owner + source, $"{((float)(data.Value * 100) / totaldmg).ToString("0.00")}%", $"({data.Value} dmg)");

    }
    public override void PostUpdateEverything()
    {
        if (Main.npc.Any(x => x.active && x.boss))
        {
            if (!isBossAlive)
            {
                isBossAlive = true;
                bossTimeIGT = 0;
                bossTimeRTA = DateTime.UtcNow;
                HPGraphList_Boss = new();
                HPGraphList_Player = new();
                MaxHPGraph_Player = 0;
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

                int playerHPTotal = 0;
                int playerMaxHP = 0;
                foreach (var item in Main.ActivePlayers)
                {
                    playerHPTotal += item.statLife;
                    playerMaxHP += item.statLifeMax2;
                }

                HPGraphList_Boss.Add(bossHPTotal);
                HPGraphList_Player.Add(playerHPTotal);
                MaxHPGraph_Player = Math.Max(MaxHPGraph_Player, playerMaxHP);
            }
            bossTimeIGT++;
        }
        else
        {
            int goalTime = 0;
            var webhook = new WebhookManager.Webhook(Embeds: new List<WebhookManager.Embed> { });
            var dotWebhook = new WebhookManager.Webhook(Embeds: new List<WebhookManager.Embed> { });
            string rta = "";
            string igt = "";
            bool printed = false;
            if (dmgdata.Count > 0)
                lastSplits = null;
            foreach (var npc in dmgdata)
                if ((npc.Key.Index == -1) ? true : !Main.npc[npc.Key.Index].active || (Main.npc[npc.Key.Index].type != npc.Key.Type))
                //^ if there's no active bosses and this NPC isn't active anymore, we'll print out the damage results. We only do this if there's no active bosses so that bosses such as Moon Lord get all the data printed at the end of the fight and not throughout.
                {
                    if (CalamityGlobalNPC.BossKillTimes.Keys.Contains(npc.Key.Type))
                        goalTime = (int)MathHelper.Max(goalTime, CalamityGlobalNPC.BossKillTimes[npc.Key.Type]);
                    int totaldmg = 0;
                    foreach (KeyValuePair<DamageSourceData, int> i in npc.Value.OrderBy(key => key.Value)) //for each damage source, add that damage amount to the total damage counter
                    {
                        totaldmg += i.Value;
                    }
                    string output = $"[c/78ffa3:{npc.Key.Name}] [c/ababab:({totaldmg} dmg dealt)]"; //Prepare to print out the total damage dealt
                    var fieldvar = new List<WebhookManager.WebhookField> { }; //this is for discord webhook integration, ignore it
                    foreach (KeyValuePair<DamageSourceData, int> i in npc.Value.OrderBy(key => -key.Value)) //Runs for each damage source, sorted from highest damage dealt to lowest damage deal
                    {
                        var l = ConstructDamageLine(i, totaldmg);
                        output += $"\n{l.Item1}: [c/ f7d57e:{l.Item2}] [c/ ababab:{l.Item3}]"; //Prepare print out the damage source, the % of the total damage from that source, and the actual value of damage from that source
                                                                                               //totaldmg += i.Value;
                        fieldvar.Add(new WebhookManager.WebhookField(l.Item1, $"{l.Item2} {l.Item3}", false)); //adds that same info to the webhook
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
                    if (lastSplits == null)
                    {
                        lastSplits = output;
                    }
                    else
                        lastSplits += $"\n{output}";
                    if (FightStatsConfig.Instance.dmg) Main.NewText(output); //print all the data saved before in one message
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
                if (HPGraphList_Boss.Count() > 0)
                {
                    int maxHP = HPGraphList_Boss.Max();

                    LastBossHPGraph = new Texture2D(Main.graphics.GraphicsDevice, HPGraphList_Boss.Count(), 100);

                    var texData = new Color[100 * HPGraphList_Boss.Count()];
                    for (var i = 0; i < texData.Length; i++)
                    {
                        int x = i % HPGraphList_Boss.Count();
                        int y = i / HPGraphList_Boss.Count();
                        texData[i] = (1 - (y / 100f) > HPGraphList_Boss[x] / (float)maxHP) ? new Color(52, 66, 119) : (x % 120 < 60 ? Color.White : Color.WhiteSmoke);
                    }
                    LastBossHPGraph.SetData(texData);
                }
                if (HPGraphList_Player.Count() > 0)
                {
                    int maxHP = MaxHPGraph_Player;

                    LastPlayerHPGraph = new Texture2D(Main.graphics.GraphicsDevice, HPGraphList_Player.Count(), 100);

                    var texData = new Color[100 * HPGraphList_Player.Count()];
                    for (var i = 0; i < texData.Length; i++)
                    {
                        int x = i % HPGraphList_Player.Count();
                        int y = i / HPGraphList_Player.Count();
                        texData[i] = (1 - (y / 100f) > HPGraphList_Player[x] / (float)maxHP) ? new Color(52, 66, 119) : (x % 120 < 60 ? Color.White : Color.WhiteSmoke);
                    }
                    LastPlayerHPGraph.SetData(texData);
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
                lastIGT = $"[c/fab698:Duration] [c/ababab:({goalText} Goal)]\n{igt} IGT - {rta} RTA";
                if (FightStatsConfig.Instance.time) Main.NewText($"[c/773241:Boss Fight Length] ({goalText} goal time)\n{igt} IGT\n{rta} RTA");

                if (DiscordConfig.Instance.autowebhook) //this sends the webhook message if enabled in config
                {
                    Vector2 igt_size = ChatManager.GetStringSize(FontAssets.MouseText.Value, lastIGT, new Vector2(1)) + new Vector2(24);
                    Vector2 split_size = ChatManager.GetStringSize(FontAssets.MouseText.Value, lastSplits, new Vector2(1)) + new Vector2(24);

                    int spacer = 4;

                    var width = (new float[] { igt_size.X, split_size.X, 124 }).Max();
                    var height = 154f + spacer + 154f + spacer + igt_size.Y + spacer + split_size.Y;

                    RenderTarget2D target = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)width, (int)height);
                    Main.graphics.GraphicsDevice.SetRenderTarget(target);
                    Main.graphics.GraphicsDevice.Clear(Color.Transparent);
                    Main.spriteBatch.Begin(default, null, SamplerState.PointClamp, null, null);
                    var y = 0;
                    WebhookManager.DrawPanel(Main.spriteBatch, new(0, y, (int)width, 154));
                    Main.spriteBatch.Draw(dpsSystem.LastBossHPGraph, new Rectangle(12, y + 42, (int)width - 24, 100), Color.White);
                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, "Boss Health Graph", new(12, y + 12), Color.White, 0, Vector2.Zero, Vector2.One, -1, 1.5f);
                    y += 154 + spacer;
                    WebhookManager.DrawPanel(Main.spriteBatch, new(0, y, (int)width, 154));
                    Main.spriteBatch.Draw(dpsSystem.LastPlayerHPGraph, new Rectangle(12, y + 42, (int)width - 24, 100), Color.White);
                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, "Player Health Graph", new(12, y + 12), Color.White, 0, Vector2.Zero, Vector2.One, -1, 1.5f);
                    y += 154 + spacer;
                    WebhookManager.DrawTexbox(Main.spriteBatch, new Vector2(0, y), lastIGT);
                    y += (int)igt_size.Y + spacer;
                    WebhookManager.DrawTexbox(Main.spriteBatch, new Vector2(0, y), lastSplits);

                    Main.spriteBatch.End();
                    Main.graphics.GraphicsDevice.SetRenderTarget(null);
                    webhook.image = target;
                    target.Dispose();

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
    public DamageSourceData sourceData;
    public IEntitySource source = null;
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        sourceData = new DamageSourceData(projectile.owner, projectile.type, DamageSourceType.Projectile);
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
            sourceData.SourceType = DamageSourceType.Item;
            sourceData.SourceID = ((EntitySource_ItemUse)source).Item.type;
        }
        if (source is EntitySource_ItemUse)
        {
            sourceData.SourceType = DamageSourceType.Item;
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
                sourceData.OwnerType = DamageSourceOwner.NPC;
                sourceData.OwnerID = (s as NPC).type;
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
         ModContent.NPCType<FalseBrain>()
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
    public int previousHP = -1; // each NPC has a default value of -1 here so that I can tell they were newly spawned, not just healing from 0 hp to full.

    public int[] PlayerMiscDamage = new int[Main.maxPlayers];
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
        if (merge.Contains(npc.type) && Main.npc.Any(x => x.boss && x.active))
        {
            //Try to get the existing damage dictionary for this NPC. if it doesn't exist, create a new one.
            if (!dmgdata.TryGetValue(new NPCData(npc.type, -1), out var NpcDamageDict))
            {
                NpcDamageDict = new() { };
            }
            //Get the DamageSourceData and damage to add for this proj
            var sourceData = projectile.GetGlobalProjectile<ProjectileSourceManager>().sourceData;
            int damageToWrite = (npc.life < 0 ? damageDone + npc.life : damageDone);
            //Add it to the NPC's dictionary
            if (NpcDamageDict.ContainsKey(sourceData))
                NpcDamageDict[sourceData] += damageToWrite;
            else
                NpcDamageDict[sourceData] = damageToWrite;

            //update the parent dictionary
            dmgdata[new NPCData(npc.type, -1)] = NpcDamageDict;

            npc.GetGlobalNPC<damagecalcGlobalNPC>().previousHP -= damageDone; //updates this NPC's previousHP variable, which I used to calculate debuff/environmental damage sources. if there's a better way to calculate debuffs, tell me plz
            npc.GetGlobalNPC<damagecalcGlobalNPC>().PlayerMiscDamage[Main.player[projectile.owner].whoAmI] -= damageDone; //Also updates the misc dmage

        }
        else if ((npc.boss) && !blacklist.Contains(npc.type))
        {
            //Try to get the existing damage dictionary for this NPC. if it doesn't exist, create a new one.
            if (!dmgdata.TryGetValue(new NPCData(npc), out var NpcDamageDict))
            {
                NpcDamageDict = new() { };
            }
            //Get the DamageSourceData and damage to add for this proj
            var sourceData = projectile.GetGlobalProjectile<ProjectileSourceManager>().sourceData;
            int damageToWrite = (npc.life < 0 ? damageDone + npc.life : damageDone);
            //Add it to the NPC's dictionary
            if (!NpcDamageDict.ContainsKey(sourceData))
                NpcDamageDict[sourceData] = damageToWrite;
            else
                NpcDamageDict[sourceData] += damageToWrite;

            //update the parent dictionary
            dmgdata[new NPCData(npc)] = NpcDamageDict;

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
            npc = ContentSamples.NpcsByNetId[npc.type];
        }
        if (merge.Contains(npc.type))
        {
            if (!dmgdata.TryGetValue(new NPCData(npc.type, -1), out var NpcDamageDict))
            {
                NpcDamageDict = new() { };
            }
            var sourceData = new DamageSourceData(player.whoAmI, item.type, DamageSourceType.Item);
            int damageToWrite = (npc.life < 0 ? damageDone + npc.life : damageDone);
            if (!NpcDamageDict.ContainsKey(sourceData))
                NpcDamageDict[sourceData] = damageToWrite;
            else
                NpcDamageDict[sourceData] += damageToWrite;
            dmgdata[new NPCData(npc.type, -1)] = NpcDamageDict;

            npc.GetGlobalNPC<damagecalcGlobalNPC>().previousHP -= damageDone;
            npc.GetGlobalNPC<damagecalcGlobalNPC>().PlayerMiscDamage[player.whoAmI] -= damageDone;
        }
        else
            if ((npc.boss) && !blacklist.Contains(npc.type))
            {
                if (!dmgdata.TryGetValue(new NPCData(npc), out var NpcDamageDict))
                {
                    NpcDamageDict = new() { };
                }
                var sourceData = new DamageSourceData(player.whoAmI, item.type, DamageSourceType.Item);
                int damageToWrite = (npc.life < 0 ? damageDone + npc.life : damageDone);
                if (!NpcDamageDict.ContainsKey(sourceData))
                    NpcDamageDict[sourceData] = damageToWrite;
                else
                    NpcDamageDict[sourceData] += damageToWrite;
                dmgdata[new NPCData(npc)] = NpcDamageDict;

                npc.GetGlobalNPC<damagecalcGlobalNPC>().previousHP -= damageDone;
                npc.GetGlobalNPC<damagecalcGlobalNPC>().PlayerMiscDamage[player.whoAmI] -= damageDone;
            }
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


        if (isFirst && (npc.boss || merge.Contains(npc.type)) && (!blacklist.Contains(npc.type) || !replaceList.Keys.Contains(npc.type)))
        {
            //Calculate dots
            //This entire system should be redone to match the main damage system.
            if (Main.npc.Any(x => x.boss && x.active))
            {
                if (!dotdata.TryGetValue(new NPCData(npc.type, merge.Contains(npc.type) ? -1 : npc.whoAmI), out var y)) //check if NPC is already in the array. if it is, "y" becomes the Dictonary<string,int> that stores the damage sources for that NPC. if it isn't in the array, we'll create a new one below.
                {
                    y = new Dictionary<string, int> { };
                    dotdata.Add(new NPCData(npc.type, merge.Contains(npc.type) ? -1 : npc.whoAmI), y);
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
                dotdata.Remove(new NPCData(npc.type, merge.Contains(npc.type) ? -1 : npc.whoAmI)); //removes this NPC from the damage data list...
                dotdata.Add(new NPCData(npc.type, merge.Contains(npc.type) ? -1 : npc.whoAmI), y); //... so we can add it back with the update damage values
            }

            if (npc.life != previousHP && previousHP != -1) //if their HP is different than it was last tick and last tick it wasn't -1 (aka newly spawned), we'll run this code to log the damage as a "DoT & Environment" damage source. Everything in this IF statement works the same as adding the projectile damage to the Dict, but with a fixed Damage Source string.
            {
                if (!dmgdata.TryGetValue(new NPCData(npc.type, (merge.Contains(npc.type) ? -1 : npc.whoAmI)), out var NpcDamageDict))
                {
                    NpcDamageDict = new() { };
                }
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
                        dmgdata[new NPCData(npc.type, (merge.Contains(npc.type) ? -1 : npc.whoAmI))] = NpcDamageDict;
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
                if (npc.life != previousHP)
                {
                    var sourceData = new DamageSourceData(-1, -1, DamageSourceType.Environment);
                    if (!NpcDamageDict.ContainsKey(sourceData))
                        NpcDamageDict[sourceData] = previousHP - npc.life;
                    else
                        NpcDamageDict[sourceData] += previousHP - npc.life;
                }

            }
            previousHP = npc.life;
        }
    }
}
