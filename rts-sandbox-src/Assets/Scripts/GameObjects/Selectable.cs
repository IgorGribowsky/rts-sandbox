using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    public GameObject SelectionCirclePrefab;

    public bool IsSelected = false;

    private GameObject selectedCircle;

    void Start()
    {
        selectedCircle = GameObject.Instantiate(SelectionCirclePrefab);
        selectedCircle.transform.parent = gameObject.transform;
        selectedCircle.transform.localPosition = Vector3.zero;

        var position = selectedCircle.transform.position;
        position.y = 0;
        selectedCircle.transform.position = position;

        selectedCircle.SetActive(IsSelected);
    }

    public void SetSelectionState(bool isSelected)
    {
        IsSelected = isSelected;
        selectedCircle.SetActive(IsSelected);
    }

    void Update()
    {
        
    }
}
