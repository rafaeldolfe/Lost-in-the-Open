using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreadedPathfinding;

 public class PathfindingCoroutine {
     public Coroutine Coroutine { get; private set; }
     public PathfindingResolution result;
     private readonly IEnumerator target;
     public PathfindingCoroutine(MonoBehaviour owner, IEnumerator target) {
         this.target = target;
         this.Coroutine = owner.StartCoroutine(Run());
     }
 
     private IEnumerator Run() {
         while(target.MoveNext()) {
             result = (PathfindingResolution) target.Current;
             yield return result;
         }
     }
 }
