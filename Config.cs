using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using System.Text.Json;
using Terraria.ModLoader.Config;

namespace TestingEfficiency;
public class DiscordConfig : ModConfig
{
	public static DiscordConfig Instance;

	public string webhookurl = "";

    [DefaultValue(false)]
    public bool autowebhook = false;
	
	/*[Label("Push Time Stats to Webhook")]
	[Tooltip("Pushes RTA/IGT breakdown when publishing webhooks")]
    [DefaultValue(true)]
    public bool timewebhook = true;

	[Label("Push Damage Stats to Webhook")]
	[Tooltip("Pushes damage breakdown when publishing webhooks")]
    [DefaultValue(true)]
    public bool dmgwebhook = true;

    [Label("Push Equipment to Webhook")]
    [Tooltip("Pushes current equipment when publishing webhooks")]
    [DefaultValue(true)]
    public bool accessorywebhook = true;*/
    public override ConfigScope Mode => ConfigScope.ClientSide;
}

public class FightStatsConfig : ModConfig
{
    public static FightStatsConfig Instance;

    [DefaultValue(true)]
    public bool time = false;

    [DefaultValue(true)]
    public bool dmg = false;

    [DefaultValue(false)]
    public bool detailedDmgStats = false;

    [DefaultValue(true)]
    public bool calamityDebuffStats = false;
    public override ConfigScope Mode => ConfigScope.ClientSide;
}

[Label("Loadout Config")]
public class TestingLoadouts : ModConfig
{
	public static TestingLoadouts Instance;

	public Dictionary<string, List<string>> meleeLoadouts = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> rangerLoadouts = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> mageLoadouts = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> summonerLoadouts = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> rogueLoadouts = new Dictionary<string, List<string>>();

	public override ConfigScope Mode => ConfigScope.ClientSide;
}

/*[Label("Misc Config")]
public class MiscConfig : ModConfig
{
	public static MiscConfig Instance;

    [Label("Enable Changing Damage Spread")]
    [Tooltip("Enables modifying damage spread amount.")]
    [DefaultValue(false)]
    public bool ChangeDmgSpread;
    [Label("Damage Variation Amount")]
	[Tooltip("Changes Damage Spread, when 'Enable Changin Damage Spread' is enabled.\nCalamity default is 5%\nVanilla default is 15%")]
	[DefaultValue(0.05)]
	public float DmgSpread;

	public override ConfigScope Mode => ConfigScope.ServerSide;
}*/
