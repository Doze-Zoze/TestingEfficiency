using CalamityMod;
using CalamityMod.Projectiles.Ranged;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TestingEfficiency.Commands;

public class BossDeathCmd : ModCommand
{
	public override CommandType Type => CommandType.Chat;

	public override string Command => "boss";

	public override string Description => "Set Boss Progression";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
        if (args.Length < 1)
        {
            Main.NewText("Please use the following syntaxes:\n/boss <toggle>, /boss all, /boss list \nValid Presets are: ks, ds, eoc, crab, evil1, evil2, qb, deerclops, skeletron, sg, wof, qs, cryo, destroyer, as, twins, brimmy, prime, calclone, plantera, levi, aureus, golem, pbg, eol, fishron, ravager, cultist, deus, ml, guardians, folly, provi, sw, cv, signus, polter, od, dog, yharon, exos, scal");
            return;
        }
        if (args[0] == "list")
        {
            Main.NewText($"ks: {NPC.downedSlimeKing}");
            Main.NewText($"eoc: {NPC.downedBoss1}");
            Main.NewText($"ds: {DownedBossSystem.downedDesertScourge}");
            Main.NewText($"crab: {DownedBossSystem.downedCrabulon}");
            Main.NewText($"evil1: {NPC.downedBoss2}");
            Main.NewText($"hivemind: {DownedBossSystem.downedHiveMind}");
            Main.NewText($"perfs: {DownedBossSystem.downedPerforator}");
            Main.NewText($"qb: {NPC.downedQueenBee}");
            Main.NewText($"deerclops: {NPC.downedDeerclops}");
            Main.NewText($"skeletron: {NPC.downedBoss3}");
            Main.NewText($"wof: {Main.hardMode}");
            Main.NewText($"sg: {DownedBossSystem.downedSlimeGod}");
            Main.NewText($"qs: {NPC.downedQueenSlime}");
            Main.NewText($"cryo: {DownedBossSystem.downedCryogen}");
            Main.NewText($"destroyer: {NPC.downedMechBoss1}");
            Main.NewText($"as: {DownedBossSystem.downedAquaticScourge}");
            Main.NewText($"twins: {NPC.downedMechBoss2}");
            Main.NewText($"brimmy: {DownedBossSystem.downedBrimstoneElemental}");
            Main.NewText($"prime: {NPC.downedMechBoss3}");
            Main.NewText($"calclone: {DownedBossSystem.downedCalamitasClone}");
            Main.NewText($"plantera: {NPC.downedPlantBoss}");
            Main.NewText($"levi: {DownedBossSystem.downedLeviathan}");
            Main.NewText($"aureus: {DownedBossSystem.downedAstrumAureus}");
            Main.NewText($"golem: {NPC.downedGolemBoss}");
            Main.NewText($"pbg: {DownedBossSystem.downedPlaguebringer}");
            Main.NewText($"eol: {NPC.downedEmpressOfLight}");
            Main.NewText($"fishron: {NPC.downedFishron}");
            Main.NewText($"ravager: {DownedBossSystem.downedRavager}");
            Main.NewText($"cultist: {NPC.downedAncientCultist}");
            Main.NewText($"deus: {DownedBossSystem.downedAstrumDeus}");
            Main.NewText($"ml: {NPC.downedMoonlord}");
            Main.NewText($"guardians: {DownedBossSystem.downedGuardians}");
            Main.NewText($"folly: {DownedBossSystem.downedDragonfolly}");
            Main.NewText($"provi: {DownedBossSystem.downedProvidence}");
            Main.NewText($"cv: {DownedBossSystem.downedCeaselessVoid}");
            Main.NewText($"signus: {DownedBossSystem.downedSignus}");
            Main.NewText($"sw: {DownedBossSystem.downedStormWeaver}");
            Main.NewText($"polter: {DownedBossSystem.downedPolterghast}");
            Main.NewText($"od: {DownedBossSystem.downedBoomerDuke}");
            Main.NewText($"dog: {DownedBossSystem.downedDoG}");
            Main.NewText($"yharon: {DownedBossSystem.downedYharon}");
            Main.NewText($"exos: {DownedBossSystem.downedExoMechs}");
            Main.NewText($"scal: {DownedBossSystem.downedCalamitas}");
            return;
        }

        if (args[0] == "ks" || args[0] == "all")
        {
            NPC.downedSlimeKing = !NPC.downedSlimeKing;
        }
        if (args[0] == "eoc" || args[0] == "all")
        {
            NPC.downedBoss1 = !NPC.downedBoss1;
        }
        if (args[0] == "ds" || args[0] == "all")
        {
            DownedBossSystem.downedDesertScourge = !DownedBossSystem.downedDesertScourge;
        }
        if (args[0] == "crab" || args[0] == "all")
        {
            DownedBossSystem.downedCrabulon = !DownedBossSystem.downedCrabulon;
        }
        if (args[0] == "evil1" || args[0] == "all")
        {
            NPC.downedBoss2 = !NPC.downedBoss2;
        }
        if (args[0] == "evil2" && DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator)
        {
            DownedBossSystem.downedHiveMind = false;
            DownedBossSystem.downedPerforator = false;
        }
        if (args[0] == "evil2" && !DownedBossSystem.downedHiveMind && !DownedBossSystem.downedPerforator)
        {
            DownedBossSystem.downedHiveMind = true;
            DownedBossSystem.downedPerforator = true;
        }
        if (args[0] == "qb" || args[0] == "all")
        {
            NPC.downedQueenBee = !NPC.downedQueenBee;
        }
        if (args[0] == "deerclops" || args[0] == "all")
        {
            NPC.downedDeerclops = !NPC.downedDeerclops;
        }
        if (args[0] == "skeletron" || args[0] == "all")
        {
            NPC.downedBoss3 = !NPC.downedBoss3;
        }
        if (args[0] == "wof" || args[0] == "all")
        {
            Main.hardMode = !Main.hardMode;
        }
        if (args[0] == "sg" || args[0] == "all")
        {
            DownedBossSystem.downedSlimeGod = !DownedBossSystem.downedSlimeGod;
        }
        if (args[0] == "qs" || args[0] == "all")
        {
            NPC.downedQueenSlime = !NPC.downedQueenSlime;
        }
        if (args[0] == "cryo" || args[0] == "all")
        {
            DownedBossSystem.downedCryogen = !DownedBossSystem.downedCryogen;
        }
        if (args[0] == "destroyer" || args[0] == "all")
        {
            NPC.downedMechBoss1 = !NPC.downedMechBoss1;
        }
        if (args[0] == "as" || args[0] == "all")
        {
            DownedBossSystem.downedAquaticScourge = !DownedBossSystem.downedAquaticScourge;
        }
        if (args[0] == "twins" || args[0] == "all")
        {
            NPC.downedMechBoss2 = !NPC.downedMechBoss2;
        }
        if (args[0] == "brimmy" || args[0] == "all")
        {
            DownedBossSystem.downedBrimstoneElemental = !DownedBossSystem.downedBrimstoneElemental;
        }
        if (args[0] == "prime" || args[0] == "all")
        {
            NPC.downedMechBoss3 = !NPC.downedMechBoss3;
        }
        if (args[0] == "calclone" || args[0] == "all")
        {
            DownedBossSystem.downedCalamitasClone = !DownedBossSystem.downedCalamitasClone;
        }
        if (args[0] == "plantera" || args[0] == "all")
        {
            NPC.downedPlantBoss = !NPC.downedPlantBoss;
        }
        if (args[0] == "levi" || args[0] == "all")
        {
            DownedBossSystem.downedLeviathan = !DownedBossSystem.downedLeviathan;
        }
        if (args[0] == "aureus" || args[0] == "all")
        {
            DownedBossSystem.downedAstrumAureus = !DownedBossSystem.downedAstrumAureus;
        }
        if (args[0] == "golem" || args[0] == "all")
        {
            NPC.downedGolemBoss = !NPC.downedGolemBoss;
        }
        if (args[0] == "pbg" || args[0] == "all")
        {
            DownedBossSystem.downedPlaguebringer = !DownedBossSystem.downedPlaguebringer;
        }
        if (args[0] == "eol" || args[0] == "all")
        {
            NPC.downedEmpressOfLight = !NPC.downedEmpressOfLight;
        }
        if (args[0] == "fishron" || args[0] == "all")
        {
            NPC.downedFishron = !NPC.downedFishron;
        }
        if (args[0] == "ravager" || args[0] == "all")
        {
            DownedBossSystem.downedRavager = !DownedBossSystem.downedRavager;
        }
        if (args[0] == "cultist" || args[0] == "all")
        {
            NPC.downedAncientCultist = !NPC.downedAncientCultist;
        }
        if (args[0] == "deus" || args[0] == "all")
        {
            DownedBossSystem.downedAstrumDeus = !DownedBossSystem.downedAstrumDeus;
        }
        if (args[0] == "ml" || args[0] == "all")
        {
            NPC.downedMoonlord = !NPC.downedMoonlord;
        }
        if (args[0] == "guardians" || args[0] == "all")
        {
            DownedBossSystem.downedGuardians = !DownedBossSystem.downedGuardians;
        }
        if (args[0] == "folly" || args[0] == "all")
        {
            DownedBossSystem.downedDragonfolly = !DownedBossSystem.downedDragonfolly;
        }
        if (args[0] == "provi" || args[0] == "all")
        {
            DownedBossSystem.downedProvidence = !DownedBossSystem.downedProvidence;
        }
        if (args[0] == "cv" || args[0] == "all")
        {
            DownedBossSystem.downedCeaselessVoid = !DownedBossSystem.downedCeaselessVoid;
        }
        if (args[0] == "signus" || args[0] == "all")
        {
            DownedBossSystem.downedSignus = !DownedBossSystem.downedSignus;
        }
        if (args[0] == "sw" || args[0] == "all")
        {
            DownedBossSystem.downedStormWeaver = !DownedBossSystem.downedStormWeaver;
        }
        if (args[0] == "polter" || args[0] == "all")
        {
            DownedBossSystem.downedPolterghast = !DownedBossSystem.downedPolterghast;
        }
        if (args[0] == "od" || args[0] == "all")
        {
            DownedBossSystem.downedBoomerDuke = !DownedBossSystem.downedBoomerDuke;
        }
        if (args[0] == "dog" || args[0] == "all")
        {
            DownedBossSystem.downedDoG = !DownedBossSystem.downedDoG;
        }
        if (args[0] == "yharon" || args[0] == "all")
        {
            DownedBossSystem.downedYharon = !DownedBossSystem.downedYharon;
        }
        if (args[0] == "exos" || args[0] == "all")
        {
            DownedBossSystem.downedExoMechs = !DownedBossSystem.downedExoMechs;
        }
        if (args[0] == "scal" || args[0] == "all")
        {
            DownedBossSystem.downedCalamitas = !DownedBossSystem.downedCalamitas;
        }

    }
}
