using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;

public class ProxNetManager : NetworkManager
{
    public static ProxNetManager DSMInstance;
    protected override void Awake()
    {
        base.Awake();
        DSMInstance = this;
    }

    [Header("Prefabs")]
    public GameObject localPlayer;
    public GameObject remotePlayer;


    protected override void Start()
    {
        base.Start();
        //client.ClientConnected += SpawnPlayer;
        //client.Connected += () => SpawnPlayer(client);
        client.ClientDisconnected += SomeClientDisconnected;
        client.ClientConnected += SomeClientConnected;
        client.Disconnected += ThisClientDisconnected;
        client.Connected += ThisClientConnected;

        server.ClientConnected += SomeClientConnectedToServer;
        server.ClientDisconnected += SomeClientDisconnectedFromServer;
    }

    void SomeClientDisconnected(Client c)
    {
        Player.Remove(c);
    }

    void SomeClientConnected(Client c)
    {
        Player.Add(c, remotePlayer);
    }

    void ThisClientDisconnected()
    {
        Player.RemoveAll();
    }

    void ThisClientConnected()
    {
        Player.Add(Client.This, localPlayer);
    }

    void SomeClientConnectedToServer(S_Client c)
    {

    }

    void SomeClientDisconnectedFromServer(S_Client c)
    {

    }

    static void Log(string msg)
    {
        Debug.Log("FOR TEST: " + msg);
    }
}
