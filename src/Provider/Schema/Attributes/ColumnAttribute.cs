using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Schema.Attributes
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

	[AttributeUsage(AttributeTargets.Property)]
	public class ColumnAttribute : Attribute
	{
		public ColumnAttribute()
		{
			this.Table = "";
			this.Column = "";
			this.Type = ColumnType.Normal;
			this.Length = 0;
			this.Identity = false;
		}

		public string Table { get; set; }

		public string Column { get; set; }

		public ColumnType Type { get; set; }

		public int Length { get; set; }

		public bool Identity { get; set; }
	}
}
