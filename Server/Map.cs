using Server.Object;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
	public struct Pos
	{
		public Pos(int y, int x) { Y = y; X = x; }
		public int Y;
		public int X;

		public static bool operator ==(Pos lhs, Pos rhs)
		{
			return lhs.Y == rhs.Y && lhs.X == rhs.X;
		}

		public static bool operator !=(Pos lhs, Pos rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return (Pos)obj == this;
		}

		public override int GetHashCode()
		{
			long value = (Y << 32) | X;
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}

	public struct PQNode : IComparable<PQNode>
	{
		public int F;
		public int G;
		public int Y;
		public int X;

		public int CompareTo(PQNode other)
		{
			if (F == other.F)
				return 0;
			return F < other.F ? 1 : -1;
		}
	}

	public struct Vector2Int
	{
		public float x;
		public float y;

		public Vector2Int(float x, float y) { this.x = x; this.y = y; }

		public static Vector2Int up { get { return new Vector2Int(0, 1); } }
		public static Vector2Int down { get { return new Vector2Int(0, -1); } }
		public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
		public static Vector2Int right { get { return new Vector2Int(1, 0); } }

		public static Vector2Int operator +(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.x + b.x, a.y + b.y);
		}

		public static Vector2Int operator -(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.x - b.x, a.y - b.y);
		}

		public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }
		public float sqrMagnitude { get { return (x * x + y * y); } }
		public float cellDistFromZero { get { return Math.Abs(x) + Math.Abs(y); } }
	}

	public class Map
    {
		bool[,] _collision;
		GameObject[,] _objects;

		public void LoadMap(string pathPrefix = "../../../../Common/MapData")
		{
			string text = File.ReadAllText($"{pathPrefix}/Map.txt");
			StringReader reader = new StringReader(text);

			_collision = new bool[100, 100];
			_objects = new GameObject[100, 100];

			for (int y = 0; y < 100; y++)
			{
				string line = reader.ReadLine();
				for (int x = 0; x < 100; x++)
					_collision[y, x] = (line[x] == '1' ? true : false);
			}
		}

		public bool CanGo(Vector2Int cellPos, bool checkObjects = true)
		{
			if (cellPos.x < -50 || cellPos.x > 50)
				return false;
			if (cellPos.y < -50 || cellPos.y > 50)
				return false;

			int x = (int)cellPos.x - -50; // 세로
			int y = 50 + (int)cellPos.y; // 가로
			return !_collision[x,y] && (!checkObjects || _objects[x,y] == null);
		}

		#region A* PathFinding

		// U D L R
		int[] _deltaY = new int[] { 1, -1, 0, 0 };
		int[] _deltaX = new int[] { 0, 0, -1, 1 };
		int[] _cost = new int[] { 10, 10, 10, 10 };

		public List<Vector2Int> FindPath(Vector2Int startCellPos, Vector2Int destCellPos, bool checkObjects = true, int maxDist = 20)
		{
			List<Pos> path = new List<Pos>();

			// 점수 매기기
			// F = G + H
			// F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
			// G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
			// H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

			// (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
			HashSet<Pos> closeList = new HashSet<Pos>(); // CloseList

			// (y, x) 가는 길을 한 번이라도 발견했는지
			// 발견X => MaxValue
			// 발견O => F = G + H
			Dictionary<Pos, int> openList = new Dictionary<Pos, int>(); // OpenList
			Dictionary<Pos, Pos> parent = new Dictionary<Pos, Pos>();

			// 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
			PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

			// CellPos -> ArrayPos
			Pos pos = Cell2Pos(startCellPos);
			Pos dest = Cell2Pos(destCellPos);

			// 시작점 발견 (예약 진행)
			openList.Add(pos, 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)));

			pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
			parent.Add(pos, pos);

			while (pq.Count > 0)
			{
				// 제일 좋은 후보를 찾는다
				PQNode pqNode = pq.Pop();
				Pos node = new Pos(pqNode.Y, pqNode.X);
				// 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
				if (closeList.Contains(node))
					continue;

				// 방문한다
				closeList.Add(node);

				// 목적지 도착했으면 바로 종료
				if (node.Y == dest.Y && node.X == dest.X)
					break;

				// 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
				for (int i = 0; i < _deltaY.Length; i++)
				{
					Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

					// 너무 멀면 스킵
					if (Math.Abs(pos.Y - next.Y) + Math.Abs(pos.X - next.X) > maxDist)
						continue;

					// 유효 범위를 벗어났으면 스킵
					// 벽으로 막혀서 갈 수 없으면 스킵
					if (next.Y != dest.Y || next.X != dest.X)
					{
						if (CanGo(Pos2Cell(next), checkObjects) == false) // CellPos
							continue;
					}

					// 이미 방문한 곳이면 스킵
					if (closeList.Contains(next))
						continue;

					// 비용 계산
					int g = 0;// node.G + _cost[i];
					int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
					// 다른 경로에서 더 빠른 길 이미 찾았으면 스킵

					int value = 0;
					if (openList.TryGetValue(next, out value) == false)
						value = Int32.MaxValue;

					if (value < g + h)
						continue;

					// 예약 진행
					if (openList.TryAdd(next, g + h) == false)
						openList[next] = g + h;

					pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });

					if (parent.TryAdd(next, node) == false)
						parent[next] = node;
				}
			}

			return CalcCellPathFromParent(parent, dest);
		}

		List<Vector2Int> CalcCellPathFromParent(Dictionary<Pos, Pos> parent, Pos dest)
		{
			List<Vector2Int> cells = new List<Vector2Int>();

			if (parent.ContainsKey(dest) == false)
			{
				Pos best = new Pos();
				int bestDist = Int32.MaxValue;

				foreach (Pos pos in parent.Keys)
				{
					int dist = Math.Abs(dest.X - pos.X) + Math.Abs(dest.Y - pos.Y);
					// 제일 우수한 후보를 뽑는다
					if (dist < bestDist)
					{
						best = pos;
						bestDist = dist;
					}
				}

				dest = best;
			}

			{
				Pos pos = dest;
				while (parent[pos] != pos)
				{
					cells.Add(Pos2Cell(pos));
					pos = parent[pos];
				}
				cells.Add(Pos2Cell(pos));
				cells.Reverse();
			}

			return cells;
		}

		Pos Cell2Pos(Vector2Int cell)
		{
			// CellPos -> ArrayPos
			return new Pos(50 - (int)cell.y, (int)cell.x - -50);
		}

		Vector2Int Pos2Cell(Pos pos)
		{
			// ArrayPos -> CellPos
			return new Vector2Int(pos.X + -50, 50 - pos.Y);
		}

		#endregion
	}
}
