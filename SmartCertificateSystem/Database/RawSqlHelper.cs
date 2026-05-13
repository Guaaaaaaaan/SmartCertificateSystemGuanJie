using System.Data;
using Microsoft.EntityFrameworkCore;
using SmartCertificateSystem.Models;

namespace SmartCertificateSystem.Database;

public class RawSqlHelper(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<int> CountUsersByRoleAsync(string role)
    {
        var connection = _db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Role = $role";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "$role";
            parameter.Value = role;
            command.Parameters.Add(parameter);

            var value = await command.ExecuteScalarAsync();
            return Convert.ToInt32(value);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    public async Task<string?> FindCertificateStatusByIdAsync(string certificateId)
    {
        var connection = _db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Status FROM Certificates WHERE CertificateId = $certificateId";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "$certificateId";
            parameter.Value = certificateId;
            command.Parameters.Add(parameter);

            return (await command.ExecuteScalarAsync())?.ToString();
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
