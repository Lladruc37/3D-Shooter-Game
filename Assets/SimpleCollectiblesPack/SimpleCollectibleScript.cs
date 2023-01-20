using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimpleCollectibleScript : MonoBehaviour
{
	public bool rotate = true;
	public int id = 0;
	public float rotationSpeed = 1.0f;

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
		if (other.tag == "Player")
		{
			Collect (other.gameObject.GetComponent<Target>());
		}
	}

	public void Collect(Target t)
	{
		if (t.health != t.maxHealth)
		{
			Debug.Log("heal");

			t.health = t.maxHealth;
			t.healed = true;
			t.healthPackId = id;
			Destroy(gameObject);
		}
	}
}
