using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Net.NetworkInformation;

namespace HideAndSeekPlugin;

public class HideAndSeekPlugin : BasePlugin, IPluginConfig<HNSConfig>
{
    public override string ModuleName => "HideAndSeekPlugin";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "ShookEagle";

    public HNSConfig Config { get; set; } = new HNSConfig();
    public static HideAndSeekPlugin Instance { get; set; } = new();
    public static CCSPlayerController? PlayerTerrorist { get; set; }
    public static CCSPlayerController? RoundWinner { get; set; }
    private static Random Random { get; set; } = new();

    public override void Load(bool hotReload)
    {
        Instance = this;

        AddCommandListener("jointeam", Command_Jointeam, HookMode.Pre);

        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventWarmupEnd>(OnWarmupEnd);
    }

    public static void Unload()
    {
    }

    public void OnConfigParsed(HNSConfig config)
    {
        config.Prefix = StringExtensions.ReplaceColorTags(config.Prefix);

        Config = config;
    }

    public static bool Find()
    {
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers();

        if (allPlayers.Count < Instance.Config.MinPlayers)
        {
            Server.PrintToChatAll(Instance.Config.Prefix + Instance.Localizer["Minimum player", Instance.Config.MinPlayers]);
            return false;
        }
        if (RoundWinner == null)
        {
            PlayerTerrorist = allPlayers[Random.Next(allPlayers.Count)];
            PlayerTerrorist.SwitchTeam(CsTeam.Terrorist);
            Server.PrintToChatAll($"{TextColor.Red}{Instance.Config.Prefix} {TextColor.Green}{PlayerTerrorist.PlayerName}{TextColor.Default} Is The Seeker");
            return true;
        }
        if (RoundWinner != null)
        {
            RoundWinner.SwitchTeam(CsTeam.Terrorist);
            Server.PrintToChatAll($"{TextColor.Red}{Instance.Config.Prefix} {TextColor.Green}{RoundWinner.PlayerName}{TextColor.Default} Is The Seeker");
            return true;
        }
        return false;
    }

    public static HookResult Command_Jointeam(CCSPlayerController? player, CommandInfo commandInfo)
    {
        return HookResult.Handled;
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (PlayerTerrorist == null && RoundWinner == null)
        {
            if (!Find())
            {
                return HookResult.Continue;
            }
        }

        CCSPlayerController? player = @event.Userid;

        if (player == null || player.Team != CsTeam.Terrorist)
        {
            return HookResult.Continue;
        }

        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return HookResult.Continue;
        }

        playerPawn.VelocityModifier = Instance.Config.TSpeed;

        return HookResult.Continue;
    }

    public static HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers();

        PlayerTerrorist = null;

        foreach (CCSPlayerController player in allPlayers)
        {
            if (!player.PawnIsAlive)
            {
                player.TakesDamage = false;
            }
            if (player.PawnIsAlive)
            {
                player.SwitchTeam(CsTeam.CounterTerrorist);
            }
        }

        Find();

        return HookResult.Continue;
    }

    public static HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        Instance.AddTimer(0.1f, () => player.ChangeTeam(CsTeam.CounterTerrorist));

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers();

        if (player == null)
        {
            return HookResult.Continue;
        }

        if (player.Team == CsTeam.Terrorist)
        {
            player.Respawn();
            return HookResult.Continue;
        }

        if (player.Team == CsTeam.CounterTerrorist)
        {
            player.ChangeTeam(CsTeam.Terrorist);
            player.Respawn();
            Server.PrintToChatAll($"{TextColor.Red}{Instance.Config.Prefix} {TextColor.Green}{player.PlayerName}{TextColor.Default} Was Found by the Seekers.");
            var CtPlayers = allPlayers.Where(c => c.Team == CsTeam.CounterTerrorist);
            if (CtPlayers.Count() <= 1)
            {
                Server.PrintToChatAll($"{TextColor.Red}{Instance.Config.Prefix} {TextColor.Purple}{player.PlayerName}{TextColor.Default} Wins and will start as next Seeker.");
                Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules?.TerminateRound(1.0f, RoundEndReason.RoundDraw);
            }
        }

        Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules?.TerminateRound(1.0f, RoundEndReason.RoundDraw);

        return HookResult.Continue;
    }

    public static HookResult OnWarmupEnd(EventWarmupEnd @event, GameEventInfo info)
    {
        Find();

        return HookResult.Continue;
    }
}
