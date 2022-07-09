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

        public float _speed = 10.0f;

        public Monster(bool randomPos = false)
        {
            ObjectType = Define.GameObjectType.Monster;
            if(!randomPos)
            {
                PosX = -44;
                PosY = 0;
                PosZ = -47;
            }
            else
            {
                PosX = _rand.Next(-50, 0);
                PosY = 0;
                PosZ = _rand.Next(-50, 0);
            }

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

            FindClosest();

            if (_target == null)
                return;
           
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

                if (dist < min && dist < 20.0f)
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

            if (Vector3.Distance(new Vector3(PosX, PosY, PosZ), new Vector3(_target.PosX, _target.PosY, _target.PosZ)) <= 0.1f)
            {
                State = Define.CreatureState.Idle;
                _target = null;
                return;
            }

            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / _speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            List<Vector2Int> path = Room.Map.FindPath(new Vector2Int(PosX, PosZ), new Vector2Int(_target.PosX, _target.PosZ), checkObjects: true);
            if (path.Count < 2 || path.Count > 30) // _chaseCellDist
            {
                _target = null;
                State = Define.CreatureState.Idle;
                //BroadcastMove();
                return;
            }



            //var dir = new Vector3(path[1].x, 0, path[1].y) - new Vector3(PosX, 0, PosZ);
            //PosX += (dir.Normalize() * _speed * 0.01f).x;
            //PosZ += (dir.Normalize() * _speed * 0.01f).z;

            if(Room.Map.CanGo(new Vector2Int(path[1].x, path[1].y)))
            {
                PosX = path[1].x * _speed * 0.1f;
                PosZ = path[1].y * _speed * 0.1f;
                BroadcastMove();
            }

        }

        void BroadcastMove()
        {
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = Id; // objectId
            move.posX = PosX;
            move.posY = PosY;
            move.posZ = PosZ;
            Room.Push(() => Room.Broadcast(move.Write()));
        }
    }
}