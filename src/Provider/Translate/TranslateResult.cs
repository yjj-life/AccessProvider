using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Provider.Sql;

namespace Provider.Translate
{
	internal class TranslateResult
	{
		internal string CommandText;
		internal List<Parameter> Parameters;
	}
}
