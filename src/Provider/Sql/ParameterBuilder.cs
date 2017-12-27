using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Sql
{
	internal class Parameter
	{
		public string Name { get; set; }
		public Object Value { get; set; }
	}

	internal class ParameterBuilder
	{
		private static int s_iParameter = -1;

		public string GenerateParameterName()
		{
			return string.Format("@value{0}", ++s_iParameter);
		}


	}
}
