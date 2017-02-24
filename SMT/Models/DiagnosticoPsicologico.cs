using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models.DB
{
    public partial class DiagnosticoPsicologico
    {

        public void crear()
        {
            using(SMTDevEntities db = new SMTDevEntities())
            {
                FechaActualizacion = DateTime.Now;
                db.DiagnosticoPsicologico.Add(this);
                db.SaveChanges();
            }

        }

        public void editar()
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                DiagnosticoPsicologico diag = db.DiagnosticoPsicologico.FirstOrDefault(i => i.IDAlumno == IDAlumno && i.IDGrupo == IDGrupo);

                if(diag != null)
                {
                    diag.Aburrimiento = Aburrimiento;
                    diag.AutonomiaVerguenza = AutonomiaVerguenza;
                    diag.ComoSeTranslada = ComoSeTranslada;
                    diag.ConductaInadecuada = ConductaInadecuada;
                    diag.ConfianzaDesconfianza = ConfianzaDesconfianza;
                    diag.CTE10 = CTE10;
                    diag.CTE8 = CTE8;
                    diag.CTE5 = CTE5;
                    diag.CuentaConCasa = CuentaConCasa;
                    diag.Edad = Edad;
                    diag.EscolaridadPadres = EscolaridadPadres;
                    diag.EtapaPreconvencional = EtapaPreconvencional;
                    diag.EtapaSesioromotora = EtapaSesioromotora;
                    diag.EvaluacionBloque1 = EvaluacionBloque1;
                    diag.EvaluacionBloque2 = EvaluacionBloque2;
                    diag.EvaluacionBloque3 = EvaluacionBloque3;
                    diag.EvaluacionBloque4 = EvaluacionBloque4;
                    diag.EvaluacionBloque5 = EvaluacionBloque5;
                    diag.ExpresaEmociones = ExpresaEmociones;
                    diag.FamiliaCompleta = FamiliaCompleta;
                    diag.FamiliaMonoparental = FamiliaMonoparental;
                    diag.FechaActualizacion = DateTime.Now;
                    diag.FechaSync = DateTime.Now;
                    diag.Generabilidad = Generabilidad;
                    diag.Hermanos = Hermanos;
                    diag.IdentidadConfusion = IdentidadConfusion;
                    diag.Indiferencia = Indiferencia;
                    diag.IniciativaCulpa = IniciativaCulpa;
                    diag.Integridad = Integridad;
                    diag.Interpersonal = Interpersonal;
                    diag.IntimidadAislamiento = IntimidadAislamiento;
                    diag.Intrapersonal = Intrapersonal;
                    diag.KinestesicaCorporal = KinestesicaCorporal;
                    diag.Laboriosidad = Laboriosidad;
                    diag.Linguistica = Linguistica;
                    diag.LogicoMatematica = LogicoMatematica;
                    diag.LugarFamilia = LugarFamilia;
                    diag.MoralidadConvencional = MoralidadConvencional;
                    diag.MoralidadPreconvencional = MoralidadPreconvencional;
                    diag.MoralidadPrincipiosAutonomos = MoralidadPrincipiosAutonomos;
                    diag.Motivado = Motivado;
                    diag.MuestraIniciativa = MuestraIniciativa;
                    diag.MuestraInteresHijo = MuestraInteresHijo;
                    diag.Musical = Musical;
                    diag.Necesidades = Necesidades;
                    diag.NEE = NEE;
                    diag.OperacionesContretas = OperacionesContretas;
                    diag.OperacionesFormales = OperacionesFormales;
                    diag.ParticipaActividadesEscuela = ParticipaActividadesEscuela;
                    diag.ProgramaBloque1 = ProgramaBloque1;
                    diag.ProgramaBloque2 = ProgramaBloque2;
                    diag.ProgramaBloque3 = ProgramaBloque3;
                    diag.ProgramaBloque4 = ProgramaBloque4;
                    diag.ProgramaBloque5 = ProgramaBloque5;
                    diag.ReconoceValia = ReconoceValia;
                    diag.Regular = Regular;
                    diag.RequiereApoyo = RequiereApoyo;
                    diag.RespetaAsiMismo = RespetaAsiMismo;
                    diag.RespetaReglas = RespetaReglas;
                    diag.ResuelveConfilctos = ResuelveConfilctos;
                    diag.ServicioMedico = ServicioMedico;
                    diag.Sexo = Sexo;
                    diag.SituacionRiesgo = SituacionRiesgo;
                    diag.Talentos = Talentos;
                    diag.Temeroso = Temeroso;
                    diag.TieneComputadora = TieneComputadora;
                    diag.VisualEspacial = VisualEspacial;
                    diag.ViveOtros = ViveOtros;
                    
                    

                    db.SaveChanges();
                }
                else
                {
                    crear();
                }

               
            }

        }

        public static List<DiagnosticoPsicologicoSimple> listar(Guid grupo)
        {
            using(SMTDevEntities db = new SMTDevEntities())
            {
                List<DiagnosticoPsicologicoSimple> result = db.DiagnosticoPsicologico
                    .Where(i => i.IDGrupo == grupo)
                    .Select(i => new DiagnosticoPsicologicoSimple()
                    {
                        Alumno = i.IDAlumno,
                        Aburrimiento = i.Aburrimiento,
                        AutonomiaVerguenza = i.AutonomiaVerguenza,
                        ComoSeTranslada = i.ComoSeTranslada,
                        ConductaInadecuada = i.ConductaInadecuada,
                        ConfianzaDesconfianza = i.ConfianzaDesconfianza,
                        CTE10 = i.CTE10,
                        CTE8 = i.CTE8,
                        CTE5 = i.CTE5,
                        CuentaConCasa = i.CuentaConCasa,
                        Edad = i.Edad,
                        EscolaridadPadres = i.EscolaridadPadres,
                        EtapaPreconvencional = i.EtapaPreconvencional,
                        EtapaSesioromotora = i.EtapaSesioromotora,
                        EvaluacionBloque1 = i.EvaluacionBloque1,
                        EvaluacionBloque2 = i.EvaluacionBloque2,
                        EvaluacionBloque3 = i.EvaluacionBloque3,
                        EvaluacionBloque4 = i.EvaluacionBloque4,
                        EvaluacionBloque5 = i.EvaluacionBloque5,
                        ExpresaEmociones = i.ExpresaEmociones,
                        FamiliaCompleta = i.FamiliaCompleta,
                        FamiliaMonoparental = i.FamiliaMonoparental,
                        FechaActualizacion = i.FechaActualizacion ?? DateTime.Now,
                        Generabilidad = i.Generabilidad,
                        Hermanos = i.Hermanos,
                        IdentidadConfusion = i.IdentidadConfusion,
                        Indiferencia = i.Indiferencia,
                        IniciativaCulpa = i.IniciativaCulpa,
                        Integridad = i.Integridad,
                        Interpersonal = i.Interpersonal,
                        IntimidadAislamiento = i.IntimidadAislamiento,
                        Intrapersonal = i.Intrapersonal,
                        KinestesicaCorporal = i.KinestesicaCorporal,
                        Laboriosidad = i.Laboriosidad,
                        Linguistica = i.Linguistica,
                        LogicoMatematica = i.LogicoMatematica,
                        LugarFamilia = i.LugarFamilia,
                        MoralidadConvencional = i.MoralidadConvencional,
                        MoralidadPreconvencional = i.MoralidadPreconvencional,
                        MoralidadPrincipiosAutonomos = i.MoralidadPrincipiosAutonomos,
                        Motivado = i.Motivado,
                        MuestraIniciativa = i.MuestraIniciativa,
                        MuestraInteresHijo = i.MuestraInteresHijo,
                        Musical = i.Musical,
                        Necesidades = i.Necesidades,
                        NEE = i.NEE,
                        OperacionesContretas = i.OperacionesContretas,
                        OperacionesFormales = i.OperacionesFormales,
                        ParticipaActividadesEscuela = i.ParticipaActividadesEscuela,
                        ProgramaBloque1 = i.ProgramaBloque1,
                        ProgramaBloque2 = i.ProgramaBloque2,
                        ProgramaBloque3 = i.ProgramaBloque3,
                        ProgramaBloque4 = i.ProgramaBloque4,
                        ProgramaBloque5 = i.ProgramaBloque5,
                        ReconoceValia = i.ReconoceValia,
                        Regular = i.Regular,
                        RequiereApoyo = i.RequiereApoyo,
                        RespetaAsiMismo = i.RespetaAsiMismo,
                        RespetaReglas = i.RespetaReglas,
                        ResuelveConfilctos = i.ResuelveConfilctos,
                        ServicioMedico = i.ServicioMedico,
                        Sexo = i.Sexo,
                        SituacionRiesgo = i.SituacionRiesgo,
                        Talentos = i.Talentos,
                        Temeroso = i.Temeroso,
                        TieneComputadora = i.TieneComputadora,
                        VisualEspacial = i.VisualEspacial,
                        ViveOtros = i.ViveOtros
            }).ToList();



                return result;
            }

        }


        public class DiagnosticoPsicologicoSimple
        {
            public Guid Alumno { get; set; }
            public string Aburrimiento { get; set; }
            public string ConductaInadecuada { get; set; }
            public int? Edad { get; set; }
            public string Indiferencia { get; set; }
            public string Interpersonal { get; set; }
            public string Intrapersonal { get; set; }
            public string KinestesicaCorporal { get; set; }
            public string Linguistica { get; set; }
            public string LogicoMatematica { get; set; }

            public string Motivado { get; set; }
            public string MuestraIniciativa { get; set; }
            public string Musical { get; set; }
            public string Necesidades { get; set; }
            public string NEE { get; set; }
            public string Regular { get; set; }
            public string RequiereApoyo { get; set; }
            public string Sexo { get; set; }
            public string SituacionRiesgo { get; set; }
            public string Talentos { get; set; }
            public string Temeroso { get; set; }
            public string VisualEspacial { get; set; }
            public string CTE10 { get; internal set; }
            public string CTE8 { get; internal set; }
            public string AutonomiaVerguenza { get; internal set; }
            public string ComoSeTranslada { get; internal set; }
            public string ConfianzaDesconfianza { get; internal set; }
            public string CTE5 { get; internal set; }
            public string CuentaConCasa { get; internal set; }
            public string EscolaridadPadres { get; internal set; }
            public string EtapaPreconvencional { get; internal set; }
            public string EtapaSesioromotora { get; internal set; }
            public string EvaluacionBloque1 { get; internal set; }
            public string EvaluacionBloque2 { get; internal set; }
            public string EvaluacionBloque3 { get; internal set; }
            public string EvaluacionBloque4 { get; internal set; }
            public string EvaluacionBloque5 { get; internal set; }
            public string ExpresaEmociones { get; internal set; }
            public string FamiliaCompleta { get; internal set; }
            public string FamiliaMonoparental { get; internal set; }
            public DateTime FechaActualizacion { get; internal set; }
            public string Generabilidad { get; internal set; }
            public int? Hermanos { get; internal set; }
            public string IdentidadConfusion { get; internal set; }
            public string IniciativaCulpa { get; internal set; }
            public string Integridad { get; internal set; }
            public string IntimidadAislamiento { get; internal set; }
            public string Laboriosidad { get; internal set; }
            public string LugarFamilia { get; internal set; }
            public string MoralidadConvencional { get; internal set; }
            public string MoralidadPreconvencional { get; internal set; }
            public string MoralidadPrincipiosAutonomos { get; internal set; }
            public string MuestraInteresHijo { get; internal set; }
            public string OperacionesContretas { get; internal set; }
            public string OperacionesFormales { get; internal set; }
            public string ParticipaActividadesEscuela { get; internal set; }
            public string ProgramaBloque1 { get; internal set; }
            public string ProgramaBloque2 { get; internal set; }
            public string ProgramaBloque3 { get; internal set; }
            public string ProgramaBloque4 { get; internal set; }
            public string ProgramaBloque5 { get; internal set; }
            public string ReconoceValia { get; internal set; }
            public string RespetaAsiMismo { get; internal set; }
            public string RespetaReglas { get; internal set; }
            public string ResuelveConfilctos { get; internal set; }
            public string ServicioMedico { get; internal set; }
            public string TieneComputadora { get; internal set; }
            public string ViveOtros { get; internal set; }
        }
    }
}