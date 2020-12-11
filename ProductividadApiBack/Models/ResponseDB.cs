using System;
using System.Data;

namespace Productividad.Models
{
    public class ResponseDB
    {
        public ResponseDB()
        {
            // TODO: Add constructor logic here
        }

        #region Propiedades

        /// <summary>
        /// Identificador de la respuesta.
        /// </summary>
        public Boolean Resp
        {
            get;
            set;
        }

        /// <summary>
        /// jsonData array data (string)
        /// </summary>
        public string jsonData
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
        /// Rows
        /// </summary>
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// Rows error
        /// </summary>
        public int Error
        {
            get;
            set;
        }

        public DataTable dtResult
        {
            get;
            set;
        }

        public Guid guidResult
        {
            get;
            set;
        }

        public object Data{get; set;}
      
        #endregion

    }
}
