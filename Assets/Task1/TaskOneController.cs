using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ViewMode
{
	Default, Simulation, GraphPlot, HeatMap
}

public class TaskOneController : MonoBehaviour,ITaskController {

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

	[Header("Heatmap panel options")]
	public GameObject heatWaitPanel;
	public GameObject heatmapQuad;
	[Space(10)]
	public ViewMode currMode = ViewMode.Default;

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
	private int incre = 0;

	public ViewMode GetCurrMode()
	{
		return currMode;
	}

	void Awake()
	{
		Camera.main.transform.GetComponent<OrbitCamera>().taskController = this;
	}

	private void Start()
	{
		ResetGridAndProperties();
		UpdateViewMode(null);
	}

	public void ResetGridAndProperties()
	{
		allCalculated = false;
		reachability = new bool[gridProp.max * 2 + 1, gridProp.max * 2 + 1];
		boneAngleValues = new float[gridProp.max * 2 + 1, gridProp.max * 2 + 1, chainRoot.ChainLength() - 1];
		targetDistValues = new float[gridProp.max * 2 + 1, gridProp.max * 2 + 1];

		indMid = (gridProp.max - gridProp.min) / 2;
		incre = 0;
	}

	IEnumerator SimulateTargetMotion()
	{
		if (incre == 0)
			incre = 1;

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

		
			simX+=incre;
			if(simX<gridProp.min)
			{
				simX = gridProp.min;
				simY++;
				incre = 1;
			}
			else if (simX > gridProp.max)
			{
				simX = gridProp.max;
				incre = -1;
				simY++;
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
			}else if (currMode == ViewMode.HeatMap)
			{
				heatWaitPanel.SetActive(false);
				PrepareHeatMap();
			}
			incre = 0;
		}

	}

	void PrepareHeatMap()
	{
		heatmapQuad.SetActive(true);
		heatmapQuad.GetComponent<HeatmapViewController>().UpdateHeatmap(1f, targetDistValues);
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
					angleRowContainer.GetChild(i*j+1).gameObject.SetActive(true);
					angleRowContainer.GetChild(i * j + 1).GetChild(0).GetComponent<Text>().text = "(" + (i - indMid).ToString() + "," + (j - indMid).ToString() + ")";
					if (reachability[i, j])
					{
						angleRowContainer.GetChild(i * j + 1).GetChild(1).GetComponent<Text>().text = boneAngleValues[i, j, 1].ToString();
						angleRowContainer.GetChild(i * j + 1).GetChild(2).GetComponent<Text>().text = boneAngleValues[i, j, 2].ToString();
						angleRowContainer.GetChild(i * j + 1).GetChild(3).GetComponent<Text>().text = boneAngleValues[i, j, 3].ToString();
						angleRowContainer.GetChild(i * j + 1).GetChild(4).GetComponent<Text>().text = boneAngleValues[i, j, 4].ToString();
						angleRowContainer.GetChild(i * j + 1).GetChild(5).GetComponent<Text>().text = boneAngleValues[i, j, 5].ToString();
					}
					else
					{
						angleRowContainer.GetChild(i * j + 1).GetChild(1).GetComponent<Text>().text = "NA";
						angleRowContainer.GetChild(i * j + 1).GetChild(2).GetComponent<Text>().text = "NA";
						angleRowContainer.GetChild(i * j + 1).GetChild(3).GetComponent<Text>().text = "NA";
						angleRowContainer.GetChild(i * j + 1).GetChild(4).GetComponent<Text>().text = "NA";
						angleRowContainer.GetChild(i * j + 1).GetChild(5).GetComponent<Text>().text = "NA";
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
					heatWaitPanel.SetActive(true);
					simX = simY = gridProp.min;
					StartCoroutine(SimulateTargetMotion());
				}
				else
				{
					heatWaitPanel.SetActive(false);
					PrepareHeatMap();
				}
				break;
		}
	}

	public void UpdateGridSize(Dropdown dropdown)
	{
		switch (dropdown.value)
		{
			case 0: //3
				gridProp.max = 3;
				gridProp.min = -3;
				break;
			case 1: //5
				gridProp.max = 5;
				gridProp.min = -5;
				break;
			case 2: //10
				gridProp.max = 10;
				gridProp.min = -10;
				break;
			case 3: //15
				gridProp.max = 15;
				gridProp.min = -15;
				break;
		}

		gridProp.UpdateGrid();
		ResetGridAndProperties();
		UpdateViewMode(null);
	}

	void HideAllViewPanel()
	{
		defaultViewPanel.SetActive(false);
		simulateViewPanel.SetActive(false);
		graphViewPanel.SetActive(false);
		heatmapViewPanel.SetActive(false);
		heatmapQuad.SetActive(false);

		CancelInvoke();
		StopAllCoroutines();
		incre = 0;
	}
}
