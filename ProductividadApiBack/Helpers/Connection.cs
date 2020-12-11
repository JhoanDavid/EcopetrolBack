
using Microsoft.Extensions.Configuration;
using Productividad.Models;
using ProductividadApiBack.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Productividad.Helpers
{
    public class Connection
    {
        private readonly IConfiguration _config;
        public Connection()
        {
            _config = Config.configuration;
            try
            {
                string strServer = _config["DBServer"];
                string strDBName = _config["DBName"];
                string strSQLUser = _config["SQLUser"];
                string strSQLPass = _config["SQLPass"];

                SqlConnectionStringBuilder bldr = new SqlConnectionStringBuilder(strConnection)
                {
                    IntegratedSecurity = false,
                    DataSource = strServer,
                    InitialCatalog = strDBName,
                    UserID = strSQLUser,
                    Password = strSQLPass
                };

                strConnection = bldr.ConnectionString;
                blError = false;
            }
            catch (Exception)
            {
                blError = true;
            }
        }

        public ResponseDB InsData(string strQuery, string[] arrayParam, string[] arrayValue, string strIp)
        {
            ResponseDB objResponseDB = new ResponseDB
            {
                Resp = true
            };
            try
            {
                SqlConnection objSQLConnection = new SqlConnection(strConnection);
                try
                {
                    objSQLConnection.Open();
                    if (objSQLConnection.State != System.Data.ConnectionState.Open)
                    {
                        objResponseDB.Resp = false;
                        objResponseDB.Msg = "The connection status is: " + objSQLConnection.State.ToString();
                        objSQLConnection.Close();
                    }
                    else { 
                        //Registro de la propuesta
                        try
                        {
                            objResponseDB.Count = 0;
                            objResponseDB.Error = 0;
                            try
                            {
                                SqlCommand cmd = new SqlCommand(strQuery, objSQLConnection);
                                cmd.CommandText = strQuery;
                                for (int j = 0; j < arrayParam.Length; j++)
                                {
                                    cmd.Parameters.Add(new SqlParameter(arrayParam[j], arrayValue[j]));
                                }
                                
                                var idInsert=cmd.ExecuteScalar();

                                if (idInsert == null)
                                {
                                    objResponseDB.Error++;
                                }
                                else
                                {   
                                    #region Registro del Log
                                    try { 
                                    objResponseDB.Count++;
                                   
                                    var objTest = CreateLog(cmd, objSQLConnection, idInsert.ToString(), "Register", strIp);
                                    
                                    if (objTest< 1)
                                    {
                                        objResponseDB.Error++;
                                    }
                                    else
                                    {
                                        objResponseDB.Count++;
                                    }

                                    }
                                    catch (Exception innerEx)
                                    {
                                        objResponseDB.Msg = innerEx.Message;
                                        objResponseDB.Error++;
                                        Console.WriteLine(innerEx.Message);
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception innerEx)
                            {
                                objResponseDB.Msg = innerEx.Message;
                                objResponseDB.Error++;
                                Console.WriteLine(innerEx.Message);
                            }
                            //Fin Registro de la propuesta
                          
                            
                            objSQLConnection.Close();

                        }
                        catch (SqlException ex)
                        {
                            // 5. Close the connection
                            if (objSQLConnection != null)
                            {
                                objResponseDB.Resp = false;
                                objResponseDB.Msg = ex.Message;
                                objSQLConnection.Close();
                            }
                        }

                    }

                }
                // Catch errors specific to the Open method
                catch (SqlException ex)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Error opening the connection:: " + ex.Message;
                }
                catch (InvalidOperationException ix)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Invalid Operation error: " + ix.Message;
                }
                catch (ConfigurationErrorsException cx)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Configuration error: " + cx.Message;
                }

            }
            catch (ArgumentException ax)  // there was something wrong in the connection string.
            {
                objResponseDB.Resp = false;
                objResponseDB.Msg = "Error creating the connection: " + ax.Message;
            }

            return objResponseDB;
        }

        public ResponseDB UpdData(string strQuery, string[] arrayParam, string[] arrayValue, string strIp, string strType)
        {
            ResponseDB objResponseDB = new ResponseDB
            {
                Resp = true
            };

            SqlConnection objSQLConnection;

            try
            {
                objSQLConnection = new SqlConnection(strConnection);

                try
                {
                    objSQLConnection.Open();

                    if (objSQLConnection.State != System.Data.ConnectionState.Open)
                    {
                        objResponseDB.Resp = false;
                        objResponseDB.Msg = "The connection status is: " + objSQLConnection.State.ToString();

                        objSQLConnection.Close();
                    }
                    else
                    {

                        try
                        {

                            objResponseDB.Count = 0;
                            objResponseDB.Error = 0;

                            try
                            {

                                SqlCommand cmd = new SqlCommand(strQuery, objSQLConnection);

                                cmd.CommandText = strQuery;

                                for (int j = 0; j < arrayParam.Length; j++)
                                {
                                    cmd.Parameters.Add(new SqlParameter(arrayParam[j], arrayValue[j]));
                                }

                                var idUpdate = cmd.ExecuteScalar();

                                if (idUpdate == null)
                                {
                                    objResponseDB.Error++;
                                }
                                else
                                {   
                                    objResponseDB.Count += 1;
                                    var objTest = CreateLog(cmd, objSQLConnection, idUpdate.ToString(), strType, strIp);
                                    if (objTest < 1)
                                    {
                                        objResponseDB.Error++;
                                    }
                                    else
                                    {
                                        objResponseDB.Count++;
                                    }

                                }

                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine(innerEx.Message);
                                objResponseDB.Msg = innerEx.Message;
                                objResponseDB.Error++;
                            }


                            objSQLConnection.Close();

                        }
                        catch (SqlException ex)
                        {
                            // 5. Close the connection
                            if (objSQLConnection != null)
                            {
                                objResponseDB.Resp = false;
                                objResponseDB.Msg = ex.Message;
                                objSQLConnection.Close();
                            }
                        }

                    }

                }
                // Catch errors specific to the Open method
                catch (SqlException ex)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Error opening the connection:: " + ex.Message;
                }
                catch (InvalidOperationException ix)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Invalid Operation error: " + ix.Message;
                }
                catch (ConfigurationErrorsException cx)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Configuration error: " + cx.Message;
                }

            }
            catch (ArgumentException ax)  // there was something wrong in the connection string.
            {
                objResponseDB.Resp = false;
                objResponseDB.Msg = "Error creating the connection: " + ax.Message;
            }

            return objResponseDB;
        }

        public ResponseDB GetRespFromQuery(int nuIni, int nuEnd, string strQueryCols, string strTable, string strCondition, string[] arrayParam, string[] arrayValue, string strOrderByCol, string strRespType,string strIp)
        {
            ResponseDB objResponseDB = new ResponseDB
            {
                Resp = true
            };

            SqlConnection objSQLConnection;

            try
            {
                objSQLConnection = new SqlConnection(strConnection);

                try
                {
                    objSQLConnection.Open();

                    if (objSQLConnection.State != System.Data.ConnectionState.Open)
                    {
                        objResponseDB.Resp = false;
                        objResponseDB.Msg = "The connection status is: " + objSQLConnection.State.ToString();
                        
                        objSQLConnection.Close();
                    }
                    else
                    {
                        SqlDataReader rdr = null;

                        try
                        {
                            bool blCount = true;
                            
                            string strQuery = "";

                            if (!strOrderByCol.Equals("NO_PAGINATE"))
                            {
                                #region QueryCount

                                string strCountQuery = "SELECT COUNT(*) AS ROWS FROM " + strTable + " WHERE "+ strCondition;
                                SqlCommand cmdCount = new SqlCommand(strCountQuery, objSQLConnection);

                                DataSet dsInfoCount = new DataSet();
                                SqlDataAdapter adapterCount = new SqlDataAdapter();

                                cmdCount.CommandText = strCountQuery;

                                if (arrayParam != null)
                                {

                                    for (int i = 0; i < arrayParam.Length; i++)
                                    {
                                        SqlParameter objParam = new SqlParameter();
                                        objParam.ParameterName = arrayParam[i];
                                        objParam.Value = arrayValue[i];
                                        cmdCount.Parameters.Add(objParam);
                                    }
                                }

                                adapterCount.SelectCommand = cmdCount;
                                adapterCount.Fill(dsInfoCount);

                                if (dsInfoCount.Tables.Count > 0 && dsInfoCount.Tables[0].Rows.Count > 0)
                                {
                                    objResponseDB.Count = int.Parse(dsInfoCount.Tables[0].Rows[0][0].ToString());
                                }
                                else
                                {
                                    blCount = false;
                                }

                                #endregion

                                strQuery = SQLUtil.GetPaginatedSQL(nuIni, nuEnd, "SELECT " + strQueryCols + "  FROM " + strTable + " WHERE " + strCondition, "ORDER BY " + strOrderByCol);
                            }
                            else
                            {
                                strQuery = "SELECT " + strQueryCols + "  FROM " + strTable + " WHERE " + strCondition;
                            }

                            if (blCount)
                            {

                                #region QueryData
                                SqlCommand cmd = new SqlCommand(strQuery, objSQLConnection);

                                DataSet dsInfo = new DataSet();
                                SqlDataAdapter adapter = new SqlDataAdapter();

                                cmd.CommandText = strQuery;

                                if (arrayParam != null)
                                {

                                    for (int i = 0; i < arrayParam.Length; i++)
                                    {
                                        SqlParameter objParam = new SqlParameter();
                                        objParam.ParameterName = arrayParam[i];
                                        objParam.Value = arrayValue[i];
                                        cmd.Parameters.Add(objParam);
                                    }
                                }

                                adapter.SelectCommand = cmd;
                                adapter.Fill(dsInfo);

                                if (dsInfo.Tables.Count > 0 && dsInfo.Tables[0].Rows.Count > 0)
                                {
                                    switch (strRespType)
                                    {
                                        case "DataTable":
                                            objResponseDB.dtResult = dsInfo.Tables[0];
                                            break;
                                    }

                                    if (strOrderByCol.Equals("NO_PAGINATE"))
                                    {
                                        objResponseDB.Count = dsInfo.Tables[0].Rows.Count;
                                    }

                                    #region Registro del Log
                                    try
                                    {
                                        objResponseDB.Count++;
                                        var UserId=objResponseDB.dtResult.Rows[0]["Id"];

                                        var objTest = 0;

                                        if (strCondition== "DocumentNumber = @DocumentNumber AND Email=@Email")
                                        {
                                            objTest = CreateLog(cmd, objSQLConnection, UserId.ToString(), "Remember Password", strIp);
                                        }else
                                        {
                                            objTest = CreateLog(cmd, objSQLConnection, UserId.ToString(), "Login", strIp);
                                        }

                                        if (objTest < 1)
                                        {
                                            objResponseDB.Error++;
                                        }
                                        else
                                        {
                                            objResponseDB.Count++;
                                        }

                                    }
                                    catch (Exception innerEx)
                                    {
                                        objResponseDB.Msg = innerEx.Message;
                                        objResponseDB.Error++;
                                        Console.WriteLine(innerEx.Message);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    objResponseDB.Count = 0;
                                }

                                #endregion QueryData
                            }


                        }
                        catch (SqlException ex)
                        {
                            // close the reader
                            if (rdr != null)
                            {
                                rdr.Close();
                            }

                            // 5. Close the connection
                            if (objSQLConnection != null)
                            {
                                objSQLConnection.Close();
                            }

                            objResponseDB.Resp = false;
                            objResponseDB.Msg = ex.Message;
                        }

                    }

                }
                // Catch errors specific to the Open method
                catch (SqlException ex)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Error al tratar de conectarse a la base de datos:: " + ex.Message;
                }
                catch (InvalidOperationException ix)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Operación no válida en la base de datos:" + ix.Message;
                }
                catch (ConfigurationErrorsException cx)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Error de configuración de la base de datos: " + cx.Message;
                }

            }
            catch (ArgumentException ax)  // there was something wrong in the connection string.
            {
                objResponseDB.Resp = false;
                objResponseDB.Msg = "Error al crear la conexión a la base de datos: " + ax.Message;
            }

            return objResponseDB;
        }

        public ResponseDB DelData(string strQuery, string[] arrayParam, string[] arrayValue)
        {
            ResponseDB objResponseDB = new ResponseDB
            {
                Resp = true
            };

            SqlConnection objSQLConnection;

            try
            {
                objSQLConnection = new SqlConnection(strConnection);

                try
                {
                    objSQLConnection.Open();

                    if (objSQLConnection.State != System.Data.ConnectionState.Open)
                    {
                        objResponseDB.Resp = false;
                        objResponseDB.Msg = "The connection status is: " + objSQLConnection.State.ToString();

                        objSQLConnection.Close();
                    }
                    else
                    {

                        try
                        {

                            objResponseDB.Count = 0;
                            objResponseDB.Error = 0;

                            try
                            {

                                SqlCommand cmd = new SqlCommand(strQuery, objSQLConnection);

                                cmd.CommandText = strQuery;

                                for (int j = 0; j < arrayParam.Length; j++)
                                {
                                    cmd.Parameters.Add(new SqlParameter(arrayParam[j], arrayValue[j]));
                                }

                                int nuRows = cmd.ExecuteNonQuery();

                                if (nuRows == 0)
                                {
                                    objResponseDB.Error++;
                                }
                                else
                                {
                                    objResponseDB.Count = nuRows;
                                }

                            }
                            catch (Exception innerEx)
                            {
                                objResponseDB.Msg = innerEx.Message;
                                objResponseDB.Error++;
                            }


                            objSQLConnection.Close();

                        }
                        catch (SqlException ex)
                        {
                            // 5. Close the connection
                            if (objSQLConnection != null)
                            {
                                objResponseDB.Resp = false;
                                objResponseDB.Msg = ex.Message;
                                objSQLConnection.Close();
                            }
                        }

                    }

                }
                // Catch errors specific to the Open method
                catch (SqlException ex)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Error opening the connection:: " + ex.Message;
                }
                catch (InvalidOperationException ix)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Invalid Operation error: " + ix.Message;
                }
                catch (ConfigurationErrorsException cx)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Configuration error: " + cx.Message;
                }

            }
            catch (ArgumentException ax)  // there was something wrong in the connection string.
            {
                objResponseDB.Resp = false;
                objResponseDB.Msg = "Error creating the connection: " + ax.Message;
            }

            return objResponseDB;
        }

        public int CreateLog(SqlCommand cmd, SqlConnection objSQLConnection,string UserId, string strType, string strIp)
        {
            DateTime localDate = DateTime.Now;
            string strQuery = "insert into [tbl_Log] values('" + localDate + "'," + UserId + ",'"+strType+"','" + strIp + "', '')";
            cmd = new SqlCommand(strQuery, objSQLConnection);
            cmd.CommandText = strQuery;
            Console.WriteLine(cmd.CommandText);
            var intRowCreated = cmd.ExecuteNonQuery();
            return intRowCreated;
        }

        public ResponseDB GetAll(string strQuery, string strRespType)
        {
            ResponseDB objResponseDB = new ResponseDB
            {
                Resp = true
            };
            SqlConnection objSQLConnection;
            try
            {
                objSQLConnection = new SqlConnection(strConnection);
                try
                {
                    objSQLConnection.Open();
                    if (objSQLConnection.State != System.Data.ConnectionState.Open)
                    {
                        objResponseDB.Resp = false;
                        objResponseDB.Msg = "The connection status is: " + objSQLConnection.State.ToString();
                        objSQLConnection.Close();
                    }
                    else
                    {
                        try
                        {
                            objResponseDB.Count = 0;
                            objResponseDB.Error = 0;
                            try
                            {
                                #region QueryData
                                SqlCommand cmd = new SqlCommand(strQuery, objSQLConnection);

                                DataSet dsInfo = new DataSet();
                                SqlDataAdapter adapter = new SqlDataAdapter();

                                cmd.CommandText = strQuery;

                                adapter.SelectCommand = cmd;
                                Console.WriteLine(strQuery);
                                adapter.Fill(dsInfo);

                                if (dsInfo.Tables.Count > 0 && dsInfo.Tables[0].Rows.Count > 0)
                                {
                                    switch (strRespType)
                                    {
                                        case "DataTable":
                                            objResponseDB.dtResult = dsInfo.Tables[0];
                                            break;
                                    }
                                }
                                else
                                {
                                    objResponseDB.Count = 0;
                                }

                                #endregion QueryData

                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine(innerEx.Message);
                                objResponseDB.Msg = innerEx.Message;
                                objResponseDB.Error++;
                            }
                            objSQLConnection.Close();
                        }
                        catch (SqlException ex)
                        {
                            // 5. Close the connection
                            if (objSQLConnection != null)
                            {
                                objResponseDB.Resp = false;
                                objResponseDB.Msg = ex.Message;
                                objSQLConnection.Close();
                            }
                        }

                    }

                }
                // Catch errors specific to the Open method
                catch (SqlException ex)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Error opening the connection:: " + ex.Message;
                }
                catch (InvalidOperationException ix)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Invalid Operation error: " + ix.Message;
                }
                catch (ConfigurationErrorsException cx)
                {
                    objResponseDB.Resp = false;
                    objResponseDB.Msg = "Configuration error: " + cx.Message;
                }
            }
            catch (ArgumentException ax)  // there was something wrong in the connection string.
            {
                objResponseDB.Resp = false;
                objResponseDB.Msg = "Error creating the connection: " + ax.Message;
            }
            return objResponseDB;
        }


        /// <summary>
        /// Contenido del mensaje.
        /// </summary>
        public string strConnection = "";

        /// <summary>
        /// Error Conexión
        /// </summary>
        public Boolean blError = false;

        /// <summary>
        /// Descripción Error Conexión
        /// </summary>
        public string strErrorMsg = "";

        public int nuPagination = 100;
    }
}
