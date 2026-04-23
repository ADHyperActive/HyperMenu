using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace MalumMenu;
public static class OverloadHandler
{
    private const float _AttackLogCooldown = 2f;
    public static float cooldown;
    public static int strength;
    private static HashSet<int> _customTargets = new();
    private static float _timer;
    private static float _attackLogTimer;
    private static Dictionary<int, int> _rpcCounters = new();
    private static int _nextTarget = int.MinValue;
    private static bool _isDone;

    public static void Run()
    {
        if (!CheatToggles.runOverload || OverloadUI.currentTargets.Count <= 0)
        {
            _timer = cooldown;
            _attackLogTimer = _AttackLogCooldown;
            _nextTarget = int.MinValue;
            _isDone = false;
            _rpcCounters.Clear();
            return;
        }

        _timer += Time.unscaledDeltaTime;
        _attackLogTimer += Time.unscaledDeltaTime;

        if (_timer >= cooldown)
        {
            if (OverloadUI.maxPossibleTargets == OverloadUI.currentTargets.Count)
            {
                int broadcastId = -1;

                Utils.Overload(broadcastId, strength);
                _timer -= cooldown;

                if (CheatToggles.olLogAttack)
                {
                    string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                    if (!CheatToggles.olVerboseLogs)
                    {
                        if (!_rpcCounters.ContainsKey(broadcastId))
                        {
                            _rpcCounters.Add(broadcastId, 0);
                        }

                        _rpcCounters.TryGetValue(broadcastId, out var rpcCount);

                        int newRpcCount = rpcCount + strength;

                        if (_attackLogTimer >= _AttackLogCooldown)
                        {
                            OverloadUI.LogConsole($"> <b><color=#{colorStr}>Broadcasted {newRpcCount} malformed RPCs to all players (ID : {broadcastId})</color></b>");

                            _attackLogTimer -= _AttackLogCooldown;

                            _rpcCounters.Clear();
                        }
                        else
                        {
                            _rpcCounters[broadcastId] = newRpcCount;
                        }
                    }
                    else
                    {
                        OverloadUI.LogConsole($"> <b><color=#{colorStr}>Broadcasted {strength} malformed RPCs to all players (ID : {broadcastId})</color></b>");
                    }
                }
            }
            else
            {
                var currentTargets = OverloadUI.currentTargets;

                foreach (NetworkedPlayerInfo targetData in currentTargets)
                {
                    int clientId = targetData.ClientId;

                    if (!_isDone)
                    {
                        if (_nextTarget == int.MinValue || clientId == _nextTarget)
                        {
                            Utils.Overload(clientId, strength);
                            _timer -= cooldown;

                            if (CheatToggles.olLogAttack)
                            {
                                string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                                if (!CheatToggles.olVerboseLogs)
                                {
                                    if (!_rpcCounters.ContainsKey(clientId))
                                    {
                                        _rpcCounters.Add(clientId, 0);
                                    }

                                    _rpcCounters.TryGetValue(clientId, out var rpcCount);

                                    int newRpcCount = rpcCount + strength;

                                    _rpcCounters[clientId] = newRpcCount;
                                }
                                else
                                {
                                    OverloadUI.LogConsole($"> <b><color=#{colorStr}>Sent {strength} malformed RPCs to {targetData.DefaultOutfit.PlayerName} (ID : {clientId})</color></b>");
                                }
                            }

                            _isDone = true;
                        }
                    }
                    else
                    {
                        _nextTarget = clientId;
                        _isDone = false;
                        return;
                    }
                }

                _nextTarget = int.MinValue;
                _isDone = false;

                if (!CheatToggles.olVerboseLogs)
                {
                    if (_attackLogTimer >= _AttackLogCooldown)
                    {
                        string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                        foreach (KeyValuePair<int, int> entry in _rpcCounters)
                        {
                            int clientId = entry.Key;
                            int rpcCount = entry.Value;

                            NetworkedPlayerInfo playerData = OverloadUI.currentTargets.FirstOrDefault(pd => pd.ClientId == clientId);

                            if (playerData != null)
                            {
                                OverloadUI.LogConsole($"> <b><color=#{colorStr}>Sent {rpcCount} malformed RPCs to {playerData.DefaultOutfit.PlayerName} (ID : {clientId})</color></b>");
                            }
                        }

                        _attackLogTimer -= _AttackLogCooldown;

                        _rpcCounters.Clear();
                    }
                }
                else
                {
                    _rpcCounters.Clear();
                }
            }
        }
    }

    public static void AddCustomTarget(NetworkedPlayerInfo playerData)
    {
        int clientId = playerData.ClientId;

        if (!_customTargets.Contains(clientId))
        {
            _customTargets.Add(clientId);
        }
    }

    public static void RemoveCustomTarget(NetworkedPlayerInfo playerData)
    {
        int clientId = playerData.ClientId;

        if (_customTargets.Contains(clientId))
        {
            _customTargets.Remove(clientId);
        }
    }

    public static bool IsCustomTarget(NetworkedPlayerInfo playerData)
    {
        return _customTargets.Contains(playerData.ClientId);
    }

    public static (HashSet<TargetType> targetTypes, bool isTarget) GetTarget(NetworkedPlayerInfo playerData)
    {
        bool isTarget = false;
        var targetTypes = new HashSet<TargetType>();

        if (CheatToggles.overloadAll)
        {
            targetTypes.Add(TargetType.All);
            isTarget = true;
        }

        bool hostTarget = CheatToggles.overloadHost && AmongUsClient.Instance.HostId == playerData.ClientId;
        if (hostTarget)
        {
            targetTypes.Add(TargetType.Host);
            isTarget = true;
        }

        if (playerData.Role != null)
        {
            RoleTeamTypes roleTeamType = playerData.Role.TeamType;

            bool crewTarget = CheatToggles.overloadCrew && roleTeamType.Equals(RoleTeamTypes.Crewmate);
            if (crewTarget)
            {
                targetTypes.Add(TargetType.Crewmate);
                isTarget = true;
            }

            bool impTarget = CheatToggles.overloadImps && roleTeamType.Equals(RoleTeamTypes.Impostor);
            if (impTarget)
            {
                targetTypes.Add(TargetType.Impostor);
                isTarget = true;
            }
        }

        bool customTarget = IsCustomTarget(playerData);
        if (customTarget)
        {
            targetTypes.Add(TargetType.Custom);
            isTarget = true;
        }

        if (!isTarget)
        {
            targetTypes.Add(TargetType.None);
        }

        return (targetTypes, isTarget);
    }

    public static void ClearCustomTargets()
    {
        _customTargets.Clear();
    }

    public static void PopulateCustomTargets(PlayerControl[] players, TargetType targetType)
    {
        int playerCount = players.Length;

        for (int i = 0; i < playerCount; i++)
        {
            NetworkedPlayerInfo playerData = players[i].Data;
            var playerTarget = GetTarget(playerData);
            bool isTarget = playerTarget.isTarget; // ? PlayerId for FreePlay ?

            if (isTarget && !IsCustomTarget(playerData))
            {
                HashSet<TargetType> currentTargetTypes = playerTarget.targetTypes;
                if (currentTargetTypes.Contains(targetType))
                {
                    AddCustomTarget(playerData);
                }
            }
        }
    }

    public static (int strength, float cooldown) CalculateAdaptedValues()
    {
        int targetCount = OverloadUI.maxPossibleTargets == OverloadUI.currentTargets.Count
                        ? 1
                        : Math.Max(1, OverloadUI.currentTargets.Count);

        float cooldown = 1f / targetCount;

        int pingLevel = Math.Max(1, Utils.GetPing() / 100);

        int strength = 500 / pingLevel / targetCount;

        return (strength, cooldown);
    }

    public enum TargetType
    {
        None,
        All,
        Custom,
        Host,
        Impostor,
        Crewmate
    }
}
