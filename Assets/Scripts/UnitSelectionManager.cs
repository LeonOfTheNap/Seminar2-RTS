using System.Collections.Generic;
using UnityEngine;

public enum FormationType
{
    Box,
    Wedge,
    Column
}

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager instance { get; set; }

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> selectedUnits = new List<GameObject>();

    public LayerMask clickable;
    public LayerMask ground;
    public GameObject groundMarker;

    private Camera camera;
    public FormationType currentFormation = FormationType.Box;

    private Vector3 formationStartPoint;
    private bool isSelectingFormation = false;

    private Vector3 lastFormationCenter = Vector3.zero;
    private Vector3 lastFormationDirection = Vector3.forward;
    private bool hasLastFormation = false;

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
        // Switch formation type and reapply if possible
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentFormation = FormationType.Box;
            if (hasLastFormation) MoveUnitsInFormation(lastFormationCenter, lastFormationDirection);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentFormation = FormationType.Wedge;
            if (hasLastFormation) MoveUnitsInFormation(lastFormationCenter, lastFormationDirection);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentFormation = FormationType.Column;
            if (hasLastFormation) MoveUnitsInFormation(lastFormationCenter, lastFormationDirection);
        }

        // Right click DOWN: Store formation center point
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            {
                formationStartPoint = hit.point;
                isSelectingFormation = true;
            }
        }

        // Right click UP: Compute direction and place formation
        if (Input.GetMouseButtonUp(1) && isSelectingFormation && selectedUnits.Count > 0)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            {
                Vector3 direction = (hit.point - formationStartPoint).normalized;
                MoveUnitsInFormation(formationStartPoint, direction);

                groundMarker.transform.position = formationStartPoint;
                groundMarker.SetActive(false);
                groundMarker.SetActive(true);

                lastFormationCenter = formationStartPoint;
                lastFormationDirection = direction;
                hasLastFormation = true;
            }

            isSelectingFormation = false;
        }

        // Selection handling (units/buildings)
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

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
    }

    private void MultiSelect(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
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
        if (!selectedUnits.Contains(unit))
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

    private void MoveUnitsInFormation(Vector3 center, Vector3 moveDirection)
    {
        int unitCount = selectedUnits.Count;
        if (unitCount == 0) return;

        float spacing = 2f;
        Quaternion rotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        if (currentFormation == FormationType.Box)
        {
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
            int row = 0, col = 0;

            for (int i = 0; i < unitCount; i++)
            {
                GameObject unit = selectedUnits[i];
                UnitMovement movement = unit.GetComponent<UnitMovement>();

                float xOffset = (col - gridSize / 2f) * spacing;
                float zOffset = (row - gridSize / 2f) * spacing;
                Vector3 offset = new Vector3(xOffset, 0, zOffset);
                Vector3 rotatedOffset = rotation * offset;
                Vector3 targetPosition = center + rotatedOffset;

                movement.MoveTo(targetPosition);
                unit.transform.rotation = Quaternion.LookRotation(moveDirection);

                col++;
                if (col >= gridSize)
                {
                    col = 0;
                    row++;
                }
            }
        }
        else if (currentFormation == FormationType.Wedge)
        {
            int unitsPlaced = 0;
            int rowSize = 1;
            int row = 0;

            while (unitsPlaced < unitCount)
            {
                for (int i = 0; i < rowSize && unitsPlaced < unitCount; i++, unitsPlaced++)
                {
                    GameObject unit = selectedUnits[unitsPlaced];
                    UnitMovement movement = unit.GetComponent<UnitMovement>();

                    float xOffset = (i - (rowSize - 1) / 2f) * spacing;
                    float zOffset = -row * spacing;
                    Vector3 offset = new Vector3(xOffset, 0, zOffset);

                    Vector3 rotatedOffset = rotation * offset;
                    Vector3 targetPosition = center + rotatedOffset;

                    movement.MoveTo(targetPosition);
                    unit.transform.rotation = Quaternion.LookRotation(moveDirection);
                }

                row++;
                rowSize++;
            }
        }
        else if (currentFormation == FormationType.Column)
        {
            for (int i = 0; i < unitCount; i++)
            {
                GameObject unit = selectedUnits[i];
                UnitMovement movement = unit.GetComponent<UnitMovement>();

                float zOffset = -(i * spacing);
                Vector3 offset = new Vector3(0, 0, zOffset);

                Vector3 rotatedOffset = rotation * offset;
                Vector3 targetPosition = center + rotatedOffset;

                movement.MoveTo(targetPosition);
                unit.transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }
    }
}
