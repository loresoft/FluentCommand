using System.Data;

using Dapper;

namespace FluentCommand;

public class ConcurrencyTokenTypeHandler : SqlMapper.TypeHandler<ConcurrencyToken>
{
    public override ConcurrencyToken Parse(object value)
    {
        return value switch
        {
            string textToken => new ConcurrencyToken(textToken),
            byte[] byteToken => new ConcurrencyToken(byteToken),
            _ => ConcurrencyToken.None
        };
    }

    public override void SetValue(IDbDataParameter parameter, ConcurrencyToken value)
    {
        parameter.Value = value.Value;
        parameter.DbType = DbType.Binary;
    }
}
