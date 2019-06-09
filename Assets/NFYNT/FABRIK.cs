using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFYNT
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
		
		public float targetChaseSpeed = 1f;

		public float totalLength;
		private float[] bonesLength;
		public Transform[] bones;
		public Vector3[] positions;
		public float distFromTarget = 0f;

		private GameObject lineRendParent;
		private Transform[] lines;
		//private Vector3 targetPos;

		private void Awake()
		{
			InitialiseRig();
			CreateLink();
			//targetPos = targetSphere.position;
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

		private void Update()
		{
			//if (Vector3.Distance(targetPos, targetSphere.position) > approxDist)
			//	targetPos = Vector3.MoveTowards(targetPos, targetSphere.position, targetChaseSpeed * Time.deltaTime);
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
					for (int i=positions.Length-1;i>0;i--)
					{
						if (i == positions.Length - 1)
							positions[i] = targetSphere.position;
						else
							positions[i] = positions[i + 1] + (positions[i + 1] - positions[i]).normalized * bonesLength[i];
					}

					//forward test
					for (int i = 1; i < positions.Length; i++)
					{
						positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLength[i - 1];
					}


					distFromTarget = Vector3.Distance(bones[bones.Length - 1].position, targetSphere.position);
					if (distFromTarget < approxDist)
					{
						//Debug.Log("Reached target pos");
						break;
					}

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


		//is target reached
		//squared distance of target from end effector
		public bool TargetReachable(ref float dist)
		{
			dist = Vector3.Distance(bones[bones.Length - 1].position, targetSphere.position);
			Math.Round(dist, 3);

			if (dist <= approxDist)
				return true;
			else
				return false;
		}

		public void UpdateAngularVelocity(float value)
		{
			if (!float.IsNaN(value))
			{
				Debug.Log("Angular velocity constraint: " + value);
			}
		}

		public Vector3 ApplyAngularConstraints(Vector3 currPos, Vector3 targetPos, float linkLen, float threshVel)
		{
			float theta1 = Mathf.Acos(currPos.x / linkLen);
			float theta2 = Mathf.Acos(targetPos.x / linkLen);

			float phi = theta2 - theta1;    //in rad

			//float angVel = AngularSpeed / (parent.Length);
			float angVel = phi / Time.deltaTime;
			//Debug.Log(gameObject.name+"Phi: " + phi);
			//Debug.Log(gameObject.name + "cal angular vel:" + angVel + "and threshold: " + AngularVelocity*Time.deltaTime);

			if (!float.IsNaN(angVel) && Mathf.Abs(angVel) > Mathf.Abs(threshVel * Time.deltaTime))
			{
				//required velocity is greater than threshold
				Debug.Log(gameObject.name + "cal angular vel:" + angVel + "and threshold: " + threshVel * Time.deltaTime);
				float theta = theta1 + Mathf.Sign(phi) * threshVel * Time.deltaTime;
				//Debug.Log("theta: " + theta + "\nB-A:" + phi + "\t len: " + parent.Length);
				//Position = new Vector3(parent.Length * Mathf.Cos(theta), 0, parent.Length * Mathf.Sin(theta));
				Vector3 newPosition = new Vector3(linkLen * Mathf.Cos(theta), 0, linkLen * Mathf.Sin(theta));
				//Debug.Log("A: " + theta1 + "\nB: " + theta2 + "\nC: " + theta + "\nB-A:" + phi);
				return newPosition;
			}

			return targetPos;
		}

		public float[] GetLinkAngles()
		{
			float[] ang = new float[bones.Length-1];

			Transform curr = bones[0];
			int i = 0;

			while (curr != null && curr.childCount > 0)
			{
				//curr.LookAt(bones[i + 1]);
				ang[i++] = curr.eulerAngles.y;

				if (i >= bones.Length - 1)
					break;

				curr = bones[i];
			}

			return ang;
		}


	}
}