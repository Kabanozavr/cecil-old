//
// LinkContext.cs
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

	public class LinkContext {

		Pipeline _pipeline;
		AssemblyAction _coreAction;
		string _outputDirectory;
		Hashtable _assemblies;

		CustomResolver _resolver;

		public Pipeline Pipeline {
			get { return _pipeline; }
		}

		public string OutputDirectory {
			get { return _outputDirectory; }
			set { _outputDirectory = value; }
		}

		public AssemblyAction CoreAction {
			get { return _coreAction; }
			set { _coreAction = value; }
		}

		public CustomResolver Resolver {
			get { return _resolver; }
		}

		public LinkContext (Pipeline pipeline)
		{
			_pipeline = pipeline;
			_resolver = new CustomResolver ();
			_assemblies = new Hashtable ();
		}

		public AssemblyDefinition Resolve (string filename)
		{
			AssemblyDefinition assembly = AssemblyFactory.GetAssembly (filename);
			AddAssembly (assembly);
			return assembly;

		}

		public AssemblyDefinition Resolve (IMetadataScope scope)
		{
			AssemblyNameReference reference;
			if (scope is ModuleDefinition) {
				AssemblyDefinition asm = ((ModuleDefinition) scope).Assembly;
				reference = asm.Name;
			} else
				reference = (AssemblyNameReference) scope;

			AssemblyDefinition assembly = _assemblies [reference.FullName] as AssemblyDefinition;
			if (assembly != null)
				return assembly;

			assembly = _resolver.Resolve (reference);

			AssemblyAction action = AssemblyAction.Link;
			if (IsCore (reference))
				action = _coreAction;

			Annotations.SetAction (assembly, action);

			AddAssembly (assembly);
			return assembly;
		}

		void AddAssembly (AssemblyDefinition assembly)
		{
			_assemblies.Add (assembly.Name.FullName, assembly);
		}

		static bool IsCore (AssemblyNameReference name)
		{
			return name.Name == "mscorlib"
				|| name.Name == "Accessibility"
				|| name.Name.StartsWith ("System")
				|| name.Name.StartsWith ("Microsoft");
		}

		public AssemblyDefinition [] GetAssemblies ()
		{
			AssemblyDefinition [] asms = new AssemblyDefinition [_assemblies.Count];
			_assemblies.Values.CopyTo (asms, 0);
			return asms;
		}
	}
}
