using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class Define
    {
        public enum GameObjectType
        {
            None,
            Player,
            Monster,
        }

        public enum CreatureState
        {
            Idle,
            Moving,
        }
    }
}
