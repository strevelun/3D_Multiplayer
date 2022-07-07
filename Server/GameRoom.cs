using Server.Object;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
	public class GameRoom : IJobQueue
	{
		JobQueue _jobQueue = new JobQueue();
		List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
		public List<Player> _players = new List<Player>();
		List<Monster> _monsters = new List<Monster>();

		public void Init()
		{
			for (int i = 0; i < 1; i++)
			{
				Monster m = new Monster();
				m.Room = this;
				Enter(m);
			}
        }

		public void Push(Action job)
		{
			_jobQueue.Push(job);
		}

		public void Update()
        {
			foreach (Monster m in _monsters)
				m.Update();
        }

		public void Flush()
		{
			foreach (Player s in _players)
				s.Session.Send(_pendingList);

			_pendingList.Clear();
		}

		public void Broadcast(ArraySegment<byte> segment)
		{
			_pendingList.Add(segment);			
		}

		public void Enter(GameObject obj)
		{
			if (obj == null)
				return;

			if(Define.GameObjectType.Player == obj.ObjectType)
            {
				Player player = obj as Player;

				_players.Add(player);
				player.Session.Room = this;

				// 신입생한테 모든 플레이어 목록 전송
				S_PlayerList players = new S_PlayerList();
				foreach (Player p in _players)
				{
					ClientSession session = p.Session;
					players.players.Add(new S_PlayerList.Player()
					{
						isSelf = (p.Session == player.Session),
						playerId = p.Id,
						posX = p.PosX,
						posY = p.PosY,
						posZ = p.PosZ,
					});
				}
				player.Session.Send(players.Write());

				S_MonsterList monsters = new S_MonsterList();
				foreach (Monster m in _monsters)
				{
					monsters.monsters.Add(new S_MonsterList.Monster()
					{
						objectId = m.Id,
						posX = m.PosX,
						posY = m.PosY,
						posZ = m.PosZ,
					});
				}
				player.Session.Send(monsters.Write());

				// 신입생 입장을 모두에게 알린다
				S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
				enter.playerId = player.Id;
				enter.posX = 0;
				enter.posY = 0;
				enter.posZ = 0;
				Broadcast(enter.Write());
			}
			else if(Define.GameObjectType.Monster == obj.ObjectType)
            {
				Monster m = obj as Monster;	

				_monsters.Add(m);

				S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
				enter.playerId = m.Id; // playerId는 objectId
				enter.posX = m.PosX;
				enter.posY = m.PosY;
				enter.posZ = m.PosZ;
				Broadcast(enter.Write());
			}
		}

		public void Leave(GameObject obj)
		{
			if (obj == null)
				return;

			if(obj.ObjectType == Define.GameObjectType.Player)
            {
				Player player = obj as Player;

				_players.Remove(player);

				// 모두에게 알린다
				S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
				leave.playerId = player.Id;
				Broadcast(leave.Write());
			}
		
		}

		public void Move(Player player, C_Move packet)
		{
			// 좌표 바꿔주고
			player.PosX = packet.posX;
			player.PosY = packet.posY;
			player.PosZ = packet.posZ;

			// 모두에게 알린다
			S_BroadcastMove move = new S_BroadcastMove();
			move.playerId = player.Id;
			move.posX = player.PosX;
			move.posY = player.PosY;
			move.posZ = player.PosZ;
			Broadcast(move.Write());
		}
	}
}
