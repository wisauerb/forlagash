//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bodega.Models.Context
{
    using System;
    using System.Collections.Generic;
    
    public partial class FACTURA
    {
        public FACTURA()
        {
            this.FACTURA_PRODUCTO = new HashSet<FACTURA_PRODUCTO>();
        }
        
        /* Se han obviado algunos atributos para este ejemplo */
        public Nullable<int> RUT_PROVEEDOR { get; set; }
        public Nullable<int> ESTADO { get; set; }
        public Nullable<int> CORRELATIVO { get; set; }
        public virtual LINEA_INVERSION LINEA_INVERSION { get; set; }
        public virtual ORIGEN ORIGEN { get; set; }
        public virtual PROVEEDOR PROVEEDOR { get; set; }
        public virtual TIPO_DOCUMENTO TIPO_DOCUMENTO { get; set; }
        public virtual ICollection<FACTURA_PRODUCTO> FACTURA_PRODUCTO { get; set; }
    }
}
