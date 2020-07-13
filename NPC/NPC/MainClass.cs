using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPC
{
    public class MainClass : Plugin<NpcConfig>
    {
        public override string Name { get; } = "NPC";
        public override string Author { get; } = "Killers0992";
        public override string Prefix { get; } = "npc";
        public override Version Version { get; } = new Version("1.0.0");

        public EventHandlers eventHandlers;

        public override void OnEnabled()
        {
            if (!Config.IsEnabled)
                return;
            eventHandlers = new EventHandlers(this);
            Exiled.Events.Handlers.Server.WaitingForPlayers += eventHandlers.OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.Left += eventHandlers.OnPlayerLeave;
        }
    }
}
