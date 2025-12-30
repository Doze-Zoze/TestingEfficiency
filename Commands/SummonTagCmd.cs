using CalamityMod;
using CalamityMod.DataStructures;
using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Systems.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TestingEfficiency.Helpers;

namespace TestingEfficiency.Commands
{
    public class SummonTagCmd : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "SummonTag";

        public override string Description => "Changes SummonTag associated to held item\n Arguments must be in form 'flat:#', 'crit:#', 'mult:#', or 'critdmg:'";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!CalamityBuffSets.SummonTagDebuff.ContainsKey(caller.Player.HeldItem.type))
            {
                Main.NewText("Held item does not have an associated SummonTag");
                return;
            }

            var sTag = CalamityBuffSets.SummonTagDebuff[caller.Player.HeldItem.type];
            if (args.Length < 1 || !int.TryParse(args[0], out int value))
            {
                Main.NewText($"Current item's SummonTag stats: flat:{sTag.FlatTagDamage}, crit:{sTag.TagCritChance}, mult:{sTag.MultiplicativeTagDamage}, critdmg:{sTag.TagCritDamage}");
                Main.NewText("Arguments to change this tag are 'flat:#', 'crit:#', 'mult:#', or 'critdmg:'");
                return;
            }
            var strToParse = args.FirstOrDefault(x => x.ToLower().Contains("flat:"));
            if (strToParse != default)
            {
                try
                {
                    sTag.FlatTagDamage = int.Parse(strToParse.Substring(6));
                }
                catch
                {
                    Main.NewText("Invalid value for 'flat:'");
                }
            }
            strToParse = args.FirstOrDefault(x => x.ToLower().Contains("crit:"));
            if (strToParse != default)
            {
                try
                {
                    sTag.TagCritChance = int.Parse(strToParse.Substring(6));
                }
                catch
                {
                    Main.NewText("Invalid value for 'crit:'");
                }
            }
            strToParse = args.FirstOrDefault(x => x.ToLower().Contains("mult:"));
            if (strToParse != default)
            {
                try
                {
                    sTag.MultiplicativeTagDamage = int.Parse(strToParse.Substring(6));
                }
                catch
                {
                    Main.NewText("Invalid value for 'mult:'");
                }
            }

            strToParse = args.FirstOrDefault(x => x.ToLower().Contains("critdmg:"));
            if (strToParse != default)
            {
                try
                {
                    sTag.TagCritDamage = int.Parse(strToParse.Substring(9));
                }
                catch
                {
                    Main.NewText("Invalid value for 'critdmg:'");
                }
            }

            Helpers.Helpers.playerLifeTotal = value;
        }
    }
}
