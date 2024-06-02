using System;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using Server = Exiled.Events.Handlers.Server;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MultiBroadcast
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name { get; } = "MultiBroadcast";
        public override string Author { get; } = "Cocoa";
        public override string Prefix { get; } = "MultiBroadcast";
        public override System.Version Version { get; } = new(1, 0, 1);
        public override System.Version RequiredExiledVersion { get; } = new(8, 8, 1);

        public Harmony Harmony { get; private set; }

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