using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Collections;

namespace Code.DatosCS
{
    public class DatosSQL
    {
        //ATRIBUTOS
       // private StringBuilder cadenaConexion = new StringBuilder("");
        private IDbConnection mConexion;//conexión actual
        private IDbTransaction mTransaccion;
        public string DataSource;
        public string InitialCatalog;

        //CONSTRUCTORES
        #region CONSTRUCTORES
        public DatosSQL()
        {
           
                      
        }
        public DatosSQL(string servidor, string dataBase)
        {
           
        }
        #endregion
        //METODOS
        #region ABRIR_CERRAR_CONEXIONES
        public void AbrirConexion()
        {          
            this.mConexion = Conexion.AbrirConexion();
            /*DataSource = Conexion.DataSource;
            InitialCatalog = Conexion.InitalCatalog;*/
        }
        public void CerrarConexion()
        {
            this.mConexion = Conexion.CerrarConexion();
        }
        #endregion
        #region COMANDO, CARGAR_PARAMETROS, CREAR_ADAPATADOR
        static Hashtable ColComandos = new Hashtable();
        private IDbCommand Comando(string ProcedimientoAlmacenado)
        {
            SqlCommand ComandoRetorno;
            //comando usa la conexión el tipo del acceso a bd y la consulta en si para 
            //que el adapatador lo adapte a una tabla
            ComandoRetorno = new SqlCommand(ProcedimientoAlmacenado, (SqlConnection) this.mConexion);
            ComandoRetorno.CommandType = CommandType.StoredProcedure;
            ComandoRetorno.Transaction = (SqlTransaction)this.mTransaccion;
            SqlCommandBuilder.DeriveParameters(ComandoRetorno);
            return (IDbCommand)ComandoRetorno;
        }
        private void CargarParametros(IDbCommand Comando, Object[] Args)
        {
            for (int i = 1; i < Comando.Parameters.Count; i++)
            {
                SqlParameter P = (SqlParameter)Comando.Parameters[i];
                if (i <= Args.Length)
                {
                    P.Value = Args[i - 1];
                }
                else
                {
                    P.Value = null;
                }
            }
        }
        private IDataAdapter CrearDataAdapter(string ProcedimientoAlmacenado, params Object[] Args)
        {
            SqlDataAdapter Da = new SqlDataAdapter((SqlCommand)Comando(ProcedimientoAlmacenado));
            if (Args.Length != 0)
            {
                CargarParametros(Da.SelectCommand, Args);
            }
            return (IDataAdapter)Da;
        }
        #endregion
        //TRAER VALOR DE PROCEDURE SIN PARAMETROS
        #region TRAER_DATASET, TRAER_VALOR, EJECUTAR
        public Object TraerValor(string ProcedimientoAlmacenado)
        {
            IDbCommand Com = Comando(ProcedimientoAlmacenado);
            Com.ExecuteNonQuery();
            Object Resp = null;
            foreach (IDbDataParameter Par in Com.Parameters)
            {
                if (Par.Direction == ParameterDirection.InputOutput || Par.Direction == ParameterDirection.Output)
                {
                    Resp = Par.Value;
                }
            }
            return Resp;
        }
        public Object TraerValor(string ProcedimientoAlmacenado, params System.Object[] Args)
        {
            IDbCommand Com = Comando(ProcedimientoAlmacenado);
            CargarParametros(Com, Args);
            Com.ExecuteNonQuery();
            Object Resp = null;
            foreach (IDbDataParameter Par in Com.Parameters)
            {
                if (Par.Direction == ParameterDirection.InputOutput || Par.Direction == ParameterDirection.Output)
                {
                    Resp = Par.Value;
                }
            }
            return Resp;
        }
        public int Ejecutar(string ProcedimientoAlmacenado)
        {
            int Resp = Comando(ProcedimientoAlmacenado).ExecuteNonQuery();
            return Resp;
            
        }
        public int Ejecutar(string ProcedimientoAlmacenado, params  System.Object[] Args)
        {
            IDbCommand Com = Comando(ProcedimientoAlmacenado);
            CargarParametros(Com, Args);
            int Resp = Com.ExecuteNonQuery();
            for (int i = 0; i < Com.Parameters.Count; i++)
            {
                IDbDataParameter Par = (IDbDataParameter)Com.Parameters[i];
                if (Par.Direction == ParameterDirection.InputOutput || Par.Direction == ParameterDirection.Output)
                {
                    Args.SetValue(Par.Value, i - 1);
                }
            }
            return Resp;
        }

        public int EjecutarConValoresDeTabla(string ProcedimientoAlmacenado, params  object[] Args)
        {
            int Resp;
            using (SqlCommand Com = new SqlCommand(ProcedimientoAlmacenado, (SqlConnection)this.mConexion))
            {
                Com.CommandType = CommandType.StoredProcedure;
                Com.Parameters.AddRange(Args);
                Resp = Com.ExecuteNonQuery();
            }
            return Resp;
        }

        public DataSet TraerDataset(string ProcedimientoAlmacenado)
        {
            DataSet mDataSet = new DataSet();
            this.CrearDataAdapter(ProcedimientoAlmacenado).Fill(mDataSet);
            return mDataSet;
        }

        public DataSet TraerDataset(string ProcedimientoAlmacenado, params System.Object[] Args)
        {
            DataSet mDataSet = new DataSet();
            this.CrearDataAdapter(ProcedimientoAlmacenado, Args).Fill(mDataSet);
            return mDataSet;
        }

        //AHORRA MEMORIA CON DATAREADER
        public DataTable TraerDataTable(string ProcedimientoAlmacenado)
        {
            DataTable mDataTable = new DataTable();
            SqlCommand Comando;
            Comando = new SqlCommand(ProcedimientoAlmacenado, (SqlConnection)this.mConexion);
            Comando.CommandType = CommandType.StoredProcedure;
            Comando.Transaction = (SqlTransaction)this.mTransaccion;
            SqlCommandBuilder.DeriveParameters(Comando);
            SqlDataReader reader = Comando.ExecuteReader();
            mDataTable.Load(reader);
            return mDataTable;
        }
        public DataTable TraerDataTable(string ProcedimientoAlmacenado, params System.Object[] Args)
        {
            DataTable mDataTable = new DataTable();
            SqlCommand Comando;
            Comando = new SqlCommand(ProcedimientoAlmacenado, (SqlConnection)this.mConexion);
            Comando.CommandType = CommandType.StoredProcedure;
            Comando.Transaction = (SqlTransaction)this.mTransaccion;
            SqlCommandBuilder.DeriveParameters(Comando);

            if (Args.Length != 0)
            {
                for (int i = 1; i < Comando.Parameters.Count; i++)
                {
                    SqlParameter P = (SqlParameter)Comando.Parameters[i];
                    if (i <= Args.Length)
                    {
                        P.Value = Args[i - 1];
                    }
                    else
                    {
                        P.Value = null;
                    }
                }
            }
            try
            {
                SqlDataReader reader = Comando.ExecuteReader();
                mDataTable.Load(reader);
            }
            catch(SqlException error)
            {
                string aux = error.ToString();
            }
           
            return mDataTable;
            

        }
       
        #endregion
        #region "Transacciones"
        protected bool EnTransaccion = false;
        //INICIAR TRANSACCIÓN
        public void IniciarTransaccion()
        {
            mTransaccion = this.mConexion.BeginTransaction();
            EnTransaccion = true;
        }
        //TERMINAR TRANSACCIÓN
        public void TerminarTransaccion()
        {
            try
            {
                mTransaccion.Commit();
                //Com.Connection.Close();
            }
            catch (Exception Ex)
            {
                throw Ex;
                //Com.Connection.Close();
            }
            finally
            {
                mTransaccion = null;
                EnTransaccion = false;
            }
        }
        //ABORTAR TRANSACCIÓN
        public void AbortarTransaccion()
        {
            try
            {
                mTransaccion.Rollback();
                //Com.Connection.Close();
            }
            catch (Exception Ex)
            {
                string a = Ex.ToString();
                //Com.Connection.Close();
                throw Ex;
            }
            finally
            {
                mTransaccion = null;
                EnTransaccion = false;
            }
        }
        #endregion	
    }
}
