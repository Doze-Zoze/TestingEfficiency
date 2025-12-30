using Terraria;
using Terraria.ModLoader;

namespace TestingEfficiency;

public class bossLockNPC : GlobalNPC
{
	public static bool bossLock;

	public override bool CheckDead(NPC npc)
	{
		if (bossLockNPC.bossLock && npc.boss)
		{
			npc.active = false;
			return false;
		}
		return base.CheckDead(npc);
	}

	public override bool PreKill(NPC npc)
	{
		ModLoader.TryGetMod("CalamityMod", out var calamity);
		if (bossLockNPC.bossLock && (npc.type == calamity.Find<ModNPC>("Providence").Type || npc.type == calamity.Find<ModNPC>("DevourerofGodsHead").Type || npc.type == calamity.Find<ModNPC>("SupremeCalamitas").Type))
		{
			return false;
		}
		return base.PreKill(npc);
	}
}
