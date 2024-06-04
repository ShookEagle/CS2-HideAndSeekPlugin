using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace HideAndSeekPlugin;

public class HNSConfig : BasePluginConfig
{
    [JsonPropertyName("hns_prefix")] public string Prefix { get; set; } = "[HNS]";
    [JsonPropertyName("hns_zombie_style")] public bool Zombie_Style { get; set; } = true;
    [JsonPropertyName("hns_min_players")] public int MinPlayers { get; set; } = 2;
    [JsonPropertyName("hns_t_instakills")] public bool Terrorist_Insta_Kills { get; set; } = true;
    [JsonPropertyName("hns_tr_t_speed")] public float TSpeed { get; set; } = 1.0f;
    [JsonPropertyName("hns_starting_ts")] public float StartingTs { get; set; } = 1.0f;
}