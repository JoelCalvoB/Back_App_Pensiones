using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pensionesBackend.Models.conexion
{
    public class conexionClase
    {

      


        public dynamic consulta(string query, bool tipoSelect = false, bool eliminando = false)
        {
            string host = backend_pensionesmovil.Properties.Resources.servidor;
            string usuario = backend_pensionesmovil.Properties.Resources.usuario;
            string password = backend_pensionesmovil.Properties.Resources.password;
            string database = backend_pensionesmovil.Properties.Resources.baseDatos;
            string port = backend_pensionesmovil.Properties.Resources.puerto;


            //host = "ec2-23-21-160-38.compute-1.amazonaws.com";
            //usuario = "hwvzppntjiviyu";
            //password = "8ec67b7ca03d1e00ba4ac06dc6cdf97e148da0ddc2b7bb69523dec23cdde5256";
            //database = "ddboilk04tmcso";
            //SSL Mode = Require; Trust Server Certificate = true
            string queryConexion = string.Format("Host={0};Username={1};Password={2};Database={3};port={4}", host, usuario, password, database, port);


            NpgsqlConnection conexion = new NpgsqlConnection(queryConexion);

            var consulta = new List<Dictionary<string, object>>();
            try
            {
                conexion.Open();

                NpgsqlCommand cmd = new NpgsqlCommand(query, conexion);
                if (!tipoSelect)
                {
                    System.Data.Common.DbDataReader datos = cmd.ExecuteReader();

                    while (datos.Read())
                    {
                        Dictionary<string, object> objeto = new Dictionary<string, object>();
                        for (int x = 0; x < datos.FieldCount; x++)
                        {
                            objeto.Add(datos.GetName(x), datos.GetValue(x));
                        }
                        consulta.Add(objeto);
                    }
                    // consulta.Clear();
                    //  consulta = new List<Dictionary<string, object>>();
                }
                else
                {



                    try
                    {
                        cmd.ExecuteNonQuery();
                        conexion.Close();
                        cmd.Dispose();
                        return true;
                    }

                    catch (Exception e)
                    {
                        if (eliminando) return false;
                        System.Diagnostics.Debug.WriteLine(e.Message);
                        conexion.Close();
                        return false;
                    }
                }
                conexion.Close();

            }
            catch (Exception e)
            {
                if (eliminando) return false;
                System.Diagnostics.Debug.WriteLine(e.Message);
                conexion.Close();
            }


            return consulta;
        }
    }
}