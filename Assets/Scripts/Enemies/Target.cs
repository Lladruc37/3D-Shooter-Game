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
    public GameObject deathBox;
    public Text deathText;

    public int health = 5;
    public int maxHealth = 5;
    public float respawnTime = 5.0f;
    float deathTimer = 0;

	void Update()
	{
		if(health <= 0)
		{
            if (deathTimer <= 0.0f)
            {
                Die();
                if (sr.isControlling)
                {
                    deathBox.SetActive(true);
                    deathText.text = "You are dead!";
                }
            }
            
            deathTimer += Time.deltaTime;
            //Debug.Log(deathTimer);
            if(deathTimer >= respawnTime)
			{
                deathBox.SetActive(false);
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
                    controller.Move(RandomizeSpawn());
                }
                else
                {
                    body.enabled = true;
                    sr.position = RandomizeSpawn();
                    sr.updateCharacter = true;
                }
                health = maxHealth;
            }
        }
	}

    Vector3 RandomizeSpawn()
	{
        Vector3 result = new Vector3(Random.Range(-115.0f, 65.0f), 1.234f, Random.Range(-105.0f, 75.0f));
        return result;
	}

	private void OnCollisionEnter(Collision collision)
	{
        if(!collision.transform.GetComponent("Target"))
		{
            sr.position = RandomizeSpawn();
		}
	}


	public bool takeDamage (int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Debug.Log("DEAD: " + sr.uid + "-" + sr.isControlling);
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
