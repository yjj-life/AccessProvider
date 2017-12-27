using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Schema.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TableAttribute : Attribute
	{
		private string _primaryTableName;
		public TableAttribute(string primaryTableName)
		{
			_primaryTableName = primaryTableName;
		}

		public string PrimaryTableName
		{
			get { return _primaryTableName; }
		}
	}
}
