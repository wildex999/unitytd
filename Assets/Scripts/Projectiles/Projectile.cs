using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    protected Monster target;

    //public projectileType type;
    public int damage;
    public float speed;


    public virtual void setTarget(Monster target)
    {
        this.target = target;
    }

    //Get the base damage this projectile does
    public virtual int getDamage()
    {
        return damage;
    }

	void Update ()
    {
        moveBullet();
	}

    public void moveBullet()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        //Move towards the target
        float velocity = Time.deltaTime * speed;

        //Check how close to the target we are
        Vector3 direction = target.transform.position - transform.position;
        if (direction.sqrMagnitude <= (velocity * velocity))
        {
            //Hit, do damage to enemy
            target.onHit(this);
            Destroy(gameObject);
        }
        else
        {
            Vector3 directionNormalized = direction.normalized;
            transform.position += (directionNormalized * velocity);

            //Rotate towards the target
            float angle = Mathf.Atan2(directionNormalized.y, directionNormalized.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle+90f));
        }
    }
}
