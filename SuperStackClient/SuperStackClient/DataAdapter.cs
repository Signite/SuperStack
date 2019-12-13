using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace StackCC
{
    public static class DataAdapterMSSQL
    {
        private static string connectionString = "";
        private static SqlConnection connection = null;

        public static string ErrorMessage { get; private set; } = "";

        static DataAdapterMSSQL()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = "orn-stack",
                InitialCatalog = "stack",
                IntegratedSecurity = false
            };
            connectionString = sqlConnectionStringBuilder.ConnectionString;
        }

        public static bool Login(string user, string password)
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(connectionString)
            {
                UserID = user,
                Password = password
            };
            connectionString = sb.ConnectionString;
            return TestConnection();
        }

        static private bool TestConnection()
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            try
            {
                sqlConnection.Open();
                sqlConnection.Close();
                return true;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return false;
            }
        }

        static private SqlConnection GetConnection() => connection ?? new SqlConnection(connectionString);

        #region Qyerys
        private static DataSet DoScalarCommand(SqlCommand command)
        {
            DataSet result = new DataSet();
            SqlDataAdapter filler = new SqlDataAdapter(command);
            try
            {
                filler.Fill(result);
                return result;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return null;
            }
        }

        private static bool DoCommand(SqlCommand command)
        {
            try
            {
                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Connection.Close();
                return true;
            }
            catch (Exception e)
            {
                command.Connection.Close();
                ErrorMessage = e.Message;
                return false;
            }
        }

        public static DataSet FindPerson(Person person)
        {
            SqlCommand sqlCommand = new SqlCommand("", GetConnection())
            {
                CommandText = "FindPerson",
                CommandType = CommandType.StoredProcedure
            };
            sqlCommand.Parameters.Add("@PersonalAccount", SqlDbType.NVarChar).Value = person.PersonalAccount;
            sqlCommand.Parameters.Add("@FIO", SqlDbType.NVarChar).Value = person.FIO;
            sqlCommand.Parameters.Add("@City", SqlDbType.NVarChar).Value = person.City;
            sqlCommand.Parameters.Add("@Street", SqlDbType.NVarChar).Value = person.Street;
            sqlCommand.Parameters.Add("@House", SqlDbType.NVarChar).Value = person.House;
            sqlCommand.Parameters.Add("@Flat", SqlDbType.NVarChar).Value = person.Flat;

            return DoScalarCommand(sqlCommand);
        }

        #region Services
        public static DataSet GetServices()
        {
            SqlCommand sqlCommand = new SqlCommand("", GetConnection())
            {
                CommandText = "GetServices",
                CommandType = CommandType.StoredProcedure
            };

            return DoScalarCommand(sqlCommand);
        }
        #endregion

        #region Regions
        public static DataSet GetRegions()
        {
            SqlCommand sqlCommand = new SqlCommand("", GetConnection())
            {
                CommandText = "GetRegions",
                CommandType = CommandType.StoredProcedure
            };

            return DoScalarCommand(sqlCommand);
        }
        #endregion

        public static bool SavePayment(Payment payment)
        {
            SqlCommand sqlCommand = new SqlCommand("", GetConnection())
            {
                CommandText = "AddPayment",
                CommandType = CommandType.StoredProcedure
            };
            sqlCommand.Parameters.Add("@Kassir", SqlDbType.VarChar).Value = payment.kassir;
            sqlCommand.Parameters.Add("@LSID", SqlDbType.Int).Value = payment.person.ID;
            sqlCommand.Parameters.Add("@ServiceID", SqlDbType.Int).Value = payment.service;
            sqlCommand.Parameters.Add("@Kassa", SqlDbType.Int).Value = payment.locationID;
            //sqlCommand.Parameters.Add("@SupplierName", SqlDbType.VarChar).Value = payment.
            sqlCommand.Parameters.Add("@Comment", SqlDbType.VarChar).Value = payment.comment;
            sqlCommand.Parameters.Add("@PlannedDate", SqlDbType.Date).Value = payment.plannedDate;
            sqlCommand.Parameters.Add("@Phone", SqlDbType.VarChar).Value = payment.phone;

            return DoCommand(sqlCommand);
        }

        /// <summary>
        /// Позволяет оперативно запретить работу программы
        /// </summary>
        /// <returns></returns>
        public static bool CheckEnabledCC()
        {
            SqlCommand sqlCommand = new SqlCommand("", GetConnection())
            {
                CommandText = "CheckEnadledCC",
                CommandType = CommandType.StoredProcedure
            };
            bool result = false;
            try
            {
                sqlCommand.Connection.Open();
                result = (int)sqlCommand.ExecuteScalar() == 1 ? true : false;
                sqlCommand.Connection.Close();
            }
            catch (Exception e)
            {
                sqlCommand.Connection.Close();
                ErrorMessage = e.Message;
                return false;
            }
            return result;
        }

        #region User functions

        /// <summary>
        /// Получаем роли пользователя в БД
        /// </summary>
        /// <returns></returns>
        public static DataSet GetUserRoles()
        {
            SqlCommand sqlCommand = new SqlCommand("", GetConnection())
            {
                CommandText = "GetUserRoles",
                CommandType = CommandType.StoredProcedure
            };

            return DoScalarCommand(sqlCommand);
        }

        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static bool AddUser(string UserName)
        {
            //if (UserExist(UserName))
            //{
            //    ErrorMessage = String.Format("A user with this name({0}) already exists.",UserName);
            //    return false;
            //}

            SqlCommand sqlCommand = new SqlCommand("", GetConnection())
            {
                CommandText = "AddUser",
                CommandType = CommandType.StoredProcedure
            };
            sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = UserName;
            return DoCommand(sqlCommand);
        }

        private static bool UserExist(string UserName)
        {
            SqlCommand sqlCommand = new SqlCommand("", GetConnection())
            {
                CommandText = "UserExist",
                CommandType = CommandType.StoredProcedure
            };
            sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = UserName;
            return DoCommand(sqlCommand);
        }

        #endregion

        #endregion        
    }
}
