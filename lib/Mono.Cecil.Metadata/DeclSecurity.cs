/*
 * Copyright (c) 2004, 2005 DotNetGuru and the individuals listed
 * on the ChangeLog entries.
 *
 * Authors :
 *   Jb Evain   (jbevain@gmail.com)
 *
 * This is a free software distributed under a MIT/X11 license
 * See LICENSE.MIT file for more details
 *
 * Generated by /CodeGen/cecil-gen.rb do not edit
 * Tue Jul 05 15:48:30 Paris, Madrid 2005
 *
 *****************************************************************************/

namespace Mono.Cecil.Metadata {

	using Mono.Cecil;

	[RId (0x0e)]
	public sealed class DeclSecurityTable : IMetadataTable {

		private RowCollection m_rows;

		public DeclSecurityRow this [int index] {
			get { return m_rows [index] as DeclSecurityRow; }
			set { m_rows [index] = value; }
		}

		public RowCollection Rows {
			get { return m_rows; }
			set { m_rows = value; }
		}

		internal DeclSecurityTable ()
		{
		}

		public void Accept (IMetadataTableVisitor visitor)
		{
			visitor.Visit (this);
			this.Rows.Accept (visitor.GetRowVisitor ());
		}
	}

	public sealed class DeclSecurityRow : IMetadataRow {

		public static readonly int RowSize = 10;
		public static readonly int RowColumns = 3;

		public SecurityAction Action;
		public MetadataToken Parent;
		public uint PermissionSet;

		internal DeclSecurityRow ()
		{
		}

		public void Accept (IMetadataRowVisitor visitor)
		{
			visitor.Visit (this);
		}
	}
}
