using UnityEngine;
using System.Collections;

public class Mob2_Flying : Monster
{

    // Use this for initialization
    public Mob2_Flying()
    {
        speed = 15;
        health = 10;
        moveType = MonsterMoveType.Flying;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        setPath(map.flyingPath);
    }

    // Update is called once per frame
    void Update()
    {
        followPathLocal();
    }

    public override void StepUpdate()
    {
        followPathFixed();
    }
}
