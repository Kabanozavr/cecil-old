﻿//
// ReflectionMerger.cs
//
// Author:
//	 Alex Prudkiy (prudkiy@gmail.com)
//
// (C) 2006 Alex Prudkiy
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;

using Mono.Cecil;

namespace Mono.Merge {

	public class ReflectionMerger : BaseMergeReflectionVisitor {

		public ReflectionMerger (MergeContext context, AssemblyDefinition target, AssemblyDefinition source)
			: base (context, target, source)
		{
		}

		public override void VisitTypeDefinitionCollection (TypeDefinitionCollection types)
		{
			VisitCollection (types);
		}

		public override void VisitTypeDefinition (TypeDefinition type)
		{
			if (type.Name == Context.MainTypeName) {
				bool clone = (type != Context.MainType);

				Console.WriteLine ("Found main type in assembly {0}. clone is {1}", type.Module.Assembly.Name.Name, clone);

				foreach (MethodDefinition method in type.Methods) {
					MethodDefinition md = Context.InternalSymbols.InsertMethod (method, clone);
					if (md != null) {
						if (clone) {
							Context.MainType.Methods.Add (md);
						}
					} else {
						Context.LinkFailed = true;
					}
				}

				foreach (FieldDefinition field in type.Fields) {
					FieldDefinition fd = Context.InternalSymbols.InsertField (field, clone);
					if (fd != null) {
						if (clone) {
							Context.MainType.Fields.Add (fd);
						}
					} else {
						Context.LinkFailed = true;
					}
				}
			} else if (!Target.MainModule.Types.Contains (type) && type.DeclaringType == null) {
				//not nested
				TypeDefinition td = type.Clone ();
				Target.MainModule.Types.Add (td);
				foreach (TypeDefinition nested in td.NestedTypes)
					Target.MainModule.Types.Add (nested);
			}
		}

		public override void VisitTypeReferenceCollection (TypeReferenceCollection refs)
		{
			foreach (TypeReference tr in refs) {
				if ((tr.Module.Assembly.Name.Name != Context.ExternalAssemblyName) && GetTypeReference (tr, false) == null)
					AddTypeReference (tr);
			}
		}

		AssemblyNameReference GetAssemblyReference (AssemblyNameReference refer)
		{
			foreach (AssemblyNameReference aref in Target.MainModule.AssemblyReferences) {
				if (aref.Name == refer.Name)
					return aref;
			}
			throw new ArgumentException ("Can't found AssemblyReference : " + refer.Name);
		}

		public void AddTypeReference (TypeReference type)
		{
			TypeReference td =
				new TypeReference (
					type.Name, type.Namespace, GetAssemblyReference ((AssemblyNameReference) type.Scope), type.IsValueType);
			Target.MainModule.TypeReferences.Add (td);
			if (type.DeclaringType != null)
				td.DeclaringType = GetTypeReference (type.DeclaringType);
		}

		public override void VisitMemberReferenceCollection (MemberReferenceCollection members)
		{
			foreach (MemberReference mr in members)
				VisitMemberReference (mr);
		}

		bool MemberReferencesContains (MemberReferenceCollection members, MemberReference member)
		{
			MemberReference mr = GetMemberReference (members, member);
			return mr != null;
		}

		public override void VisitMemberReference (MemberReference member)
		{
			if ((member.DeclaringType.Module.Assembly.Name.Name != Context.ExternalAssemblyName) && !MemberReferencesContains (Target.MainModule.MemberReferences, member)) {
				member.DeclaringType = GetTypeReference (member.DeclaringType);
				Target.MainModule.MemberReferences.Add (member);
			}
		}

		public override void VisitInterfaceCollection (InterfaceCollection interfaces)
		{
			VisitCollection (interfaces);
		}

		public override void VisitExternTypeCollection (ExternTypeCollection externs)
		{
			VisitCollection (externs);
		}

		public override void VisitOverrideCollection (OverrideCollection meth)
		{
			VisitCollection (meth);
		}

		public override void VisitNestedTypeCollection (NestedTypeCollection nestedTypes)
		{
			VisitCollection (nestedTypes);
		}

		public override void VisitParameterDefinitionCollection (ParameterDefinitionCollection parameters)
		{
			VisitCollection (parameters);
		}

		public override void VisitMethodDefinitionCollection (MethodDefinitionCollection methods)
		{
			VisitCollection (methods);
		}
		#if false
		public override void VisitMethodDefinition (MethodDefinition method)
		{
			if (method.Name == ".start") {
				//TODO More sanity checks here...
				Console.WriteLine ("### Method '.start' found!");
				//entryPoint = method;
			} else {
				Console.WriteLine ("### Method {0} is not named '.start'", method.Name);
			}
		}
		#endif

		public override void VisitConstructorCollection (ConstructorCollection ctors)
		{
			VisitCollection (ctors);
		}

		public override void VisitEventDefinitionCollection (EventDefinitionCollection events)
		{
			VisitCollection (events);
		}

		public override void VisitFieldDefinitionCollection (FieldDefinitionCollection fields)
		{
			VisitCollection (fields);
		}

		public override void VisitPropertyDefinitionCollection (PropertyDefinitionCollection properties)
		{
			VisitCollection (properties);
		}

		public override void VisitCustomAttributeCollection (CustomAttributeCollection customAttrs)
		{
			VisitCollection (customAttrs);
		}
	}
}
