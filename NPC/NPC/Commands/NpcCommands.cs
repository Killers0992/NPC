using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPC.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NpcCommands : ICommand
    {
        public string Command { get; } = "npc";
        public string[] Aliases { get; }
        public string Description { get; } = "Npc plugin.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                sender.Respond(" NPC Commands:");
                sender.Respond(" - npc list");
                sender.Respond(" - npc remove <npcid>");
                sender.Respond(" - npc create <name> <roletype>");
                sender.Respond(" - npc edit <npcid>");
                sender.Respond(" - npc bring");
                sender.Respond(" - npc setpos <posX> <posY> <posZ>");
                sender.Respond(" - npc setrot <rotX> <rotY> <rotZ>");
                sender.Respond(" - npc setscale <scaleX> <scaleY> <scaleZ>");
                sender.Respond(" ");
            }
            else if (arguments.Count >= 1)
            {
                switch(arguments.At(0).ToLower())
                {
                    case "list":
                        sender.Respond(" NPC List:");
                        foreach(var npc in NpcManager.singleton.npcs)
                        {
                            sender.Respond($" - [{npc.Key}] {npc.Value.NpcName} | Pos: [{npc.Value.Position[0]}, {npc.Value.Position[1]}, {npc.Value.Position[2]}", true);
                        }
                        sender.Respond(" ");
                        break;
                    case "create":
                        if (arguments.Count == 2)
                        {
                            Player p = Player.Get((sender as CommandSender).SenderId);
                            int id = NpcManager.singleton.CreateNPC(arguments.At(1), short.Parse(arguments.At(2)), p.Position, p.Rotation, new UnityEngine.Vector3(1f, 1f, 1f));
                            if (NpcManager.singleton.selectedNpcs.ContainsKey(p.UserId))
                                NpcManager.singleton.selectedNpcs[p.UserId] = id;
                            else
                                NpcManager.singleton.selectedNpcs.Add(p.UserId, id);
                            sender.Respond("Created npc with name " + arguments.At(1));
                            sender.Respond("Selected npc with id " + id);
                        }
                        break;
                    case "remove":
                        if (arguments.Count == 1)
                        {
                            Player p = Player.Get((sender as CommandSender).SenderId);
                            int npcid = int.Parse(arguments.At(1));
                            if (NpcManager.singleton.npcs.ContainsKey(npcid))
                            {
                                NpcManager.singleton.RemoveNPC(npcid);
                                sender.Respond("Removed npc with id " + npcid);
                            }
                        }
                        break;
                    case "edit":
                        if (arguments.Count == 1)
                        {
                            Player p = Player.Get((sender as CommandSender).SenderId);
                            int npcid = int.Parse(arguments.At(1));
                            if (NpcManager.singleton.npcs.ContainsKey(npcid))
                            {
                                if (NpcManager.singleton.selectedNpcs.ContainsKey(p.UserId))
                                    NpcManager.singleton.selectedNpcs[p.UserId] = npcid;
                                else
                                    NpcManager.singleton.selectedNpcs.Add(p.UserId, npcid);
                                sender.Respond("Selected npc with id " + npcid);
                            }
                        }
                        break;
                }
            }
            response = "";
            return true;
        }
    }
}
