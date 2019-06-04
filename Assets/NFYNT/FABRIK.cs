using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NFYNT.IK
{
	/// <summary>
	/// Attach to the leaf node
	/// </summary>
	public class FABRIK : MonoBehaviour
	{
		public GameObject lineRend;
		public Transform targetSphere;
		public int iterationsPerUpdate = 5;
		public int chainLength = 5;

		public float approxDist = 0.01f;
		[Range(0, 1)]
		public float snapBackStrength = 1f;

		public float totalLength;
		private float[] bonesLength;
		private Transform[] bones;
		private Vector3[] positions;

		private GameObject lineRendParent;
		private Transform[] lines;

		private void Awake()
		{
			InitialiseRig();
			CreateLink();
		}

		void InitialiseRig()
		{

			bones = new Transform[chainLength + 1];
			positions = new Vector3[chainLength + 1];
			bonesLength = new float[chainLength]; //as first bone as 0 length

			totalLength = 0;

			var current = transform;

			for (int i = bones.Length - 1; i >= 0; i--)
			{
				bones[i] = current;

				if(i==bones.Length-1)
				{
					//bonesLength[i] = 0;
				}
				else
				{
					bonesLength[i] = (bones[i+1].position - current.position).magnitude;
					totalLength += bonesLength[i];
				}

				current = current.parent;

			}
		}

		void CreateLink()
		{
			lineRendParent = new GameObject("Lines");
			lines = new Transform[bones.Length - 1];

			for(int i=0;i<bones.Length-1;i++)
			{
				GameObject go = Instantiate(lineRend, lineRendParent.transform) as GameObject;
				lines[i] = go.transform;
				go.GetComponent<LineRenderer>().SetPosition(0, bones[i].position);
				go.GetComponent<LineRenderer>().SetPosition(1, bones[i+1].position);
			}
		}

		private void LateUpdate()
		{
			ResolveIK();
			UpdateLinks();
		}

		void ResolveIK()
		{
			if (targetSphere == null)
				return;

			if (bonesLength.Length != chainLength)
				InitialiseRig();

			for(int i=0;i<bones.Length;i++)
				positions[i] = bones[i].position;

			if((targetSphere.position - bones[0].position).sqrMagnitude >= totalLength*totalLength)
			{
				var dir = (targetSphere.position - bones[0].position).normalized;

				for (int i = 1; i < bones.Length; i++)
					positions[i] = positions[i - 1] + dir * bonesLength[i - 1];
			}
			else
			{
				//the target within reach
				for(int ii=0;ii<iterationsPerUpdate;ii++)
				{
					//backward test
					for(int i=positions.Length-1;i>0;i--)
					{
						if (i == positions.Length - 1)
							positions[i] = targetSphere.position;
						else
							positions[i] = positions[i + 1] + (positions[i + 1] - positions[i]).normalized * bonesLength[i];
					}

					//forward test
					for (int i = 1; i < positions.Length; i++)
						positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLength[i - 1];
					
					if ((positions[positions.Length - 1] - targetSphere.position).sqrMagnitude < approxDist * approxDist)
						break;
				}
			}

			for (int i = 0; i < bones.Length; i++)
				bones[i].position = positions[i];
		}

		void UpdateLinks()
		{
			for (int i = 0; i < bones.Length - 1; i++)
			{
				lines[i].GetComponent<LineRenderer>().SetPosition(0, bones[i].position);
				lines[i].GetComponent<LineRenderer>().SetPosition(1, bones[i + 1].position);
			}
		}
	}
}