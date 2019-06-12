using UnityEngine;
using System.Collections;

public class FABRIKEffector : MonoBehaviour
{
	public float weight = 1.0F;

	[HideInInspector]
	public Vector3 upAxisConstraint = Vector3.up;

	[HideInInspector]
	public Vector3 forwardAxisConstraint = Vector3.forward;

	[HideInInspector]
	public float swingConstraint = float.NaN;

	[HideInInspector]
	public float twistConstraint = float.NaN;
	public float angularConstrinat = float.NaN;

	private FABRIKEffector parent = null;

	private Vector3 position;
	private Quaternion rotation;

	public float Weight
	{
		get
		{
			return weight;
		}
	}

	public Vector3 Position
	{
		get
		{
			return parent != null ? position : transform.position;
		}

		set
		{
			position = value;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return parent != null ? rotation : transform.rotation;
		}

		set
		{
			rotation = value;
		}
	}

	public float Length
	{
		get;
		set;
	}

	public float SwingConstraint
	{
		get
		{
			return swingConstraint * 0.5F * Mathf.Deg2Rad;
		}
	}

	public float TwistConstraint
	{
		get
		{
			return twistConstraint * 0.5F * Mathf.Deg2Rad;
		}
	}

	public float AngularConstraint
	{
		get { return angularConstrinat; }
	}

	public bool SwingConstrained
	{
		get
		{
			return !float.IsNaN(swingConstraint);
		}
	}

	public bool TwistConstrained
	{
		get
		{
			return !float.IsNaN(twistConstraint);
		}
	}

	public bool AngularConstrained
	{
		get { return !float.IsNaN(angularConstrinat); }
	}

	public void ApplyConstraints(Vector3 direction)
	{
		if (parent)
		{
			// Neither axis is constrained; set to LookRotation
			if (!SwingConstrained && !TwistConstrained)
			{
				Rotation = Quaternion.LookRotation(direction, parent.Rotation * Vector3.up);
			}
			else
			{
				// Take our world-space direction and world-space up vector of the constraining rotation
				// Multiply this by the inverse of the constraining rotation to derive a local rotation
				Quaternion rotation_global = Quaternion.LookRotation(parent.Rotation * forwardAxisConstraint, parent.Rotation * upAxisConstraint);
				Quaternion rotation_local = Quaternion.Inverse(rotation_global) * Quaternion.LookRotation(direction);

				Quaternion swing, twist;

				// Decompose our local rotation to swing-twist about the forward vector of the constraining rotation
				rotation_local.Decompose(Vector3.forward, out swing, out twist);
				
				// Constrain the swing and twist quaternions
				if (SwingConstrained)
				{
					swing = swing.Constrain(SwingConstraint);
				}

				if (TwistConstrained)
				{
					twist = twist.Constrain(TwistConstraint);
				}

				// Multiply the constrained swing-twist by our constraining rotation to get a world-space rotation
				Rotation = rotation_global * swing * twist;
			}
		}
		else
		{
			Rotation = Quaternion.LookRotation(direction);
		}
	}

	public void ApplyAngularVelocityConstraint()
	{
		if (float.IsNaN(angularConstrinat) || angularConstrinat == 0 || !AngularConstrained)
			return;

		//Debug.Log("bef_"+gameObject.name + "_" + Position);

		float theta1 = Mathf.Acos(transform.position.x / parent.Length);
		float theta2 = Mathf.Acos(this.Position.x / parent.Length);

		float phi = theta2 - theta1;    //in rad
		
		float angVel = phi / Time.deltaTime;
		//Debug.Log(gameObject.name+"Phi: " + phi);
		Debug.Log(angVel.ToString() + "____" + (AngularConstraint * Time.deltaTime).ToString());
		if (!float.IsNaN(angVel) && Mathf.Abs(angVel) > Mathf.Abs(AngularConstraint * Time.deltaTime))
		{
			//required velocity is greater than threshold
			//Debug.Log(gameObject.name + "cal angular vel:" + angVel + "and threshold: " + angularVelocity * Time.deltaTime);
			float theta = theta1 + AngularConstraint * Time.deltaTime;

			//caliberated pos
			this.Position = new Vector3(parent.Length * Mathf.Cos(theta), 0, parent.Length * Mathf.Sin(theta));

			//CheckNewPositionLength(newPosition);

			//adjust next link for updated join
			if (transform.childCount > 0)
			{
				Transform child = transform.GetChild(0);
				Vector3 parentPos = Position;
				while (child != null)
				{
					FABRIKEffector fe = child.GetComponent<FABRIKEffector>();
					parentPos = fe.Position = parentPos + Vector3.Normalize(parentPos - child.position) * Length;
				}
			}
		}

		if (parent != null)
			Debug.Log("after_" + gameObject.name + "_" + Vector3.Distance(Position, parent.Position));
	}

	void Awake()
	{
		parent = transform.parent != null ? transform.parent.gameObject.GetComponent<FABRIKEffector>() : null;

		Position = transform.position;
		Rotation = transform.rotation;
	}

	public void UpdateTransform()
	{
		Quaternion X90 = new Quaternion(Mathf.Sqrt(0.5F), 0.0F, 0.0F, Mathf.Sqrt(0.5F));

		transform.rotation = Rotation * X90;
		transform.position = Position;

		DebugDrawBounds();
	}

	private void DebugDrawBounds()
	{
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

		if (meshFilter == null)
		{
			return;
		}

		Bounds bounds = meshFilter.mesh.bounds;

		Vector3[] vertices = new Vector3[8];

		vertices[0] = transform.TransformPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));
		vertices[1] = transform.TransformPoint(new Vector3(-bounds.max.x, bounds.max.y, bounds.max.z));
		vertices[2] = transform.TransformPoint(new Vector3(-bounds.max.x, bounds.max.y, -bounds.max.z));
		vertices[3] = transform.TransformPoint(new Vector3(bounds.max.x, bounds.max.y, -bounds.max.z));
		vertices[4] = transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));
		vertices[5] = transform.TransformPoint(new Vector3(-bounds.min.x, bounds.min.y, bounds.min.z));
		vertices[6] = transform.TransformPoint(new Vector3(-bounds.min.x, bounds.min.y, -bounds.min.z));
		vertices[7] = transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, -bounds.min.z));

		Debug.DrawLine(vertices[0], vertices[1], Color.red, 0.0F, false);
		Debug.DrawLine(vertices[1], vertices[2], Color.red, 0.0F, false);
		Debug.DrawLine(vertices[2], vertices[3], Color.red, 0.0F, false);
		Debug.DrawLine(vertices[3], vertices[0], Color.red, 0.0F, false);

		Debug.DrawLine(vertices[4], vertices[5], Color.green, 0.0F, false);
		Debug.DrawLine(vertices[5], vertices[6], Color.green, 0.0F, false);
		Debug.DrawLine(vertices[6], vertices[7], Color.green, 0.0F, false);
		Debug.DrawLine(vertices[7], vertices[4], Color.green, 0.0F, false);

		Debug.DrawLine(vertices[0], vertices[6], Color.blue, 0.0F, false);
		Debug.DrawLine(vertices[1], vertices[7], Color.blue, 0.0F, false);
		Debug.DrawLine(vertices[2], vertices[4], Color.blue, 0.0F, false);
		Debug.DrawLine(vertices[3], vertices[5], Color.blue, 0.0F, false);
	}
}
