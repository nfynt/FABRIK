using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestIK : FABRIK {

	public Transform target;
	public List<Transform> firstChain = new List<Transform>();
	public GameObject lineRend;

	private int chainLen;
	private Transform[] chainLinks;

	public override void OnFABRIK()
	{
		float speed = 100.0F;
		float step = Time.deltaTime * speed;
		target.position = new Vector3(target.position.x, 0, target.position.z);
		FABRIKChain right = GetEndChain("Cylinder (6)_end_effector");

		right.Target = Vector3.MoveTowards(right.EndEffector.Position, target.position, step);

		UpdateLinks();
	}

	public override void SetFirstChain()
	{
		Transform current = transform;	//GetRootTransform();
		if (current == null)
			return;

		firstChain.Add(current);
		chainLen = 1;

		while (current.childCount == 1 && current.GetComponent<FABRIKEffector>() != null)
		{
			chainLen++;
			current = current.GetChild(0);
			firstChain.Add(current);
		}

		CreateLink();
	}


	void CreateLink()
	{
		GameObject lineRendParent = new GameObject("Lines");
		chainLinks = new Transform[chainLen - 1];

		for (int i = 0; i < chainLen - 1; i++)
		{
			GameObject go = Instantiate(lineRend, lineRendParent.transform) as GameObject;
			chainLinks[i] = go.transform;
			go.GetComponent<LineRenderer>().SetPosition(0, firstChain[i].position);
			go.GetComponent<LineRenderer>().SetPosition(1, firstChain[i + 1].position);
		}
	}
	
	void UpdateLinks()
	{
		for (int i = 0; i < chainLen - 1; i++)
		{
			chainLinks[i].GetComponent<LineRenderer>().SetPosition(0, firstChain[i].position);
			chainLinks[i].GetComponent<LineRenderer>().SetPosition(1, firstChain[i + 1].position);
		}
	}
}
