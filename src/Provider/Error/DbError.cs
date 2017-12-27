using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Error
{
	enum ErrorCode
	{
		Success,
		InvalidOperation,
		DatabaseException,
		Conflict
	}

	class DbError
	{
		public ErrorCode Code { get; set; }
		public string Text { get; set; }
	}
}
