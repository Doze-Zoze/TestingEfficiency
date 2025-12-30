using System.Collections.Generic;
using CalamityMod.Items.PermanentBoosters;
using Terraria;
using Terraria.ModLoader;

namespace TestingEfficiency.Commands;

public class ReforgeCmd : ModCommand
{
	public override CommandType Type => CommandType.Chat;

	public override string Command => "reforge";

	public override string Description => "Reforge Accessories";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		Player player = caller.Player;
        ApplyReforge(player, args);


    }

	public static void ApplyReforge(Player player, string[] args)
	{
        string errorMsg = string.Empty;
        Dictionary<string, int> preset = new Dictionary<string, int>
        {
            { "lucky", 0 },
            { "menacing", 1 },
            { "violent", 2 },
            { "lucky.offense", 3 },
            { "menacing.offense", 4 },
            { "violent.offense", 5 },
            { "warding", 6 }
        };
        if (args.Length == 0)
        {
            Main.NewText("Please use one of the following syntaxes:\n/reforge <preset>\n/reforge help");
            return;
        }
        if (!preset.ContainsKey(args[0]))
        {
            foreach (string item in preset.Keys)
            {
                errorMsg = ((!(errorMsg != string.Empty)) ? (errorMsg + item) : (errorMsg + ", " + item));
            }
            Main.NewText("Please use one of the following presets:\n" + errorMsg);
            return;
        }
        if (preset[args[0]] == 0)
        {
            player.armor[3].prefix = 65;
            player.armor[4].prefix = 65;
            player.armor[5].prefix = 65;
            player.armor[6].prefix = 68;
            player.armor[7].prefix = 68;
            player.armor[8].prefix = 68;
            player.armor[9].prefix = 65;
        }
        else if (preset[args[0]] == 1)
        {
            player.armor[3].prefix = 65;
            player.armor[4].prefix = 65;
            player.armor[5].prefix = 65;
            player.armor[6].prefix = 72;
            player.armor[7].prefix = 72;
            player.armor[8].prefix = 72;
            player.armor[9].prefix = 65;
        }
        else if (preset[args[0]] == 2)
        {
            player.armor[3].prefix = 65;
            player.armor[4].prefix = 65;
            player.armor[5].prefix = 65;
            player.armor[6].prefix = 80;
            player.armor[7].prefix = 80;
            player.armor[8].prefix = 68;
            player.armor[9].prefix = 65;
        }
        if (preset[args[0]] == 3)
        {
            player.armor[3].prefix = 68;
            player.armor[4].prefix = 68;
            player.armor[5].prefix = 68;
            player.armor[6].prefix = 68;
            player.armor[7].prefix = 68;
            player.armor[8].prefix = 68;
            player.armor[9].prefix = 68;
        }
        if (preset[args[0]] == 4)
        {
            player.armor[3].prefix = 72;
            player.armor[4].prefix = 72;
            player.armor[5].prefix = 72;
            player.armor[6].prefix = 72;
            player.armor[7].prefix = 72;
            player.armor[8].prefix = 72;
            player.armor[9].prefix = 72;
        }
        if (preset[args[0]] == 5)
        {
            player.armor[3].prefix = 80;
            player.armor[4].prefix = 80;
            player.armor[5].prefix = 80;
            player.armor[6].prefix = 68;
            player.armor[7].prefix = 68;
            player.armor[8].prefix = 68;
            player.armor[9].prefix = 68;
        }
        if (preset[args[0]] == 6)
        {
            player.armor[3].prefix = 65;
            player.armor[4].prefix = 65;
            player.armor[5].prefix = 65;
            player.armor[6].prefix = 65;
            player.armor[7].prefix = 65;
            player.armor[8].prefix = 65;
            player.armor[9].prefix = 65;
        }
    }
}
