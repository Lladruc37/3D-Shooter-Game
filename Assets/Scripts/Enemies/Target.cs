using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    // Meshes
    public MeshRenderer bodyMesh;
    public MeshRenderer gunBarrelMesh;
    public MeshRenderer gunBodyMesh;

    // Gameplay
    public LayerMask ceilingMask;
    public LayerMask playerMask;
    public Gun gun;
    public CharacterController controller;
    public CapsuleCollider bodyCollider;
    public SendReceive sr;

    // UI
    public GameObject deathBoxUI;
    public Text deathText;
    public AudioSource hit, revive, collectSound;

    // Data
    public int health = 5;
    public int maxHealth = 5;
    public float respawnTime = 5.0f;
    float deathTimer = 0.0f;
    public bool healed = false;
    public int healthPackId = 0;
    bool damageTaken = false;

	void Update()
	{
        if(damageTaken)
		{
            damageTaken = false;
            if (sr.isControlling)
            {
                hit.Play();
            }
        }
        if (health <= 0)
		{
            if (deathTimer <= 0.0f) //First frame you are dead
            {
                Die();
                if (sr.isControlling) //You died
                {
                    deathBoxUI.SetActive(true);
                    deathText.text = "You are dead!";
                }
            }

            deathTimer += Time.deltaTime;
            if(deathTimer >= respawnTime) //Respawning
			{
                deathBoxUI.SetActive(false);
                deathText.text = "";
                deathTimer = 0.0f;
                bodyMesh.enabled = true;
                gunBarrelMesh.enabled = true;
                gunBodyMesh.enabled = true;
                gun.enabled = true;
                if (sr.isControlling) //You respawn
                {
                    controller.enabled = true;
                    this.transform.position = RandomizeSpawn();
                    revive.Play();
                }
                else //Another player respawns in your world
                {
                    bodyCollider.enabled = true;
                    sr.position = RandomizeSpawn();
                    sr.updateCharacter = true;
                }
                health = maxHealth;
            }
        }
	}

    public Vector3 RandomizeSpawn() //Return a random position within the map bounds
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

    void OnTriggerEnter(Collider other) // Collect health packs
    {
        if (other.tag == "Collectible" && healed)
        {
            collectSound.Play();
            healed = false;
            sr.gm.healthPack = true;
            sr.gm.healthPackId = healthPackId;
            sr.gm.healthPacks.Remove(sr.gm.healthPacks.Find(s => s.id == healthPackId));
        }
    }

    public bool TakeDamage (int amount) //Reduce HP and return true if dead
    {
        damageTaken = true;
        health -= amount;
        if (health <= 0)
        {
            Debug.Log("TakeDamage(): DEAD: " + sr.uid + "-" + sr.isControlling);
            return true;
        }
        return false;
    }

    void Die() //Handles death
    {
        bodyMesh.enabled = false;
        gunBarrelMesh.enabled = false;
        gunBodyMesh.enabled = false;
        gun.enabled = false;
        controller.enabled = false;
        bodyCollider.enabled = false;
    }
}
