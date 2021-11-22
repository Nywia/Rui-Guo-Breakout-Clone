using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ScrNetworkManagerBreakout : NetworkManager
{
    [Header("Player Paddles")]
    public Transform PlayerSpawn1;
    public Transform PlayerSpawn2;

    Dictionary<GameObject, GameObject> Players = new Dictionary<GameObject, GameObject>();

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Adds player at their spawn position and spawn them
        Transform start = numPlayers == 0 ? PlayerSpawn1 : PlayerSpawn2;
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        // Create the player's ball and assign authority to them
        GameObject ball = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Ball"));
        NetworkServer.Spawn(ball, player);

        // Add to dictonary for tracking
        Players.Add(player, ball);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // Calls the base functionality (Actually destroys the player)
        base.OnServerDisconnect(conn);
    }
}
