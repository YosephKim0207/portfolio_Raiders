﻿using System;
using UnityEngine;

// Token: 0x02000B7D RID: 2941
[AddComponentMenu("Image Effects/Motion Blur (Color Accumulation)")]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class MotionBlur : ImageEffectBase
{
	// Token: 0x06003D81 RID: 15745 RVA: 0x00133B6C File Offset: 0x00131D6C
	protected override void Start()
	{
		if (!SystemInfo.supportsRenderTextures)
		{
			base.enabled = false;
			return;
		}
		base.Start();
	}

	// Token: 0x06003D82 RID: 15746 RVA: 0x00133B88 File Offset: 0x00131D88
	protected override void OnDisable()
	{
		base.OnDisable();
		UnityEngine.Object.DestroyImmediate(this.accumTexture);
	}

	// Token: 0x06003D83 RID: 15747 RVA: 0x00133B9C File Offset: 0x00131D9C
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (this.accumTexture == null || this.accumTexture.width != source.width || this.accumTexture.height != source.height)
		{
			UnityEngine.Object.DestroyImmediate(this.accumTexture);
			this.accumTexture = new RenderTexture(source.width, source.height, 0);
			this.accumTexture.hideFlags = HideFlags.HideAndDontSave;
			Graphics.Blit(source, this.accumTexture);
		}
		if (this.extraBlur)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);
			Graphics.Blit(this.accumTexture, temporary);
			Graphics.Blit(temporary, this.accumTexture);
			RenderTexture.ReleaseTemporary(temporary);
		}
		this.blurAmount = Mathf.Clamp(this.blurAmount, 0f, 0.92f);
		base.material.SetTexture("_MainTex", this.accumTexture);
		base.material.SetFloat("_AccumOrig", 1f - this.blurAmount);
		Graphics.Blit(source, this.accumTexture, base.material);
		Graphics.Blit(this.accumTexture, destination);
	}

	// Token: 0x04002FD7 RID: 12247
	public float blurAmount = 0.8f;

	// Token: 0x04002FD8 RID: 12248
	public bool extraBlur;

	// Token: 0x04002FD9 RID: 12249
	private RenderTexture accumTexture;
}
