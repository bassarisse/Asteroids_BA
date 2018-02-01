using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHolder : MonoBehaviour {

	public static readonly Dictionary<GameObject, ScoreHolder> Cache = new Dictionary<GameObject, ScoreHolder>();

	public int Score;

	public void OnEnable() { Cache.Add(gameObject, this); }
	public void OnDisable() { Cache.Remove(gameObject); }
	
}
