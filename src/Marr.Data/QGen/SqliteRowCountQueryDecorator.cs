using System.Text;

namespace Marr.Data.QGen
{
    public class SqliteRowCountQueryDecorator : IQuery
    {
        private SelectQuery _innerQuery;

        public SqliteRowCountQueryDecorator(SelectQuery innerQuery)
        {
            _innerQuery = innerQuery;
        }

        public string Generate()
        {
            StringBuilder sql = new StringBuilder();

            BuildSelectCountClause(sql);

            if (_innerQuery.IsJoin)
            {
                sql.Append(" FROM (");
                _innerQuery.BuildSelectClause(sql);
                _innerQuery.BuildFromClause(sql);
                _innerQuery.BuildJoinClauses(sql);
                _innerQuery.BuildWhereClause(sql);
                _innerQuery.BuildGroupBy(sql);
                sql.Append(") ");

                return sql.ToString();
            }

            _innerQuery.BuildFromClause(sql);
            _innerQuery.BuildJoinClauses(sql);
            _innerQuery.BuildWhereClause(sql);

            return sql.ToString();
        }

        private void BuildSelectCountClause(StringBuilder sql)
        {
            sql.AppendLine("SELECT COUNT(*)");
        }
    }
}
