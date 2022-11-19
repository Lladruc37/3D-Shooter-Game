using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    public MeshRenderer bodyMesh;
    public MeshRenderer gunMesh;
    public MeshRenderer gunBod;

    public Gun gun;
    public CharacterController controller;
    public CapsuleCollider body;
    public SendRecieve sr;
    public Text deathText;

    public int health = 3;
    public int maxHealth = 3;
    public float respawnTime = 5.0f;
    float deathTimer = 0;

	void Update()
	{
		if(health <= 0)
		{
            if (sr.isControlling) deathText.text = "You are dead!";
            deathTimer += Time.deltaTime;
            //Debug.Log(deathTimer);
            if(deathTimer >= respawnTime)
			{
                deathText.text = "";
                deathTimer = 0.0f;
                bodyMesh.enabled = true;
                gunMesh.enabled = true;
                gunBod.enabled = true;
                gun.enabled = true;
                Debug.Log("RESPAWN");
                if (sr.isControlling)
                {
                    Debug.Log("YOU RESPAWN");
                    controller.enabled = true;
                }
                else
                {
                    body.enabled = true;
                }
                sr.updateCharacter = true;
            }
        }
	}

	public bool takeDamage (int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Debug.Log("DEAD: " + sr.uid + "-" + sr.isControlling);
            if (sr.isControlling) Debug.Log("YOU ARE DEAD!");
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
