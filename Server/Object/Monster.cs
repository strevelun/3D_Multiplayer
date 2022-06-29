using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Object
{
    class Monster : GameObject
    {
        Random _rand = new Random();
        Player _target = null; // 타겟이 있을때만 움직임
        long _nextMoveTick = 0;

        public float _speed = 25.0f;

        public Monster()
        {
            ObjectType = Define.GameObjectType.Monster;
            PosX = _rand.Next(-20, 20);
            PosY = 0;
            PosZ = _rand.Next(-20, 20);

            Id = ObjectManager.Instance.GenerateId(ObjectType);

            State = Define.CreatureState.Idle;
        }

        public void Update()
        {
            switch(State)
            {
                case Define.CreatureState.Idle:
                    UpdateIdle();
                    break;
                case Define.CreatureState.Moving:
                    UpdateMoving();
                    break;
            }
        }

        public void UpdateIdle()
        {
            if (Room._players.Count == 0)
                return;

            Room.Push(FindClosest);
           
            State = Define.CreatureState.Moving;
        }

        void FindClosest()
        {
            var min = 9999999f;
            Player target = null;

            foreach (Player p in Room._players)
            {
                target = p;
                var dist = Vector3.Distance(new Vector3(PosX, PosY, PosZ), new Vector3(target.PosX, target.PosY, target.PosZ));

                if (dist < min && dist < 10.0f)
                {
                    min = dist;
                    _target = target;
                }
            }
        }

        public void UpdateMoving()
        {
            if (_target == null)
            {
                State = Define.CreatureState.Idle;
                return;
            }

            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / _speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            Room.Push(FindClosest);

            if (Vector3.Distance(new Vector3(PosX, PosY, PosZ), new Vector3(_target.PosX, _target.PosY, _target.PosZ)) <= 0.1f)
            {
                State = Define.CreatureState.Idle;
                return;
            }

            var dir = new Vector3(_target.PosX, _target.PosY, _target.PosZ) - new Vector3(PosX, PosY, PosZ);
            PosX += (dir.Normalize() * GameObject.DeltaTime * _speed).x;
            PosZ += (dir.Normalize() * GameObject.DeltaTime * _speed).z;

            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = Id;
            move.posX = PosX;
            move.posY = PosY;
            move.posZ = PosZ;
            Room.Push(() => Room.Broadcast(move.Write()));
        }
    }
}