using Terraria;
using Terraria.ModLoader;

namespace TestingEfficiency.Commands;

public class bossLockCmd : ModCommand
{
	public override CommandType Type => CommandType.Chat;

	public override string Command => "aftermath";

	public override string Description => "Toggle Boss Aftermath";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
        Toggle();
	}
	public static void Toggle()
    {
        if (bossLockNPC.bossLock)
        {
            bossLockNPC.bossLock = false;
            Main.NewText("Boss Aftermath Enabled");
        }
        else
        {
            bossLockNPC.bossLock = true;
            Main.NewText("Boss Aftermath Disabled");
        }

    }
}
