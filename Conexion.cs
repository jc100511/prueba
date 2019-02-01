using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Configuration;
using System.IO;
using System.Net;

namespace Code.DatosCS
{
    public static class Conexion
    {
        public static IDbConnection mConexion; //conexión actual
        public static string cadenaConexion; // cadena de conexión

        /*
        private static string get(string key)
        {
            return ConfigurationManager.AppSettings.Get(key);
        }
        */

        public static IDbConnection AbrirConexion()
        {
            if (cadenaConexion == null)
            {
                string ip = "";
                WebClient request = new WebClient();
                request.Proxy = null;
                string url = "http://www.codeperu.com";
                request.Credentials = new NetworkCredential("ip", "ip132");
                try
                {
                    byte[] newFileData = request.DownloadData(new Uri(url));
                    ip = System.Text.Encoding.UTF8.GetString(newFileData);
                    request.Dispose();

                    //cadenaConexion = "data source = " + ip + "; initial catalog = BD; persist security info = True ; integrated Security = False; user id = 'sa';password='PASSW'; packet size= 4096; Pooling=true; Max Pool Size=10;";
                    cadenaConexion = "data source = 192.168.1.111; initial catalog =_bd_reinco_pruebas_code; persist security info = True ; integrated Security = False;user id = 'civil';password='@contracivil**@'; packet size= 4096; Pooling=true; Max Pool Size=10;";
                    //cadenaConexion = "data source =.; initial catalog =bdcivil; persist security info = True ; integrated Security = False;user id = 'civil';password='civil'; packet size= 4096; Pooling=true; Max Pool Size=10;";
                }
                catch
                {
                    cadenaConexion = null;
                }
            }
            // abrimos si no hay una conexiòn activa, caso contrario retornamos la ya abierta
            if (mConexion == null)
            {
                mConexion = (IDbConnection)new SqlConnection(cadenaConexion.ToString());
                mConexion.Open();
            }
            else if (mConexion.State != ConnectionState.Open)//conexión cerrada
            {
                mConexion.ConnectionString = cadenaConexion.ToString();
                mConexion.Open();
            }
            return mConexion;

        }
        public static IDbConnection CerrarConexion()
        {
            if (mConexion != null)
            {
                if (mConexion.State == ConnectionState.Open)
                {
                    mConexion.Dispose();
                    mConexion.Close();
                }
            }
            return mConexion;
        }
    }
}
