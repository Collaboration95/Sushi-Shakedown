using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDataSO", menuName = "Scriptable Objects/GameDataSO")]
public class GameDataSO : ScriptableObject
{


    //store mouseposition for offset calculation, calculated in draggable objects
    
    public Vector3 mousePosition;
    public LayerMask interactableLayers;

    //use to track what is being dragged right now, so as to trigger appropriate methods when left click released

}
