﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ProyectoEstacionamientoEntities : DbContext
    {
        public ProyectoEstacionamientoEntities()
            : base("name=ProyectoEstacionamientoEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Acepta_Metodo_De_Pago> Acepta_Metodo_De_Pago { get; set; }
        public virtual DbSet<Categoria_Vehiculo> Categoria_Vehiculo { get; set; }
        public virtual DbSet<Dueño> Dueño { get; set; }
        public virtual DbSet<Estacionamiento> Estacionamiento { get; set; }
        public virtual DbSet<Incidencias> Incidencias { get; set; }
        public virtual DbSet<Metodos_De_Pago> Metodos_De_Pago { get; set; }
        public virtual DbSet<Ocupacion> Ocupacion { get; set; }
        public virtual DbSet<Pago_Ocupacion> Pago_Ocupacion { get; set; }
        public virtual DbSet<Playero> Playero { get; set; }
        public virtual DbSet<Plaza> Plaza { get; set; }
        public virtual DbSet<Tarifa> Tarifa { get; set; }
        public virtual DbSet<Tipos_Tarifa> Tipos_Tarifa { get; set; }
        public virtual DbSet<Turno> Turno { get; set; }
        public virtual DbSet<Usuarios> Usuarios { get; set; }
        public virtual DbSet<Vehiculo> Vehiculo { get; set; }
        public virtual DbSet<Vehiculo_Abonado> Vehiculo_Abonado { get; set; }
    }
}
