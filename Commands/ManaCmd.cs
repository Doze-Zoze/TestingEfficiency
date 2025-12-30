using CalamityMod;
using CalamityMod.Items.PermanentBoosters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TestingEfficiency.Commands
{
    public class ManaCmd : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "mana";

        public override string Description => "Sets Mana permanent upgrades to the specified value";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int value))
            {
                Main.NewText("Please use the following syntax:\n/mana <amount>");
                return;
            }
            Helpers.Helpers.playerManaTotal = value;
        }
    }
}
