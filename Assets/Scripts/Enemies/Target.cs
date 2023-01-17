using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    public MeshRenderer bodyMesh;
    public MeshRenderer gunBarrelMesh;
    public MeshRenderer gunBodyMesh;

    public LayerMask ceilingMask;
    public LayerMask playerMask;
    public Gun gun;
    public CharacterController controller;
    public CapsuleCollider bodyCollider;
    public SendReceive sr;
    public GameObject deathBoxUI;
    public Text deathText;

    public int health = 5;
    public int maxHealth = 5;
    public float respawnTime = 5.0f;
    float deathTimer = 0.0f;
    public bool healed = false;

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
                Debug.Log("Respawning entity...");
                if (sr.isControlling) //you respawn
                {
                    Debug.Log("You have respawned!");
                    controller.enabled = true;
                    this.transform.position = RandomizeSpawn();
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

    public Vector3 RandomizeSpawn() //return a random position within the map bounds
	{
        List<int> blacklistedSpawns = new List<int>();
        int randomSpawnIndex = UnityEngine.Random.Range(0, 15);
        bool collide = Physics.CheckSphere(sr.gm.spawnpoints[randomSpawnIndex], 35.0f, 6);
        while (collide)
        {
            blacklistedSpawns.Add(randomSpawnIndex);
            randomSpawnIndex = UnityEngine.Random.Range(0, 15);
            while (blacklistedSpawns.Contains(randomSpawnIndex))
            {
                randomSpawnIndex = UnityEngine.Random.Range(0, 15);
            }
            collide = Physics.CheckSphere(sr.gm.spawnpoints[randomSpawnIndex], 35.0f, 6);
        }

        return sr.gm.spawnpoints[randomSpawnIndex];
    }

    //private void OnCollisionEnter(Collision collision) //in case you spawn inside another player
    //{
    //    if (!collision.transform.GetComponent("Target"))
    //    {
    //        sr.position = RandomizeSpawn();
    //    }
    //}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Collectible" && healed)
        {
            healed = false;
            sr.gm.healthPack = true;
        }
    }



    public bool TakeDamage (int amount) //reduce HP and return true if dead
    {
        health -= amount;
        if (health <= 0)
        {
            Debug.Log("TakeDamage(): DEAD: " + sr.uid + "-" + sr.isControlling);
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
