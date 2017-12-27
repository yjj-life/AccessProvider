using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Schema
{
	public enum ColumnType
	{
		Normal,                 // 通常の項目
		PrimaryKey,             // プライマリキー
		UpdCount,               // シーケンシャルナンバー
		LogYMD,                 // 更新日付
		LogClient,              // 更新クライアント名称
		LogEmpCode,             // 更新オペレータコード
		ModifyFlag              // 更新フラグ
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class PrimaryTableAttribute : Attribute
	{
		private string _primaryTableName;
		public PrimaryTableAttribute(string primaryTableName)
		{
			_primaryTableName = primaryTableName;
		}

		public string PrimaryTableName
		{
			get { return _primaryTableName; }
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class ColumnAttribute : Attribute
	{
		private string _Table;
		private string _Column;
		private ColumnType _Type;
		private int _Length;
		private bool _Identity;

		public ColumnAttribute(string Table = "", string Column = "", ColumnType Type = ColumnType.Normal, int Length = 0, bool Identity = false)
		{
			_Table = Table;
			_Column = Column;
			_Type = Type;
			_Length = Length;
			_Identity = Identity;
		}

		public string Table
		{
			get { return _Table; }
		}

		public string Column
		{
			get { return _Column; }
		}

		public ColumnType Type
		{
			get { return _Type; }
		}

		public int Length
		{
			get { return _Length; }
		}

		public bool Identity
		{
			get
			{
				return _Identity;
			}
		}
	}
}
