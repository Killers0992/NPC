using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPC
{
    public class EventHandlers
    {
        private MainClass plugin;
        public EventHandlers(MainClass plugin)
        {
            this.plugin = plugin;
        }

        public void OnWaitingForPlayers()
        {
            NpcManager.singleton.LoadNpcsAfterRestart();
        }

        public void OnPlayerLeave(LeftEventArgs ev)
        {
            if (NpcManager.singleton.selectedNpcs.ContainsKey(ev.Player.UserId))
                NpcManager.singleton.selectedNpcs.Remove(ev.Player.UserId);
        }
    }
}
