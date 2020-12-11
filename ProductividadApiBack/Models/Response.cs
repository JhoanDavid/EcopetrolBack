using Microsoft.AspNetCore.Mvc;

namespace ProductividadApiBack.Models
{
    /// <summary>
    /// Summary description for Response
    /// </summary>
    public class Response
    {
        public Response()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Propiedades

        /// <summary>
        /// Identificador de la respuesta.
        /// </summary>
        public bool Resp
        {
            get;
            set;
        }

        /// <summary>
        /// Contenido del mensaje.
        /// </summary>
        public string Msg
        {
            get;
            set;
        }

        /// <summary>
        /// Validación de la licensia.
        /// </summary>
        public bool License
        {
            get;
            set;
        }

        /// <summary>
        /// Tipo del mensaje.
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Redirect to Page?
        /// </summary>
        public string Redirect
        {
            get;
            set;
        }
        /// <summary>
        /// Json Data Resp
        /// </summary>
        public object Data
        {
            get;
            set;
        }
        /// <summary>
        /// Json Menu
        /// </summary>
        public string Menu
        {
            get;
            set;
        }

        /// <summary>
        /// Json Labels
        /// </summary>
        public string Label
        {
            get;
            set;
        }

        /// <summary>
        /// Detalle del error.
        /// </summary>
        public string Detail
        {
            get;
            set;
        }

        /// <summary>
        /// Json Aditional Data
        /// </summary>
        public string Json
        {
            get;
            set;
        }

        /// <summary>
        /// Row Count
        /// </summary>
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// Row INDEX
        /// </summary>
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Guid PK
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Guid PK
        /// </summary>
        public string Token
        {
            get;
            set;
        }

        #endregion
    }
}
