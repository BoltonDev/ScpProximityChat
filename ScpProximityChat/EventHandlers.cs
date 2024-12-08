using System.Collections.Generic;
using AdminToys;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Mirror;
using PlayerRoles.FirstPersonControl;
using ScpProximityChat.Enums;
using UserSettings.ServerSpecific;
using VoiceChat;
using VoiceChat.Networking;
using Object = UnityEngine.Object;

namespace ScpProximityChat
{
    internal class EventHandlers
    {
        private readonly Config _config;
        private readonly Dictionary<Player, SpeakerToy> _toggledPlayers;

        internal EventHandlers(Config config)
        {
            _config = config;
            _toggledPlayers = new Dictionary<Player, SpeakerToy>();
        }

        internal void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestarting;
            Exiled.Events.Handlers.Player.VoiceChatting += OnVoiceChatting;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;

            switch (_config.ActivationType)
            {
                case ActivationType.ServerSpecificSettings:
                    ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingValueReceived;
                    Exiled.Events.Handlers.Player.Verified += OnVerified;
                    break;
                case ActivationType.NoClip:
                    Exiled.Events.Handlers.Player.TogglingNoClip += OnTogglingNoClip;
                    break;
            }
        }

        internal void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestarting;
            Exiled.Events.Handlers.Player.VoiceChatting -= OnVoiceChatting;
            Exiled.Events.Handlers.Player.ChangingRole-= OnChangingRole;

            switch (_config.ActivationType)
            {
                case ActivationType.ServerSpecificSettings:
                    ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingValueReceived;
                    Exiled.Events.Handlers.Player.Verified -= OnVerified;
                    break;
                case ActivationType.NoClip:
                    Exiled.Events.Handlers.Player.TogglingNoClip -= OnTogglingNoClip;
                    break;
            }
        }

        private void OnRoundRestarting()
        {
            _toggledPlayers.Clear();
        }

        private void OnVoiceChatting(VoiceChattingEventArgs ev)
        {
            Player player = ev.Player;

            if (ev.VoiceMessage.Channel != VoiceChatChannel.ScpChat)
                return;

            if (_toggledPlayers.TryGetValue(player, out SpeakerToy speaker))
            {
                OpusHandler opusHandler = OpusHandler.Get(player);
                float[] decodedBuffer = new float[480];
                opusHandler.Decoder.Decode(ev.VoiceMessage.Data, ev.VoiceMessage.DataLength, decodedBuffer);

                for (int i = 0; i < decodedBuffer.Length; i++)
                {
                    decodedBuffer[i] *= _config.Volume;
                }

                byte[] encodedData = new byte[512];
                int dataLen = opusHandler.Encoder.Encode(decodedBuffer, encodedData);

                AudioMessage audioMessage = new AudioMessage(speaker.ControllerId, encodedData, dataLen);
                foreach (Player target in Player.List)
                {
                    if (target.Role is not IVoiceRole voiceRole || voiceRole.VoiceModule.ValidateReceive(player.ReferenceHub, VoiceChatChannel.Proximity) == VoiceChatChannel.None)
                        continue;

                    if(_config.UseDefaultScpChat && target.IsScp)
                        continue;

                    target.ReferenceHub.connectionToClient.Send(audioMessage);
                }

                ev.IsAllowed = _config.UseDefaultScpChat;
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            Player player = ev.Player;

            if(_toggledPlayers.ContainsKey(player))
            {
                ToggleProximity(player);
            }

            if(_config.ScpRoles.Contains(ev.NewRole))
            {
                player.Broadcast(_config.ProximityChatBroadcast);
            }
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub);
        }

        private void OnSettingValueReceived(ReferenceHub hub, ServerSpecificSettingBase settingBase)
        {
            if (!Player.TryGet(hub, out Player player) || !_config.ScpRoles.Contains(player.Role.Type))
                return;

            if (settingBase is SSKeybindSetting keyindSetting && keyindSetting.SettingId == _config.KeybindId && keyindSetting.SyncIsPressed)
            {
                ToggleProximity(player);
            }
        }

        private void OnTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            Player player = ev.Player;

            if (FpcNoclip.IsPermitted(player.ReferenceHub) || !_config.ScpRoles.Contains(player.Role.Type))
                return;

            ToggleProximity(player);

            ev.IsAllowed = false;
        }

        private void ToggleProximity(Player player)
        {
            if (_toggledPlayers.ContainsKey(player))
            {
                NetworkServer.Destroy(_toggledPlayers[player].gameObject);
                _toggledPlayers.Remove(player);

                OpusHandler.Remove(player);

                player.ShowHint(_config.ProximityChatDisabled);
            }
            else
            {
                SpeakerToy speaker = Object.Instantiate(PrefabHelper.GetPrefab<SpeakerToy>(PrefabType.SpeakerToy), player.Transform, true);
                NetworkServer.Spawn(speaker.gameObject);
                speaker.NetworkControllerId = (byte)player.Id;
                speaker.NetworkMinDistance = _config.MinDistance;
                speaker.NetworkMaxDistance = _config.MaxDistance;
                speaker.transform.position = player.Position;

                _toggledPlayers.Add(player, speaker);

                player.ShowHint(_config.ProximityChatEnabled);
            }
        }
    }
}