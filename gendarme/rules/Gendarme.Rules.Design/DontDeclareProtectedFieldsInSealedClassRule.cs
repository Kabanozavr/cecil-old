//
// Gendarme.Rules.Dodgy.DontDeclareProtectedFieldsInSealedClassRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//
// Copyright (c) <2007> Nidhi Rawal
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Design
{
	public class DontDeclareProtectedFieldsInSealedClassRule: ITypeRule
	{
		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			MessageCollection messageCollection = new MessageCollection ();
			if (type.IsSealed) {
				foreach (FieldDefinition field in type.Fields) {
					if (field.Attributes.ToString () == "Family") {
						Location location = new Location (type.FullName, type.Name, 0);
						Message message = new Message ("This sealed class contains protected field(s)", location, MessageType.Error);
						messageCollection.Add (message);
					}
				}
			}
			if (messageCollection.Count == 0)
				return null;
			return messageCollection;
		}
	}
}