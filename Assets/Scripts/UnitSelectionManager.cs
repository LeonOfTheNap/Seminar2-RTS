using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
   public static UnitSelectionManager instance { get; set; }
   public List<GameObject> allUnitsList = new List<GameObject>();
   public List<GameObject> selectedUnits = new List<GameObject>();

   public LayerMask clickable;
   public LayerMask ground;
   public GameObject groundMarker;

   private Camera camera;
   
   private void Awake()
   {
      if (instance != null && instance != this)
      {
         Destroy(this.gameObject);
      }
      else
      {
         instance = this;
      }
      
      
   }

   private void Start()
   {
      camera = Camera.main;
   }

   private void Update()
   {
      if (Input.GetMouseButtonDown(0))
      {
         RaycastHit hit;
            
         //check if you hit something with right mouse button
         Ray ray = camera.ScreenPointToRay(Input.mousePosition);

         //if you're clicking a clickable object (unit or building)
         if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
         {
            if (Input.GetKey(KeyCode.LeftShift))
            {
               MultiSelect(hit.collider.gameObject);
            }
            else
            {
               SelectByClicking(hit.collider.gameObject);
            }
         }
         else
         {

            if (!Input.GetKey(KeyCode.LeftShift))
            {
               DeselectAll();
            }
         }
      }

      if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
      {
         RaycastHit hit;
         Ray ray = camera.ScreenPointToRay(Input.mousePosition);

         if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
         {
            groundMarker.transform.position = hit.point;
            
            
            groundMarker.SetActive(false);
            groundMarker.SetActive(true);
         }
         
      }
   }

   private void MultiSelect(GameObject unit)
   {
      if (selectedUnits.Contains(unit) == false)
      {
         selectedUnits.Add(unit);
         SelectUnit(unit, true);
      }
      else
      {
         SelectUnit(unit, false);
         selectedUnits.Remove(unit);
      }
   }

   public void DeselectAll()
   {
      foreach (var unit in selectedUnits)
      {
         SelectUnit(unit, false);
      }
      
      groundMarker.SetActive(false);
      
      selectedUnits.Clear();
   }

   private void SelectByClicking(GameObject unit)
   {
      DeselectAll();
      
      selectedUnits.Add(unit);
      
      SelectUnit(unit, true);
   }
   
   public void DragSelect(GameObject unit)
   {
      if (selectedUnits.Contains(unit) == false)
      {
         selectedUnits.Add(unit);
         SelectUnit(unit, true);
      }
   }

   private void SelectUnit(GameObject unit, bool isSelected)
   {
      TriggerSelectionIndicator(unit, isSelected);
      EnableUnitMovement(unit, isSelected);
   }

   private void EnableUnitMovement(GameObject unit, bool enableMove)
   {
      unit.GetComponent<UnitMovement>().enabled = enableMove;
   }

   private void TriggerSelectionIndicator(GameObject unit, bool isVisible)
   {
      unit.transform.GetChild(0).gameObject.SetActive(isVisible);
   }

   
}
