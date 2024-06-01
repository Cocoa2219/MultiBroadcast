using Exiled.API.Features;

namespace MultiBroadcast
{
    public class Plugin : Plugin<Config>
    {
        public override string Name { get; } = "MultiBroadcast";
        public override string Author { get; } = "Cocoa";
        public override string Prefix { get; } = "MultiBroadcast";
        public override System.Version Version { get; } = new(1, 0, 0);
        public override System.Version RequiredExiledVersion { get; } = new(8, 8, 1);

        public static Plugin Instance { get; private set; }

        public override void OnEnabled()
        {
            base.OnEnabled();
            Instance = this;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            Instance = null;
        }
    }
}