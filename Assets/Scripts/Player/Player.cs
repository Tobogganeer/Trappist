using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobo.Net;
//using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> All = new Dictionary<ushort, Player>();

    public ushort ID { get; private set; }
    public string Username { get; private set; }

    public NetTransform netTransform;
    public PlayerAnimation animator;

    public bool LocalPlayer => ID == NetworkManager.MyID;
    public Vector3 Position => transform.position;

    //Transform cam;


    private void Start()
    {
        if (LocalPlayer)
        {
            //cam = GetComponentInChildren<Camera>().transform;
            //LobbyCam.Disable(cam);
        }

        //Scourge.AddPlayer(this);
    }

    private void OnDestroy()
    {
        //if (All.Count == 1)
        //    LobbyCam.Enable(cam);

        if (All.ContainsKey(ID))
            All.Remove(ID);
    }

    public static void Add(Client c, GameObject obj)
    {
        Player player = Instantiate(obj, Vector3.up, Quaternion.identity).GetComponent<Player>();

        player.ID = c.ID;
        player.Username = c.Username;
        player.name = $"Player {c.Username} ({c.ID})";

        All.Add(c.ID, player);
    }

    public static void Remove(Client c)
    {
        if (All.TryGetValue(c.ID, out Player p))
        {
            Destroy(p.gameObject);
        }
    }

    public static void RemoveAll()
    {
        foreach (Player p in All.Values)
        {
            Destroy(p.gameObject);
        }
    }
}
