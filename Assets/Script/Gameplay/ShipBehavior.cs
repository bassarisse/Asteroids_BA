using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShipBehavior : MonoBehaviour {

	const string CRASH_SFX = "ship_crash";

	public Rigidbody2D TargetBody;
	public ParticleSystem FireParticle;
	public AudioSource EngineAudio;
	public GameObject LaserObject;
	public GameObject LaserCrashObject;
	public float MoveForceMultiplier = 0.2f;
	public float TurnRate = 3f;
	public int Life = 3;
	public int MaxBullets = 4;
	public IntEvent OnLifeChange;
	public UnityEvent OnRestore;

	ObjectPool _bulletPool;
	ObjectPool _bulletCrashPool;
	float _deathTime = 0f;

	void Awake () {
		AudioHandler.Load (CRASH_SFX);

		if (OnLifeChange == null)
			OnLifeChange = new IntEvent ();
		if (OnRestore == null)
			OnRestore = new UnityEvent ();

		_bulletCrashPool = new ObjectPool (LaserCrashObject, MaxBullets, 2);
		_bulletPool = new ObjectPool (LaserObject, MaxBullets, 0, ConfigBullet);

	}

	void ConfigBullet(GameObject newObject) {
		var bulletBehavior = newObject.GetComponent<BulletBehavior> ();
		if (bulletBehavior != null) {
			bulletBehavior.CrashParticlePool = _bulletCrashPool;
		}
	}

	void OnEnable() {
		OnLifeChange.Invoke (Life);
	}

	void Update () {
		
		if (_deathTime > 0f) {
			EngineAudio.volume = 0f;
			EngineAudio.pitch = 1f;
			return;
		}

		var main = FireParticle.main;
		var emission = FireParticle.emission;

		if (InputExtensions.Holding.Up) {
			this.TargetBody.AddForce (this.TargetBody.transform.up * this.MoveForceMultiplier);
			main.startSpeed = 6;
			emission.rateOverTime = 120;
		} else {
			main.startSpeed = 2;
			emission.rateOverTime = 30;
		}

		if (InputExtensions.Holding.Right) {
			this.TargetBody.transform.Rotate (0, 0, -this.TurnRate);
		}

		if (InputExtensions.Holding.Left) {
			this.TargetBody.transform.Rotate (0, 0, this.TurnRate);
		}

		if (InputExtensions.Pressed.A) {
			this.FireLaser ();
		}

		EngineAudio.volume = (InputExtensions.Holding.Up ? 0.15f : 0.1f) + Mathf.Min (0.15f, TargetBody.velocity.sqrMagnitude / 60f);
		EngineAudio.pitch = 1f + Mathf.Min (0.6f, TargetBody.velocity.sqrMagnitude / 60f);
		
	}

	void FixedUpdate() {

		if (_deathTime > 0f && this.Life > 0) {
			_deathTime -= Time.fixedDeltaTime;
			if (_deathTime <= 0f) {
				OnRestore.Invoke ();
				FireParticle.Clear ();
				FireParticle.Play ();
			}
		}
		
	}

	void FireLaser() {
		var bullet = _bulletPool.GetObject ();
		if (bullet == null)
			return;
		bullet.transform.position = transform.position;
		bullet.transform.rotation = transform.rotation;
		bullet.SetActive (true);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (_deathTime > 0f)
			return;
		var cache = ShipHit.Cache;
		var key = collider.gameObject;
		if (cache.ContainsKey(key))
			cache [key].Hit (gameObject, collider);
	}

	public void Damage(GameObject originGameObject, Collider2D collider) {
		
		AudioHandler.Play (CRASH_SFX);
		_deathTime = 2f;
		FireParticle.Stop ();
		_bulletPool.Reclaim ();
		_bulletCrashPool.Reclaim ();

		this.transform.SetPositionAndRotation (Vector3.zero, Quaternion.Euler (Vector3.zero));
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.angularVelocity = 0f;

		this.Life -= 1;
		OnLifeChange.Invoke (this.Life);

		if (this.Life == 0) // LOSE!!!
			SceneManager.LoadScene ("Game");
		
	}
}
