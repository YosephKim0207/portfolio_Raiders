﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace tk2dRuntime.TileMap
{
	// Token: 0x02000BF6 RID: 3062
	public static class ColliderBuilder3D
	{
		// Token: 0x06004138 RID: 16696 RVA: 0x00150078 File Offset: 0x0014E278
		public static void Build(tk2dTileMap tileMap, bool forceBuild)
		{
			bool flag = !forceBuild;
			int num = tileMap.Layers.Length;
			for (int i = 0; i < num; i++)
			{
				Layer layer = tileMap.Layers[i];
				if (!layer.IsEmpty && tileMap.data.Layers[i].generateCollider)
				{
					for (int j = 0; j < layer.numRows; j++)
					{
						int num2 = j * layer.divY;
						for (int k = 0; k < layer.numColumns; k++)
						{
							int num3 = k * layer.divX;
							SpriteChunk chunk = layer.GetChunk(k, j);
							if (!flag || chunk.Dirty)
							{
								if (!chunk.IsEmpty)
								{
									ColliderBuilder3D.BuildForChunk(tileMap, chunk, num3, num2);
									PhysicMaterial physicMaterial = tileMap.data.Layers[i].physicMaterial;
									if (chunk.meshCollider != null)
									{
										chunk.meshCollider.sharedMaterial = physicMaterial;
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06004139 RID: 16697 RVA: 0x00150198 File Offset: 0x0014E398
		public static void BuildForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY)
		{
			Vector3[] array = new Vector3[0];
			int[] array2 = new int[0];
			ColliderBuilder3D.BuildLocalMeshForChunk(tileMap, chunk, baseX, baseY, ref array, ref array2);
			if (array2.Length > 6)
			{
				array = ColliderBuilder3D.WeldVertices(array, ref array2);
				array2 = ColliderBuilder3D.RemoveDuplicateFaces(array2);
			}
			foreach (EdgeCollider2D edgeCollider2D in chunk.edgeColliders)
			{
				if (edgeCollider2D != null)
				{
					tk2dUtil.DestroyImmediate(edgeCollider2D);
				}
			}
			chunk.edgeColliders.Clear();
			if (array.Length > 0)
			{
				if (chunk.colliderMesh != null)
				{
					tk2dUtil.DestroyImmediate(chunk.colliderMesh);
					chunk.colliderMesh = null;
				}
				if (chunk.meshCollider == null)
				{
					chunk.meshCollider = chunk.gameObject.GetComponent<MeshCollider>();
					if (chunk.meshCollider == null)
					{
						chunk.meshCollider = tk2dUtil.AddComponent<MeshCollider>(chunk.gameObject);
					}
				}
				chunk.colliderMesh = tk2dUtil.CreateMesh();
				chunk.colliderMesh.vertices = array;
				chunk.colliderMesh.triangles = array2;
				chunk.colliderMesh.RecalculateBounds();
				chunk.meshCollider.sharedMesh = chunk.colliderMesh;
			}
			else
			{
				chunk.DestroyColliderData(tileMap);
			}
		}

		// Token: 0x0600413A RID: 16698 RVA: 0x001502FC File Offset: 0x0014E4FC
		private static void BuildLocalMeshForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY, ref Vector3[] vertices, ref int[] indices)
		{
			List<Vector3> list = new List<Vector3>();
			List<int> list2 = new List<int>();
			int num = tileMap.SpriteCollectionInst.spriteDefinitions.Length;
			Vector3 tileSize = tileMap.data.tileSize;
			GameObject[] tilePrefabs = tileMap.data.tilePrefabs;
			float num2 = 0f;
			float num3 = 0f;
			tileMap.data.GetTileOffset(out num2, out num3);
			int[] spriteIds = chunk.spriteIds;
			for (int i = 0; i < chunk.Height; i++)
			{
				float num4 = (float)((baseY + i) & 1) * num2;
				for (int j = 0; j < chunk.Width; j++)
				{
					int num5 = spriteIds[i * chunk.Width + j];
					int tileFromRawTile = BuilderUtil.GetTileFromRawTile(num5);
					Vector3 vector = new Vector3(tileSize.x * ((float)j + num4), tileSize.y * (float)i, 0f);
					if (tileFromRawTile >= 0 && tileFromRawTile < num)
					{
						if (!tilePrefabs[tileFromRawTile])
						{
							bool flag = BuilderUtil.IsRawTileFlagSet(num5, tk2dTileFlags.FlipX);
							bool flag2 = BuilderUtil.IsRawTileFlagSet(num5, tk2dTileFlags.FlipY);
							bool flag3 = BuilderUtil.IsRawTileFlagSet(num5, tk2dTileFlags.Rot90);
							bool flag4 = false;
							if (flag)
							{
								flag4 = !flag4;
							}
							if (flag2)
							{
								flag4 = !flag4;
							}
							tk2dSpriteDefinition tk2dSpriteDefinition = tileMap.SpriteCollectionInst.spriteDefinitions[tileFromRawTile];
							int count = list.Count;
							if (tk2dSpriteDefinition.colliderType == tk2dSpriteDefinition.ColliderType.Box)
							{
								Vector3 vector2 = tk2dSpriteDefinition.colliderVertices[0];
								Vector3 vector3 = tk2dSpriteDefinition.colliderVertices[1];
								Vector3 vector4 = vector2 - vector3;
								Vector3 vector5 = vector2 + vector3;
								Vector3[] array = new Vector3[]
								{
									new Vector3(vector4.x, vector4.y, vector4.z),
									new Vector3(vector4.x, vector4.y, vector5.z),
									new Vector3(vector5.x, vector4.y, vector4.z),
									new Vector3(vector5.x, vector4.y, vector5.z),
									new Vector3(vector4.x, vector5.y, vector4.z),
									new Vector3(vector4.x, vector5.y, vector5.z),
									new Vector3(vector5.x, vector5.y, vector4.z),
									new Vector3(vector5.x, vector5.y, vector5.z)
								};
								for (int k = 0; k < 8; k++)
								{
									Vector3 vector6 = BuilderUtil.ApplySpriteVertexTileFlags(tileMap, tk2dSpriteDefinition, array[k], flag, flag2, flag3);
									list.Add(vector6 + vector);
								}
								int[] array2 = new int[]
								{
									2, 1, 0, 3, 1, 2, 4, 5, 6, 6,
									5, 7, 6, 7, 3, 6, 3, 2, 1, 5,
									4, 0, 1, 4
								};
								int[] array3 = array2;
								for (int l = 0; l < array3.Length; l++)
								{
									int num6 = ((!flag4) ? l : (array3.Length - 1 - l));
									list2.Add(count + array3[num6]);
								}
							}
							else if (tk2dSpriteDefinition.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
							{
							}
						}
					}
				}
			}
			vertices = list.ToArray();
			indices = list2.ToArray();
		}

		// Token: 0x0600413B RID: 16699 RVA: 0x001506B8 File Offset: 0x0014E8B8
		private static int CompareWeldVertices(Vector3 a, Vector3 b)
		{
			float num = 0.01f;
			float num2 = a.x - b.x;
			if (Mathf.Abs(num2) > num)
			{
				return (int)Mathf.Sign(num2);
			}
			float num3 = a.y - b.y;
			if (Mathf.Abs(num3) > num)
			{
				return (int)Mathf.Sign(num3);
			}
			float num4 = a.z - b.z;
			if (Mathf.Abs(num4) > num)
			{
				return (int)Mathf.Sign(num4);
			}
			return 0;
		}

		// Token: 0x0600413C RID: 16700 RVA: 0x00150738 File Offset: 0x0014E938
		private static Vector3[] WeldVertices(Vector3[] vertices, ref int[] indices)
		{
			int[] array = new int[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				array[i] = i;
			}
			Array.Sort<int>(array, (int a, int b) => ColliderBuilder3D.CompareWeldVertices(vertices[a], vertices[b]));
			List<Vector3> list = new List<Vector3>();
			int[] array2 = new int[vertices.Length];
			Vector3 vector = vertices[array[0]];
			list.Add(vector);
			array2[array[0]] = list.Count - 1;
			for (int j = 1; j < array.Length; j++)
			{
				Vector3 vector2 = vertices[array[j]];
				if (ColliderBuilder3D.CompareWeldVertices(vector2, vector) != 0)
				{
					vector = vector2;
					list.Add(vector);
					array2[array[j]] = list.Count - 1;
				}
				array2[array[j]] = list.Count - 1;
			}
			for (int k = 0; k < indices.Length; k++)
			{
				indices[k] = array2[indices[k]];
			}
			return list.ToArray();
		}

		// Token: 0x0600413D RID: 16701 RVA: 0x00150864 File Offset: 0x0014EA64
		private static int CompareDuplicateFaces(int[] indices, int face0index, int face1index)
		{
			for (int i = 0; i < 3; i++)
			{
				int num = indices[face0index + i] - indices[face1index + i];
				if (num != 0)
				{
					return num;
				}
			}
			return 0;
		}

		// Token: 0x0600413E RID: 16702 RVA: 0x00150898 File Offset: 0x0014EA98
		private static int[] RemoveDuplicateFaces(int[] indices)
		{
			int[] sortedFaceIndices = new int[indices.Length];
			for (int i = 0; i < indices.Length; i += 3)
			{
				int[] array = new int[]
				{
					indices[i],
					indices[i + 1],
					indices[i + 2]
				};
				Array.Sort<int>(array);
				sortedFaceIndices[i] = array[0];
				sortedFaceIndices[i + 1] = array[1];
				sortedFaceIndices[i + 2] = array[2];
			}
			int[] array2 = new int[indices.Length / 3];
			for (int j = 0; j < indices.Length; j += 3)
			{
				array2[j / 3] = j;
			}
			Array.Sort<int>(array2, (int a, int b) => ColliderBuilder3D.CompareDuplicateFaces(sortedFaceIndices, a, b));
			List<int> list = new List<int>();
			for (int k = 0; k < array2.Length; k++)
			{
				if (k != array2.Length - 1 && ColliderBuilder3D.CompareDuplicateFaces(sortedFaceIndices, array2[k], array2[k + 1]) == 0)
				{
					k++;
				}
				else
				{
					for (int l = 0; l < 3; l++)
					{
						list.Add(indices[array2[k] + l]);
					}
				}
			}
			return list.ToArray();
		}
	}
}
