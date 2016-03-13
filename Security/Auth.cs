using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; 

namespace Bodega.Security
{
    public class Auth
    {
        private WS_SERVIUVIII.Cliente objeto_cliente = new WS_SERVIUVIII.Cliente();
        WS_SERVIUVIII.WS_ClientesSoapClient  o = new WS_SERVIUVIII.WS_ClientesSoapClient();
 
        public Auth() { 
        
        }

        //setea los datos del cliente
        public void Login(string userName, string password) {

            string rut, _dv;
            string _password = password;
            int _rut;

           

            //Si el largo del rut es menor a 10, entonces enviar error
            if (userName.Length >= 10)
            {
                rut = userName.Replace(".", ""); /* quitar los puntos del rut */

                _rut = Convert.ToInt32(rut.Substring(0, (rut.Length - 2))); /* convierto el string rut a entero 32bits */
                //_dv = rut.Substring((rut.Length - 2), 1); /* obtengo el dv */

                _dv = rut.Substring((rut.Length - 1), 1); /* obtengo el dv */
                objeto_cliente = o.Cliente_existe(1, _rut, _dv, _password, ""); /* buscar en WS los datos del usuario logueado */
               
            }
            else
            {
                //devolver error del rut ingresado
                objeto_cliente.Cliente_Mensaje_error = "Error en rut";
            }

        }


        //@userName = Rut del usuario actual
        public string[] UserData(string userName)
        {
            string rut, _dv; 
            int _rut;

            //WS_SERVIUVIII.WS_ClientesSoapClient o = new WS_SERVIUVIII.WS_ClientesSoapClient();

            //Si el largo del rut es menor a 10, entonces enviar error
            if (userName.Length >= 10)
            {
                rut = userName.Replace(".", ""); /* quitar los puntos del rut */

                _rut = Convert.ToInt32(rut.Substring(0, (rut.Length - 2))); /* convierto el string rut a entero 32bits */
                _dv = rut.Substring((rut.Length - 1), 1); /* obtengo el dv */
                objeto_cliente =  o.Cliente_existe(2,_rut,_dv,"",""); /* buscar en WS los datos del usuario logueado */
               
                /*Obtener arreglo roles[] del usuario*/;  
                var roles = System.Web.Security.Roles.GetRolesForUser(userName).ToList();
                 
                /* Agregar los datos personales del usuario actual */
                roles.Add(objeto_cliente.Cliente_Nombres);
                roles.Add(objeto_cliente.Cliente_Paterno);
                roles.Add(objeto_cliente.Cliente_Materno);
                  
                return roles.ToArray();
 
            }
            else
            {
                //devolver error del rut ingresado
                //return new string[]{ objeto_cliente.Cliente_Mensaje_error};
                return null;
            }

        }

        /*obtiene los datos del funcionario dado un rut sin dv*/
        public string[] UserData(int userName)
        {
            string _dv;
            int _rut = userName;
            _dv = getDV(userName.ToString());
           
            string rutCompleto = userName.ToString() + "-" + _dv; 
            objeto_cliente = o.Cliente_existe(2, _rut, _dv, "", ""); /* buscar en WS los datos del usuario logueado */

            /*Obtener arreglo roles[] del usuario*/
            var roles = System.Web.Security.Roles.GetRolesForUser(rutCompleto).ToList();

            /* Agregar los datos personales del usuario actual */
            roles.Add(objeto_cliente.Cliente_Nombres);
            roles.Add(objeto_cliente.Cliente_Paterno);
            roles.Add(objeto_cliente.Cliente_Materno);

            return roles.ToArray(); 
        }

        /* Obtiene el ID de la dependencia de un usuario*/
        public int getDependecia(int userName)
        {
            string _dv;
            int _rut = userName;
            _dv = getDV(userName.ToString());

            string rutCompleto = userName.ToString() + "-" + _dv;
            objeto_cliente = o.Cliente_existe(2, _rut, _dv, "", ""); /* buscar en WS los datos del usuario logueado */

            return objeto_cliente.Cliente_Dependencia;

        }

        /* Obtiene el nombre de la dependencia de un usuario */
        public string getNombreDependecia(int userName)
        {
            string _dv;
            int _rut = userName;
            _dv = getDV(userName.ToString());

            string rutCompleto = userName.ToString() + "-" + _dv;
            objeto_cliente = o.Cliente_existe(2, _rut, _dv, "", ""); /* buscar en WS los datos del usuario logueado */

            return objeto_cliente.Cliente_DependenciaDesc;

        }
        
        /* calcula el digito verificador*/
        public string getDV(string r)
        {
            int suma = 0;
            for (int x = r.Length - 1; x >= 0; x--)
                suma += int.Parse(char.IsDigit(r[x]) ? r[x].ToString() : "0") * (((r.Length - (x + 1)) % 6) + 2);
            int numericDigito = (11 - suma % 11);
            string digito = numericDigito == 11 ? "0" : numericDigito == 10 ? "K" : numericDigito.ToString();
            
            return digito;
        }

        public bool isValid() {

            if (String.IsNullOrEmpty(objeto_cliente.Cliente_Mensaje_error))
            {
                 return true;
            }
            else return false;

        }

        public int getRut(){
            return objeto_cliente.Cliente_Rut;
        }

        public string getDV(){
            return objeto_cliente.Cliente_RutDV;
        }
        public string getNombres() {
            return objeto_cliente.Cliente_Nombres;
        }

        public string getApeMa() {
            return objeto_cliente.Cliente_Materno;
        }

        public string getApePa() {
            return objeto_cliente.Cliente_Paterno;
        }

        public string getUsername() {

            return objeto_cliente.Cliente_Login;
        }

       

    }
}