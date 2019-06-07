using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapViewController : MonoBehaviour {

	public GridGenerator gridProp;
	public Material material;
	public Renderer viewObject;

	//[HideInInspector]
	public Vector4[] positions;
	//[HideInInspector]
	public Vector4[] properties;


	void OnEnable()
	{
	}

	//private void Update()
	//{
	//	if (Input.GetKeyUp(KeyCode.P))
	//	{
	//		material = new Material(material);
	//		viewObject.material = material;
	//		material.SetInt("_Points_Length", positions.Length);
	//		material.SetVectorArray("_Points", positions);
	//		material.SetVectorArray("_Properties", properties);
	//	}
	//	else if(positions.Length>0)
	//	{

	//		material.SetInt("_Points_Length", positions.Length);
	//		material.SetVectorArray("_Points", positions);
	//		material.SetVectorArray("_Properties", properties);
	//	}
	//}

	public void UpdateHeatmap(float radius, float[,] distances)
	{
		material = new Material(material);
		viewObject.material = material;

		int row = distances.GetLength(0);
		int col = distances.GetLength(1);
		int posCnt = 0;
		float indMid = gridProp.max;

		positions = new Vector4[row*col];
		properties = new Vector4[row*col];

		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < row; j++)
			{
				positions[posCnt] = new Vector4(i-indMid, 0, j-indMid, 0);
				properties[posCnt++] = new Vector4(radius, Mathf.Clamp(distances[i, j],0.2f,2f));
				//Debug.Log((i - indMid).ToString() + "," + (j - indMid).ToString() + "___" + Mathf.Clamp(distances[i, j], 0.2f, 2f).ToString());
			}
		}

		material.SetInt("_Points_Length", row*col);
		material.SetVectorArray("_Points", positions);
		material.SetVectorArray("_Properties", properties);

		Debug.Log(material.GetInt("_Points_Length"));
	}
}
