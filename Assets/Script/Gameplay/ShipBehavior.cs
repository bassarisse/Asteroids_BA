using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShipBehavior : Waiter {

	const string NEW_LIFE_SFX = "new_life";
	const string CRASH_SFX = "ship_crash";
	const string HYPERSPACE_SFX = "hyperspace";

	internal enum ShipState {
		Idle = 0,
		Entering = 1,
		Hyperspacing = 2,
		Dead = 3,
	}

	public Rigidbody2D TargetBody;
	public ParticleSystem FireParticle;
	public SpaceTeleport SpaceTeleportBehavior;
	public AudioSource EngineAudio;
	public List<TrailRenderer> WingTrails;
	public Vector3 BeforeEnterPosition;
	public int Life = 3;
	public int MaxLasers = 4;
	public float MoveForceMultiplier = 7f;
	public float TurnRate = 3f;
	public float EnterImpulseMultiplier = 9.5f;
	public float EnterTime = 1f;
	public float AfterEnterTime = 1f;
	public float DeathTime = 2f;
	public float HyperspaceMargin = 1f;
	public float HyperspaceTransitionTime = 0.75f;
	public float HyperspaceWaitTime = 0.5f;
	public IntEvent OnLifeChange;
	public UnityEvent OnEnter;
	public UnityEvent OnRestore;
	public UnityEvent OnDie;
	public GameObject ShipExplosionObject;
	public GameObject HyperspaceObject;
	public GameObject HyperspaceFinishObject;
	public GameObject LaserObject;
	public GameObject LaserCrashObject;

	ShipState _state = ShipState.Idle;

	ObjectPool _shipExplosionPool;
	ObjectPool _hyperspacePool;
	ObjectPool _hyperspaceFinishPool;
	ObjectPool _laserPool;
	ObjectPool _laserCrashPool;
	bool _canHyperspace = true;
	Camera _camera;

	void Awake () {
		AudioHandler.Load (NEW_LIFE_SFX);
		AudioHandler.Load (CRASH_SFX);
		AudioHandler.Load (HYPERSPACE_SFX);

		if (OnLifeChange == null)
			OnLifeChange = new IntEvent ();
		if (OnEnter == null)
			OnEnter = new UnityEvent ();
		if (OnRestore == null)
			OnRestore = new UnityEvent ();
		if (OnDie == null)
			OnDie = new UnityEvent ();

		_shipExplosionPool = new ObjectPool (ShipExplosionObject, 1, 1);
		_hyperspacePool = new ObjectPool (HyperspaceObject, 1, 1);
		_hyperspaceFinishPool = new ObjectPool (HyperspaceFinishObject, 1, 1);
		_laserCrashPool = new ObjectPool (LaserCrashObject, MaxLasers, 2);
		_laserPool = new ObjectPool (LaserObject, MaxLasers, 0, ConfigLaser);

		_camera = Camera.main;

	}

	void ConfigLaser(GameObject newObject) {
		var laserBehavior = newObject.GetComponent<LaserBehavior> ();
		if (laserBehavior != null) {
			laserBehavior.CrashParticlePool = _laserCrashPool;
		}
	}

	void OnEnable() {
		OnLifeChange.Invoke (Life);
	}

	void Start() {
		StartCoroutine(Enter ());
	}

	void Update () {
		
		if (_state == ShipState.Dead) {
			EngineAudio.volume = 0f;
			EngineAudio.pitch = 1f;
			return;
		}

		var engineIsOn = InputExtensions.Holding.Up && _state == ShipState.Idle || _state == ShipState.Entering;

		HandleInput ();
		UpdateFire (engineIsOn);
		UpdateVolume (engineIsOn);

	}

	void HandleInput() {

		if (_state != ShipState.Idle)
			return;

		if (InputExtensions.Holding.Up) {
			this.TargetBody.AddForce (transform.up * this.MoveForceMultiplier);
		}

		if (InputExtensions.Holding.Right) {
			this.TargetBody.transform.Rotate (0, 0, -this.TurnRate);
		}

		if (InputExtensions.Holding.Left) {
			this.TargetBody.transform.Rotate (0, 0, this.TurnRate);
		}

		if (InputExtensions.Pressed.A) {
			ShootLaser ();
		}

		if (InputExtensions.Pressed.X) {
			StartCoroutine(Hyperspace ());
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

		var volume = (engineIsOn ? 0.1f : 0.075f) + Mathf.Min (0.15f, TargetBody.velocity.sqrMagnitude / 60f);

		if (_state == ShipState.Hyperspacing)
			volume /= 5f;

		EngineAudio.volume = EngineAudio.volume + (volume - EngineAudio.volume) * Time.deltaTime;
		EngineAudio.pitch = 1f + Mathf.Min (0.6f, TargetBody.velocity.sqrMagnitude / 60f);

	}

	IEnumerator Enter() {

		_state = ShipState.Entering;

		transform.position = BeforeEnterPosition;

		var hitList = new List<VoidHit> ();
		foreach (var item in VoidHit.Cache.Values)
			hitList.Add(item);
		foreach (var item in hitList)
			item.Hit ();

		FireParticle.Clear ();
		FireParticle.Play ();

		OnEnter.Invoke ();

		this.TargetBody.AddForce (transform.up * EnterImpulseMultiplier, ForceMode2D.Impulse);

		yield return Wait(EnterTime);

		_state = ShipState.Idle;

		yield return Wait(AfterEnterTime);

		OnRestore.Invoke ();
		SpaceTeleportBehavior.enabled = true;

	}

	void ShootLaser() {
		var laser = _laserPool.GetObject ();
		if (laser == null)
			return;
		laser.transform.position = transform.position;
		laser.transform.rotation = transform.rotation;
		laser.SetActive (true);
	}

	IEnumerator Hyperspace() {
		if (!_canHyperspace)
			yield break;

		_state = ShipState.Hyperspacing;
		_canHyperspace = false;

		SpaceTeleportBehavior.enabled = false;
		AudioHandler.Play (HYPERSPACE_SFX);

		var hyperspacePosition = GetHyperspacePosition ();

		var hyperspaceParticle = _hyperspacePool.GetObject ();
		hyperspaceParticle.transform.position = transform.position;
		hyperspaceParticle.transform.rotation = transform.rotation;
		hyperspaceParticle.SetActive (true);

		var hyperspaceFinishParticle = _hyperspaceFinishPool.GetObject ();
		hyperspaceFinishParticle.transform.position = hyperspacePosition;
		hyperspaceFinishParticle.transform.rotation = transform.rotation;
		hyperspaceFinishParticle.SetActive (true);

		transform.position = BeforeEnterPosition;
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.angularVelocity = 0f;

		ClearTrails ();
		FireParticle.Stop ();

		yield return Wait (HyperspaceTransitionTime);

		_state = ShipState.Idle;

		transform.position = hyperspacePosition;
		SpaceTeleportBehavior.enabled = true;
		ClearTrails ();
		FireParticle.Play ();

		yield return Wait (HyperspaceWaitTime);

		_canHyperspace = true;

	}

	IEnumerator Die() {

		_state = ShipState.Dead;

		SpaceTeleportBehavior.enabled = false;
		AudioHandler.Play (CRASH_SFX);

		var explosionParticle = _shipExplosionPool.GetObject ();
		explosionParticle.transform.position = transform.position;
		explosionParticle.transform.rotation = transform.rotation;
		explosionParticle.SetActive (true);

		transform.SetPositionAndRotation (BeforeEnterPosition, Quaternion.Euler (Vector3.zero));
		this.TargetBody.velocity = Vector2.zero;
		this.TargetBody.angularVelocity = 0f;

		ClearTrails ();
		FireParticle.Stop ();

		this.Life -= 1;
		OnLifeChange.Invoke (this.Life);

		yield return Wait (DeathTime);

		if (this.Life == 0) {
			OnDie.Invoke();
		} else {
			StartCoroutine(Enter ());
		}

	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (_state != ShipState.Idle)
			return;
		var cache = ShipHit.Cache;
		var key = collider.gameObject;
		if (cache.ContainsKey(key))
			cache [key].Hit (gameObject, collider);
	}

	public void Damage(GameObject originGameObject, Collider2D collider) {
		StartCoroutine (Die ());
	}

	public void ReceiveNewLife() {
		if (this.Life <= 0 || _state == ShipState.Dead)
			return;
		
		AudioHandler.Play (NEW_LIFE_SFX);
		this.Life += 1;
		OnLifeChange.Invoke (this.Life);

	}

	public void ClearTrails() {
		if (WingTrails == null)
			return;

		for (var i = 0; i < WingTrails.Count; i++) {
			WingTrails [i].Clear ();
		}

	}

	Vector3 GetHyperspacePosition() {

		var pos = transform.position;

		if (_camera == null)
			return pos;

		var center = _camera.transform.position;
		var height = _camera.orthographicSize * 2.0f;
		var width = height * _camera.aspect;

		return new Vector3(
			center.x + (Random.value - 0.5f) * (width - HyperspaceMargin * 2),
			center.y + (Random.value - 0.5f) * (height - HyperspaceMargin * 2),
			pos.z
		);
	}
}
