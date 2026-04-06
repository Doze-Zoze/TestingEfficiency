using CalamityMod.Buffs.Summon;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using TestingEfficiency.Helpers;
using static TestingEfficiency.DataStructures;

namespace TestingEfficiency.DamageStats;


public class DamageStatsSystem : ModSystem
{
    public static BossTestData lastTestData;

    public static TestData CurrentTestSession = new();

    public static int bossTimeIGT;

    public static DateTime bossTimeRTA;

    public static bool isBossAlive = false;

    public static int bossRushTimeIGT = 0;

    public static DateTime bossRushTimeRTA;

    public static Dictionary<NPCData, Dictionary<DamageSourceData, int>> DamageData = new Dictionary<NPCData, Dictionary<DamageSourceData, int>>(); //This is where I store the damage data. the NPCData refers to the NPC, the String refers to the damage source, and the int is the damage amount.

    public static Dictionary<NPCData, Dictionary<DamageSourceData, int>> DoTData = new Dictionary<NPCData, Dictionary<DamageSourceData, int>>();

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
        if (Main.npc.Any(x => x.active && (x.boss || DamageStatsRecorder.countsAsBoss.Contains(x.type))))
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
                    if ((item.boss || DamageStatsRecorder.countsAsBoss.Contains(item.type)) && !DamageStatsRecorder.blacklist.Contains(item.type))
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
            var webhook = new WebhookManager.Webhook();
            string rta = "";
            string igt = "";
            bool printed = false;
            (string name,int dmgdone) bossName = ("",0);
            if (DamageData.Count > 0)
                lastSplits = null;
            foreach (var npc in DamageData)
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

                    if (totaldmg > bossName.dmgdone)
                        bossName = (npc.Key.Name, totaldmg);

                    foreach (KeyValuePair<DamageSourceData, int> i in npc.Value.OrderBy(key => -key.Value)) //Runs for each damage source, sorted from highest damage dealt to lowest damage deal
                    {
                        var l = ConstructDamageLine(i, totaldmg);
                        output += $"\n{l.Item1}: [c/ f7d57e:{l.Item2}] [c/ ababab:{l.Item3}]"; //Prepare print out the damage source, the % of the total damage from that source, and the actual value of damage from that
                    }

                    //DoT
                    if (DoTData.Any(x => x.Key.Index == npc.Key.Index))
                    {
                        int totalRegen = 0;
                        var dotData = DoTData.First(x => x.Key.Index == npc.Key.Index);
                        foreach (var dot in dotData.Value)
                        {
                            totalRegen += dot.Value;
                        }
                        output += $"\n[c/78ffa3:DoT Estimates] [c/ababab:({-totalRegen / 120} dmg dealt)]";

                        foreach (var dot in dotData.Value)
                        {
                            if (dot.Value == 0)
                                continue;
                            output += $"\n{Lang.GetBuffName(dot.Key.SourceID)}: [c/f7d57e:{((float)(dot.Value * 100) / totalRegen).ToString("0.00")}%] [c/ababab:({-dot.Value / 120} dmg)]";
                        }

                        DoTData.Remove(dotData.Key);
                    }
                    if (lastSplits == null)
                    {
                        lastSplits = output;
                    }
                    else
                        lastSplits += $"\n{output}";
                    if (FightStatsConfig.Instance.dmg) Main.NewText(output); //print all the data saved before in one message
                    DamageData.Remove(npc.Key); //removes that NPC from the damage list, now that the info has been printed out
                    DoTData.Remove(npc.Key);
                    printed = true;
                }
            if (printed)
            {
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
                lastTestData = new();

                lastTestData.time = bossTimeIGT/60;

                if (!Main.player.Any(x => x.active && !x.dead))
                    lastTestData.died = (HPGraphList_Boss.Last() / (float)HPGraphList_Boss.Max());

                if (!string.IsNullOrEmpty(bossName.name))
                    lastTestData.name = bossName.name;

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
                    Main.spriteBatch.Draw(DamageStatsSystem.LastBossHPGraph, new Rectangle(12, y + 42, (int)width - 24, 100), Color.White);
                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, "Boss Health Graph", new(12, y + 12), Color.White, 0, Vector2.Zero, Vector2.One, -1, 1.5f);
                    y += 154 + spacer;
                    WebhookManager.DrawPanel(Main.spriteBatch, new(0, y, (int)width, 154));
                    Main.spriteBatch.Draw(DamageStatsSystem.LastPlayerHPGraph, new Rectangle(12, y + 42, (int)width - 24, 100), Color.White);
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




