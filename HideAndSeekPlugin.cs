using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using System.Threading;
using CounterStrikeSharp.API.Modules.Entities;



namespace HideAndSeekPlugin;

public class HideAndSeekPlugin : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "HideAndSeekPlugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "ShookEagle";

    public Config Config { get; set; } = new Config();
    public static HideAndSeekPlugin Instance { get; private set; } = new();
    public List<CCSPlayerController>? Winners = new List<CCSPlayerController>();
    public Random Random = new Random();

    public override void Load(bool hotReload)
    {
        Instance = this;
        AddCommandListener("jointeam", Command_Jointeam, HookMode.Pre);
    }

    public void OnConfigParsed(Config config)
    {
        Config = config;
    }

    public HookResult Command_Jointeam(CCSPlayerController? player, CommandInfo commandInfo)
    {
        int arg = Convert.ToInt32(commandInfo.GetArg(1));
        if (arg == 2 && player != null)
        {
            player.PrintToChat($"[{ChatColors.Blue}{Instance.Config.Prefix}{ChatColors.White}] You cannot join T Team");
            return HookResult.Handled;
        }
        if (arg == 3 && player != null && player.Team != CsTeam.Terrorist)
        {
            player.SwitchTeam(CsTeam.CounterTerrorist);
            return HookResult.Continue;
        }
        if (arg == 3 && player != null && player.Team == CsTeam.Terrorist)
        {
            player.PrintToChat($"[{ChatColors.Blue}{Instance.Config.Prefix}{ChatColors.White}] You may not leave Seekers");
            return HookResult.Handled;
        }
        return HookResult.Handled;
    }

    public void Find()
    {
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers().Where(p => p.Team != CsTeam.Spectator).ToList();
        List<CCSPlayerController> seekers = new List<CCSPlayerController>();
        if (allPlayers.Count < Instance.Config.MinPlayers)
        {
            Server.PrintToChatAll($"[{ChatColors.Blue}{Instance.Config.Prefix}{ChatColors.White}] Minimum {ChatColors.LightBlue}{Instance.Config.MinPlayers}{ChatColors.Default} required to start.");
            return;
        }
        if (Winners != null)
        {
            seekers.AddRange(Winners);
            Winners.Clear();
        }
        if (seekers.Count < Instance.Config.StartingTs)
        {
            seekers.AddRange(RandomSeeker(Instance.Config.StartingTs - seekers.Count));
        }
        foreach (var player in seekers)
        {
            if (player != null)
            {
                player.SwitchTeam(CsTeam.Terrorist);
                player.GiveNamedItem("weapon_knife");
                Server.PrintToChatAll($"[{ChatColors.Blue}{Instance.Config.Prefix}{ChatColors.White}] {ChatColors.Green}{player.PlayerName}{ChatColors.White} is now seeking.");
            }
        }
        seekers.Clear();
    }

    public List<CCSPlayerController> RandomSeeker(int num)
    {
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers().Where(p => p.Team != CsTeam.Spectator).ToList();
        List<CCSPlayerController> randomplayers = new List<CCSPlayerController>();
        for (int i = 0; i < num; i++)
        {
            randomplayers.Add(allPlayers[Random.Next(allPlayers.Count)]);
        }
        return randomplayers;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        var seeker = @event.Attacker;
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers();

        if (player != null)
        {
            if (player.Team == CsTeam.Terrorist)
            {
                Server.RunOnTickAsync(Server.TickCount + 32, () =>
                {
                    player.Respawn();
                    player.RemoveWeapons();
                    player.GiveNamedItem("weapon_knife");
                });
                return HookResult.Continue;
            }
            else if (player.Team == CsTeam.CounterTerrorist)
            {
                Server.RunOnTickAsync(Server.TickCount + 32, () =>
                {
                    player.SwitchTeam(CsTeam.Terrorist);
                    player.Respawn();
                    player.RemoveWeapons();
                    player.GiveNamedItem("weapon_knife");

                    if (seeker != null && seeker.IsValid)
                    {
                        Server.PrintToChatAll($"[{ChatColors.Blue}{Instance.Config.Prefix}{ChatColors.White}] {ChatColors.Green}{player.PlayerName}{ChatColors.White} was found by {ChatColors.Red}{seeker.PlayerName}{ChatColors.White}");
                    }
                    else //Crashes Server if no valid attacker -- aka using /slay would kill server :p
                    {
                        Server.PrintToChatAll($"[{ChatColors.Blue}{Instance.Config.Prefix}{ChatColors.White}] {ChatColors.Green}{player.PlayerName}{ChatColors.White} was found by the Seekers");
                    }
                    List<CCSPlayerController> CtPlayers = allPlayers.Where(c => c.Team == CsTeam.CounterTerrorist).ToList();
                    if (CtPlayers.Count <= Instance.Config.StartingTs)
                    {
                        var winners = CtPlayers.Where(w => w.UserId != player.UserId).ToList();
                        foreach (var winner in winners)
                        {
                            if (winner != null)
                            {
                                Winners.Add(winner);
                                Server.PrintToChatAll($"[{ChatColors.Blue}{Instance.Config.Prefix}{ChatColors.White}] {ChatColors.Purple}{winner.PlayerName}{ChatColors.White} has Won and will start as a Seeker.");
                            }
                        }
                        Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules?.TerminateRound(1.0f, RoundEndReason.TerroristsWin);
                    }
                });
                return HookResult.Continue;
            }
        }
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers().Where(p => p.Team != CsTeam.CounterTerrorist).ToList();
        Find();
        foreach (var player in allPlayers)
        {
            player.RemoveWeapons();
        }
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers().Where(p => p.Team != CsTeam.Spectator).ToList();
        foreach (var player in allPlayers)
        {
            if (player != null)
            {
                player.SwitchTeam(CsTeam.CounterTerrorist);
                player.RemoveWeapons();
            }
        }
        return HookResult.Continue;
    }

    [ConsoleCommand("css_decoy", "Give all Ct's A Decoy")]
    [RequiresPermissions("@css/rcon")]
    public void Ondecoy(CCSPlayerController player, CommandInfo info)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p.Team == CsTeam.CounterTerrorist);
        foreach (var t in allPlayers)
        {
            t.GiveNamedItem("weapon_decoy");
        }
    }
}


