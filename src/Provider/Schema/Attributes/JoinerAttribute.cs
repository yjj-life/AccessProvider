using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Schema.Attributes
{
	public enum JoinType
	{
		Inner,
		Left,
		Right,
		Full
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class JoinerAttribute : Attribute
	{
		public JoinerAttribute()
		{
			this.Table = "";
			this.Column = "";
			this.JoinType = JoinType.Inner;
			this.JoinOrder = 0;
		}

		public string Table { get; set; }
		public string Column { get; set; }
		public JoinType JoinType { get; set; }
		public int JoinOrder { get; set; }
	}
}
