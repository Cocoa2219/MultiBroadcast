using System.ComponentModel;
using Exiled.API.Interfaces;

namespace MultiBroadcast
{
    public class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Indicates whether the plugin is in debug mode or not.")]
        public bool Debug { get; set; } = false;
    }
}