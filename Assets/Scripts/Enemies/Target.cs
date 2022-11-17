using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public MeshRenderer mesh;
    public CharacterController controller;

    public float health = 50.0f;
    public float maxHealth = 50.0f;
    public float respawnTime = 5.0f;
    float deathTimer = 0;

	void Update()
	{
		if(health <= 0)
		{
            deathTimer += Time.deltaTime;
            Debug.Log(deathTimer);
            if(deathTimer >= respawnTime)
			{
                deathTimer = 0;
                health = maxHealth;
                mesh.enabled = true;
                controller.enabled = true;
                transform.position = new Vector3(Random.Range(-20.0f, 20.0f), 0.0f, Random.Range(-10.0f, 10.0f));
            }
        }
	}

	public bool takeDamage (float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    void Die()
    {
        mesh.enabled = false;
        controller.enabled = false;
        //Destroy(gameObject);
    }
}
