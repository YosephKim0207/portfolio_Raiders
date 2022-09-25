﻿using System;
using FullInspector;
using UnityEngine;

// Token: 0x02000D47 RID: 3399
public class ShootBehavior : BasicAttackBehavior
{
	// Token: 0x060047C9 RID: 18377 RVA: 0x0017A0DC File Offset: 0x001782DC
	private bool ShowBulletScript()
	{
		return string.IsNullOrEmpty(this.BulletName);
	}

	// Token: 0x060047CA RID: 18378 RVA: 0x0017A0EC File Offset: 0x001782EC
	private bool ShowBulletName()
	{
		return this.BulletScript == null || this.BulletScript.IsNull;
	}

	// Token: 0x060047CB RID: 18379 RVA: 0x0017A108 File Offset: 0x00178308
	private bool ShowImmobileDuringStop()
	{
		return this.StopDuring != ShootBehavior.StopType.None;
	}

	// Token: 0x060047CC RID: 18380 RVA: 0x0017A118 File Offset: 0x00178318
	private bool ShowChargeTime()
	{
		return !string.IsNullOrEmpty(this.ChargeAnimation);
	}

	// Token: 0x060047CD RID: 18381 RVA: 0x0017A128 File Offset: 0x00178328
	private bool ShowOverrideFireDirection()
	{
		return this.ShowBulletName() && this.ShouldOverrideFireDirection;
	}

	// Token: 0x17000A65 RID: 2661
	// (get) Token: 0x060047CE RID: 18382 RVA: 0x0017A140 File Offset: 0x00178340
	public bool IsBulletScript
	{
		get
		{
			return this.BulletScript != null && !string.IsNullOrEmpty(this.BulletScript.scriptTypeName);
		}
	}

	// Token: 0x17000A66 RID: 2662
	// (get) Token: 0x060047CF RID: 18383 RVA: 0x0017A164 File Offset: 0x00178364
	public bool IsSingleBullet
	{
		get
		{
			return !string.IsNullOrEmpty(this.BulletName);
		}
	}

	// Token: 0x060047D0 RID: 18384 RVA: 0x0017A174 File Offset: 0x00178374
	public override void Start()
	{
		base.Start();
		if (this.SpecifyAiAnimator)
		{
			this.m_aiAnimator = this.SpecifyAiAnimator;
		}
		if (!string.IsNullOrEmpty(this.TellAnimation))
		{
			tk2dSpriteAnimator spriteAnimator = this.m_aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(this.AnimEventTriggered));
			if (this.m_aiAnimator.ChildAnimator)
			{
				tk2dSpriteAnimator spriteAnimator2 = this.m_aiAnimator.ChildAnimator.spriteAnimator;
				spriteAnimator2.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator2.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(this.AnimEventTriggered));
			}
		}
	}

	// Token: 0x060047D1 RID: 18385 RVA: 0x0017A228 File Offset: 0x00178428
	public override void Upkeep()
	{
		base.Upkeep();
		if (this.state == ShootBehavior.State.WaitingForCharge)
		{
			base.DecrementTimer(ref this.m_chargeTimer, false);
		}
	}

	// Token: 0x060047D2 RID: 18386 RVA: 0x0017A24C File Offset: 0x0017844C
	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != BehaviorResult.Continue)
		{
			return behaviorResult;
		}
		if (!this.IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (this.RequiresTarget && this.m_behaviorSpeculator.TargetRigidbody == null)
		{
			return BehaviorResult.Continue;
		}
		if (this.UseVfx && !string.IsNullOrEmpty(this.Vfx))
		{
			this.m_aiAnimator.PlayVfx(this.Vfx, null, null, null);
		}
		if (!this.m_gameObject.activeSelf)
		{
			this.m_gameObject.SetActive(true);
			this.m_beganInactive = true;
		}
		if (this.m_behaviorSpeculator.TargetRigidbody)
		{
			this.m_cachedTargetCenter = this.m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		if (this.ClearGoop)
		{
			this.SetGoopClearing(true);
		}
		this.state = ShootBehavior.State.Idle;
		if (!string.IsNullOrEmpty(this.ChargeAnimation))
		{
			this.m_aiAnimator.PlayUntilFinished(this.ChargeAnimation, true, null, -1f, false);
			this.state = ShootBehavior.State.WaitingForCharge;
		}
		else if (!string.IsNullOrEmpty(this.TellAnimation))
		{
			if (!string.IsNullOrEmpty(this.TellAnimation))
			{
				this.m_aiAnimator.PlayUntilCancelled(this.TellAnimation, true, null, -1f, false);
			}
			else
			{
				this.m_aiAnimator.PlayUntilFinished(this.TellAnimation, true, null, -1f, false);
			}
			this.state = ShootBehavior.State.WaitingForTell;
			if (this.HideGun && this.m_aiShooter)
			{
				this.m_aiShooter.ToggleGunAndHandRenderers(false, "ShootBulletScript");
			}
		}
		else
		{
			this.Fire();
		}
		if (this.MoveSpeedModifier != 1f)
		{
			this.m_cachedMovementSpeed = this.m_aiActor.MovementSpeed;
			this.m_aiActor.MovementSpeed *= this.MoveSpeedModifier;
		}
		if (this.LockFacingDirection)
		{
			this.m_aiAnimator.FacingDirection = (this.m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - this.m_specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
			this.m_aiAnimator.LockFacingDirection = true;
		}
		if (this.PreventTargetSwitching && this.m_aiActor)
		{
			this.m_aiActor.SuppressTargetSwitch = true;
		}
		this.m_updateEveryFrame = true;
		if (this.OverrideBaseAnims && this.m_aiAnimator)
		{
			if (!string.IsNullOrEmpty(this.OverrideIdleAnim))
			{
				this.m_aiAnimator.OverrideIdleAnimation = this.OverrideIdleAnim;
			}
			if (!string.IsNullOrEmpty(this.OverrideMoveAnim))
			{
				this.m_aiAnimator.OverrideMoveAnimation = this.OverrideMoveAnim;
			}
		}
		if (this.StopDuring == ShootBehavior.StopType.None || this.StopDuring == ShootBehavior.StopType.TellOnly)
		{
			return BehaviorResult.RunContinuousInClass;
		}
		return BehaviorResult.RunContinuous;
	}

	// Token: 0x060047D3 RID: 18387 RVA: 0x0017A53C File Offset: 0x0017873C
	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (this.m_behaviorSpeculator.TargetRigidbody)
		{
			this.m_cachedTargetCenter = this.m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		if (this.state == ShootBehavior.State.WaitingForCharge)
		{
			if ((this.ChargeTime > 0f && this.m_chargeTimer <= 0f) || (this.ChargeTime <= 0f && !this.m_aiAnimator.IsPlaying(this.ChargeAnimation)))
			{
				if (!string.IsNullOrEmpty(this.TellAnimation))
				{
					this.m_aiAnimator.PlayUntilFinished(this.TellAnimation, true, null, -1f, false);
					this.state = ShootBehavior.State.WaitingForTell;
				}
				else
				{
					this.Fire();
				}
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (this.state == ShootBehavior.State.WaitingForTell)
		{
			if (this.LockFacingDirection && this.ContinueAimingDuringTell && !this.m_isAimLocked && this.m_behaviorSpeculator.TargetRigidbody)
			{
				this.m_aiAnimator.FacingDirection = (this.m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - this.m_specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
			}
			if (!this.m_aiAnimator.IsPlaying(this.TellAnimation))
			{
				this.Fire();
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (this.state == ShootBehavior.State.Firing)
		{
			if (!this.IsBulletScriptEnded)
			{
				return ContinuousBehaviorResult.Continue;
			}
			tk2dSpriteAnimationClip.WrapMode wrapMode;
			if (!string.IsNullOrEmpty(this.TellAnimation) && this.m_aiAnimator.IsPlaying(this.TellAnimation) && this.m_aiAnimator.GetWrapType(this.TellAnimation, out wrapMode) && wrapMode == tk2dSpriteAnimationClip.WrapMode.Once)
			{
				return ContinuousBehaviorResult.Continue;
			}
			if (!string.IsNullOrEmpty(this.FireAnimation) && this.m_aiAnimator.IsPlaying(this.FireAnimation) && this.m_aiAnimator.GetWrapType(this.FireAnimation, out wrapMode) && wrapMode == tk2dSpriteAnimationClip.WrapMode.Once)
			{
				return ContinuousBehaviorResult.Continue;
			}
			if (!string.IsNullOrEmpty(this.PostFireAnimation))
			{
				this.state = ShootBehavior.State.WaitingForPostAnim;
				this.m_aiAnimator.PlayUntilFinished(this.PostFireAnimation, false, null, -1f, false);
				return ContinuousBehaviorResult.Continue;
			}
			return ContinuousBehaviorResult.Finished;
		}
		else
		{
			if (this.state == ShootBehavior.State.WaitingForPostAnim)
			{
				return (!this.m_aiAnimator.IsPlaying(this.PostFireAnimation)) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
			}
			return ContinuousBehaviorResult.Finished;
		}
	}

	// Token: 0x060047D4 RID: 18388 RVA: 0x0017A7A8 File Offset: 0x001789A8
	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		this.CeaseFire();
		if (this.ClearGoop)
		{
			this.SetGoopClearing(false);
		}
		if (this.HideGun && this.m_aiShooter)
		{
			this.m_aiShooter.ToggleGunAndHandRenderers(true, "ShootBulletScript");
		}
		if (!string.IsNullOrEmpty(this.ChargeAnimation))
		{
			this.m_aiAnimator.EndAnimationIf(this.ChargeAnimation);
		}
		if (!string.IsNullOrEmpty(this.TellAnimation))
		{
			this.m_aiAnimator.EndAnimationIf(this.TellAnimation);
		}
		if (!string.IsNullOrEmpty(this.FireAnimation))
		{
			this.m_aiAnimator.EndAnimationIf(this.FireAnimation);
		}
		if (this.UseVfx && !string.IsNullOrEmpty(this.Vfx))
		{
			this.m_aiAnimator.StopVfx(this.Vfx);
		}
		if (this.UseVfx && !string.IsNullOrEmpty(this.ChargeVfx))
		{
			this.m_aiAnimator.StopVfx(this.ChargeVfx);
		}
		if (this.UseVfx && !string.IsNullOrEmpty(this.TellVfx))
		{
			this.m_aiAnimator.StopVfx(this.TellVfx);
		}
		if (this.UseVfx && !string.IsNullOrEmpty(this.FireVfx))
		{
			this.m_aiAnimator.StopVfx(this.FireVfx);
		}
		if (this.EnabledDuringAttack != null)
		{
			for (int i = 0; i < this.EnabledDuringAttack.Length; i++)
			{
				this.EnabledDuringAttack[i].SetActive(false);
			}
		}
		if (this.m_beganInactive)
		{
			this.m_aiAnimator.gameObject.SetActive(false);
			this.m_beganInactive = false;
		}
		if (this.MoveSpeedModifier != 1f)
		{
			this.m_aiActor.MovementSpeed = this.m_cachedMovementSpeed;
		}
		if (this.StopDuring == ShootBehavior.StopType.TellOnly)
		{
			this.m_behaviorSpeculator.PreventMovement = false;
		}
		if (this.m_aiActor && this.StopDuring != ShootBehavior.StopType.None && this.ImmobileDuringStop)
		{
			this.m_aiActor.knockbackDoer.SetImmobile(false, "ShootBulletScript");
		}
		if (this.LockFacingDirection)
		{
			this.m_aiAnimator.LockFacingDirection = false;
		}
		if (this.PreventTargetSwitching && this.m_aiActor)
		{
			this.m_aiActor.SuppressTargetSwitch = false;
		}
		if (this.OverrideBaseAnims && this.m_aiAnimator)
		{
			if (!string.IsNullOrEmpty(this.OverrideIdleAnim))
			{
				this.m_aiAnimator.OverrideIdleAnimation = null;
			}
			if (!string.IsNullOrEmpty(this.OverrideMoveAnim))
			{
				this.m_aiAnimator.OverrideMoveAnimation = null;
			}
		}
		this.m_updateEveryFrame = false;
		this.state = ShootBehavior.State.Idle;
		this.UpdateCooldowns();
	}

	// Token: 0x060047D5 RID: 18389 RVA: 0x0017AA84 File Offset: 0x00178C84
	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		this.m_specRigidbody = this.m_behaviorSpeculator.specRigidbody;
		this.m_bulletBank = this.m_behaviorSpeculator.bulletBank;
	}

	// Token: 0x060047D6 RID: 18390 RVA: 0x0017AAB4 File Offset: 0x00178CB4
	public override bool IsOverridable()
	{
		return !this.Uninterruptible;
	}

	// Token: 0x060047D7 RID: 18391 RVA: 0x0017AAC0 File Offset: 0x00178CC0
	private void Fire()
	{
		if (this.LockFacingDirection && this.ReaimOnFire && this.m_behaviorSpeculator.TargetRigidbody)
		{
			this.m_aiAnimator.FacingDirection = (this.m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - this.m_specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
		}
		if (!string.IsNullOrEmpty(this.FireAnimation))
		{
			this.m_aiAnimator.EndAnimation();
			this.m_aiAnimator.PlayUntilFinished(this.FireAnimation, false, null, -1f, false);
		}
		if (this.UseVfx && !string.IsNullOrEmpty(this.FireVfx))
		{
			this.m_aiAnimator.PlayVfx(this.FireVfx, null, null, null);
		}
		this.SpawnProjectiles();
		if (this.EnabledDuringAttack != null)
		{
			for (int i = 0; i < this.EnabledDuringAttack.Length; i++)
			{
				this.EnabledDuringAttack[i].SetActive(true);
			}
		}
		if (this.StopDuring == ShootBehavior.StopType.TellOnly)
		{
			this.m_behaviorSpeculator.PreventMovement = false;
			if (this.m_aiActor && this.ImmobileDuringStop)
			{
				this.m_aiActor.knockbackDoer.SetImmobile(false, "ShootBulletScript");
			}
		}
		else if (this.StopDuring != ShootBehavior.StopType.None)
		{
			this.StopMoving();
		}
		this.state = ShootBehavior.State.Firing;
		if (this.HideGun && this.m_aiShooter)
		{
			this.m_aiShooter.ToggleGunAndHandRenderers(false, "ShootBulletScript");
		}
	}

	// Token: 0x060047D8 RID: 18392 RVA: 0x0017AC74 File Offset: 0x00178E74
	private void CeaseFire()
	{
		if (this.IsBulletScript && this.m_bulletSource && !this.m_bulletSource.IsEnded)
		{
			this.m_bulletSource.ForceStop();
		}
	}

	// Token: 0x060047D9 RID: 18393 RVA: 0x0017ACAC File Offset: 0x00178EAC
	private void StopMoving()
	{
		if (this.m_aiActor)
		{
			this.m_aiActor.ClearPath();
			if (this.StopDuring == ShootBehavior.StopType.TellOnly)
			{
				this.m_behaviorSpeculator.PreventMovement = true;
			}
			if (this.ImmobileDuringStop)
			{
				this.m_aiActor.knockbackDoer.SetImmobile(true, "ShootBulletScript");
			}
		}
	}

	// Token: 0x060047DA RID: 18394 RVA: 0x0017AD10 File Offset: 0x00178F10
	protected override Vector2 GetOrigin(ShootBehavior.TargetAreaOrigin origin)
	{
		if (origin == ShootBehavior.TargetAreaOrigin.ShootPoint)
		{
			return this.ShootPoint.transform.position.XY();
		}
		return base.GetOrigin(origin);
	}

	// Token: 0x060047DB RID: 18395 RVA: 0x0017AD38 File Offset: 0x00178F38
	private void SpawnProjectiles()
	{
		if (this.IsBulletScript)
		{
			if (!this.m_bulletSource)
			{
				this.m_bulletSource = this.ShootPoint.GetOrAddComponent<BulletScriptSource>();
			}
			this.m_bulletSource.BulletManager = this.m_bulletBank;
			this.m_bulletSource.BulletScript = this.BulletScript;
			this.m_bulletSource.Initialize();
			return;
		}
		if (this.IsSingleBullet)
		{
			AIBulletBank.Entry bullet = this.m_bulletBank.GetBullet(this.BulletName);
			GameObject bulletObject = bullet.BulletObject;
			Vector2 vector = this.m_cachedTargetCenter;
			if (this.m_behaviorSpeculator.TargetRigidbody)
			{
				vector = this.m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			float num;
			if (this.ShouldOverrideFireDirection)
			{
				num = this.OverrideFireDirection;
			}
			else
			{
				if (this.LeadAmount > 0f)
				{
					Vector2 vector2 = this.ShootPoint.transform.position;
					float? num2 = ((!bullet.OverrideProjectile) ? null : new float?(bullet.ProjectileData.speed));
					Projectile component = bulletObject.GetComponent<Projectile>();
					Vector2 predictedTargetPosition = component.GetPredictedTargetPosition(vector, this.m_behaviorSpeculator.TargetVelocity, new Vector2?(vector2), num2);
					vector = Vector2.Lerp(vector, predictedTargetPosition, this.LeadAmount);
				}
				Vector2 vector3 = vector - this.ShootPoint.transform.position.XY();
				num = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
			}
			GameObject gameObject = this.m_bulletBank.CreateProjectileFromBank(this.ShootPoint.transform.position, num, this.BulletName, null, false, true, false);
			if (this.m_bulletBank.OnProjectileCreatedWithSource != null)
			{
				this.m_bulletBank.OnProjectileCreatedWithSource(this.ShootPoint.transform.name, gameObject.GetComponent<Projectile>());
			}
			ArcProjectile component2 = gameObject.GetComponent<ArcProjectile>();
			if (component2)
			{
				component2.AdjustSpeedToHit(vector);
			}
		}
	}

	// Token: 0x060047DC RID: 18396 RVA: 0x0017AF48 File Offset: 0x00179148
	private void SetGoopClearing(bool value)
	{
		if (!this.ClearGoop || !this.m_aiActor || !this.m_aiActor.specRigidbody)
		{
			return;
		}
		if (value)
		{
			this.m_goopExceptionId = DeadlyDeadlyGoopManager.RegisterUngoopableCircle(this.m_aiActor.specRigidbody.UnitCenter, 2f);
		}
		else
		{
			if (this.m_goopExceptionId != -1)
			{
				DeadlyDeadlyGoopManager.DeregisterUngoopableCircle(this.m_goopExceptionId);
			}
			this.m_goopExceptionId = -1;
		}
	}

	// Token: 0x17000A67 RID: 2663
	// (get) Token: 0x060047DD RID: 18397 RVA: 0x0017AFD0 File Offset: 0x001791D0
	public bool IsBulletScriptEnded
	{
		get
		{
			if (this.IsBulletScript)
			{
				return this.m_bulletSource.IsEnded;
			}
			return !this.IsSingleBullet || true;
		}
	}

	// Token: 0x17000A68 RID: 2664
	// (get) Token: 0x060047DE RID: 18398 RVA: 0x0017AFF8 File Offset: 0x001791F8
	// (set) Token: 0x060047DF RID: 18399 RVA: 0x0017B000 File Offset: 0x00179200
	private ShootBehavior.State state
	{
		get
		{
			return this.m_state;
		}
		set
		{
			if (this.m_state != value)
			{
				this.EndState(this.m_state);
				this.m_state = value;
				this.BeginState(this.m_state);
			}
		}
	}

	// Token: 0x060047E0 RID: 18400 RVA: 0x0017B030 File Offset: 0x00179230
	private void BeginState(ShootBehavior.State state)
	{
		if (state == ShootBehavior.State.WaitingForCharge)
		{
			if (this.UseVfx && !string.IsNullOrEmpty(this.ChargeVfx))
			{
				this.m_aiAnimator.PlayVfx(this.ChargeVfx, null, null, null);
			}
			if (this.StopDuring == ShootBehavior.StopType.Charge)
			{
				this.StopMoving();
			}
			this.m_chargeTimer = this.ChargeTime;
		}
		else if (state == ShootBehavior.State.WaitingForTell)
		{
			if (this.UseVfx && !string.IsNullOrEmpty(this.TellVfx))
			{
				this.m_aiAnimator.PlayVfx(this.TellVfx, null, null, null);
			}
			if (this.StopDuring == ShootBehavior.StopType.Tell || this.StopDuring == ShootBehavior.StopType.TellOnly)
			{
				this.StopMoving();
			}
			this.m_isAimLocked = false;
		}
	}

	// Token: 0x060047E1 RID: 18401 RVA: 0x0017B124 File Offset: 0x00179324
	private void EndState(ShootBehavior.State state)
	{
		if (state == ShootBehavior.State.WaitingForCharge)
		{
			if (this.UseVfx && !string.IsNullOrEmpty(this.ChargeVfx))
			{
				this.m_aiAnimator.StopVfx(this.ChargeVfx);
			}
		}
		else if (state == ShootBehavior.State.WaitingForTell)
		{
			if (this.UseVfx && !string.IsNullOrEmpty(this.TellVfx))
			{
				this.m_aiAnimator.StopVfx(this.TellVfx);
			}
			if (this.OverrideBaseAnims)
			{
				if (!string.IsNullOrEmpty(this.OverrideIdleAnim))
				{
					this.m_aiAnimator.OverrideIdleAnimation = this.OverrideIdleAnim;
				}
				if (!string.IsNullOrEmpty(this.OverrideMoveAnim))
				{
					this.m_aiAnimator.OverrideMoveAnimation = this.OverrideMoveAnim;
				}
				if (!string.IsNullOrEmpty(this.TellAnimation))
				{
					this.m_aiAnimator.EndAnimationIf(this.TellAnimation);
				}
			}
		}
		else if (state == ShootBehavior.State.Firing && this.UseVfx && !string.IsNullOrEmpty(this.FireVfx))
		{
			this.m_aiAnimator.StopVfx(this.FireVfx);
		}
	}

	// Token: 0x060047E2 RID: 18402 RVA: 0x0017B244 File Offset: 0x00179444
	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		bool flag = this.state == ShootBehavior.State.WaitingForTell;
		if (this.MultipleFireEvents)
		{
			flag |= this.state == ShootBehavior.State.Firing;
		}
		if (flag && frame.eventInfo == "fire")
		{
			this.Fire();
		}
		if (this.LockFacingDirection && this.ContinueAimingDuringTell && frame.eventInfo == "stopAiming")
		{
			this.m_isAimLocked = true;
		}
	}

	// Token: 0x04003B18 RID: 15128
	public GameObject ShootPoint;

	// Token: 0x04003B19 RID: 15129
	[InspectorShowIf("ShowBulletScript")]
	public BulletScriptSelector BulletScript;

	// Token: 0x04003B1A RID: 15130
	[InspectorShowIf("ShowBulletName")]
	public string BulletName;

	// Token: 0x04003B1B RID: 15131
	[InspectorShowIf("IsSingleBullet")]
	public float LeadAmount;

	// Token: 0x04003B1C RID: 15132
	public ShootBehavior.StopType StopDuring;

	// Token: 0x04003B1D RID: 15133
	[InspectorShowIf("ShowImmobileDuringStop")]
	public bool ImmobileDuringStop;

	// Token: 0x04003B1E RID: 15134
	public float MoveSpeedModifier = 1f;

	// Token: 0x04003B1F RID: 15135
	public bool LockFacingDirection;

	// Token: 0x04003B20 RID: 15136
	[InspectorIndent]
	[InspectorShowIf("LockFacingDirection")]
	public bool ContinueAimingDuringTell;

	// Token: 0x04003B21 RID: 15137
	[InspectorIndent]
	[InspectorShowIf("LockFacingDirection")]
	public bool ReaimOnFire;

	// Token: 0x04003B22 RID: 15138
	public bool MultipleFireEvents;

	// Token: 0x04003B23 RID: 15139
	public bool RequiresTarget = true;

	// Token: 0x04003B24 RID: 15140
	public bool PreventTargetSwitching;

	// Token: 0x04003B25 RID: 15141
	public bool Uninterruptible;

	// Token: 0x04003B26 RID: 15142
	public bool ClearGoop;

	// Token: 0x04003B27 RID: 15143
	[InspectorIndent]
	[InspectorShowIf("ClearGoop")]
	public float ClearGoopRadius = 2f;

	// Token: 0x04003B28 RID: 15144
	[InspectorShowIf("ShowBulletName")]
	public bool ShouldOverrideFireDirection;

	// Token: 0x04003B29 RID: 15145
	[InspectorIndent]
	[InspectorShowIf("ShowOverrideFireDirection")]
	public float OverrideFireDirection;

	// Token: 0x04003B2A RID: 15146
	[InspectorCategory("Visuals")]
	public AIAnimator SpecifyAiAnimator;

	// Token: 0x04003B2B RID: 15147
	[InspectorCategory("Visuals")]
	public string ChargeAnimation;

	// Token: 0x04003B2C RID: 15148
	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowChargeTime")]
	public float ChargeTime;

	// Token: 0x04003B2D RID: 15149
	[InspectorCategory("Visuals")]
	public string TellAnimation;

	// Token: 0x04003B2E RID: 15150
	[InspectorCategory("Visuals")]
	public string FireAnimation;

	// Token: 0x04003B2F RID: 15151
	[InspectorCategory("Visuals")]
	public string PostFireAnimation;

	// Token: 0x04003B30 RID: 15152
	[InspectorCategory("Visuals")]
	public bool HideGun = true;

	// Token: 0x04003B31 RID: 15153
	[InspectorCategory("Visuals")]
	public bool OverrideBaseAnims;

	// Token: 0x04003B32 RID: 15154
	[InspectorShowIf("OverrideBaseAnims")]
	[InspectorIndent]
	[InspectorCategory("Visuals")]
	public string OverrideIdleAnim;

	// Token: 0x04003B33 RID: 15155
	[InspectorIndent]
	[InspectorCategory("Visuals")]
	[InspectorShowIf("OverrideBaseAnims")]
	public string OverrideMoveAnim;

	// Token: 0x04003B34 RID: 15156
	[InspectorCategory("Visuals")]
	public bool UseVfx;

	// Token: 0x04003B35 RID: 15157
	[InspectorCategory("Visuals")]
	[InspectorShowIf("UseVfx")]
	[InspectorIndent]
	public string ChargeVfx;

	// Token: 0x04003B36 RID: 15158
	[InspectorShowIf("UseVfx")]
	[InspectorCategory("Visuals")]
	[InspectorIndent]
	public string TellVfx;

	// Token: 0x04003B37 RID: 15159
	[InspectorCategory("Visuals")]
	[InspectorShowIf("UseVfx")]
	[InspectorIndent]
	public string FireVfx;

	// Token: 0x04003B38 RID: 15160
	[InspectorIndent]
	[InspectorCategory("Visuals")]
	[InspectorShowIf("UseVfx")]
	public string Vfx;

	// Token: 0x04003B39 RID: 15161
	[InspectorCategory("Visuals")]
	public GameObject[] EnabledDuringAttack;

	// Token: 0x04003B3A RID: 15162
	private SpeculativeRigidbody m_specRigidbody;

	// Token: 0x04003B3B RID: 15163
	private AIBulletBank m_bulletBank;

	// Token: 0x04003B3C RID: 15164
	private BulletScriptSource m_bulletSource;

	// Token: 0x04003B3D RID: 15165
	private float m_chargeTimer;

	// Token: 0x04003B3E RID: 15166
	private bool m_beganInactive;

	// Token: 0x04003B3F RID: 15167
	private bool m_isAimLocked;

	// Token: 0x04003B40 RID: 15168
	private float m_cachedMovementSpeed;

	// Token: 0x04003B41 RID: 15169
	private Vector2 m_cachedTargetCenter;

	// Token: 0x04003B42 RID: 15170
	private int m_goopExceptionId = -1;

	// Token: 0x04003B43 RID: 15171
	private ShootBehavior.State m_state;

	// Token: 0x02000D48 RID: 3400
	public enum StopType
	{
		// Token: 0x04003B45 RID: 15173
		None,
		// Token: 0x04003B46 RID: 15174
		Tell,
		// Token: 0x04003B47 RID: 15175
		Attack,
		// Token: 0x04003B48 RID: 15176
		Charge,
		// Token: 0x04003B49 RID: 15177
		TellOnly
	}

	// Token: 0x02000D49 RID: 3401
	private enum State
	{
		// Token: 0x04003B4B RID: 15179
		Idle,
		// Token: 0x04003B4C RID: 15180
		WaitingForCharge,
		// Token: 0x04003B4D RID: 15181
		WaitingForTell,
		// Token: 0x04003B4E RID: 15182
		Firing,
		// Token: 0x04003B4F RID: 15183
		WaitingForPostAnim
	}

	// Token: 0x02000D4A RID: 3402
	public enum TargetAreaOrigin
	{
		// Token: 0x04003B51 RID: 15185
		HitboxCenter,
		// Token: 0x04003B52 RID: 15186
		ShootPoint
	}

	// Token: 0x02000D4B RID: 3403
	public abstract class FiringAreaStyle
	{
		// Token: 0x060047E4 RID: 18404
		public abstract bool TargetInFiringArea(Vector2 origin, Vector2 targetCenter);

		// Token: 0x060047E5 RID: 18405
		public abstract void DrawDebugLines(Vector2 origin, Vector2 targetCenter, AIActor actor);

		// Token: 0x04003B53 RID: 15187
		public ShootBehavior.TargetAreaOrigin targetAreaOrigin;
	}

	// Token: 0x02000D4C RID: 3404
	public class ArcFiringArea : ShootBehavior.FiringAreaStyle
	{
		// Token: 0x060047E7 RID: 18407 RVA: 0x0017B2E0 File Offset: 0x001794E0
		public override bool TargetInFiringArea(Vector2 origin, Vector2 targetCenter)
		{
			return BraveMathCollege.IsAngleWithinSweepArea((targetCenter - origin).ToAngle(), this.StartAngle, this.SweepAngle);
		}

		// Token: 0x060047E8 RID: 18408 RVA: 0x0017B300 File Offset: 0x00179500
		public override void DrawDebugLines(Vector2 origin, Vector2 targetCenter, AIActor actor)
		{
			BasicAttackBehavior.m_arcCount++;
		}

		// Token: 0x04003B54 RID: 15188
		public float StartAngle;

		// Token: 0x04003B55 RID: 15189
		public float SweepAngle;
	}

	// Token: 0x02000D4D RID: 3405
	public class RectFiringArea : ShootBehavior.FiringAreaStyle
	{
		// Token: 0x17000A69 RID: 2665
		// (get) Token: 0x060047EA RID: 18410 RVA: 0x0017B318 File Offset: 0x00179518
		private Vector2 offset
		{
			get
			{
				Vector2 areaOriginOffset = this.AreaOriginOffset;
				if (this.AreaDimensions.x < 0f)
				{
					areaOriginOffset.x += this.AreaDimensions.x;
				}
				if (this.AreaDimensions.y < 0f)
				{
					areaOriginOffset.y += this.AreaDimensions.y;
				}
				return areaOriginOffset;
			}
		}

		// Token: 0x17000A6A RID: 2666
		// (get) Token: 0x060047EB RID: 18411 RVA: 0x0017B38C File Offset: 0x0017958C
		private Vector2 dimensions
		{
			get
			{
				return new Vector2(Mathf.Abs(this.AreaDimensions.x), Mathf.Abs(this.AreaDimensions.y));
			}
		}

		// Token: 0x060047EC RID: 18412 RVA: 0x0017B3B4 File Offset: 0x001795B4
		public override bool TargetInFiringArea(Vector2 origin, Vector2 targetCenter)
		{
			origin += this.offset;
			return targetCenter.x >= origin.x && targetCenter.x <= origin.x + this.dimensions.x && targetCenter.y >= origin.y && targetCenter.y <= origin.y + this.dimensions.y;
		}

		// Token: 0x060047ED RID: 18413 RVA: 0x0017B43C File Offset: 0x0017963C
		public override void DrawDebugLines(Vector2 origin, Vector2 targetCenter, AIActor actor)
		{
			origin += this.offset;
		}

		// Token: 0x04003B56 RID: 15190
		public Vector2 AreaOriginOffset;

		// Token: 0x04003B57 RID: 15191
		public Vector2 AreaDimensions;
	}
}
