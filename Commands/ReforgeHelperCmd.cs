using System;
using Terraria;
using Terraria.ModLoader;

namespace TestingEfficiency.Commands;

public class ReforgeHelperCmd : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "luckyOrMenacing";

    public override string Description => "Tells whether Lucky or Menacing will be better on your weapon and loadout";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        //IL_0007: Unknown result type (might be due to invalid IL or missing references)
        //IL_000d: Expected O, but got Unknown
        Player player = caller.Player;
        _ = string.Empty;
        bool success = true;
        int defense = 0;
        if (args.Length != 0)
        {
            success = int.TryParse(args[0], out defense);
        }
        if (!success)
        {
            return;
        }
        player.GetWeaponDamage(player.HeldItem);
        float dmg = player.GetTotalDamage(player.HeldItem.DamageType).Additive;
        int crit = player.GetWeaponCrit(player.HeldItem);
        for (int i = 3; i < 10; i++)
        {
            Item item = new Item();
            item = player.armor[i];
            if (item.prefix == 72)
            {
                dmg -= 0.04f;
            }
            if (item.prefix == 71)
            {
                dmg -= 0.34f;
            }
            if (item.prefix == 70)
            {
                dmg -= 0.02f;
            }
            if (item.prefix == 69)
            {
                dmg -= 0.01f;
            }
            if (item.prefix == 68)
            {
                crit -= 4;
            }
            if (item.prefix == 67)
            {
                crit -= 2;
            }
        }
        Main.NewText($"The ideal choice of Menacing or Lucky for your weapon against an enemy with {defense} defense (assuming no on-crit effects) is:");
        for (int j = 0; j < 7; j++)
        {
            float num = (float)player.HeldItem.damage * (dmg + 0.04f) - (float)defense + ((float)player.HeldItem.damage * (dmg + 0.04f) - (float)defense) * ((float)Math.Clamp(crit, 0, 100) / 100f);
            float luckyAverage = (float)player.HeldItem.damage * dmg - (float)defense + ((float)player.HeldItem.damage * dmg - (float)defense) * ((float)Math.Clamp(crit + 4, 0, 100) / 100f);
            if (num > luckyAverage)
            {
                Main.NewText("Menacing");
                dmg += 0.04f;
            }
            else
            {
                Main.NewText("Lucky");
                crit += 4;
            }
        }
    }
}
