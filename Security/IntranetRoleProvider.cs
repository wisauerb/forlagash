using System;
using System.Linq;
using System.Web.Security;
using Bodega.Models;
using Bodega.Models.Context;
using System.Collections.Generic;
using Bodega.Helpers;

namespace Bodega.Security
{
    public class IntranetRoleProvider : RoleProvider
    {
        //Obtiene un valor que indica si el usuario especificado está incluido en el rol especificado para el applicationName especificado.
        public override bool IsUserInRole(string username, string roleName)
        { 
            return true;
        }

        //Obtiene una lista de los roles en los que está incluido un usuario especificado para el applicationName configurado.
        public override string[] GetRolesForUser(string username)
        {
            //return result.ToList();
            RutHelper sanitizar = new RutHelper();
            int rut = sanitizar.getRut(username);

            int id_sistema = 100; //Bodega

            using (var context = new BD_BODEGA())
            { 
                //obtener el perfil de un usuario en un sistema //exec dbo.SP_GUSP
                IEnumerable<SP_GUSP_Result> roles = context.Database.SqlQuery<SP_GUSP_Result>("exec SP_GUSP @id_user, @id_sist", new System.Data.SqlClient.SqlParameter("id_user", rut), new System.Data.SqlClient.SqlParameter("id_sistema", id_sistema)).ToList();
                //IEnumerable<SP_LISTA_SISTEMAS_Result> sistemas = context.Database.SqlQuery<SP_LISTA_SISTEMAS_Result>("exec SP_LISTA_SISTEMAS").ToList();

                if (roles == null) { 
                    return new string[1] {"Nulo"};
                }

                //return roles.Select(u => u.ID_GRUPO).Select(u => u.DESC_GRUPO).Select(u => u.DESC_SISTEMA).ToArray<string>;

                List<string> userRolesName = new List<string>();

                foreach (var rol in roles)
                {
                    //userRoles[i++] = rol.DESC_GRUPO;
                    userRolesName.Add(rol.DESC_GRUPO);
                }

                //retorno el array de roles
                return userRolesName.ToArray();
                //return new string[2] { "ADMIN", "SOLICITANTE" };
            }
        }

        //Obtiene una lista de usuarios incluidos en el rol especificado.
        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }
 
        //Obtiene una lista de todos los roles del applicationName configurado.
        public override string[] GetAllRoles()
        {
            //using (var usersContext = new UsersContext())
            //{
            //    return usersContext.Roles.Select(r => r.RoleName).ToArray();
            //}
            throw new NotImplementedException();
            //return new string[1] { "ADMIN" };
        }

        #region unused
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();

        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }

        #endregion

    }
}