using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateTargetPosition : MonoBehaviour {

	public TaskTwoController taskController;
	public GridGenerator gridProp;
	public Transform targetObj;
	public GameObject placeholdeTarget;
	public bool mouseOver = false;

	public void OnMouseEnter()
	{
		mouseOver = true;
	}

	private void LateUpdate()
	{
		if(mouseOver && taskController.currMode == ViewMode.Default)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray,out hit))
			{
				//Debug.Log(hit.transform.gameObject.name);
				Vector3 newTargetPos = new Vector3(Mathf.RoundToInt(hit.point.x), 0, Mathf.RoundToInt(hit.point.z));

				if((newTargetPos.x>=gridProp.min && newTargetPos.x<=gridProp.max) && (newTargetPos.z >= gridProp.min && newTargetPos.z <= gridProp.max))
				{
					//new point lies within grid
					if (Input.GetMouseButtonUp(0))
					{
						targetObj.position = newTargetPos;
					}

					placeholdeTarget.SetActive(true);
					placeholdeTarget.transform.position = newTargetPos;
				}
				else
				{
					placeholdeTarget.SetActive(false);
				}
			}
		}
	}

	public void OnMouseExit()
	{
		mouseOver = false;
	}
}
