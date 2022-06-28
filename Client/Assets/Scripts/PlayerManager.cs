using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer _myPlayer;
    Dictionary<int, GameObject> _players = new Dictionary<int, GameObject>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(S_PlayerList packet)
    {
        Object obj = Resources.Load("Player");

        foreach (S_PlayerList.Player p in packet.players)
        {
            UnityEngine.GameObject go = Object.Instantiate(obj) as UnityEngine.GameObject;

            if (p.isSelf)
            {
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.ObjectId = p.playerId;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _myPlayer = myPlayer;
            }
            else
            {
                GameObject player = go.AddComponent<GameObject>();
                player.ObjectId = p.playerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _players.Add(p.playerId, player);
            }
        }
    }

    public void Move(S_BroadcastMove packet)
    {
        if (_myPlayer.ObjectId == packet.playerId)
        {
            //_myPlayer.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
            _myPlayer.transform.position = Vector3.Lerp(_myPlayer.transform.position, new Vector3(packet.posX, packet.posY, packet.posZ), 0.5f);
        }
        else
        {
            GameObject player = null;
            if (_players.TryGetValue(packet.playerId, out player))
            {
                player.transform.position = Vector3.Lerp(player.transform.position, new Vector3(packet.posX, packet.posY, packet.posZ), 0.5f);
            }
        }
    }

    public void EnterGame(S_BroadcastEnterGame packet)
    {
        if (packet.playerId == _myPlayer.ObjectId)
            return;

        Object obj = Resources.Load("Player");
        UnityEngine.GameObject go = Object.Instantiate(obj) as UnityEngine.GameObject;

		GameObject player = go.AddComponent<GameObject>();
		player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
		_players.Add(packet.playerId, player);
	}

    public void LeaveGame(S_BroadcastLeaveGame packet)
    {
        if (_myPlayer.ObjectId == packet.playerId)
        {
            UnityEngine.GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else
        {
            GameObject player = null;
            if (_players.TryGetValue(packet.playerId, out player))
            {
                UnityEngine.GameObject.Destroy(player.gameObject);
                _players.Remove(packet.playerId);
            }
        }
    }
}
