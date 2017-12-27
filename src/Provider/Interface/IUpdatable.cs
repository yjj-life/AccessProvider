using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Interface
{
	public interface IUpdatable<TTable> : ITableQuery<TTable>
	{
		bool Delete();
		bool Delete(TTable record);
		bool Delete(List<TTable> records);

		bool AddNew(TTable record);
		bool AddNew(Func<TTable> record);
		bool AddNew(List<TTable> records);

		bool Update(TTable record);
		bool Update(Func<TTable> record);
		bool Update(List<TTable> records);
	}
}
