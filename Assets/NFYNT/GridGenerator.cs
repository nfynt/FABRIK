using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour {

	public GameObject lineRend;
	public int min = -15;
	public int max = 15;

	private GameObject[,] gridLines;
	private int indOffset;

	void Start()
	{
		DrawGrid();

	}

	private void FixedUpdate()
	{
	}

	void DrawGrid()
	{
		int r = 4 * max, c = 4 * max;

		gridLines = new GameObject[r+2,c+2];

		r = c = -1;
		for (int i = min; i <= max; i++)
		{
			r++;
			gridLines[0, r] = Instantiate(lineRend, transform) as GameObject;
			gridLines[0, r].GetComponent<LineRenderer>().SetPosition(0, new Vector3(i , 0, min));
			gridLines[0, r].GetComponent<LineRenderer>().SetPosition(1, new Vector3(i, 0, max));
			gridLines[0, r].GetComponent<LineRenderer>().startWidth = 0.1f;
			gridLines[0, r].GetComponent<LineRenderer>().endWidth = 0.1f;

			gridLines[1, r] = Instantiate(lineRend, transform) as GameObject;
			gridLines[1, r].GetComponent<LineRenderer>().SetPosition(0, new Vector3(min, 0, i));
			gridLines[1, r].GetComponent<LineRenderer>().SetPosition(1, new Vector3(max, 0, i));
			gridLines[1, r].GetComponent<LineRenderer>().startWidth = 0.1f;
			gridLines[1, r].GetComponent<LineRenderer>().endWidth = 0.1f;

			if (i!=max)
			{
				r++;
				gridLines[0, r] = Instantiate(lineRend, transform) as GameObject;
				gridLines[0, r].GetComponent<LineRenderer>().SetPosition(0, new Vector3(i+0.5f, 0, min));
				gridLines[0, r].GetComponent<LineRenderer>().SetPosition(1, new Vector3(i+0.5f, 0, max));
				gridLines[0, r].GetComponent<LineRenderer>().startWidth = 0.05f;
				gridLines[0, r].GetComponent<LineRenderer>().endWidth = 0.05f;
				gridLines[0, r].GetComponent<LineRenderer>().startColor = Color.gray;
				gridLines[0, r].GetComponent<LineRenderer>().endColor = Color.gray;

				gridLines[1, r] = Instantiate(lineRend, transform) as GameObject;
				gridLines[1, r].GetComponent<LineRenderer>().SetPosition(0, new Vector3(min, 0, i + 0.5f));
				gridLines[1, r].GetComponent<LineRenderer>().SetPosition(1, new Vector3(max, 0, i + 0.5f));
				gridLines[1, r].GetComponent<LineRenderer>().startWidth = 0.05f;
				gridLines[1, r].GetComponent<LineRenderer>().endWidth = 0.05f;
				gridLines[1, r].GetComponent<LineRenderer>().startColor = Color.gray;
				gridLines[1, r].GetComponent<LineRenderer>().endColor = Color.gray;
			}

		}
	}
}
