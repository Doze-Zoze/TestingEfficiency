/*using System.Collections.Generic;
using CalamityMod.Items.PermanentBoosters;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.WorldBuilding;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using System.Runtime.InteropServices;
using Terraria.ObjectData;
using CalamityMod.Systems;
using CalamityMod.World;
using CalamityMod.Tiles.Astral;
using CalamityMod.Tiles.SunkenSea;


namespace TestingEfficiency.Commands;

public class SubworldCmd : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "arena";

    public override string Description => "Enter the Arena subworld";

    public static string biome = "forest";
    public static string layer = "surface";
    public static int distance = 20;
    public static int width = 1000;
    public static int height = 1000;

    public static List<string> biomeList = new List<string>()
    {
        "forest",
        "corruption",
        "crimson",
        "snow",
        "desert",
        "jungle",
        "astral",
        "sunkensea",
        "hallow"
    };

    public static List<string> Layer = new List<string>()
    {
        "space",
        "surface",
        "underground",
        "cavern",
        "hell"
    };


    public override void Action(CommandCaller caller, string input, string[] args)
    {
        //IL_01b4: Unknown result type (might be due to invalid IL or missing references)
        //IL_01bb: Expected O, but got Unknown
        if (args.Length == 0)
        {

            if (SubworldSystem.IsActive<Arena>())
            {
                SubworldSystem.Exit();
                return;
            } else
            {

                Main.NewText("Please use one of the following syntaxes. Note that unset values will be set to their default value:\n/arena <platform distance> <biome> <layer> <width> <height>\n/arena help");
                return;
            }
        }
        distance = (args.Length >= 1 ? (int.TryParse(args[0], out int x) ? x : 20 ) : 20);

        biome = (args.Length >= 2 ? args[1] : "forest");

        layer = (args.Length >= 3 ? args[2] : "surface");

        width = (args.Length >= 4 ? (int.TryParse(args[3], out x) ? x : 3000) : 3000);

        height = (args.Length >= 5 ? (int.TryParse(args[4], out x) ? x : 1000) : 1000);

        if (!SubworldSystem.IsActive<Arena>())
        {
            SubworldSystem.Enter<Arena>();
        }
        else
        {
            SubworldSystem.Exit();
        }
    }
}

public class SubworldPersistance : ModSystem
{
    public static bool revenge;
    public static bool death;
    public override void PreUpdateNPCs()
    {
        if (SubworldSystem.IsActive<Arena>())
        {
            return;
        }
        revenge = CalamityWorld.revenge;
        death = CalamityWorld.death;
    }
}

public class Arena : Subworld
{
    public override int Width => SubworldCmd.width;
    public override int Height => SubworldCmd.height;

    public override bool ShouldSave => false;
    public override bool NoPlayerSaving => false;


    public override List<GenPass> Tasks => new List<GenPass>()
    {
        new ArenaGenPass()
    };

    // Sets the time to the middle of the day whenever the subworld loads
    public override void OnLoad()
    {
        CalamityWorld.revenge = SubworldPersistance.revenge;
        CalamityWorld.death = SubworldPersistance.death;
    }

    public override void Update()
    {

        foreach (var player in Main.ActivePlayers)
        {
            player.AddBuff(BuffID.Campfire, 60);

            player.AddBuff(BuffID.HeartLamp, 60);

            player.AddBuff(BuffID.CatBast, 60);

            player.AddBuff(BuffID.StarInBottle, 60);

            player.AddBuff(BuffID.Sunflower, 60);
        }
    }


}

public class ArenaGenPass : GenPass
{
    //TODO: remove this once tML changes generation passes
    public ArenaGenPass() : base("Terrain", 1) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating terrain";
        if (SubworldCmd.layer == "surface")
        {

            Main.worldSurface = SubworldCmd.height;
            Main.rockLayer = Main.worldSurface+= 50;
        } else if (SubworldCmd.layer == "underground")
        {
            Main.worldSurface = 0;
            Main.rockLayer = SubworldCmd.height;
        } else
        {
            Main.worldSurface = 0;
            Main.rockLayer = 0;
        }
        
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                Tile tile = Main.tile[i, j];
                if (j % SubworldCmd.distance == 0)
                {
                    tile.HasTile = true;
                    tile.TileType = TileID.TeamBlockWhitePlatform;
                }
                else if ((j % SubworldCmd.distance % 10 == 1) || (j % SubworldCmd.distance % 10 == 2))
                {
                    if (SubworldCmd.biome == "forest")
                    {
                    }
                    else if (SubworldCmd.biome == "corruption")
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.Ebonstone;
                        tile.IsActuated = true;
                        tile.IsTileInvisible = true;
                    }
                    else if (SubworldCmd.biome == "crimson")
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.Crimstone;
                        tile.IsActuated = true;
                        tile.IsTileInvisible = true;
                    }
                    else if (SubworldCmd.biome == "crimson")
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.Crimstone;
                        tile.IsActuated = true;
                        tile.IsTileInvisible = true;
                    }
                    else if (SubworldCmd.biome == "snow")
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.IceBlock;
                        tile.IsActuated = true;
                        tile.IsTileInvisible = true;
                    }
                    else if (SubworldCmd.biome == "desert")
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.HardenedSand;
                        tile.IsActuated = true;
                        tile.IsTileInvisible = true;
                    }
                    else if (SubworldCmd.biome == "hallow")
                    {
                        tile.HasTile = true;
                        tile.TileType = TileID.Pearlstone;
                        tile.IsActuated = true;
                        tile.IsTileInvisible = true;
                    }
                    else if (SubworldCmd.biome == "astral")
                    {
                        WorldGen.PlaceTile(i, j, ModContent.TileType<AstralDirt>());
                        tile.HasTile = true;
                        tile.IsActuated = true;
                        tile.IsTileInvisible = true;
                    }
                    else if (SubworldCmd.biome == "sunkensea")
                    {
                        WorldGen.PlaceTile(i, j, ModContent.TileType<EutrophicSand>());
                        tile.HasTile = true;
                        tile.IsActuated = true;
                        tile.IsTileInvisible = true;
                    }
                }
                else if ((j % SubworldCmd.distance % 10 == 3) && i % 10 == 0)
                {
                    tile.HasTile = true;
                    tile.TileType = TileID.Torches;
                    tile.IsTileInvisible = true;
                }
                tile.WallType = WallID.StoneSlab;
                tile.IsWallInvisible = true;
            }
        }
    }
}
*/