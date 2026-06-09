using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestingEfficiency.Commands;

public class MeleeGearCmd : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "melee";

    public override string Description => "Equip Melee Gear";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        //IL_01b4: Unknown result type (might be due to invalid IL or missing references)
        //IL_01bb: Expected O, but got Unknown
        if (args.Length == 0)
        {
            Main.NewText("Please use one of the following syntaxes:\n/melee <preset>\n/melee help");
            return;
        }
        Dictionary<string, List<string>> gear = TestingLoadouts.Instance.meleeLoadouts;
        string errorMsg = string.Empty;
        if (!gear.ContainsKey(args[0]))
        {
            foreach (string item in gear.Keys)
            {
                errorMsg = ((!(errorMsg != string.Empty)) ? (errorMsg + item) : (errorMsg + ", " + item));
            }
            Main.NewText("Please use one of the following presets:\n" + errorMsg);
            return;
        }
        Player player = caller.Player;
        for (int i = 0; i < gear[args[0]].Count; i++)
        {
            Item item2 = new Item();
            if (gear[args[0]][i].Split(":")[0] != "Terraria")
            {
                item2 = new Item(ItemID.Search.GetId(gear[args[0]][i].Split(":")[0] + "/" + gear[args[0]][i].Split(":")[1]));
            }
            else if (gear[args[0]][i].Split(":")[1] != "None")
            {
                item2 = new Item(ItemID.Search.GetId(gear[args[0]][i].Split(":")[1]));
            }
            player.armor[i] = item2;
        }
        new ReforgeCmd().Action(caller, input, new string[1] { "menacing" });
    }
}






