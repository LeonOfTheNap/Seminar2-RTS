using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       UnitSelectionManager.instance.allUnitsList.Add(gameObject); 
    }

    private void OnDestroy()
    {
        UnitSelectionManager.instance.allUnitsList.Remove(gameObject); 
    }
}
