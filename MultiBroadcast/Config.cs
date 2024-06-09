using System.ComponentModel;
using Exiled.API.Interfaces;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MultiBroadcast
{
    public class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Indicates whether the plugin is in debug mode or not.")]
        public bool Debug { get; set; } = false;

        [Description(
            "Indicates order of broadcasts. Desending = newest broadcasts add on top, Ascending = newest broadcasts add on bottom"
        )]
        public BroadcastOrder Order { get; set; } = BroadcastOrder.Desending;
    }

    public enum BroadcastOrder
    {
        Desending,
        Ascending
    }
}