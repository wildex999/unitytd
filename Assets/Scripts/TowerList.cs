using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerList
{
    //List of towers that are to be added to the game
    public static Dictionary<string, GameObject> towers = new Dictionary<string, GameObject>();

    static TowerList()
    {
        AddTower("TestTower");
        //ADD NEW TOWERS HERE
    }

    private static void AddTower(string name)
    {
        GameObject obj = Resources.Load<GameObject>("Towers/" + name);
        if (obj == null)
        {
            Debug.Log("Failed to load tower: " + name);
            return;
        }
        towers[name] = obj;
    }
}
