//
// SweepStep.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2006 Jb Evain
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
//

using System.Collections;

using Mono.Cecil;

namespace Mono.Linker {

	public class SweepStep : IStep {

		public void Process (LinkContext context)
		{
			foreach (AssemblyMarker am in context.GetAssemblies ())
				SweepAssembly (am, context);
		}

		static void SweepAssembly (AssemblyMarker am, LinkContext context)
		{
			if (am.Action != AssemblyAction.Link)
				return;

			foreach (TypeMarker tm in am.GetTypes ())
				SweepType (tm);

			foreach (TypeDefinition type in Clone (am.Assembly.MainModule.Types)) {
				if (IsMarked (type))
					continue;

				am.Assembly.MainModule.Types.Remove (type);
				SweepTypeReferences (am, type, context);
			}
		}

		static ICollection Clone (ICollection collection)
		{
			return new ArrayList (collection);
		}

		static void SweepTypeReferences (AssemblyMarker am, TypeDefinition type, LinkContext context)
		{
			foreach (AssemblyMarker marker in context.GetAssemblies ()) {
				ModuleDefinition module = marker.Assembly.MainModule;
				if (!module.TypeReferences.Contains (type))
					continue;

				TypeReference typeRef = module.TypeReferences [type.FullName];
				if (AssemblyMatch (am.Assembly, typeRef))
					module.TypeReferences.Remove (typeRef);
			}
		}

		static bool AssemblyMatch (AssemblyDefinition assembly, TypeReference type)
		{
			AssemblyNameReference reference = type.Scope as AssemblyNameReference;
			if (reference == null)
				return false;

			return assembly.Name.FullName == reference.FullName;
		}

		static void SweepType (TypeMarker tm)
		{
			SweepMethods (tm);
			SweepFields (tm);
		}

		static void SweepFields (TypeMarker tm)
		{
			TypeDefinition type = tm.Type;
			foreach (FieldDefinition field in Clone (type.Fields))
				if (!IsMarked (field))
					type.Fields.Remove (field);
		}

		static bool IsMarked (IAnnotationProvider provider)
		{
			return provider.Annotations.Contains (Marker.MarkerKey);
		}

		static void SweepMethods (TypeMarker tm)
		{
			TypeDefinition type = tm.Type;
			foreach (MethodDefinition meth in Clone (type.Methods))
				if (!IsMarked (meth))
					type.Methods.Remove (meth);

			foreach (MethodDefinition ctor in Clone (type.Constructors))
				if (!IsMarked (ctor))
					type.Constructors.Remove (ctor);
		}
	}
}
