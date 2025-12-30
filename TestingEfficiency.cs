
using System;
using Terraria;
using Terraria.ModLoader;
using System;
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
}
