using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityMod.Items.PermanentBoosters;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace TestingEfficiency.Commands;

public class configCmd : ModCommand
{
	public override CommandType Type => CommandType.Chat;

	public override string Command => "config";

	public override string Description => "Add/Edit Presets";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		if (args.Length < 2)
		{
			Main.NewText("Usage: /config <class> <preset-name>");
			return;
		}
		if (!new List<string> { "melee", "ranger", "mage", "summoner", "rogue" }.Contains(args[0]))
		{
			Main.NewText("Not a valid class! Use one of the following:\nmelee, ranger, mage, summoner, rogue");
			return;
        }
        Player player = caller.Player;
        Save(player, args);
        
	}

	public static List<string> Save(Player player, string[] args)
	{
        List<string> loadout = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            Item item = player.armor[i];
            loadout.Add(configCmd.GetFullNameById(item.type));
        }
        if (args[0] == "melee")
        {
            if (TestingLoadouts.Instance.meleeLoadouts.ContainsKey(args[1]))
            {
                TestingLoadouts.Instance.meleeLoadouts[args[1]] = loadout;
            }
            else
            {
                TestingLoadouts.Instance.meleeLoadouts.Add(args[1], loadout);
            }
        }
        if (args[0] == "ranger")
        {
            if (TestingLoadouts.Instance.rangerLoadouts.ContainsKey(args[1]))
            {
                TestingLoadouts.Instance.rangerLoadouts[args[1]] = loadout;
            }
            else
            {
                TestingLoadouts.Instance.rangerLoadouts.Add(args[1], loadout);
            }
        }
        if (args[0] == "mage")
        {
            if (TestingLoadouts.Instance.mageLoadouts.ContainsKey(args[1]))
            {
                TestingLoadouts.Instance.mageLoadouts[args[1]] = loadout;
            }
            else
            {
                TestingLoadouts.Instance.mageLoadouts.Add(args[1], loadout);
            }
        }
        if (args[0] == "summoner")
        {
            if (TestingLoadouts.Instance.summonerLoadouts.ContainsKey(args[1]))
            {
                TestingLoadouts.Instance.summonerLoadouts[args[1]] = loadout;
            }
            else
            {
                TestingLoadouts.Instance.summonerLoadouts.Add(args[1], loadout);
            }
        }
        if (args[0] == "rogue")
        {
            if (TestingLoadouts.Instance.rogueLoadouts.ContainsKey(args[1]))
            {
                TestingLoadouts.Instance.rogueLoadouts[args[1]] = loadout;
            }
            else
            {
                TestingLoadouts.Instance.rogueLoadouts.Add(args[1], loadout);
            }
        }
        MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
        if ((object)saveMethodInfo != null)
        {
            saveMethodInfo.Invoke(null, new object[1] { TestingLoadouts.Instance });
            return loadout;
        }
        throw new Exception("A file could not be created, or updated at:\n'{path}'\n\n If you ARE using Onedrive, please reinstall tModloader in a different location.\n If you ARE NOT using Onedrive, please disable Windows Real-Time protection.");
    }

	internal static string GetFullNameById(int id)
	{
		ModItem modItem = ItemLoader.GetItem(id);
		if (modItem != null)
		{
			return modItem.Mod.Name + ":" + modItem.Name;
		}
		if (id < 5125)
		{
			return "Terraria:" + ItemID.Search.GetName(id);
		}
		return null;
	}
}
