//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Proyecto_Estacionamiento
{
    using System;
    using System.Collections.Generic;
    
    public partial class Ocupacion
    {
        public int Est_id { get; set; }
        public int Plaza_id { get; set; }
        public System.DateTime Ocu_fecha_Hora_Inicio { get; set; }
        public string Vehiculo_Patente { get; set; }
        public int Tarifa_id { get; set; }
        public Nullable<int> Pago_id { get; set; }
        public Nullable<System.DateTime> Ocu_fecha_Hora_Fin { get; set; }
    
        public virtual Pago_Ocupacion Pago_Ocupacion { get; set; }
        public virtual Plaza Plaza { get; set; }
        public virtual Tarifa Tarifa { get; set; }
        public virtual Vehiculo Vehiculo { get; set; }
    }
}
