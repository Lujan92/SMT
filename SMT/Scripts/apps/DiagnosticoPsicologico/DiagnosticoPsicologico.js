var DiagnosticoPsicologico = new function () {

    var _datos = [];

    this.desplegarTabla = function (grupo) {

        var content = $('[data-psicologico="' + grupo + '"]').html('<h2 class="text-center"><span class="fa fa-refresh fa-spin"></span></h2>');

       
        Alumnos.listar(grupo).then(function (alumnos) {
            cargarLista(grupo).then(function (diagnosticos) {
                    

                content.html(Templates.items['tablaPsicologico']);


                alumnos.map(function (a) {

                    a.Aburrimiento = '';
                    a.AutonomiaVerguenza = '';
                    a.ComoSeTranslada = '';
                    a.ConductaInadecuada = '';
                    a.ConfianzaDesconfianza = '';
                    a.CTE10 = '';
                    a.CTE5 = '';
                    a.CTE8 = '';
                    a.CuentaConCasa = '';
                    a.Edad = '';
                    a.EscolaridadPadres = '';
                    a.EtapaPreconvencional = '';
                    a.EtapaSesioromotora = '';
                    a.EvaluacionBloque1 = '';
                    a.EvaluacionBloque2 = '';
                    a.EvaluacionBloque3 = '';
                    a.EvaluacionBloque4 = '';
                    a.EvaluacionBloque5 = '';
                    a.ExpresaEmociones = '';
                    a.FamiliaCompleta = '';
                    a.FamiliaMonoparental = '';
                    a.Generabilidad = '';
                    a.Hermanos = '';
                    a.IdentidadConfusion = '';
                    a.Indiferencia = '';
                    a.IniciativaCulpa = '';
                    a.Integridad = '';
                    a.Interpersonal = '';
                    a.IntimidadAislamiento = '';
                    a.Intrapersonal = '';
                    a.KinestesicaCorporal = '';
                    a.Laboriosidad = '';
                    a.Linguistica = '';
                    a.LogicoMatematica = '';
                    a.LugarFamilia = '';
                    a.MoralidadConvencional = '';
                    a.MoralidadPreconvencional = '';
                    a.MoralidadPrincipiosAutonomos = '';
                    a.Motivado = '';
                    a.MuestraIniciativa = '';
                    a.MuestraInteresHijo = '';
                    a.Musical = '';
                    a.Necesidades = '';
                    a.NEE = '';
                    a.OperacionesContretas = '';
                    a.OperacionesFormales = '';
                    a.ParticipaActividadesEscuela = '';
                    a.ProgramaBloque1 = '';
                    a.ProgramaBloque2 = '';
                    a.ProgramaBloque3 = '';
                    a.ProgramaBloque4 = '';
                    a.ProgramaBloque5 = '';
                    a.ReconoceValia = '';
                    a.Regular = '';
                    a.RequiereApoyo = '';
                    a.RespetaAsiMismo = '';
                    a.RespetaReglas = '';
                    a.ResuelveConfilctos = '';
                    a.ServicioMedico = '';
                    a.Sexo = '';
                    a.SituacionRiesgo = '';
                    a.Talentos = '';
                    a.Temeroso = '';
                    a.TieneComputadora = '';
                    a.VisualEspacial = '';
                    a.ViveOtros = '';

                    for (var d in diagnosticos) {
                        var diag = diagnosticos[d];
                        if (a.id == diag.Alumno) {
                            diag.id = diag.Alumno;
                            diag.nombre = a.nombre + ' ' + a.apellidoPaterno + ' ' + a.apellidoMaterno;
                            actualizarObjeto(a,diag);
                            break;
                        }
                    }
                });

                alumnos.map(function (alumno) {
                    content.find('tbody').append(Templates.items['rowPsicologico'].format(alumno));
                });


            });
        });
        

    

    }

    var actualizarObjeto = function (a,b) {
        for (var name in a) {
            if (b[name] != undefined && b[name] != null) {
                a[name] = b[name];
            }
            else {
                a[name] = '';
            }
        }
    }

    var cargarLista = function (grupo) {
        return new Promise(function (success) {

            var data = Cache.validarCache('psico' + grupo);

            if (data == false) {

                $.ajax({
                    url: '/DiagnosticosPsicologicos/listar',
                    type: 'get',
                    data: { grupo: grupo },
                    success: function (data) {
                        _datos = data;
                        Cache.almacenarCache(data, 'psico' + grupo, 1);
                        success(data);
                    }
                });
            }
            else {
                _datos = data;
                success(data);
            }
        });
       


    }

    this.Imprimir = function () {
        open('/DiagnosticosPsicologicos/imprimir?grupo=' + _grupo);
    }

    $('body:not(.visualizando)').delegate('td[data-campo]:not(.selected)', 'focusin', function () {
        $(this).addClass('selected');
        var valor = $(this).text();

        var respuesta = $(this).attr('data-respuesta');
        if (parseInt(respuesta) >= 1 && parseInt(respuesta) <= 11) {
            $(this).html(Templates.items['respuesta' + respuesta]);
            $(this).find('input,select').val(valor);
        }
        else{
            $(this).html('<input type="text" value="' + valor + '" class="from-control fom-control-oculto" />')
        }

        $(this).find('select').focus(function () {
            // Esto hace que se carge el select abierto.. no encontre otra manera
            $(this).attr('size', $(this).attr("expandto"));
        });
        $(this).find('input,select').focus();
    });

    $('body:not(.visualizando)').delegate('td[data-campo] input,td[data-campo] select', 'focusout', function () {
        $(this).parent().removeClass('selected');
        $(this).parent().html($(this).val());
        
    });

    $('body:not(.visualizando)').delegate('tr', 'focusout', function () {
        var tr = $(this);
        setTimeout(function () {
            if (tr.find('input').length == 0) {
                var params = [];

                params.push({
                    name: tr.data('campo'),
                    value: tr.data('id')
                });

                params.push({
                    name: 'IDGrupo',
                    value: _grupo
                });

                tr.find('[data-campo]').each(function () {

                    params.push({
                        name: $(this).data('campo'),
                        value: $(this).text()
                    });
                });

                tr.removeClass('success');
                $.ajax({
                    url: '/DiagnosticosPsicologicos/Actualizar',
                    type: 'post',
                    data: params,
                    beforeSend: function () {
                        tr.resaltar('warning',1500);
                    },
                    success: function (response) {
                        if (response.result == true) {
                            tr.resaltar('info',1000);
                            Cache.vaciar();
                        }
                        else {
                            tr.resaltar('danger',2000);
                        }
                    }
                });
            }
        },200);
        
    });

    Templates.load('tablaPsicologico', '/Scripts/apps/DiagnosticoPsicologico/views/Table.html');
    Templates.load('rowPsicologico', '/Scripts/apps/DiagnosticoPsicologico/views/Row.html');

    Templates.load('respuesta1', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta1.html');
    Templates.load('respuesta2', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta2.html');
    Templates.load('respuesta3', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta3.html');
    Templates.load('respuesta4', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta4.html');
    Templates.load('respuesta5', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta5.html');
    Templates.load('respuesta6', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta6.html');
    Templates.load('respuesta7', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta7.html');
    Templates.load('respuesta8', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta8.html');
    Templates.load('respuesta9', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta9.html');
    Templates.load('respuesta10', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta10.html');
    Templates.load('respuesta11', '/Scripts/apps/DiagnosticoPsicologico/views/Respuesta11.html');
}

