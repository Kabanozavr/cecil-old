//
// CodeWriter.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2005 Jb Evain
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

namespace Mono.Cecil.Cil {

	using System;

	using Mono.Cecil;
	using Mono.Cecil.Binary;
	using Mono.Cecil.Metadata;
	using Mono.Cecil.Signatures;

	class CodeWriter : BaseCodeVisitor {

		ReflectionWriter m_reflectWriter;
		MemoryBinaryWriter m_binaryWriter;
		MemoryBinaryWriter m_codeWriter;

		public CodeWriter (ReflectionWriter reflectWriter, MemoryBinaryWriter writer)
		{
			m_reflectWriter = reflectWriter;
			m_binaryWriter = writer;
			m_codeWriter = new MemoryBinaryWriter ();
		}

		public RVA WriteMethodBody (MethodDefinition meth)
		{
			if (meth.Body == null)
				return RVA.Zero;

			RVA ret = m_reflectWriter.MetadataWriter.GetDataCursor ();
			meth.Body.Accept (this);
			return ret;
		}

		public override void VisitMethodBody (MethodBody body)
		{
			m_codeWriter.Empty ();
		}

		void WriteToken (MetadataToken token)
		{
			if (token.RID == 0)
				m_codeWriter.Write (0);
			else
				m_codeWriter.Write (((uint) token.TokenType) | token.RID);
		}

		int GetParameterIndex (MethodBody body, ParameterDefinition p)
		{
			int idx = body.Method.Parameters.IndexOf (p);
			if (body.Method.HasThis)
				idx++;

			return idx;
		}

		public override void VisitInstructionCollection (InstructionCollection instructions)
		{
			MethodBody body = instructions.Container;
			long start = m_codeWriter.BaseStream.Position;

			foreach (Instruction instr in instructions) {

				instr.Offset = (int) (m_codeWriter.BaseStream.Position - start);

				if (instr.OpCode.Size == 1)
					m_codeWriter.Write (instr.OpCode.Op2);
				else {
					m_codeWriter.Write (instr.OpCode.Op1);
					m_codeWriter.Write (instr.OpCode.Op2);
				}

				if (instr.OpCode.OperandType != OperandType.InlineNone &&
					instr.Operand == null)
					throw new ReflectionException ("OpCode {0} have null operand", instr.OpCode.Name);

				switch (instr.OpCode.OperandType) {
				case OperandType.InlineNone :
					break;
				case OperandType.InlineSwitch :
					Instruction [] targets = instr.Operand as Instruction [];
					for (int i = 0; i < targets.Length + 1; i++)
						m_codeWriter.Write ((uint) 0);
					break;
				case OperandType.ShortInlineBrTarget :
					m_codeWriter.Write ((byte) 0);
					break;
				case OperandType.InlineBrTarget :
					m_codeWriter.Write (0);
					break;
				case OperandType.ShortInlineI :
					if (instr.OpCode.Equals (OpCodes.Ldc_I4_S))
						m_codeWriter.Write ((sbyte) instr.Operand);
					else
						m_codeWriter.Write ((byte) instr.Operand);
					break;
				case OperandType.ShortInlineVar :
					m_codeWriter.Write ((byte) body.Variables.IndexOf (
						(VariableDefinition) instr.Operand));
					break;
				case OperandType.ShortInlineParam :
					m_codeWriter.Write ((byte) GetParameterIndex (body, (ParameterDefinition) instr.Operand));
					break;
				case OperandType.InlineSig :
				case OperandType.InlineI :
					m_codeWriter.Write ((int) instr.Operand);
					break;
				case OperandType.InlineVar :
					m_codeWriter.Write ((short) body.Variables.IndexOf (
						(VariableDefinition) instr.Operand));
					break;
				case OperandType.InlineParam :
					m_codeWriter.Write ((short) GetParameterIndex (
							body, (ParameterDefinition) instr.Operand));
					break;
				case OperandType.InlineI8 :
					m_codeWriter.Write ((long) instr.Operand);
					break;
				case OperandType.ShortInlineR :
					m_codeWriter.Write ((float) instr.Operand);
					break;
				case OperandType.InlineR :
					m_codeWriter.Write ((double) instr.Operand);
					break;
				case OperandType.InlineString :
					WriteToken (new MetadataToken (TokenType.String,
							m_reflectWriter.MetadataWriter.AddUserString (instr.Operand as string)));
					break;
				case OperandType.InlineField :
					if (instr.Operand is IFieldReference)
						WriteToken ((instr.Operand as IFieldReference).MetadataToken);
					else
						throw new ReflectionException ("Wrong operand for InlineField: {0}",
							instr.Operand.GetType ().FullName);
					break;
				case OperandType.InlineMethod :
					if (instr.Operand is IMethodReference)
						WriteToken ((instr.Operand as IMethodReference).MetadataToken);
					else
						throw new ReflectionException ("Wrong operand for InlineMethod: {0}",
							instr.Operand.GetType ().FullName);
					break;
				case OperandType.InlineType :
					if (instr.Operand is ITypeReference)
						WriteToken (m_reflectWriter.GetTypeDefOrRefToken (
								instr.Operand as ITypeReference));
					else
						throw new ReflectionException ("Wrong operand for InlineType: {0}",
							instr.Operand.GetType ().FullName);
					break;

				case OperandType.InlineTok :
					if (instr.Operand is ITypeReference)
						WriteToken (m_reflectWriter.GetTypeDefOrRefToken (
								instr.Operand as ITypeReference));
					else if (instr.Operand is IMetadataTokenProvider)
						WriteToken ((instr.Operand as IMetadataTokenProvider).MetadataToken);
					else
						throw new ReflectionException (
							string.Format ("Wrong operand for {0} OpCode: {1}",
								instr.OpCode.OperandType.ToString (),
								instr.Operand.GetType ().FullName));
					break;
				}
			}

			// patch branches
			long pos = m_codeWriter.BaseStream.Position;

			foreach (Instruction instr in instructions) {
				switch (instr.OpCode.OperandType) {
				case OperandType.InlineSwitch :
					m_codeWriter.BaseStream.Position = instr.Offset + instr.OpCode.Size;
					Instruction [] targets = instr.Operand as Instruction [];
					m_codeWriter.Write ((uint) targets.Length);
					foreach (Instruction tgt in targets)
						m_codeWriter.Write ((tgt.Offset - (instr.Offset +
							instr.OpCode.Size + (4 * (targets.Length + 1)))));
					break;
				case OperandType.ShortInlineBrTarget :
					m_codeWriter.BaseStream.Position = instr.Offset + instr.OpCode.Size;
					m_codeWriter.Write ((byte) ((instr.Operand as Instruction).Offset -
						(instr.Offset + instr.OpCode.Size + 1)));
					break;
				case OperandType.InlineBrTarget :
					m_codeWriter.BaseStream.Position = instr.Offset + instr.OpCode.Size;
					m_codeWriter.Write ((instr.Operand as Instruction).Offset -
						(instr.Offset + instr.OpCode.Size + 4));
					break;
				}
			}

			m_codeWriter.BaseStream.Position = pos;
		}

		bool IsFat (ExceptionHandlerCollection seh)
		{
			bool fat = false;
			for (int i = 0; i < seh.Count && !fat; i++) {
				IExceptionHandler eh = seh [i];
				fat = eh.TryEnd.Offset - eh.TryStart.Offset > 255;
			}

			return fat;
		}

		void WriteExceptionHandlerCollection (ExceptionHandlerCollection seh)
		{
			m_codeWriter.QuadAlign ();

			if (!IsFat (seh)) {
				m_codeWriter.Write ((byte) MethodDataSection.EHTable);
				m_codeWriter.Write ((byte) (seh.Count * 12 + 4));
				m_codeWriter.Write (new byte [2]);
				foreach (ExceptionHandler eh in seh) {
					m_codeWriter.Write ((ushort) eh.Type);
					m_codeWriter.Write ((ushort) eh.TryStart.Offset);
					m_codeWriter.Write ((byte) (eh.TryEnd.Offset - eh.TryStart.Offset));
					m_codeWriter.Write ((ushort) eh.HandlerStart.Offset);
					m_codeWriter.Write ((byte) (eh.HandlerEnd.Offset - eh.HandlerStart.Offset));
					WriteHandlerSpecific (eh);
				}
			} else {
				m_codeWriter.Write ((byte) (MethodDataSection.FatFormat | MethodDataSection.EHTable));
				WriteFatBlockSize (seh);
				foreach (ExceptionHandler eh in seh) {
					m_codeWriter.Write ((uint) eh.Type);
					m_codeWriter.Write ((uint) eh.TryStart.Offset);
					m_codeWriter.Write ((uint) (eh.TryEnd.Offset - eh.TryStart.Offset));
					m_codeWriter.Write ((uint) eh.HandlerStart.Offset);
					m_codeWriter.Write ((uint) (eh.HandlerEnd.Offset - eh.HandlerStart.Offset));
					WriteHandlerSpecific (eh);
				}
			}
		}

		void WriteFatBlockSize (ExceptionHandlerCollection seh)
		{
			int size = seh.Count * 24 + 4;
			m_codeWriter.Write ((byte) (size & 0xff));
			m_codeWriter.Write ((byte) ((size >> 8) & 0xff));
			m_codeWriter.Write ((byte) ((size >> 16) & 0xff));
		}

		void WriteHandlerSpecific (ExceptionHandler eh)
		{
			switch (eh.Type) {
			case ExceptionHandlerType.Catch :
				WriteToken (eh.CatchType.MetadataToken);
				break;
			case ExceptionHandlerType.Filter :
				m_codeWriter.Write ((uint) eh.FilterStart.Offset);
				break;
			default :
				m_codeWriter.Write (0);
				break;
			}
		}

		public override void VisitVariableDefinitionCollection (VariableDefinitionCollection variables)
		{
			MethodBody body = variables.Container;
			StandAloneSigTable sasTable = m_reflectWriter.MetadataTableWriter.GetStandAloneSigTable ();
			StandAloneSigRow sasRow = m_reflectWriter.MetadataRowWriter.CreateStandAloneSigRow (
				m_reflectWriter.SignatureWriter.AddLocalVarSig (
					GetLocalVarSig (variables)));

			sasTable.Rows.Add (sasRow);
			body.LocalVarToken = sasTable.Rows.Count;
		}

		public override void TerminateMethodBody (MethodBody body)
		{
			long pos = m_binaryWriter.BaseStream.Position;

			if (body.Variables.Count > 0 || body.ExceptionHandlers.Count > 0
				|| m_codeWriter.BaseStream.Length >= 64 || body.MaxStack > 8) {

				MethodHeader header = MethodHeader.FatFormat;
				if (body.InitLocals)
					header |= MethodHeader.InitLocals;
				if (body.ExceptionHandlers.Count > 0)
					header |= MethodHeader.MoreSects;

				m_binaryWriter.Write ((byte) header);
				m_binaryWriter.Write ((byte) 0x30); // (header size / 4) << 4
				m_binaryWriter.Write ((short) body.MaxStack);
				m_binaryWriter.Write ((int) m_codeWriter.BaseStream.Length);
				m_binaryWriter.Write (((int) TokenType.Signature | body.LocalVarToken));

				WriteExceptionHandlerCollection (body.ExceptionHandlers);

				m_binaryWriter.Write (m_codeWriter);
			} else {
				m_binaryWriter.Write ((byte) ((byte) MethodHeader.TinyFormat |
					m_codeWriter.BaseStream.Length << 2));
				m_binaryWriter.Write (m_codeWriter);
			}

			m_binaryWriter.QuadAlign ();

			m_reflectWriter.MetadataWriter.AddData (
				(int) (m_binaryWriter.BaseStream.Position - pos));
		}

		LocalVarSig GetLocalVarSig (VariableDefinitionCollection vars)
		{
			LocalVarSig lvs = new LocalVarSig ();
			lvs.CallingConvention |= 0x7;
			lvs.Count = vars.Count;
			lvs.LocalVariables = new LocalVarSig.LocalVariable [lvs.Count];
			for (int i = 0; i < lvs.Count; i++) {
				LocalVarSig.LocalVariable lv = new LocalVarSig.LocalVariable ();
				ITypeReference type = vars [i].Variable;
				if (type is PinnedType) {
					lv.Constraint |= Constraint.Pinned;
					type = (type as PinnedType).ElementType;
				}

				if (type is IReferenceType) {
					lv.ByRef = true;
					type = (type as IReferenceType).ElementType;
				}

				lv.Type = m_reflectWriter.GetSigType (type);

				lvs.LocalVariables [i] = lv;
			}
			return lvs;
		}
	}
}