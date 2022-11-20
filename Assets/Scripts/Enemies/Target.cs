using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    public MeshRenderer bodyMesh;
    public MeshRenderer gunBarrelMesh;
    public MeshRenderer gunBodyMesh;

    public Gun gun;
    public CharacterController controller;
    public CapsuleCollider bodyCollider;
    public SendRecieve sr;
    public GameObject deathBoxUI;
    public Text deathText;

    public int health = 5;
    public int maxHealth = 5;
    public float respawnTime = 5.0f;
    float deathTimer = 0.0f;

	void Update()
	{
		if(health <= 0)
		{
            if (deathTimer <= 0.0f) //first frame you are dead
            {
                Die();
                if (sr.isControlling) //you died
                {
                    deathBoxUI.SetActive(true);
                    deathText.text = "You are dead!";
                }
            }

            deathTimer += Time.deltaTime;
            if(deathTimer >= respawnTime) //respawning
			{
                deathBoxUI.SetActive(false);
                deathText.text = "";
                deathTimer = 0.0f;
                bodyMesh.enabled = true;
                gunBarrelMesh.enabled = true;
                gunBodyMesh.enabled = true;
                gun.enabled = true;
                Debug.Log("RESPAWN");
                if (sr.isControlling) //you respawn
                {
                    Debug.Log("YOU RESPAWN");
                    controller.enabled = true;
                    controller.Move(RandomizeSpawn());
                }
                else //another player respawns in your world
                {
                    bodyCollider.enabled = true;
                    sr.position = RandomizeSpawn();
                    sr.updateCharacter = true;
                }
                health = maxHealth;
            }
        }
	}

    Vector3 RandomizeSpawn() //return a random position within the map bounds
	{
        Vector3 result = new Vector3(Random.Range(-115.0f, 65.0f), 1.234f, Random.Range(-105.0f, 75.0f));
        return result;
	}

	private void OnCollisionEnter(Collision collision) //in case you spawn inside another object
	{
        if(!collision.transform.GetComponent("Target"))
		{
            sr.position = RandomizeSpawn();
		}
	}


	public bool takeDamage (int amount) //reduce HP and return true if dead
    {
        health -= amount;
        if (health <= 0)
        {
            Debug.Log("DEAD: " + sr.uid + "-" + sr.isControlling);
            return true;
        }
        return false;
    }

    void Die() //handles death
    {
        bodyMesh.enabled = false;
        gunBarrelMesh.enabled = false;
        gunBodyMesh.enabled = false;
        gun.enabled = false;
        controller.enabled = false;
        bodyCollider.enabled = false;
    }
}
