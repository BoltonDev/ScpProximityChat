using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using PlayerRoles;
using ScpProximityChat.Enums;

namespace ScpProximityChat
{
    public class Config : IConfig
    {
        [Description("Whether the plugin is enabled or disabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether the debug mode is enabled or disabled.")]
        public bool Debug { get; set; } = false;

        [Description("The activation type: ServerSpecificSettings (custom settings) or NoClip (no clip key).")]
        public ActivationType ActivationType { get; set; } = ActivationType.ServerSpecificSettings;

        [Description("Scp role that can use the proximity chat.")]
        public HashSet<RoleTypeId> ScpRoles { get; set; } = new()
        {
            RoleTypeId.Scp049,
            RoleTypeId.Scp0492,
            RoleTypeId.Scp096,
            RoleTypeId.Scp106,
            RoleTypeId.Scp173,
            RoleTypeId.Scp939,
        };

        [Description("If false, SCPs won't hear those with proximity chat enabled until they are near of them.")]
        public bool UseDefaultScpChat { get; set; } = true;

        [Description("The volume of the voice.")]
        public float Volume { get; set; } = 10f;

        [Description("Represents the distance at which the audio is full volume.")]
        public float MinDistance { get; set; } = 2f;

        [Description("Max voice distance through the proximity chat.")]
        public float MaxDistance { get; set; } = 10f;

        [Description("Hint displayed when a scp activates its proximity chat.")]
        public Hint ProximityChatEnabled { get; set; } = new()
        {
            Content = "<b>Proximity chat is now <color=green>enabled</color>.</b>",
            Duration = 3f,
            Show = true,
        };

        [Description("Hint displayed when a scp deactivates its proximity chat.")]
        public Hint ProximityChatDisabled { get; set; } = new()
        {
            Content = "<b>Proximity chat is now <color=red>disabled</color>.</b>",
            Duration = 3f,
            Show = true,
        };

        public Exiled.API.Features.Broadcast ProximityChatBroadcast { get; set; } = new()
        {
            Content = "<b>You can toggle proximity chat by using the keybind configured in your settings.</b>",
            Duration = 10,
            Show = true,
        };

        [Description("The centered text (header) of the category.")]
        public string SettingHeaderLabel { get; set; } = "ScpProximityChat";

        [Description("The unique id of the setting.")]
        public int KeybindId { get; set; } = 200;

        [Description("The keybind label.")]
        public string KeybindLabel { get; set; } = "Toggle scp proximity chat";

        [Description("The keybind hint used to provides additional information.")]
        public string KeybindHint { get; set; } = "Toggles the scp proximity chat.";
    }
}