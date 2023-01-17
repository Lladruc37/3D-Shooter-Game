using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleCollectibleScript : MonoBehaviour
{
	public bool rotate = true;
	public int id = 0;
	public float rotationSpeed = 1.0f;
	public AudioClip collectSound;
	public GameObject collectEffect;

	// Use this for initialization
	void Start ()
	{}
	
	// Update is called once per frame
	void Update ()
	{
		if (rotate)
			transform.Rotate (Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
	}

	void OnTriggerEnter(Collider other)
	{
		Debug.Log("COLLISIONSNS");
		if (other.tag == "Player") {
			Collect (other.gameObject.GetComponent<Target>());
		}
	}

	public void Collect(Target t)
	{
		Debug.Log("PLAYER COLLISION");

		if (t.health != t.maxHealth)
		{
			Debug.Log("HEAL");
			if (collectSound)
				AudioSource.PlayClipAtPoint(collectSound, transform.position);
			if (collectEffect)
				Instantiate(collectEffect, transform.position, Quaternion.identity);

			t.health = t.maxHealth;
			t.healed = true;
			Destroy(gameObject);
		}
	}
}
