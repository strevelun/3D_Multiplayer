using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Server.Define;

namespace Assets.Scripts
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();
        Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public void Enter(S_BroadcastEnterGame pkt)
        {
            GameObjectType type = GetObjectTypeById(pkt.playerId);
            if (type == GameObjectType.Player)
            {
                PlayerManager.Instance.EnterGame(pkt);
            }
            else if (type == GameObjectType.Monster)
            {
                UnityEngine.Object obj = Resources.Load("Monster");
                UnityEngine.GameObject go = UnityEngine.Object.Instantiate(obj) as UnityEngine.GameObject;

                GameObject monster = go.AddComponent<GameObject>();
                monster.transform.position = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
                _objects.Add(pkt.playerId, monster);
            }
        }

        public void Add(S_MonsterList packet)
        {
            UnityEngine.Object obj = Resources.Load("Monster");

            foreach (S_MonsterList.Monster m in packet.monsters)
            {
                UnityEngine.GameObject go = UnityEngine.Object.Instantiate(obj) as UnityEngine.GameObject;

                GameObject monster = go.AddComponent<GameObject>();
                monster.ObjectId = m.objectId;
                monster.transform.position = new Vector3(m.posX, m.posY, m.posZ);
                _objects.Add(m.objectId, monster);
            }
        }

        public void Move(S_BroadcastMove packet)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(packet.playerId);
            if(type == GameObjectType.Player)
            {
                PlayerManager.Instance.Move(packet);
            }
            else
            {
                _objects[packet.playerId].gameObject.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
            }
        }
    }
}
