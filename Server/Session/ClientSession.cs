using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Server.Object;

namespace Server
{
	public class ClientSession : PacketSession
	{
		public int SessionId { get; set; }
		public GameRoom Room { get; set; }
		public Player MyPlayer { get; set; }

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected(Server::ClientSession) : {endPoint}");

			MyPlayer = new Player();
			MyPlayer.Id = ObjectManager.Instance.GenerateId(MyPlayer.ObjectType);
			MyPlayer.Session = this;
			MyPlayer.PosX = 0;
			MyPlayer.PosY = 0;
			MyPlayer.PosZ = 0;



			Program.Room.Push(() =>
			{
				Program.Room.Enter(MyPlayer);
			});
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			SessionManager.Instance.Remove(this);
			if (Room != null)
			{
				GameRoom room = Room;
				room.Push(() => room.Leave(MyPlayer));
				Room = null;
			}

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
