//
// Gendarme.Rules.Security.NonVirtualMethodWithInheritanceDemandRule
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

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Security {

	public class NonVirtualMethodWithInheritanceDemandRule : IMethodRule {

		public bool CheckMethod (IAssemblyDefinition assembly, IModuleDefinition module, ITypeDefinition type, IMethodDefinition method)
		{
			// #1 - this rule apply only to methods with an inheritance demand
			if (method.SecurityDeclarations.Count == 0)
				return true;

			bool inherit = false;
			foreach (ISecurityDeclaration declsec in method.SecurityDeclarations) {
				switch (declsec.Action) {
				case SecurityAction.InheritDemand:
				case SecurityAction.NonCasInheritance:
					inherit = true;
					break;
				}
			}
			if (!inherit)
				return true;

			// *** ok, the rule applies! ***

			// #2 - InheritanceDemand doesn't make sense on methods that cannot be overriden
			return method.IsVirtual;
		}
	}
}