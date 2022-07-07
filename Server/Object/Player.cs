using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Object
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = Define.GameObjectType.Player;
        }
    }
}
