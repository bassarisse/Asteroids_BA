﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidBehavior : MonoBehaviour {

	const string CRASH_SFX = "asteroid_crash";

	public Rigidbody2D Body;
	public int Level = 0;
	public float ImpulseForce = 1.5f;
	public GameObjectPool CrashPool;

	public AsteroidEvent OnStruck;
	public AsteroidEvent OnDie;
	public AsteroidEvent OnGone;

	void Awake() {
		
		AudioHandler.Load (CRASH_SFX);

		if (OnStruck == null)
			OnStruck = new AsteroidEvent ();
		if (OnDie == null)
			OnDie = new AsteroidEvent ();
		if (OnGone == null)
			OnGone = new AsteroidEvent ();

		CrashPool.Fill ();
		
	}

	void OnEnable () {
		this.Body.AddForce (this.transform.up * this.ImpulseForce, ForceMode2D.Impulse);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		var cache = AsteroidHit.Cache;
		var key = collider.gameObject;
		if (cache.ContainsKey(key))
			cache [key].Hit (gameObject, collider);
	}

	public void Struck() {
		OnStruck.Invoke (gameObject, this);
		Die ();
	}

	public void Die() {
		Gone ();
		OnDie.Invoke (gameObject, this);
	}

	public void Gone() {
		
		AudioHandler.Play (CRASH_SFX);

		var crashParticle = CrashPool.GetObject ();
		if (crashParticle != null) {
			crashParticle.transform.position = transform.position;
			crashParticle.SetActive (true);
		}
		
		this.Body.velocity = Vector2.zero;

		OnGone.Invoke (gameObject, this);

	}
}
