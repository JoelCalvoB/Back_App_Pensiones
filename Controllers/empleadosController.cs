using Microsoft.Reporting.WinForms;
using Npgsql;
using pensionesBackend.Models;
using pensionesBackend.Models.conexion;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Filters;

namespace pensionesBackend.Controllers
{

    [RoutePrefix("api/empleados")]
    
    public class empleadosController : ApiController
    {

        

        public List<Dictionary<string,object>> Get() {

           // string jubilado = obtenejubilado(concatenacion);

            string query = "select jpp,num, nombre from nominas_catalogos.maestro where superviven = 'S'";

            conexionClase conexion = new conexionClase();
            List<Dictionary<string, object>> resultado =conexion.consulta(query);
            return resultado;
        }


        [HttpPost]
        [Route("ingresar")]
        public Dictionary<string, object> logearse(empleado empleado) {
            string query = $"select jpp,num,rfc,nombre,fnacimien,categ,domicilio,fching,nivel,tiporel,llave_qr as code from nominas_catalogos.maestro where llave_qr = '{empleado.code}' and superviven = 'S'";
            List<Dictionary<string, object>> resultado = new conexionClase().consulta(query);
            if (resultado.Count != 0)
            {
                Dictionary<string, object> obj = resultado[0];
                return obj;
            }
            else {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,"No existe información en el sistema"));
            }
        }


        [HttpGet]
        [Route("{id}")]
        public List<Dictionary<string, object>> GetUno(string id)
        {
            id = id.ToUpper(); 

            string query = $"select jpp,num, nombre from nominas_catalogos.maestro where superviven = 'S' and concat(jpp,num)= '{id}'";

            conexionClase conexion = new conexionClase();
            List<Dictionary<string, object>> resultado = conexion.consulta(query);
            return resultado;
        }


        [HttpGet]
        [Route("{identificador}/sobres")]
       
        public Dictionary<string, object> GetSobres(string identificador,string anio) {
            

            

            string query = $"select archivo from nominas_catalogos.respaldos_nominas where concat(jpp,numjpp)= (select concat(jpp,num) from nominas_catalogos.maestro where llave_qr = '{identificador}') and substr(archivo, 1,2) = '{anio.Substring(2,2)}'  group by archivo order by archivo desc";

            conexionClase conexion = new conexionClase();
            List<Dictionary<string, object>> resultado = conexion.consulta(query);
            string[] meses = {"","Enero","Febrero","Marzo","Abril","Mayo","Junio","Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre" };
            foreach (Dictionary<string,object> item in resultado) {
                int tipo = Convert.ToInt32(Convert.ToString(item["archivo"]).Substring(2, 2));
                item.Add("mesnombre",meses[tipo]);
            }

            query = $"select substr(archivo, 1,2) as anios from nominas_catalogos.respaldos_nominas where concat(jpp,numjpp)= (select concat(jpp,num) from nominas_catalogos.maestro where llave_qr = '{identificador}') and substr(archivo, 1,2) >='19'   group by substr(archivo, 1,2) order by substr(archivo, 1,2) desc";
            List<Dictionary<string, object>> resultado2 = conexion.consulta(query);

            Dictionary<string, object> resultadoUsuarioFinal = new Dictionary<string, object>();
            resultadoUsuarioFinal.Add("listasobre",resultado);
            resultadoUsuarioFinal.Add("listaanios", resultado2);

            return resultadoUsuarioFinal;
        }


        [HttpGet]
        [Route("{identificador}/sobres/{aniomes}")]
        public List<Dictionary<string, object>> obtenerSobre(string identificador, string aniomes, string tiponomina)
        {


            

            string query = $"select clave,secuen,descri,pago4,pagot,leyen,folio,monto,tipo_nomina  from nominas_catalogos.respaldos_nominas where concat(jpp,numjpp)= (select concat(jpp,num) from nominas_catalogos.maestro where llave_qr = '{identificador}') and archivo = '{aniomes}' and tipo_nomina = '{tiponomina}' order by clave";
            List<Dictionary<string, object>> resultado = new conexionClase().consulta(query);
            return resultado;
        }


        [HttpGet]
        [Route("{identificador}/sobres/{aniomes}/tiponomina/{tiponomina}/visualizar")]
        public HttpResponseMessage getPdf(string identificador,string aniomes,string tiponomina) {
            Dictionary<string, object> diccionario = new Dictionary<string, object>();
            diccionario.Add("resultado","pdfbase64");


            //****************************

            System.Diagnostics.Debug.WriteLine("generando el jubilado " + identificador);
           
            DateTime fec2 = DateTime.Now;
            

            string año1 = "20";
            string mes1 = "06";

            string archivo1 = año1 + mes1;



            string tipo_nomina = "N";

            string añostr = "20" + aniomes.ToString().Substring(0,2);

            string[] meses = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

            DateTime tiempo = new DateTime(Convert.ToInt32(añostr),Convert.ToInt32(aniomes.Substring(2)),1);
            tiempo = tiempo.AddMonths(1);
            tiempo = tiempo.AddDays(-1);


            DateTime fechaSobres = new DateTime(Convert.ToInt32(añostr), Convert.ToInt32(aniomes.Substring(2)), 1); ;
            fechaSobres = fechaSobres.AddDays(-1);


            int mesint = Convert.ToInt32(aniomes.Substring(2));

            string messeleccionado = meses[mesint];

            string mensaje = $"Del 01 al {tiempo.Day} de {messeleccionado} del {añostr}";


            string query = $"CREATE TEMP TABLE respaldoNom AS ( SELECT		*	FROM	nominas_catalogos.respaldos_nominas WHERE	archivo = '{aniomes}');" +
               $"SELECT concat (a1.jpp, a1.num) AS proyecto, a1.nombre,a1.curp,a1.rfc,a1.imss,a1.categ,a2.clave,a2.descri,a2.monto,	a2.pago4 AS pagon,	a2.pagot,	a2.leyen FROM nominas_catalogos.maestro a1 JOIN respaldoNom a2 ON a1.num = a2.numjpp WHERE a1.superviven = 'S'AND a1.jpp = a2.jpp AND concat (a1.jpp, a1.num) = (select concat(jpp,num) from nominas_catalogos.maestro where llave_qr = '{identificador}') AND a2.tipo_nomina = '{tipo_nomina}'";
            List<Dictionary<string, object>> resultado = new conexionClase().consulta(query);
         
            string compara = Convert.ToString(resultado[0]["proyecto"]);

            query = "select  clave,descri from nominas_catalogos.perded order by clave";
            List<Dictionary<string, object>> perded = new conexionClase().consulta(query);
            resultado.ForEach(o => {
                o["descri"] = perded.Where(p => Convert.ToString(o["clave"]) == Convert.ToString(p["clave"])).First()["descri"];
                //  o["descri"] += " (RETROACTIVO)";
            });


            object[] aux2 = new object[resultado.Count];
            int contadorPercepcion = 0;
            int contadorDeduccion = 0;

            string proyecto = string.Empty;
            string nombre = string.Empty;
            string curp = string.Empty;
            string rfc = string.Empty;
            string imss = string.Empty;
            string categ = string.Empty;
            string clave = string.Empty;
            string descri = string.Empty;
            string monto = string.Empty;
            string fecha = fec2.ToString();
            string periodo = mensaje;
            string archivo = string.Empty;
            int año = 0;
            int mes = 0;
            string pago4 = string.Empty;
            string pagot = string.Empty;

            foreach (var item in resultado)
            {

                 proyecto = string.Empty;
                 nombre = string.Empty;
                 curp = string.Empty;
                 rfc = string.Empty;
                 imss = string.Empty;
                 categ = string.Empty;
                 clave = string.Empty;
                 descri = string.Empty;
                 monto = string.Empty;
                 fecha = fec2.ToString();
                 periodo = mensaje;
                 archivo = string.Empty;
                 año = 0;
                 mes = 0;
                 pago4 = string.Empty;
                 pagot = string.Empty;
                try
                {

                    proyecto = Convert.ToString(item["proyecto"]);
                    nombre = Convert.ToString(item["nombre"]);
                    curp = Convert.ToString(item["curp"]);
                    rfc = Convert.ToString(item["rfc"]);
                    imss = Convert.ToString(item["imss"]);
                    categ = Convert.ToString(item["categ"]);
                    clave = Convert.ToString(item["clave"]);
                    descri = Convert.ToString(item["descri"]) + (string.IsNullOrWhiteSpace(Convert.ToString(item["leyen"])) ? "" : $"({Convert.ToString(item["leyen"])})");
                    monto = string.Format("{0:C}", Convert.ToDouble(item["monto"])).Replace("$", "");

                    año = Convert.ToInt32(2020);
                    mes = 06 + 1;

                    fechaSobres = fechaSobres;

                    //fechaSobres = fechaSobres.AddDays(-1);
                    pago4 = Convert.ToString(item["pagon"]);
                    pagot = Convert.ToString(item["pagot"]);

                }
                catch
                {

                }
                object[] tt1 = { "", "", "", "", "", "", "", "", "", "", "", "", "" };
                if (Convert.ToInt32(clave) < 60)
                {
                    if (aux2[contadorPercepcion] == null)
                    {
                        tt1[6] = clave;
                        tt1[7] = descri;
                        tt1[8] = monto;
                        aux2[contadorPercepcion] = tt1;
                    }
                    else
                    {
                        object[] tmp = (object[])aux2[contadorPercepcion];
                        tmp[6] = clave;
                        tmp[7] = descri;
                        tmp[8] = monto;
                    }
                    contadorPercepcion++;
                }
                else
                {

                    if (aux2[contadorDeduccion] == null)
                    {
                        tt1[9] = clave;
                        tt1[10] = descri;
                        tt1[12] = (string.IsNullOrWhiteSpace(pago4) || pago4 == "0") ? "" : $"{pago4}/{pagot}";
                        tt1[11] = monto;
                        aux2[contadorDeduccion] = tt1;
                    }
                    else
                    {
                        object[] tmp = (object[])aux2[contadorDeduccion];
                        tmp[9] = clave;
                        tmp[10] = descri;
                        tmp[12] = (string.IsNullOrWhiteSpace(pago4) || pago4 == "0") ? "" : $"{pago4}/{pagot}";
                        tmp[11] = monto;
                    }
                    contadorDeduccion++;
                }
            }

            //Restablece los objetos para evitar el break del reporteador

            int contadorPrincipal = 0;
            try
            {
                while (aux2[contadorPrincipal] != null)
                    contadorPrincipal++;
            }
            catch
            {

            }

            object[] objeto = new object[13];
            for (int x = 0; x < 13; x++)
            {
                object[] tt1 = { "", "", "", "", "", "", "", "", "", "", "", "", "" };
                objeto[x] = tt1;
            }
            double sumaPercepciones = 0;
            double sumaDeducciones = 0;

            aux2.Sum(o =>
            {
                object[] a = (object[])o;
                sumaDeducciones += o == null ? 0 : globales.convertDouble(Convert.ToString(a[11]));
                sumaPercepciones += o == null ? 0 : globales.convertDouble(Convert.ToString(a[8]));
                return 0;
            });


            for (int x = 0; x < contadorPrincipal; x++)
            {
                if (x == 13)
                {
                   // System.Diagnostics.Debug.WriteLine(proyecto + " " + nombre + " " + rfc);
                    break;
                }
                objeto[x] = aux2[x];
                object[] sacarDato = (object[])aux2[x];
                //  double percepcion = string.IsNullOrWhiteSpace(Convert.ToString(sacarDato[8])) ? 0 : Convert.ToDouble(sacarDato[8]);
                // double deduccion = string.IsNullOrWhiteSpace(Convert.ToString(sacarDato[11])) ? 0 : Convert.ToDouble(sacarDato[11]);
                //sumaPercepciones += percepcion;
                //sumaDeducciones += deduccion;

            }



            object[] parametros = { "proyecto", "nombre", "curp", "rfc", "imss", "categ", "fechapago", "periodo", "sumaPercepcion", "sumaDeduccion" };
            object[] valor = { proyecto, nombre, curp, rfc, imss, categ, string.Format("{0:d}", fechaSobres), periodo, sumaPercepciones.ToString(), sumaDeducciones.ToString() };
            object[][] enviarParametros = new object[2][];

            enviarParametros[0] = parametros;
            enviarParametros[1] = valor;


            //  globales.ocultarEpilep = true;
            string subcarpeta = @"\" + tipo_nomina;
            if (tipo_nomina.Contains("N"))
            {
                subcarpeta = string.Empty;
            }
            byte[] bytes = globales.reportes("sobres_pago", "sobres", objeto, "", true, enviarParametros, true, identificador, subcarpeta);

            string base64 = Convert.ToBase64String(bytes);

            MemoryStream enmemoria = new MemoryStream(bytes);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
           // System.IO.FileStream fileStream = File.OpenRead(@"C:\Users\samv\pdfjubilados\JUB5.pdf");
            response.Content = new StreamContent(enmemoria);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return response;

            //*************




        }


    }


   


}
