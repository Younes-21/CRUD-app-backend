using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DEBUT.Models
{
    public class SGBD : IDisposable
    {
        public SqlConnection cn;
        private SqlCommand cmd;
        private SqlDataAdapter da;
        private DataSet ds;

        public SGBD()
        {
            this.cn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }
        public SGBD(string context)
        {
            this.cn = new SqlConnection(ConfigurationManager.ConnectionStrings[context].ConnectionString);
        }

        // CMD
        public DataTable Cmd(string sqlQuery, Dictionary<string, object> parametres = null)
        {
            try
            {
                using (cmd = new SqlCommand(sqlQuery, cn))
                {
                    if (parametres != null)
                    {
                        foreach (var item in parametres)
                        {
                            if (item.Value == null)
                                cmd.Parameters.AddWithValue(item.Key, DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue(item.Key, item.Value);
                        }
                    }
                    //cmd.CommandTimeout = 0;

                    using (da = new SqlDataAdapter(cmd))
                    {
                        using (ds = new DataSet())
                        {
                            da.Fill(ds, "tbl");
                            return ds.Tables["tbl"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO LOG EXCEPTION
                throw ex;
            }
        }

        public async Task<DataTable> CmdAsync(string sqlQuery, Dictionary<string, object> parametres = null)
        {
            try
            {
                using (cmd = new SqlCommand(sqlQuery, cn))
                {
                    cmd.CommandTimeout = 0;
                    if (parametres != null)
                    {
                        foreach (var item in parametres)
                        {
                            if (item.Value == null)
                                cmd.Parameters.AddWithValue(item.Key, DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue(item.Key, item.Value);
                        }
                    }

                    using (da = new SqlDataAdapter(cmd))
                    {
                        var temp = new DataTable();
                        await Task.Run(() => da.Fill(temp));
                        return temp;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<dynamic>> ICmdAsync(string sqlQuery, Dictionary<string, object> parametres = null)
        {
            var table = await this.CmdAsync(sqlQuery, parametres);
            var cols = table.Columns;

            return table.AsEnumerable().Select(row => new JObject(
                    table.Columns.Cast<DataColumn>()
                          .Select(c => new JProperty(c.ColumnName, JToken.FromObject(row[c])))
                ));
        }
        public async Task<List<T>> ICmdAsync<T>(string sqlQuery, Dictionary<string, object> parametres = null)
        {
            var table = await this.CmdAsync(sqlQuery, parametres);
            var cols = table.Columns;

            return ConvertDataTable<T>(table);
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (pro.PropertyType.Name == "String")
                        {
                            pro.SetValue(obj, dr[column.ColumnName] == DBNull.Value ? "" : dr[column.ColumnName], null);
                        }
                        else
                            pro.SetValue(obj, dr[column.ColumnName], null);
                    }
                    else
                        continue;
                }
            }
            return obj;
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (da != null) da.Dispose();
                    if (cmd != null) cmd.Dispose();
                    if (cn != null) cn.Dispose();
                    if (ds != null) ds.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SGBD() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
