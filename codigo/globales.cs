using SISPE_MIGRACION.codigo.herramientas.forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


public class globales
    {
        public static double convertDouble(string numero)
        {
            string strNumero = (string.IsNullOrWhiteSpace(numero)) ? "0" : numero;
            double dblNumero = 0;
            try
            {
                dblNumero = double.Parse(strNumero, System.Globalization.NumberStyles.Currency);
            }
            catch
            {
                dblNumero = 0;
            }
            return dblNumero;
        }

    public static byte[] reportes(string nombreReporte, string tablaDataSet, object[] objeto, string mensaje = "", bool imprimir = false, object[] parametros = null, bool espdf = false, string nombrePdf = "", string subcarpeta = "")
        
    {
        frmReporte reporte = new frmReporte(nombreReporte, subcarpeta, tablaDataSet);


        reporte.setParametrosExtra(espdf, nombrePdf);
        reporte.cargarDatos(tablaDataSet, objeto, mensaje, imprimir, parametros);
        
        return reporte.bytes;
    }
}
