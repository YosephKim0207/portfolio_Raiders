﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	// Token: 0x02000A74 RID: 2676
	[ActionCategory(ActionCategory.Transform)]
	[Tooltip("Smoothly Rotates a 2d Game Object so its right vector points at a Target. The target can be defined as a 2d Game Object or a 2d/3d world Position. If you specify both, then the position will be used as a local offset from the object's position.")]
	public class SmoothLookAt2d : FsmStateAction
	{
		// Token: 0x060038E4 RID: 14564 RVA: 0x00123D58 File Offset: 0x00121F58
		public override void Reset()
		{
			this.gameObject = null;
			this.targetObject = null;
			this.targetPosition2d = new FsmVector2
			{
				UseVariable = true
			};
			this.targetPosition = new FsmVector3
			{
				UseVariable = true
			};
			this.rotationOffset = 0f;
			this.debug = false;
			this.speed = 5f;
			this.finishTolerance = 1f;
			this.finishEvent = null;
		}

		// Token: 0x060038E5 RID: 14565 RVA: 0x00123DE0 File Offset: 0x00121FE0
		public override void OnEnter()
		{
			this.previousGo = null;
		}

		// Token: 0x060038E6 RID: 14566 RVA: 0x00123DEC File Offset: 0x00121FEC
		public override void OnLateUpdate()
		{
			this.DoSmoothLookAt();
		}

		// Token: 0x060038E7 RID: 14567 RVA: 0x00123DF4 File Offset: 0x00121FF4
		private void DoSmoothLookAt()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			GameObject value = this.targetObject.Value;
			if (this.previousGo != ownerDefaultTarget)
			{
				this.lastRotation = ownerDefaultTarget.transform.rotation;
				this.desiredRotation = this.lastRotation;
				this.previousGo = ownerDefaultTarget;
			}
			Vector3 vector = new Vector3(this.targetPosition2d.Value.x, this.targetPosition2d.Value.y, 0f);
			if (!this.targetPosition.IsNone)
			{
				vector += this.targetPosition.Value;
			}
			if (value != null)
			{
				vector = value.transform.position;
				Vector3 vector2 = Vector3.zero;
				if (!this.targetPosition.IsNone)
				{
					vector2 += this.targetPosition.Value;
				}
				if (!this.targetPosition2d.IsNone)
				{
					vector2.x += this.targetPosition2d.Value.x;
					vector2.y += this.targetPosition2d.Value.y;
				}
				if (!this.targetPosition2d.IsNone || !this.targetPosition.IsNone)
				{
					vector += value.transform.TransformPoint(this.targetPosition2d.Value);
				}
			}
			Vector3 vector3 = vector - ownerDefaultTarget.transform.position;
			vector3.Normalize();
			float num = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
			this.desiredRotation = Quaternion.Euler(0f, 0f, num - this.rotationOffset.Value);
			this.lastRotation = Quaternion.Slerp(this.lastRotation, this.desiredRotation, this.speed.Value * Time.deltaTime);
			ownerDefaultTarget.transform.rotation = this.lastRotation;
			if (this.debug.Value)
			{
				Debug.DrawLine(ownerDefaultTarget.transform.position, vector, Color.grey);
			}
			if (this.finishEvent != null)
			{
				float num2 = Vector3.Angle(this.desiredRotation.eulerAngles, this.lastRotation.eulerAngles);
				if (Mathf.Abs(num2) <= this.finishTolerance.Value)
				{
					base.Fsm.Event(this.finishEvent);
				}
			}
		}

		// Token: 0x04002B3A RID: 11066
		[RequiredField]
		[Tooltip("The GameObject to rotate to face a target.")]
		public FsmOwnerDefault gameObject;

		// Token: 0x04002B3B RID: 11067
		[Tooltip("A target GameObject.")]
		public FsmGameObject targetObject;

		// Token: 0x04002B3C RID: 11068
		[Tooltip("A target position. If a Target Object is defined, this is used as a local offset.")]
		public FsmVector2 targetPosition2d;

		// Token: 0x04002B3D RID: 11069
		[Tooltip("A target position. If a Target Object is defined, this is used as a local offset.")]
		public FsmVector3 targetPosition;

		// Token: 0x04002B3E RID: 11070
		[Tooltip("Set the GameObject starting offset. In degrees. 0 if your object is facing right, 180 if facing left etc...")]
		public FsmFloat rotationOffset;

		// Token: 0x04002B3F RID: 11071
		[Tooltip("How fast the look at moves.")]
		[HasFloatSlider(0.5f, 15f)]
		public FsmFloat speed;

		// Token: 0x04002B40 RID: 11072
		[Tooltip("Draw a line in the Scene View to the look at position.")]
		public FsmBool debug;

		// Token: 0x04002B41 RID: 11073
		[Tooltip("If the angle to the target is less than this, send the Finish Event below. Measured in degrees.")]
		public FsmFloat finishTolerance;

		// Token: 0x04002B42 RID: 11074
		[Tooltip("Event to send if the angle to target is less than the Finish Tolerance.")]
		public FsmEvent finishEvent;

		// Token: 0x04002B43 RID: 11075
		private GameObject previousGo;

		// Token: 0x04002B44 RID: 11076
		private Quaternion lastRotation;

		// Token: 0x04002B45 RID: 11077
		private Quaternion desiredRotation;
	}
}
