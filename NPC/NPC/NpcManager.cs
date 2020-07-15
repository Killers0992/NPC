using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using Newtonsoft.Json;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NPC
{
    public class NpcManager
    {
        private MainClass plugin;
        public static NpcManager singleton;
        public Dictionary<string, int> selectedNpcs = new Dictionary<string, int>();


        public NpcManager(MainClass plugin)
        {
            singleton = this;
            this.plugin = plugin;
            if (!File.Exists(Path.Combine(plugin.pluginDir, "npcs.json")))
                File.WriteAllText(Path.Combine(plugin.pluginDir, "npcs.json"), JsonConvert.SerializeObject(new Dictionary<ushort, List<PlayerNPC>>(), Formatting.Indented));
            playerNpcsData = JsonConvert.DeserializeObject<Dictionary<ushort, List<PlayerNPC>>>(File.ReadAllText(Path.Combine(plugin.pluginDir, "npcs.json")));
            if (!playerNpcsData.ContainsKey(ServerStatic.ServerPort))
                playerNpcsData.Add(ServerStatic.ServerPort, new List<PlayerNPC>());
            File.WriteAllText(Path.Combine(plugin.pluginDir, "npcs.json"), JsonConvert.SerializeObject(playerNpcsData, Formatting.Indented));
        }
        public void Reload()
        {
            if (!playerNpcsData.ContainsKey(ServerStatic.ServerPort))
                return;
            foreach (var npc in playerNpcsData[ServerStatic.ServerPort])
            {
                if (npc.npcObject != null)
                    NetworkServer.Destroy(npc.npcObject);
            }
            playerNpcsData.Clear();
            playerNpcsData = JsonConvert.DeserializeObject<Dictionary<ushort, List<PlayerNPC>>>(File.ReadAllText(Path.Combine(plugin.pluginDir, "npcs.json")));
            selectedNpcs.Clear();
            LoadNpcsAfterRestart();
        }

        public void LoadNpcsAfterRestart()
        {
            foreach(var npc in playerNpcsData[ServerStatic.ServerPort])
            {
                if (npc.npcObject != null)
                    NetworkServer.Destroy(npc.npcObject);
                CreateNPC(npc.NpcName, npc.RoleName, npc.RoleColor, npc.RoleType, new Vector3(
                    npc.Position.x,
                    npc.Position.y,
                    npc.Position.z), new Vector3(
                        npc.Rotation.x,
                        npc.Rotation.y,
                        npc.Rotation.z), new Vector3(
                            npc.Size.x,
                            npc.Size.y,
                            npc.Size.z), npc.NpcID, npc.ItemInHand);
            }
        }

        public int GetFreeID()
        {
            for (int i = 1; i < int.MaxValue; i++)
            {
                if (playerNpcsData[ServerStatic.ServerPort].Any(p => p.NpcID == i))
                    continue;

                return i;
            }
            return -1;
        }

        public void RemoveNPC(int npcId)
        {
            var d = playerNpcsData[ServerStatic.ServerPort].Where(p => p.NpcID == npcId).FirstOrDefault();
            if (d != null)
            {
                NetworkServer.Destroy(d.npcObject);
                playerNpcsData[ServerStatic.ServerPort].Remove(d);
            }
            if (!playerNpcsData.ContainsKey(ServerStatic.ServerPort))
                return;
            playerNpcsData[ServerStatic.ServerPort].Remove(playerNpcsData[ServerStatic.ServerPort].Where(p => p.NpcID == npcId).FirstOrDefault());
            File.WriteAllText(Path.Combine(plugin.pluginDir, "npcs.json"), JsonConvert.SerializeObject(playerNpcsData, Formatting.Indented));
        }

        public int CreateNPC(string npcName, string roleName, string roleColor, short RoleType, Vector3 position, Vector3 rotation, Vector3 scale, int id = -1, int itemId = -1)
        {
            int npcId = id != -1 ? id : GetFreeID();
            var npcc = UnityEngine.Object.Instantiate(LiteNetLib4MirrorNetworkManager.singleton.playerPrefab, position, Quaternion.identity);
            var hub = ReferenceHub.GetHub(npcc);

            foreach (var component in npcc.GetComponents<Behaviour>())
            {
                if (component is ReferenceHub || component is CharacterClassManager || component is PlayerMovementSync || component is PlayerPositionManager || component is Handcuffs || component is ServerRoles || component is Inventory)
                    continue;

                component.enabled = false;
            }


            hub.characterClassManager.NetworkCurClass = (global::RoleType)RoleType;
            hub.serverRoles.NetworkMyText = roleName;
            hub.serverRoles.NetworkMyColor = roleColor;
            npcc.transform.localScale = scale;
            npcc.transform.rotation = Quaternion.Euler(rotation);
            hub.characterClassManager.GodMode = true;
            if (itemId != -1)
                hub.inventory.NetworkcurItem = (ItemType)itemId;
            Timing.CallDelayed(1f, () =>
            {
                hub.nicknameSync.MyNick = $"{npcName}";
                NetworkServer.Spawn(npcc);
            });

            hub.queryProcessor.PlayerId = id;

            hub.playerMovementSync._realModelPosition = position;
            if (id == -1)
            {
                playerNpcsData[ServerStatic.ServerPort].Add(new PlayerNPC()
                {
                    npcObject = npcc,
                    NpcID = npcId,
                    NpcName = npcName,
                    RoleType = RoleType,
                    Position = new Vector3Json() { x = position.x, y = position.y, z = position.z },
                    Rotation = new Vector3Json() { x = rotation.x, y = rotation.y, z = rotation.z },
                    Size = new Vector3Json() { x = scale.x, y = scale.y, z = scale.z }
                });
                File.WriteAllText(Path.Combine(plugin.pluginDir, "npcs.json"), JsonConvert.SerializeObject(playerNpcsData, Formatting.Indented));
            }
            else
                playerNpcsData[ServerStatic.ServerPort].Where(tt => tt.NpcID == npcId).FirstOrDefault().npcObject = npcc;
            return npcId;
        }

        public Dictionary<ushort, List<PlayerNPC>> playerNpcsData = new Dictionary<ushort, List<PlayerNPC>>();

        public class PlayerNPC
        {
            [JsonIgnore]
            public GameObject npcObject;
            public int NpcID { get; set; } = 0;
            public string NpcName { get; set; } = "Unknown";
            public string RoleName { get; set; } = "default";
            public string RoleColor { get; set; } = "default";
            public short RoleType { get; set; } = 1;
            public int ItemInHand { get; set; } = -1;
            public Vector3Json Position { get; set; } = new Vector3Json() { x=0f, y=0f, z=0f};
            public Vector3Json Rotation { get; set; } = new Vector3Json() { x = 0f, y = 0f, z = 0f };
            public Vector3Json Size { get; set; } = new Vector3Json() { x = 1f, y = 1f, z = 1f };
        }

        public class Vector3Json
        {
            public float x { get; set; } = 0f;
            public float y { get; set; } = 0f;
            public float z { get; set; } = 0f;
        }
    }
}
