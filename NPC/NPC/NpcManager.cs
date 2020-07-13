using Exiled.API.Extensions;
using Exiled.API.Features;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
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
        public Dictionary<int, PlayerNPC> npcs = new Dictionary<int, PlayerNPC>();
        public Dictionary<string, int> selectedNpcs = new Dictionary<string, int>();


        public NpcManager(MainClass plugin)
        {
            singleton = this;
            this.plugin = plugin;
        }

        public void LoadNpcsAfterRestart()
        {
            foreach(var npc in npcs)
            {
                if (npc.Value.npcObject != null)
                    NetworkServer.Destroy(npc.Value.npcObject);
                CreateNPC(npc.Value.NpcName, npc.Value.RoleType, new Vector3(
                    npc.Value.Position[0],
                    npc.Value.Position[1],
                    npc.Value.Position[2]), new Vector3(
                        npc.Value.Rotation[0],
                        npc.Value.Rotation[1],
                        npc.Value.Rotation[2]), new Vector3(
                            npc.Value.Size[0],
                            npc.Value.Size[1],
                            npc.Value.Size[2]), npc.Key);
            }
        }

        public int GetFreeID()
        {
            for (int i = 1; i < int.MaxValue; i++)
            {
                if (npcs.Any(p => p.Key == i))
                    continue;

                return i;
            }
            return -1;
        }

        public void RemoveNPC(int npcId)
        {
            if (npcs.ContainsKey(npcId))
            {
                NetworkServer.Destroy(npcs[npcId].npcObject);
                npcs.Remove(npcId);
            }
        }

        public int CreateNPC(string npcName, short RoleType, Vector3 position, Vector3 rotation, Vector3 scale, int id = -1)
        {
            int npcId = id == -1 ? id : GetFreeID();
            GameObject obj =
                UnityEngine.Object.Instantiate(
                    NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
            ccm.CurClass = (RoleType)RoleType;
            ccm.RefreshPlyModel();
            obj.GetComponent<NicknameSync>().Network_myNickSync = npcName;
            obj.GetComponent<QueryProcessor>().PlayerId = npcId;
            obj.GetComponent<QueryProcessor>().NetworkPlayerId = npcId;
            obj.transform.localScale = scale;
            obj.transform.position = position;
            obj.transform.rotation = Quaternion.Euler(rotation);
            NetworkServer.Spawn(obj);
            if (id != -1)
                npcs.Add(npcId, new PlayerNPC()
                {
                    npcObject = obj,
                    NpcName = npcName,
                    RoleType = RoleType,
                    Position = new List<float>() { { position.x }, { position.y }, { position.z } },
                    Rotation = new List<float>() { { rotation.x }, { rotation.y }, { rotation.z } },
                    Size = new List<float>() { { scale.x }, { scale.y }, { scale.z } }
                });
            return npcId;
        }


        public class PlayerNPC
        {
            public GameObject npcObject;
            public string NpcName { get; set; } = "Unknown";
            public short RoleType { get; set; } = 1;
            public List<float> Position { get; set; } = new List<float>() { { 0f }, { 0f }, { 0f } };
            public List<float> Rotation { get; set; } = new List<float>() { { 0f }, { 0f }, { 0f } };
            public List<float> Size { get; set; } = new List<float>() { { 1f }, { 1f }, { 1f } };
        }
    }
}
