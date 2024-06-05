using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp939;
using Player = Exiled.Events.Handlers.Player;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MultiBroadcast
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name => "MultiBroadcast";
        public override string Author => "Cocoa";
        public override string Prefix => "MultiBroadcast";
        public override Version Version { get; } = new(1, 0, 1);
        public override Version RequiredExiledVersion { get; } = new(8, 8, 1);

        private Harmony Harmony { get; set; }

        public override void OnEnabled()
        {
            base.OnEnabled();
            Instance = this;
            Harmony = new Harmony($"cocoa.multi_broadcast.{DateTime.Now.Ticks}");
            Harmony.PatchAll();
        }

        public override void OnDisabled()
        {
            Harmony.UnpatchAll();
            Harmony = null;
            Instance = null;
            base.OnDisabled();
        }
    }
}