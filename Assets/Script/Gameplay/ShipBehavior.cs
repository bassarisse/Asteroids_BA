using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShipBehavior : MonoBehaviour {

	const string CRASH_SFX = "ship_crash";
	const string HYPERSPACE_SFX = "hyperspace";

	public Rigidbody2D TargetBody;
	public ParticleSystem FireParticle;
	public SpaceTeleport SpaceTeleportBehavior;
	public List<TrailRenderer> WingTrails;
	public AudioSource EngineAudio;
	public GameObject ShipExplosionObject;
	public GameObject HyperspaceObject;
	public GameObject HyperspaceFinishObject;
	public GameObject LaserObject;
	public GameObject LaserCrashObject;
	public Vector3 BeforeEnterPosition;
	public float MoveForceMultiplier = 0.2f;
	public float TurnRate = 3f;
	public int Life = 3;
	public int MaxBullets = 4;
	public float EnterTime = 2f;
	public float HyperspaceMargin = 1f;
	public float HyperspaceTransitionTime = 0.75f;
	public float HyperspaceWaitTime = 0.5f;
	public IntEvent OnLifeChange;
	public UnityEvent OnEnter;
	public UnityEvent OnRestore;

	ObjectPool _shipExplosionPool;
	ObjectPool _hyperspacePool;
	ObjectPool _hyperspaceFinishPool;
	ObjectPool _bulletPool;
	ObjectPool _bulletCrashPool;
	float _enterTime = 0f;
	float _deathTime = 0f;
	float _hyperspaceTime = 0f;
	float _hyperspaceWaitTime = 0f;
	Vector3 _hyperspacePosition;
	Camera _camera;

	void Awake () {
		AudioHandler.Load (CRASH_SFX);
		AudioHandler.Load (HYPERSPACE_SFX);

		if (OnLifeChange == null)
			OnLifeChange = new IntEvent ();
		if (OnEnter == null)
			OnEnter = new UnityEvent ();
		if (OnRestore == null)
			OnRestore = new UnityEvent ();

		_shipExplosionPool = new ObjectPool (ShipExplosionObject, 1, 1);
		_hyperspacePool = new ObjectPool (HyperspaceObject, 1, 1);
		_hyperspaceFinishPool = new ObjectPool (HyperspaceFinishObject, 1, 1);
		_bulletCrashPool = new ObjectPool (LaserCrashObject, MaxBullets, 2);
		_bulletPool = new ObjectPool (LaserObject, MaxBullets, 0, ConfigBullet);

		_camera = Camera.main;

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

	void Start() {
		Enter ();
	}

	void Enter() {
		
		transform.position = BeforeEnterPosition;

		var hitList = new List<VoidHit> ();
		foreach (var item in VoidHit.Cache.Values)
			hitList.Add(item);
		foreach (var item in hitList)
			item.Hit ();

		_enterTime = EnterTime;
		FireParticle.Clear ();
		FireParticle.Play ();

		OnEnter.Invoke ();

	}

	void Update () {
		
		if (_deathTime > 0f) {
			EngineAudio.volume = 0f;
			EngineAudio.pitch = 1f;
			return;
		}

		var engineIsOn = (InputExtensions.Holding.Up || _enterTime > 0) && _hyperspaceTime <= 0f;

		HandleInput ();
		UpdateFire (engineIsOn);
		UpdateVolume (engineIsOn);

	}

	void HandleInput() {
		if (_enterTime > 0f || _hyperspaceTime > 0f)
			return;

		if (InputExtensions.Holding.Up) {
			this.TargetBody.AddForce (this.TargetBody.transform.up * this.MoveForceMultiplier);
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

		if (InputExtensions.Pressed.X) {
			this.Hyperspace ();
		}

	}

	void UpdateFire(bool engineIsOn) {
		
		var main = FireParticle.main;
		var emission = FireParticle.emission;

		if (engineIsOn) {
			main.startSpeed = 6;
			emission.rateOverTime = 120;
		} else {
			main.startSpeed = 2;
			emission.rateOverTime = 30;
		}

	}

	void UpdateVolume(bool engineIsOn) {

		var volume = (engineIsOn ? 0.125f : 0.1f) + Mathf.Min (0.15f, TargetBody.velocity.sqrMagnitude / 60f);

		if (_hyperspaceTime > 0f)
			volume /= 5f;

		EngineAudio.volume = EngineAudio.volume + (volume - EngineAudio.volume) * Time.deltaTime;
		EngineAudio.pitch = 1f + Mathf.Min (0.6f, TargetBody.velocity.sqrMagnitude / 60f);

	}

	void FixedUpdate() {

		if (_hyperspaceTime > 0f) {
			_hyperspaceTime -= Time.fixedDeltaTime;
			if (_hyperspaceTime <= 0f) {
				_hyperspaceWaitTime = HyperspaceWaitTime;
				this.transform.position = _hyperspacePosition;
				SpaceTeleportBehavior.enabled = true;
				ClearTrails ();
				FireParticle.Play ();
			}
		}

		if (_hyperspaceWaitTime > 0f) {
			_hyperspaceWaitTime -= Time.fixedDeltaTime;
		}

		if (_enterTime > 0f) {
			_enterTime -= Time.fixedDeltaTime;
			this.transform.position = BeforeEnterPosition * (_enterTime / EnterTime);
			if (_enterTime <= 0f) {
				OnRestore.Invoke ();
				SpaceTeleportBehavior.enabled = true;
			}
		}

		if (_deathTime > 0f) {
			_deathTime -= Time.fixedDeltaTime;
			if (_deathTime <= 0f) {
				if (this.Life == 0) { // LOSE!!!
					SceneManager.LoadScene ("Title");
				} else {
					Enter ();
				}
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

	void Hyperspace() {
		if (_hyperspaceWaitTime > 0f)
			return;
		if (_camera == null)
			return;

		var hyperspaceParticle = _hyperspacePool.GetObject ();
		hyperspaceParticle.transform.position = transform.position;
		hyperspaceParticle.transform.rotation = transform.rotation;
		hyperspaceParticle.SetActive (true);

		SpaceTeleportBehavior.enabled = false;

		AudioHandler.Play (HYPERSPACE_SFX);
		_hyperspaceTime = HyperspaceTransitionTime;
		_hyperspacePosition = GetHyperspacePosition ();

		var hyperspaceFinishParticle = _hyperspaceFinishPool.GetObject ();
		hyperspaceFinishParticle.transform.position = _hyperspacePosition;
		hyperspaceFinishParticle.transform.rotation = transform.rotation;
		hyperspaceFinishParticle.SetActive (true);

		this.transform.position = BeforeEnterPosition;
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.angularVelocity = 0f;

		ClearTrails ();
		FireParticle.Stop ();

	}

	Vector3 GetHyperspacePosition() {

		var pos = transform.position;
		var center = _camera.transform.position;
		var height = _camera.orthographicSize * 2.0f;
		var width = height * _camera.aspect;

		return new Vector3(
			center.x + (Random.value - 0.5f) * (width - HyperspaceMargin * 2),
			center.y + (Random.value - 0.5f) * (height - HyperspaceMargin * 2),
			pos.z
		);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (_deathTime > 0f || _enterTime > 0f || _hyperspaceTime > 0f)
			return;
		var cache = ShipHit.Cache;
		var key = collider.gameObject;
		if (cache.ContainsKey(key))
			cache [key].Hit (gameObject, collider);
	}

	public void Damage(GameObject originGameObject, Collider2D collider) {

		var explosionParticle = _shipExplosionPool.GetObject ();
		explosionParticle.transform.position = transform.position;
		explosionParticle.transform.rotation = transform.rotation;
		explosionParticle.SetActive (true);

		SpaceTeleportBehavior.enabled = false;
		
		AudioHandler.Play (CRASH_SFX);
		_deathTime = 2f;

		this.transform.SetPositionAndRotation (BeforeEnterPosition, Quaternion.Euler (Vector3.zero));
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.angularVelocity = 0f;

		ClearTrails ();
		FireParticle.Stop ();

		this.Life -= 1;
		OnLifeChange.Invoke (this.Life);
		
	}

	public void ClearTrails() {

		if (WingTrails != null) {
			for (var i = 0; i < WingTrails.Count; i++) {
				WingTrails [i].Clear ();
			}
		}

	}
}
