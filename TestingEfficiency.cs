using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestingEfficiency;

public class TestingEfficiency : Mod
{
    public static bool CalamityLoaded = false;

    public static Mod CalamityMod = null;
    public override void Load()
    {

        CalamityLoaded = ModLoader.TryGetMod("CalamityMod", out CalamityMod);
        //Terraria.On_Main.DamageVar_float_float += On_Main_DamageVar_float_float;
    }
    /*
    private int On_Main_DamageVar_float_float(On_Main.orig_DamageVar_float_float orig, float dmg, float luck)
    {
        if (MiscConfig.Instance.ChangeDmgSpread) return (int)(dmg* (1- MiscConfig.Instance.DmgSpread + Main.rand.NextFloat(MiscConfig.Instance.DmgSpread)*2f));
        return orig(dmg, luck);
    }*/

    public static List<(string name, float tier, Func<bool> getter, Action<bool> setter, Func<Asset<Texture2D>> texture)> BossTogles = new()
    {
        ("King Slime", 1f,
            () => NPC.downedSlimeKing,
            x => NPC.downedSlimeKing = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.KingSlime]]),

        ("Eye of Cthulhu", 2f,
            () => NPC.downedBoss1,
            x => NPC.downedBoss1 = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.EyeofCthulhu]]),

        ("Eater of Worlds / Brain of Cthulhu", 3f,
            () => NPC.downedBoss2,
            x => NPC.downedBoss2 = x,
            () => TextureAssets.NpcHeadBoss[
                NPCID.Sets.BossHeadTextures[
                    WorldGen.crimson ? NPCID.BrainofCthulhu : NPCID.EaterofWorldsHead
                ]
            ]),

        ("Queen Bee", 4f,
            () => NPC.downedQueenBee,
            x => NPC.downedQueenBee = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.QueenBee]]),

        ("Deerclops", 5f,
            () => NPC.downedDeerclops,
            x => NPC.downedDeerclops = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.Deerclops]]),

        ("Skeletron", 6f,
            () => NPC.downedBoss3,
            x => NPC.downedBoss3 = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.SkeletronHead]]),

        ("Wall of Flesh", 7f,
            () => Main.hardMode,
            x => Main.hardMode = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.WallofFlesh]]),

        ("Queen Slime", 8f,
            () => NPC.downedQueenSlime,
            x => NPC.downedQueenSlime = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.QueenSlimeBoss]]),

        ("The Twins", 9f,
            () => NPC.downedMechBoss2,
            x => NPC.downedMechBoss2 = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.Spazmatism]]),

        ("The Destroyer", 10f,
            () => NPC.downedMechBoss1,
            x => NPC.downedMechBoss1 = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.TheDestroyer]]),

        ("Skeletron Prime", 11f,
            () => NPC.downedMechBoss3,
            x => NPC.downedMechBoss3 = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.SkeletronPrime]]),

        ("Plantera", 12f,
            () => NPC.downedPlantBoss,
            x => NPC.downedPlantBoss = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.Plantera]]),

        ("Golem", 13f,
            () => NPC.downedGolemBoss,
            x => NPC.downedGolemBoss = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.GolemHead]]),

        ("Duke Fishron", 14f,
            () => NPC.downedFishron,
            x => NPC.downedFishron = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.DukeFishron]]),

        ("Empress of Light", 15f,
            () => NPC.downedEmpressOfLight,
            x => NPC.downedEmpressOfLight = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.HallowBoss]]),

        ("Lunatic Cultist", 16f,
            () => NPC.downedAncientCultist,
            x => NPC.downedAncientCultist = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.CultistBoss]]),

        ("Moon Lord", 17f,
            () => NPC.downedMoonlord,
            x => NPC.downedMoonlord = x,
            () => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.MoonLordHead]])
    };
    public override object Call(params object[] args)
    {
        if (args.Length < 1)
            return new ArgumentException("No arguments provided");
        switch (args[0] as string)
        {
            case "RegisterBoss":
                if (args.Length < 6)
                    return new ArgumentException("RegisterBoss requires 5 arguments: string name, float tier, Func<bool> getter, Action<bool> setter, Func<Asset<Texture2D>> texture");

                if (args[1] is not string name)
                    return new ArgumentException("Argument 1 must be a string representing the boss name");

                if (args[2] is not float tier)
                    return new ArgumentException("Argument 2 must be a float representing the boss tier");

                if (args[3] is not Func<bool> getter)
                    return new ArgumentException("Argument 3 must be a Func<bool> representing the downed check");

                if (args[4] is not Action<bool> setter)
                    return new ArgumentException("Argument 4 must be an Action<bool> representing the downed setter");

                if (args[5] is not Func<Asset<Texture2D>> texture)
                    return new ArgumentException("Argument 5 must be a Func<Asset<Texture2D>> representing the boss icon");

                BossTogles.Add((name, tier, getter, setter, texture));

                return null;
            default:
                return new ArgumentException("Provided arguments did not match any TestingEfficiency mod calls");
        }
    }
}
