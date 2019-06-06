using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ViewMode
{
	Default, Simulation, GraphPlot, HeatMap
}

public class TaskOneController : MonoBehaviour {

	public GameObject defaultViewPanel;
	public GameObject simulateViewPanel;
	public GameObject graphViewPanel;
	public GameObject heatmapViewPanel;
	public float simWaitTime = 0.05f;
	public TestIK chainRoot;
	public GridGenerator gridProp;

	[Header("Graph panel options")]
	public RectTransform angleRowContainer;
	public GameObject rowObject;
	public GameObject graphWaitPanel;

	//Simulation params
	private float simX, simY;
	//[X,Y]
	private bool[,] reachability;
	//[X,Y,A]
	private float[,,] boneAngleValues;
	//[X,Y]
	private float[,] targetDistValues;
	private bool allCalculated = false;
	private int indMid;
	private ViewMode currMode = ViewMode.Default;

	private void Start()
	{
		UpdateViewMode(null);
		allCalculated = false;
		reachability = new bool[gridProp.max * 2 + 1, gridProp.max * 2 + 1];
		boneAngleValues = new float[gridProp.max * 2 + 1, gridProp.max * 2 + 1, chainRoot.ChainLength()-1];
		targetDistValues = new float[gridProp.max * 2 + 1, gridProp.max * 2 + 1];

		indMid = (gridProp.max - gridProp.min) / 2;
	}

	IEnumerator SimulateTargetMotion()
	{
		chainRoot.target.position = new Vector3(simX, 0, simY);

		yield return new WaitForSeconds(simWaitTime);

		reachability[(int)simX+indMid, (int)simY + indMid] = chainRoot.TargetReachable(ref targetDistValues[(int)simX + indMid, (int)simY + indMid]);
		//Debug.Log(((int)simX + indMid).ToString() + "," + ((int)simY + indMid).ToString() + "--" + 
		//	reachability[(int)simX + indMid, (int)simY + indMid].ToString()+";"+ targetDistValues[(int)simX + indMid, (int)simY + indMid].ToString());

		int i = 0;
		float[] linkAngs = chainRoot.GetLinkAngles();
		
		foreach (float f in linkAngs)
		{
			boneAngleValues[(int)simX + indMid, (int)simY + indMid, i++] = f;
			//Debug.Log(((int)simX + indMid).ToString() + "," + ((int)simY + indMid).ToString() +"--" +boneAngleValues[(int)simX + indMid, (int)simY + indMid, i-1].ToString());
		}

		//targetDistValues[(int)simX + indMid, (int)simY + indMid] = chainRoot.TargetDistance();

		simulateViewPanel.GetComponent<Text>().text = "X: " + simX.ToString() + "; Y: " + simY.ToString()
			+ "\n" + "Dist: " + targetDistValues[(int)simX + indMid, (int)simY + indMid].ToString("F2") + 
			"; Reach: " + reachability[(int)simX + indMid, (int)simY + indMid].ToString();

		if (simX>=gridProp.max)
		{
			simX = gridProp.min;
			simY++;
		}
		else
		{
			simX++;
		}
		//yield return new WaitForEndOfFrame();
		if (simX <= gridProp.max && simY <= gridProp.max)
			StartCoroutine(SimulateTargetMotion());
		else
		{
			Debug.Log("Simulation finished");
			allCalculated = true;
			if(currMode == ViewMode.GraphPlot)
			{
				graphWaitPanel.SetActive(false);
				UpdateGraphTableEntries();
			}
		}

	}

	void UpdateGraphTableEntries()
	{
		foreach (Transform t in angleRowContainer.GetComponentsInChildren<Transform>())
		{
			if (t.GetComponent<HorizontalLayoutGroup>() != null)
				t.gameObject.SetActive(false);
		}

		for(int i=0;i<boneAngleValues.GetLength(0);i++)
		{
			for (int j = 0; j < boneAngleValues.GetLength(1); j++) {
				//Debug.Log(((int)i - indMid).ToString() + "," + ((int)j - indMid).ToString() + "--" + boneAngleValues[i, j, 1].ToString());
				if (i==0 && j==0)
				{
					angleRowContainer.GetChild(i*j).gameObject.SetActive(true);
					angleRowContainer.GetChild(i * j).GetChild(0).GetComponent<Text>().text = "(" + (i - indMid).ToString() + "," + (j - indMid).ToString() + ")";
					if (reachability[i, j])
					{
						angleRowContainer.GetChild(i * j).GetChild(1).GetComponent<Text>().text = boneAngleValues[i, j, 1].ToString();
						angleRowContainer.GetChild(i * j).GetChild(2).GetComponent<Text>().text = boneAngleValues[i, j, 2].ToString();
						angleRowContainer.GetChild(i * j).GetChild(3).GetComponent<Text>().text = boneAngleValues[i, j, 3].ToString();
						angleRowContainer.GetChild(i * j).GetChild(4).GetComponent<Text>().text = boneAngleValues[i, j, 4].ToString();
						angleRowContainer.GetChild(i * j).GetChild(5).GetComponent<Text>().text = boneAngleValues[i, j, 5].ToString();
					}
					else
					{
						angleRowContainer.GetChild(i * j).GetChild(1).GetComponent<Text>().text = "NA";
						angleRowContainer.GetChild(i * j).GetChild(2).GetComponent<Text>().text = "NA";
						angleRowContainer.GetChild(i * j).GetChild(3).GetComponent<Text>().text = "NA";
						angleRowContainer.GetChild(i * j).GetChild(4).GetComponent<Text>().text = "NA";
						angleRowContainer.GetChild(i * j).GetChild(5).GetComponent<Text>().text = "NA";
					}
				}
				else
				{
					GameObject go = Instantiate(rowObject, angleRowContainer) as GameObject;
					go.gameObject.SetActive(true);
					go.transform.GetChild(0).GetComponent<Text>().text = "(" + (i - indMid).ToString() + "," + (j - indMid).ToString() + ")";
					if (reachability[i, j])
					{
						go.transform.GetChild(1).GetComponent<Text>().text = boneAngleValues[i, j, 1].ToString();
						go.transform.GetChild(2).GetComponent<Text>().text = boneAngleValues[i, j, 2].ToString();
						go.transform.GetChild(3).GetComponent<Text>().text = boneAngleValues[i, j, 3].ToString();
						go.transform.GetChild(4).GetComponent<Text>().text = boneAngleValues[i, j, 4].ToString();
						go.transform.GetChild(5).GetComponent<Text>().text = boneAngleValues[i, j, 5].ToString();
					}
					else
					{
						go.transform.GetChild(1).GetComponent<Text>().text = "NA";
						go.transform.GetChild(2).GetComponent<Text>().text = "NA";
						go.transform.GetChild(3).GetComponent<Text>().text = "NA";
						go.transform.GetChild(4).GetComponent<Text>().text = "NA";
						go.transform.GetChild(5).GetComponent<Text>().text = "NA";
					}
				}
			}
		}
	}

	public void UpdateViewMode(Dropdown viewDropdown)
	{
		HideAllViewPanel();
		if(viewDropdown==null)
		{
			//Default
			defaultViewPanel.SetActive(true);
			return;
		}
		switch(viewDropdown.value)
		{
			case 0://Default
				currMode = ViewMode.Default;
				defaultViewPanel.SetActive(true);
				break;
			case 1://Simulation
				currMode = ViewMode.Simulation;
				simulateViewPanel.SetActive(true);
				simX = simY = gridProp.min;
				StartCoroutine(SimulateTargetMotion());
				break;
			case 2://graph plot
				currMode = ViewMode.GraphPlot;
				graphViewPanel.SetActive(true);
				if (!allCalculated)
				{
					graphWaitPanel.SetActive(true);
					simX = simY = gridProp.min;
					StartCoroutine(SimulateTargetMotion());
				}
				else
				{
					graphWaitPanel.SetActive(false);
					UpdateGraphTableEntries();
				}
				break;
			case 3://heat maps
				currMode = ViewMode.HeatMap;
				heatmapViewPanel.SetActive(true);
				if (!allCalculated)
				{
					graphWaitPanel.SetActive(true);
					simX = simY = gridProp.min;
					StartCoroutine(SimulateTargetMotion());
				}
				else
				{
					graphWaitPanel.SetActive(false);
					
				}
				break;
		}
	}

	void HideAllViewPanel()
	{
		defaultViewPanel.SetActive(false);
		simulateViewPanel.SetActive(false);
		graphViewPanel.SetActive(false);
		heatmapViewPanel.SetActive(false);

		CancelInvoke();
		StopAllCoroutines();
	}
}
