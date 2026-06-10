using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TestingEfficiency.DamageStats;

namespace TestingEfficiency
{
    public class DataStructures
    {
        public struct NPCData //this is a way for me to store the NPC's Type, index in the Main.NPC Array (aka the npc.whoAmI value), and the NPC's name all in one variable. Useful for ensuring it's the same NPC and not a new one with the same type or index.
        {
            public NPCData(NPC npc) //Sets the type/index/name automatically from a passed NPC
            {

                Type = npc.type;
                Index = npc.whoAmI;
                Name = npc.TypeName;
            }
            public NPCData(int type, int index) //Automatically gets the name from the npc type
            {
                Type = type;
                Index = index;
                if (!IDSets.ShouldMergeInstances[type] && index >= 0)
                {
                    Name = Main.npc[index].TypeName;
                }
                else
                {
                    Name = ContentSamples.NpcsByNetId[type].TypeName;
                }
            }

            public NPCData(int type, int index, string name) //Manually define all 3 values
            {
                Type = type;
                Index = index;
                Name = name;
            }

            public string Name { get; }
            public int Type { get; }
            public int Index { get; }
        }

        public enum DamageSourceType
        {
            Projectile,
            Item,
            Misc,
            DoT,
            Environment
        }

        public enum DamageSourceOwner
        {
            Player,
            NPC
        }

        public struct DamageSourceData
        {

            public DamageSourceData(int playerid, int sourceid, DamageSourceType sourceType)
            {
                OwnerID = playerid;
                SourceID = sourceid;
                SourceType = sourceType;
            }

            /// <summary>
            /// Player.whoAmI
            /// </summary>
            public int OwnerID;
            /// <summary>
            /// Proctile.type / Item.type / Debuff type
            /// </summary>
            public int SourceID;
            public DamageSourceType SourceType;
            public DamageSourceOwner OwnerType = DamageSourceOwner.Player;
        }


        public class TestData
        {
            public string tester { get; set; } = string.Empty;
            public string weapon { get; set; } = string.Empty;
            public string version { get; set; } = "2.1.2";
            public string? notes { get { return field == string.Empty ? null : field; } set; } = string.Empty;
            public List<string> tags { get; set; } = new();
            public string? tier { get { return field == string.Empty ? null : field; } set; } = string.Empty;

            public List<BossTestData> bosses { get; set; } = new();
        }
        public class BossTestData
        {
            public string name { get; set; } = string.Empty;

            public float? died { get { return field == 0 ? null : field; } set; }
            public int time { get; set; }
            public string? note { get { return field == string.Empty ? null : field; } set; } = string.Empty;
            public string? gear { get { return field == string.Empty ? null : field; } set; } = string.Empty;
            public bool validTest { get; set; } = true;

            [JsonIgnore]
            public string timeString
            {
                get
                {
                    if (time == 0)
                        return string.Empty;
                    return $"{time / 60}:{(time % 60).ToString("00")}";
                }
                set
                {

                    if (value.Contains(":"))
                    {
                        var s = value.Split(":");
                        if (int.TryParse(s[0], out int s0) && int.TryParse(s[1], out int s1))
                            time = s0 * 60 + s1;
                    }
                    else if (int.TryParse(value, out int parsed))
                        time = parsed;
                }
            }

            [JsonIgnore]
            public string diedString
            {
                get
                {
                    if (died == null)
                        return string.Empty;
                    return $"{((died ?? 0) * 100).ToString("##.##")}%";
                }
                set
                {
                    value = value.Replace("%", "");
                    died = Single.Parse(value) / 100f;
                }
            }
        }
    }
}
