
using Providers.Domain.Entities;
using Providers.Domain.Interfaces; 
using Providers.Infrastructure.Persistences;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Providers.Infrastructure.Repository
{
    public class ProviderRepository : IRepository<Provider> 
    {
        private readonly DatabaseConnection _db;

        public ProviderRepository()
        {
            _db = DatabaseConnection.Instance;
        }

        private Provider MapProvider(DbDataReader reader)
        {
            return new Provider(
                id: reader.GetInt32("id"),
                first_name: reader.GetString("first_name"),
                last_name: reader.GetString("last_name"),
                email: reader.IsDBNull("email") ? null : reader.GetString("email"),
                phone: reader.IsDBNull("phone") ? string.Empty : reader.GetString("phone"),
                is_deleted: reader.GetBoolean("is_deleted")
            );
        }

        public async Task<Provider> Create(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = @"
                INSERT INTO providers (first_name, last_name, phone, email, created_by, updated_by)
                VALUES (@first_name, @last_name, @phone, @email, @created_by, @updated_by);
            ";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@first_name", entity.first_name);
            cmd.Parameters.AddWithValue("@last_name", entity.last_name);
            cmd.Parameters.AddWithValue("@email", (object?)entity.email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", entity.phone);
            cmd.Parameters.AddWithValue("@created_by", entity.created_by); 
            cmd.Parameters.AddWithValue("@updated_by", entity.updated_by); 

            await cmd.ExecuteNonQueryAsync();
            entity.id = (int)cmd.LastInsertedId;
            return entity;
        }


        public async Task<Provider?> GetById(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();
            const string query = "SELECT id, first_name, last_name, email, phone, is_deleted FROM providers WHERE id = @id AND is_deleted = FALSE;";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", entity.id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapProvider(reader);
            }
            return null;
        }

        public async Task<IEnumerable<Provider>> GetAll()
        {
            var list = new List<Provider>();
            using var connection = _db.GetConnection();
            await connection.OpenAsync();
            const string query = "SELECT id, first_name, last_name, email, phone, is_deleted FROM providers WHERE is_deleted = FALSE ORDER BY last_name ASC, first_name ASC;";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapProvider(reader));
            }
            return list;
        }


        public async Task Update(Provider entity)
        {
            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            const string query = @"
                UPDATE providers 
                SET first_name = @first_name,
                    last_name  = @last_name,
                    email      = @email,
                    phone      = @phone,
                    updated_by = @updated_by
                WHERE id = @id;
            ";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@first_name", entity.first_name);
            cmd.Parameters.AddWithValue("@last_name", entity.last_name);
            cmd.Parameters.AddWithValue("@email", (object?)entity.email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", entity.phone);
            cmd.Parameters.AddWithValue("@updated_by", entity.updated_by); 
            cmd.Parameters.AddWithValue("@id", entity.id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(Provider entity)
        {
            string query = "UPDATE providers SET is_deleted = TRUE, updated_by = @updated_by WHERE id=@id";

            using var connection = _db.GetConnection();
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@updated_by", entity.updated_by); 
            cmd.Parameters.AddWithValue("@id", entity.id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}