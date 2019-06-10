using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {
	
	[Tooltip("velocity in degrees")]
	public float thresholdAngularVelocity = 5f;
	public float linearVelocity = 100f;
	public InputField omegaInp;
	public TestIK testIK;
	public TaskOneController taskController;

	private void Start()
	{
		omegaInp.text = thresholdAngularVelocity.ToString();
		UpdateThreaholdAngularVelocity();
	}

	public void UpdateThreaholdAngularVelocity()
	{
		//float.TryParse(omegaInp.text, out thresholdAngularVelocity);
		float.TryParse(omegaInp.text, out linearVelocity);

		foreach (FABRIKEffector fe in FindObjectsOfType<FABRIKEffector>())
		{
			if (fe.transform == transform)
				continue;
			if (thresholdAngularVelocity == 0)
				fe.angularConstrinat = float.NaN;
			else
				fe.angularConstrinat = thresholdAngularVelocity;
		}

		testIK.MoveSpeed = linearVelocity;
		if (taskController != null)
			taskController.ResetGridAndProperties();	//reset calculated values
	}
	
}
