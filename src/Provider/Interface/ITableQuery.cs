using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Interface
{
	public interface ITableQuery<TTable>
	{
		ITableQuery<TTable> Where(Expression<Func<TTable, bool>> predicate);
		ITableQuery<TTable> WhereAnd(Expression<Func<TTable, bool>> predicate);
		ITableQuery<TTable> WhereOr(Expression<Func<TTable, bool>> predicate);

		ITableQuery<TTable> WhereIn<TKey>(Expression<Func<TTable, Tuple<TKey, TKey[]>>> predicate);
		ITableQuery<TTable> WhereInAnd<TKey>(Expression<Func<TTable, Tuple<TKey, TKey[]>>> predicate);
		ITableQuery<TTable> WhereInOr<TKey>(Expression<Func<TTable, Tuple<TKey, TKey[]>>> predicate);

		ITableQuery<TTable> OrderBy<TKey>(Expression<Func<TTable, TKey>> predicate);
		ITableQuery<TTable> OrderByDescending<TKey>(Expression<Func<TTable, TKey>> predicate);

		ITableQuery<TTable> Take();
		ITableQuery<TTable> Take<TKey>(Expression<Func<TTable, TKey>> predicate);
		ITableQuery<TTable> Skip();
		ITableQuery<TTable> Skip<TKey>(Expression<Func<TTable, TKey>> predicate);

		List<TTable> Select(int count = -1);
	}
}
