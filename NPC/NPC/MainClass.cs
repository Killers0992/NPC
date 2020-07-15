using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
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
        public NpcManager npcManager;
        public string pluginDir;

        public override void OnEnabled()
        {
            if (!Config.IsEnabled)
                return;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            pluginDir = Path.Combine(folderPath, "EXILED-PTB", "Plugins", "NPC");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);
            eventHandlers = new EventHandlers(this);
            npcManager = new NpcManager(this);
            Exiled.Events.Handlers.Server.WaitingForPlayers += eventHandlers.OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.Left += eventHandlers.OnPlayerLeave;
        }
    }
}
