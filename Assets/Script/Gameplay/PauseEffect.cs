using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Rigidbody2DState {
	public Vector2 velocity;
	public float angularVelocity;
}

public class TrailRendererState {
	public float time;
}

public class PauseEffect : MonoBehaviour {

	public static readonly Dictionary<GameObject, PauseEffect> Cache = new Dictionary<GameObject, PauseEffect>();
	public void OnEnable() { Cache.Add(gameObject, this); }
	public void OnDisable() { Cache.Remove(gameObject); }

	public bool InvertStates = false;
	public UnityEvent OnPause;
	public UnityEvent OnResume;
	public GameObject[] GameObjectToDisable;
	public MonoBehaviour[] ComponentsToDisable;
	public ParticleSystem[] Particles;
	public Rigidbody2D[] Bodies;
	public TrailRenderer[] Trails;

	List<Rigidbody2DState> _bodyStates;
	List<TrailRendererState> _trailStates;

	void Awake () {
		
		if (OnPause == null)
			OnPause = new UnityEvent ();
		if (OnResume == null)
			OnResume = new UnityEvent ();

		_bodyStates = new List<Rigidbody2DState>();
		if (Bodies != null) {
			for (var i = 0; i < Bodies.Length; i++) {
				_bodyStates.Add (new Rigidbody2DState ());
			}
		}

		_trailStates = new List<TrailRendererState>();
		if (Trails != null) {
			for (var i = 0; i < Trails.Length; i++) {
				_trailStates.Add (new TrailRendererState ());
			}
		}

	}

	public void Pause() {
		SetPause (!InvertStates);
		OnPause.Invoke ();
	}

	public void Resume() {
		SetPause (InvertStates);
		OnResume.Invoke ();
	}

	void SetPause(bool paused) {

		if (GameObjectToDisable != null) {
			for (var i = 0; i < GameObjectToDisable.Length; i++) {
				GameObjectToDisable [i].SetActive(!paused);
			}
		}

		if (ComponentsToDisable != null) {
			for (var i = 0; i < ComponentsToDisable.Length; i++) {
				ComponentsToDisable [i].enabled = !paused;
			}
		}

		if (Particles != null) {
			for (var i = 0; i < Particles.Length; i++) {
				var particle = Particles [i];
				if (!particle.gameObject.activeInHierarchy)
					continue;
				if (paused && particle.isPlaying)
					particle.Pause (true);
				else if (!paused && particle.isPaused)
					particle.Play (true);
			}
		}

		if (Bodies != null) {
			for (var i = 0; i < Bodies.Length; i++) {
				var body = Bodies [i];
				var bodyState = _bodyStates [i];
				if (paused) {
					bodyState.velocity = body.velocity;
					bodyState.angularVelocity = body.angularVelocity;
					body.isKinematic = true;
					body.velocity = Vector2.zero;
					body.angularVelocity = 0f;
				} else {
					body.isKinematic = false;
					body.velocity = bodyState.velocity;
					body.angularVelocity = bodyState.angularVelocity;
					body.WakeUp ();
				}
			}
		}

		if (Trails != null) {
			for (var i = 0; i < Trails.Length; i++) {
				var trail = Trails [i];
				var trailState = _trailStates [i];
				if (paused) {
					trailState.time = trail.time;
					trail.time = Mathf.Infinity;
				} else {
					trail.time = trailState.time;
				}
			}
		}

	}

}
