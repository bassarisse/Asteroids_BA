using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Collider2DEvent : UnityEvent<GameObject, Collider2D> {
}

[System.Serializable]
public class IntEvent : UnityEvent<int> {
}