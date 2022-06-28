using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Object
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();
        //Dictionary<int, Player> _players = new Dictionary<int, Player>();

        int _counter = 0;

        public int GenerateId(Define.GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24) | (_counter++);
            }
        }

        public static Define.GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (Define.GameObjectType)type;
        }
    }
}
