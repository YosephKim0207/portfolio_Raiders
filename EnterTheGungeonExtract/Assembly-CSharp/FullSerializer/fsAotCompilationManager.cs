﻿using System;
using System.Collections.Generic;
using System.Text;
using FullSerializer.Internal;

namespace FullSerializer
{
	// Token: 0x0200059B RID: 1435
	public class fsAotCompilationManager
	{
		// Token: 0x17000674 RID: 1652
		// (get) Token: 0x060021FA RID: 8698 RVA: 0x00095AC4 File Offset: 0x00093CC4
		public static Dictionary<Type, string> AvailableAotCompilations
		{
			get
			{
				for (int i = 0; i < fsAotCompilationManager._uncomputedAotCompilations.Count; i++)
				{
					fsAotCompilationManager.AotCompilation aotCompilation = fsAotCompilationManager._uncomputedAotCompilations[i];
					fsAotCompilationManager._computedAotCompilations[aotCompilation.Type] = fsAotCompilationManager.GenerateDirectConverterForTypeInCSharp(aotCompilation.Type, aotCompilation.Members, aotCompilation.IsConstructorPublic);
				}
				fsAotCompilationManager._uncomputedAotCompilations.Clear();
				return fsAotCompilationManager._computedAotCompilations;
			}
		}

		// Token: 0x060021FB RID: 8699 RVA: 0x00095B34 File Offset: 0x00093D34
		public static bool TryToPerformAotCompilation(Type type, out string aotCompiledClassInCSharp)
		{
			if (fsMetaType.Get(type).EmitAotData())
			{
				aotCompiledClassInCSharp = fsAotCompilationManager.AvailableAotCompilations[type];
				return true;
			}
			aotCompiledClassInCSharp = null;
			return false;
		}

		// Token: 0x060021FC RID: 8700 RVA: 0x00095B5C File Offset: 0x00093D5C
		public static void AddAotCompilation(Type type, fsMetaProperty[] members, bool isConstructorPublic)
		{
			fsAotCompilationManager._uncomputedAotCompilations.Add(new fsAotCompilationManager.AotCompilation
			{
				Type = type,
				Members = members,
				IsConstructorPublic = isConstructorPublic
			});
		}

		// Token: 0x060021FD RID: 8701 RVA: 0x00095B94 File Offset: 0x00093D94
		private static string GenerateDirectConverterForTypeInCSharp(Type type, fsMetaProperty[] members, bool isConstructorPublic)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = type.CSharpName(true);
			string text2 = type.CSharpName(true, true);
			stringBuilder.AppendLine("using System;");
			stringBuilder.AppendLine("using System.Collections.Generic;");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("namespace FullSerializer {");
			stringBuilder.AppendLine("    partial class fsConverterRegistrar {");
			stringBuilder.AppendLine(string.Concat(new string[] { "        public static Speedup.", text2, "_DirectConverter Register_", text2, ";" }));
			stringBuilder.AppendLine("    }");
			stringBuilder.AppendLine("}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("namespace FullSerializer.Speedup {");
			stringBuilder.AppendLine(string.Concat(new string[] { "    public class ", text2, "_DirectConverter : fsDirectConverter<", text, "> {" }));
			stringBuilder.AppendLine("        protected override fsResult DoSerialize(" + text + " model, Dictionary<string, fsData> serialized) {");
			stringBuilder.AppendLine("            var result = fsResult.Success;");
			stringBuilder.AppendLine();
			foreach (fsMetaProperty fsMetaProperty in members)
			{
				stringBuilder.AppendLine(string.Concat(new string[] { "            result += SerializeMember(serialized, \"", fsMetaProperty.JsonName, "\", model.", fsMetaProperty.MemberName, ");" }));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("            return result;");
			stringBuilder.AppendLine("        }");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref " + text + " model) {");
			stringBuilder.AppendLine("            var result = fsResult.Success;");
			stringBuilder.AppendLine();
			for (int j = 0; j < members.Length; j++)
			{
				fsMetaProperty fsMetaProperty2 = members[j];
				stringBuilder.AppendLine(string.Concat(new object[] { "            var t", j, " = model.", fsMetaProperty2.MemberName, ";" }));
				stringBuilder.AppendLine(string.Concat(new object[] { "            result += DeserializeMember(data, \"", fsMetaProperty2.JsonName, "\", out t", j, ");" }));
				stringBuilder.AppendLine(string.Concat(new object[] { "            model.", fsMetaProperty2.MemberName, " = t", j, ";" }));
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("            return result;");
			stringBuilder.AppendLine("        }");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("        public override object CreateInstance(fsData data, Type storageType) {");
			if (isConstructorPublic)
			{
				stringBuilder.AppendLine("            return new " + text + "();");
			}
			else
			{
				stringBuilder.AppendLine("            return Activator.CreateInstance(typeof(" + text + "), /*nonPublic:*/true);");
			}
			stringBuilder.AppendLine("        }");
			stringBuilder.AppendLine("    }");
			stringBuilder.AppendLine("}");
			return stringBuilder.ToString();
		}

		// Token: 0x0400182A RID: 6186
		private static Dictionary<Type, string> _computedAotCompilations = new Dictionary<Type, string>();

		// Token: 0x0400182B RID: 6187
		private static List<fsAotCompilationManager.AotCompilation> _uncomputedAotCompilations = new List<fsAotCompilationManager.AotCompilation>();

		// Token: 0x0200059C RID: 1436
		private struct AotCompilation
		{
			// Token: 0x0400182C RID: 6188
			public Type Type;

			// Token: 0x0400182D RID: 6189
			public fsMetaProperty[] Members;

			// Token: 0x0400182E RID: 6190
			public bool IsConstructorPublic;
		}
	}
}
