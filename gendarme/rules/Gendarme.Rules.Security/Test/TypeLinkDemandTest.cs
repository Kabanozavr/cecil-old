//
// Unit tests for TypeLinkDemandRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Reflection;
using System.Security.Permissions;
using SSP = System.Security.Permissions;

using Gendarme.Framework;
using Gendarme.Rules.Security;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Security {

	[TestFixture]
	public class TypeLinkDemandTest {

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		class NonPublicClass {

			public NonPublicClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		public sealed class SealedClass {

			public SealedClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		public class LinkDemandClass {

			public LinkDemandClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		public class LinkDemandVirtualMethodClass {

			public LinkDemandVirtualMethodClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[EnvironmentPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class InheritanceDemandClass {

			public InheritanceDemandClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		[EnvironmentPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class NoIntersectionClass {

			public NoIntersectionClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		[EnvironmentPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class NoIntersectionVirtualMethodClass {

			public NoIntersectionVirtualMethodClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlAppDomain = true)]
		[SecurityPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class IntersectionClass {

			public IntersectionClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		private ITypeRule rule;
		private IAssemblyDefinition assembly;
		private IModuleDefinition module;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			module = assembly.MainModule;
			rule = new TypeLinkDemandRule ();
		}

		private ITypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Security.TypeLinkDemandTest/" + name;
			return assembly.MainModule.Types[fullname];
		}

		[Test]
		public void NonPublic ()
		{
			ITypeDefinition type = GetTest ("NonPublicClass");
			Assert.IsNull (rule.CheckType (assembly, module, type, new MinimalRunner()));
		}

		[Test]
		public void Sealed ()
		{
			ITypeDefinition type = GetTest ("SealedClass");
			Assert.IsNull (rule.CheckType (assembly, module, type, new MinimalRunner()));
		}

		[Test]
		public void LinkDemand ()
		{
			ITypeDefinition type = GetTest ("LinkDemandClass");
			Assert.IsNull (rule.CheckType (assembly, module, type, new MinimalRunner()));
		}

		[Test]
		public void LinkDemandVirtualMethod ()
		{
			ITypeDefinition type = GetTest ("LinkDemandVirtualMethodClass");
			Assert.IsNotNull (rule.CheckType (assembly, module, type, new MinimalRunner()));
		}

		[Test]
		public void InheritanceDemand ()
		{
			ITypeDefinition type = GetTest ("InheritanceDemandClass");
			Assert.IsNull (rule.CheckType (assembly, module, type, new MinimalRunner()));
		}

		[Test]
		public void NoIntersection ()
		{
			ITypeDefinition type = GetTest ("NoIntersectionClass");
			Assert.IsNull (rule.CheckType (assembly, module, type, new MinimalRunner()));
		}

		[Test]
		public void NoIntersectionVirtualMethod ()
		{
			ITypeDefinition type = GetTest ("NoIntersectionVirtualMethodClass");
			Assert.IsNotNull (rule.CheckType (assembly, module, type, new MinimalRunner()));
		}

		[Test]
		public void Intersection ()
		{
			ITypeDefinition type = GetTest ("IntersectionClass");
			Assert.IsNull (rule.CheckType (assembly, module, type, new MinimalRunner()));
		}
	}
}
