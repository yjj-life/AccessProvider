using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Interface
{
	public interface IQueryJoinable<TTable>
	{
		IQueryJoinable<TTable> Join<TPrimary, TSecondary>(Expression<Func<TPrimary, TSecondary, bool>> predicate);
		IQueryJoinable<TTable> LeftJoin<TPrimary, TSecondary>(Expression<Func<TPrimary, TSecondary, bool>> predicate);
		IQueryJoinable<TTable> RightJoin<TPrimary, TSecondary>(Expression<Func<TPrimary, TSecondary, bool>> predicate);

		IQueryJoinable<TTable> Where<TJoiner>(Expression<Func<TJoiner, bool>> predicate);
		IQueryJoinable<TTable> WhereAnd<TJoiner>(Expression<Func<TJoiner, bool>> predicate);
		IQueryJoinable<TTable> WhereOr<TJoiner>(Expression<Func<TJoiner, bool>> predicate);

		IQueryJoinable<TTable> Where<TJoiner1, TJoiner2>(Expression<Func<TJoiner1, TJoiner2, bool>> predicate);
		IQueryJoinable<TTable> WhereAnd<TJoiner1, TJoiner2>(Expression<Func<TJoiner1, TJoiner2, bool>> predicate);
		IQueryJoinable<TTable> WhereOr<TJoiner1, TJoiner2>(Expression<Func<TJoiner1, TJoiner2, bool>> predicate);

		IQueryJoinable<TTable> Where<TJoiner1, TJoiner2, TJoiner3>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, bool>> predicate);
		IQueryJoinable<TTable> WhereAnd<TJoiner1, TJoiner2, TJoiner3>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, bool>> predicate);
		IQueryJoinable<TTable> WhereOr<TJoiner1, TJoiner2, TJoiner3>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, bool>> predicate);

		IQueryJoinable<TTable> Where<TJoiner1, TJoiner2, TJoiner3, TJoiner4>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, bool>> predicate);
		IQueryJoinable<TTable> WhereAnd<TJoiner1, TJoiner2, TJoiner3, TJoiner4>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, bool>> predicate);
		IQueryJoinable<TTable> WhereOr<TJoiner1, TJoiner2, TJoiner3, TJoiner4>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, bool>> predicate);

		IQueryJoinable<TTable> Where<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5, bool>> predicate);
		IQueryJoinable<TTable> WhereAnd<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5, bool>> predicate);
		IQueryJoinable<TTable> WhereOr<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5, bool>> predicate);

		IQueryJoinable<TTable> WhereIn<TJoiner, TKey>(Expression<Func<TJoiner, Tuple<TKey, TKey[]>>> predicate);
		IQueryJoinable<TTable> WhereInAnd<TJoiner, TKey>(Expression<Func<TJoiner, Tuple<TKey, TKey[]>>> predicate);
		IQueryJoinable<TTable> WhereInOr<TJoiner, TKey>(Expression<Func<TJoiner, Tuple<TKey, TKey[]>>> predicate);

		IQueryJoinable<TTable> OrderBy<TJoiner, TKey>(Expression<Func<TJoiner, TKey>> predicate);
		IQueryJoinable<TTable> OrderByDescending<TJoiner, TKey>(Expression<Func<TJoiner, TKey>> predicate);

		IQueryJoinable<TTable> Take<TJoiner, TKey>(Expression<Func<TJoiner, TKey>> predicate);
		IQueryJoinable<TTable> Skip<TJoiner, TKey>(Expression<Func<TJoiner, TKey>> predicate);
	}
}
