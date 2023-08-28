using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace first.DAL.DAO
{
    internal class ProductGroupDao
    {
        private readonly SqlConnection _connection;

        public ProductGroupDao(SqlConnection connection)
        {
            _connection = connection;
        }

        public void CreateTable()
        {
            using SqlCommand command = new() { Connection = _connection };
            command.CommandText = @"CREATE TABLE ProductGroups (
                                    Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                                    Name        NVARCHAR(50)     NOT NULL,
                                    Description NTEXT            NOT NULL,
                                    Picture     NVARCHAR(50)     NULL
                                )";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Получения всех неудалённых товарных групп
        /// </summary>
        /// <returns>Группы ввиде коллекции</returns>
        public List<Entity.ProductGroup> GetAll()
        {
            using SqlCommand command = new() { Connection = _connection };
            command.CommandText = @"SELECT pg.* FROM ProductGroups AS pg WHERE DeleteDt IS NULL";
            try
            {
                using SqlDataReader reader = command.ExecuteReader();
                var ProductGroups = new List<Entity.ProductGroup>();
                while (reader.Read())
                {
                    ProductGroups.Add(new()
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        Picture = reader.GetString(3),
                    });
                }
                return ProductGroups;
            }
            catch { throw; }
        }

        public void Add(Entity.ProductGroup productGroup)
        {
            using SqlCommand command = new() { Connection = _connection };
            command.CommandText = @"INSERT INTO ProductGroups (Id, Name, Description, Picture)
                                    VALUES (@id, @name, @description, @picture)";
            // подготовка запроса -- создание временной хранимой процедуры в СУБД
            command.Prepare();
            // задаём типы и ограничения параметров
            command.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier));
            command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 50));
            command.Parameters.Add(new SqlParameter("@description", SqlDbType.NText));
            command.Parameters.Add(new SqlParameter("@picture", SqlDbType.NVarChar, 50));
            // задаём значение параметров
            command.Parameters[0].Value = productGroup.Id;
            command.Parameters[1].Value = productGroup.Name;
            command.Parameters[2].Value = productGroup.Description;
            command.Parameters[3].Value = productGroup.Picture;
            // выполняем запрос
            command.ExecuteNonQuery();
        }

        public void Delete(Entity.ProductGroup productGroup)
        {
            using SqlCommand command = new() { Connection = _connection };
            command.CommandText = @$"UPDATE ProductGroups
                                     SET DeleteDt = @datetime WHERE Id = '{productGroup.Id}'";
            command.Prepare();
            command.Parameters.Add(new SqlParameter("@datetime", SqlDbType.DateTime));
            command.Parameters[0].Value = DateTime.Now;

            command.ExecuteNonQuery();
        }

        public void Update(Entity.ProductGroup productGroup)
        {
            using SqlCommand command = new() { Connection = _connection };
            command.CommandText = @$"UPDATE ProductGroups
                                     SET Name = @name, Description = @description, Picture = @picture WHERE Id = '{productGroup.Id}'";
            command.Prepare();

            command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 50));
            command.Parameters.Add(new SqlParameter("@description", SqlDbType.NText));
            command.Parameters.Add(new SqlParameter("@picture", SqlDbType.NVarChar, 50));

            command.Parameters[0].Value = productGroup.Name;
            command.Parameters[1].Value = productGroup.Description;
            command.Parameters[2].Value = productGroup.Picture;

            command.ExecuteNonQuery();
        }

        public int GetAllCount()
        {
            using SqlCommand command = new() { Connection = _connection };
            command.CommandText = "SELECT COUNT(*) FROM ProductGroups WHERE Deletedt IS NULL";
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
