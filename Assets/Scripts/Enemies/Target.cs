using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public MeshRenderer bodyMesh;
    public MeshRenderer gunMesh;
    public MeshRenderer gunBod;

    public Gun gun;
    public CharacterController controller;
    public CapsuleCollider body;
    public bool isControlling;

    public int health = 3;
    public int maxHealth = 3;
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
                deathTimer = 0.0f;
                health = maxHealth;
                bodyMesh.enabled = true;
                gunMesh.enabled = true;
                gunBod.enabled = true;
                gun.enabled = true;
                if (isControlling) controller.enabled = true;
                else body.enabled = true;
                transform.localPosition = new Vector3(Random.Range(-20.0f, 20.0f), 0.0f, Random.Range(-10.0f, 10.0f));
            }
        }
	}

	public bool takeDamage (int amount)
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
        bodyMesh.enabled = false;
        gunMesh.enabled = false;
        gunBod.enabled = false;
        gun.enabled = false;
        controller.enabled = false;
        body.enabled = false;
    }
}
