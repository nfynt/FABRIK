using System;
using UnityEngine;

namespace NFYNT
{
	/// <summary>
	/// Attach to the leaf node
	/// </summary>
	public class NfyntIKFabric : MonoBehaviour
	{
		public GameObject lineRend;
		public int noOfLinks = 5;
		
		public Transform targetObj;
		public Transform Pole;
		
		[Header("Solver Parameters")]
		public int iterations = 10;
		public float approxDist = 0.01f;
		public float distFromTarget = 0f;
		/// <summary>
		/// Threshold angular velocity
		/// </summary>
		[Space(10)]
		public float angularVelocity = 5f;	//in rad/s
		/// <summary>
		/// Strength of going back to the start position.
		/// </summary>
		[Range(0, 1)]
		public float SnapBackStrength = 1f;


		protected float[] linkLengths; //targetObj to Origin
		protected float totalLength;
		protected Transform[] joints;
		protected Vector3[] positions;
		//private Vector3 targetPos;
		protected Vector3[] startDirection;
		protected Quaternion[] startRotationJoint;
		protected Quaternion startRotationTarget;
		protected Quaternion startRotationRoot;

		private GameObject lineRendParent;
		private Transform[] lines;

		void Start()
		{
			InitializeArticuledBody();
			CreateLink();
		}

		void InitializeArticuledBody()
		{
			joints = new Transform[noOfLinks + 1];
			positions = new Vector3[noOfLinks + 1];
			linkLengths = new float[noOfLinks];
			startDirection = new Vector3[noOfLinks + 1];
			startRotationJoint = new Quaternion[noOfLinks + 1];
			
			if (targetObj == null)
			{
				Debug.LogError("Please attach target body");
				targetObj = new GameObject(gameObject.name + " targetObj").transform;
				targetObj.position = transform.position;
			}
			startRotationTarget = targetObj.rotation;
			totalLength = 0;

			
			var current = transform;
			for (var i = joints.Length - 1; i >= 0; i--)
			{
				joints[i] = current;
				startRotationJoint[i] = current.rotation;

				if (i == joints.Length - 1)
				{
					startDirection[i] = targetObj.position - current.position;
					//bonesLength[i] = 0;
					//Debug.Log(startDirection[i]);
				}
				else
				{
					startDirection[i] = joints[i + 1].position - current.position;
					linkLengths[i] = startDirection[i].magnitude;
					totalLength += linkLengths[i];
				}

				current = current.parent;
			}

			//Debug.Log(totalLength);
			
			startRotationRoot = (joints[0].parent != null) ? joints[0].parent.rotation : Quaternion.identity;
		}

		void CreateLink()
		{
			lineRendParent = new GameObject("Lines");
			lines = new Transform[joints.Length - 1];

			for (int i = 0; i < joints.Length - 1; i++)
			{
				GameObject go = Instantiate(lineRend, lineRendParent.transform) as GameObject;
				lines[i] = go.transform;
				go.GetComponent<LineRenderer>().SetPosition(0, joints[i].position);
				go.GetComponent<LineRenderer>().SetPosition(1, joints[i + 1].position);
			}
		}



		void LateUpdate()
		{
			//restrict to XZ plane
			targetObj.position = new Vector3(targetObj.position.x, 0, targetObj.position.z);
			ResolveIK();
			ApplyAngularConstraints();
			UpdateTransform();
			UpdateLinks();
		}

		private void ResolveIK()
		{
			if (targetObj == null)
				return;

			if (linkLengths.Length != noOfLinks)
				InitializeArticuledBody();
			
			for (int i = 0; i < joints.Length; i++)
				positions[i] = joints[i].position;

			var RootRot = (joints[0].parent != null) ? joints[0].parent.rotation : Quaternion.identity;
			var RootRotDiff = RootRot * Quaternion.Inverse(startRotationRoot);

			
			if ((targetObj.position - joints[0].position).sqrMagnitude >= totalLength * totalLength)
			{
				var direction = (targetObj.position - positions[0]).normalized;
				for (int i = 1; i < positions.Length; i++)
					positions[i] = positions[i - 1] + direction * linkLengths[i - 1];
			}
			else
			{
				//backward
				for (int i = 0; i < positions.Length - 1; i++)
					positions[i + 1] = Vector3.Lerp(positions[i + 1], positions[i] + RootRotDiff * startDirection[i], SnapBackStrength);

				for (int ii = 0; ii < iterations; ii++)
				{
					for (int i = positions.Length - 1; i > 0; i--)
					{
						if (i == positions.Length - 1)
							positions[i] = targetObj.position; //set it to target
						else
							positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * linkLengths[i]; //set in line on distance
					}

					//forward
					for (int i = 1; i < positions.Length; i++)
						positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * linkLengths[i - 1];
					
					if ((positions[positions.Length - 1] - targetObj.position).sqrMagnitude < approxDist * approxDist)
						break;
				}
			}

			//move towards pole
			if (Pole != null)
			{
				for (int i = 1; i < positions.Length - 1; i++)
				{

					var plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
					var projectedPole = plane.ClosestPointOnPlane(Pole.position);
					var projectedBone = plane.ClosestPointOnPlane(positions[i]);
					var angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);
					positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
				}
			}

			distFromTarget = Vector3.Distance(joints[joints.Length - 1].position, targetObj.position);
		}

		void ApplyAngularConstraints()
		{
			if (float.IsNaN(angularVelocity) || angularVelocity == 0)
				return;


			for (int i = 1; i < joints.Length; i++)
			{
				float theta1 = Mathf.Acos(joints[i].position.x / linkLengths[i-1]);
				float theta2 = Mathf.Acos(positions[i].x / linkLengths[i-1]);

				float phi = theta2 - theta1;    //in rad

				//float angVel = AngularSpeed / (parent.Length);
				float angVel = phi / Time.deltaTime;
				//Debug.Log(gameObject.name+"Phi: " + phi);
				
				if (!float.IsNaN(angVel) && Mathf.Abs(angVel) > Mathf.Abs(angularVelocity * Time.deltaTime))
				{
					//required velocity is greater than threshold
					//Debug.Log(gameObject.name + "cal angular vel:" + angVel + "and threshold: " + angularVelocity * Time.deltaTime);
					float theta = theta1 + angularVelocity * Time.deltaTime;
					
					//caliberated pos
					positions[i] = new Vector3(linkLengths[i-1] * Mathf.Cos(theta), 0, linkLengths[i-1] * Mathf.Sin(theta));
					
					//CheckNewPositionLength(newPosition);
					if (i + 1 < joints.Length)
					{
						//adjust next link for updated join
						positions[i + 1] = positions[i] + Vector3.Normalize(positions[i] - positions[i+1]) * linkLengths[i];
					}
				}
			}

		}

		//void CheckNewPositionLength(Vector3 newPos)
		//{
		//	if (parent != null)
		//	{
		//		//Position = parent.position + Vector3.Normalize(parent.Position - newPos) * parent.Length;
		//	}
		//}


		void UpdateTransform()
		{
			//set position & rotation
			for (int i = 0; i < positions.Length; i++)
			{
				if (i == positions.Length - 1)
					joints[i].rotation = targetObj.rotation * Quaternion.Inverse(startRotationTarget) * startRotationJoint[i];
				else
					joints[i].rotation = Quaternion.FromToRotation(startDirection[i], positions[i + 1] - positions[i]) * startRotationJoint[i];
				joints[i].position = positions[i];
			}

		}

		void UpdateLinks()
		{
			for (int i = 0; i < joints.Length - 1; i++)
			{
				lines[i].GetComponent<LineRenderer>().SetPosition(0, joints[i].position);
				lines[i].GetComponent<LineRenderer>().SetPosition(1, joints[i + 1].position);
			}
		}

		//is target reached
		//squared distance of target from end effector
		public bool TargetReachable(ref float dist)
		{
			dist = Vector3.Distance(joints[joints.Length - 1].position, targetObj.position);
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
				//Debug.Log("Angular velocity constraint: " + value);
				angularVelocity = value;
			}
		}

		public float[] GetLinkAngles()
		{
			float[] ang = new float[joints.Length - 1];

			Transform curr = joints[0];
			int i = 0;

			while (curr != null && curr.childCount > 0)
			{
				//curr.LookAt(bones[i + 1]);
				ang[i++] = curr.eulerAngles.y;

				if (i >= joints.Length - 1)
					break;

				curr = joints[i];
			}

			return ang;
		}


	}
}