using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Bodega.Models;
using System.Web; 

namespace Bodega.Security
{
    public class IntranetMembershipProvider : MembershipProvider
    { 
        //Obtiene o establece el nombre de la aplicación para la que se va a almacenar y recuperar información de perfil.
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        Auth usuario = new Auth();
        int RUT;

        //Comprueba que el nombre de usuario y la contraseña proporcionados son válidos.
        public override bool ValidateUser(string username, string password)
        { 
            //obtener el resultado mediante el objeto Authentication del proyecto Login
            //(Authentication from WerbService)

            Auth usuario = new Auth();

            usuario.Login(username, password);
            RUT = usuario.getRut();

            string name = usuario.getNombres();
            
            //datos session?

             
            return usuario.isValid(); //Return True or False
            //return true;
        }

        /* Recibe la información del origen de datos para el usuario de pertenencia asociado al identificador único especificado. 
         * Actualiza la marca de fecha y hora de la última actividad para el usuario, si se ha especificado.
         * Ej: u = Membership.GetUser(User.Identity.Name);
         * Return: Objeto MembershipUser 
         * http://msdn.microsoft.com/es-es/library/ms152020(v=vs.110).aspx
        */

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
 
            Auth user = new Auth();

            //obtener datos desde el WS dado un rut
            string[] userdata = user.UserData(username);

            //convertir los datos del arreglo en un string


            //setear aqui los datos del usuario en la sesion?
            HttpCookie authcookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authcookie.Value);

            //string datosUsuario = authTicket.UserData; 

            //esto es solo para obtener los datos del usuario y mostrarlos en los menus
            string datosUsuario = "";
            int i=0;
            foreach(var item in userdata)
            {
                i++;
                //cuando pase por primera vez no incluir la coma
                if (i <= 1)
                {
                    datosUsuario = datosUsuario  + item.ToString();
                }
                else {
                    datosUsuario = datosUsuario + "," + item.ToString();
                } 

            }     

            if (userdata == null)
            {
                datosUsuario = "No hay datos";
            }

            var memUser = new MembershipUser("IntranetMembershipProvider", datosUsuario, "", "",
                                                            string.Empty, string.Empty,
                                                            true, false, DateTime.MinValue,
                                                            DateTime.MinValue,
                                                            DateTime.MinValue,
                                                            DateTime.Now, DateTime.Now); 
    
            return memUser;
         
            //obtener los datos a partir del web service
            //return null;
        }
         
        /*Agrega un nuevo usuario con los valores de propiedades especificados al almacén de datos y devuelve un parámetro de estado que indica que el usuario se ha creado correctamente o el motivo del error en la creación del usuario.*/
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != string.Empty)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            var user = GetUser(username, true);
 
            status = MembershipCreateStatus.DuplicateUserName;

            return null;
        }

        public static string GetMd5Hash(string value)
        {
            var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        #region unused
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 6; }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        #endregion
        
    }
}