using Terraria;
using Terraria.ModLoader;

namespace TestingEfficiency.Commands
{
    public class LifeCmd : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "life";

        public override string Description => "Set Life";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int value))
            {
                Main.NewText("Please use the following syntax:\n/life <amount>");
                return;
            }
            Helpers.Helpers.playerLifeTotal = value;
        }
    }
}
