var Trabajo = new function () {
    var _a = this;

    var tdsCaptura = '',
        rowNumeros = '',
        rowNombres = '',
        rowInacisistencia = '',
        rowMedios='',
        rowAsistencias = '';

    var generarNombreCache = function () {
        return 'trabajo-' + _grupo + '-' + _bimestre;
    }

    this.listar = function () {
        return new Promise(function (success) {
           
            _data = [];
            var consultaParcial = function (page) {
                $.ajax({
                    url: '/Trabajos/cargarTrabajos',
                    data: {
                        grupo: _grupo,
                        bimestre: _bimestre,
                        page: page
                    },
                    beforeSend:function(){
                        Loading('Cargando trabajos');
                    },
                    error: function () {
                        success(_data);
                    },
                    success: function (data) {
                        _data = _data.concat(data);
                        if (data.length > 0) {
                            // Hacer recursivo hasta que ya no hallan registros
                            page++;
                            consultaParcial(page);
                        }
                        else {
                            Loading();
                            success(_data);
                        }
                    }
                });
            }

            // Se inicia el cargado de datos por partes
            consultaParcial(1);
            
        });
    }

    this.generarTrabajo = function (selector, trabajo, grupo, focus, autoOrdenar) {

        Templates.load('rowTrabajo', '/Scripts/apps/Bimestre/views/rowTrabajo.html').then(function (template) {

            trabajo.num = $(selector).find('[data-trabajo-id]').length + 1;
            trabajo.tds = tdsCaptura;
            trabajo.grupo = grupo;

            var elementSesion = $(template.format(trabajo)).appendTo($(selector).find('tbody'));


            // Actualizar los tds a los datos reales
            trabajo.entrega.map(function (a) {
                elementSesion.find('[data-alumno-id="' + a.id + '"]').attr('data-alumno-trabajo-estado', a.estado).html('<div class="w100">'+obtenerHtmlEstado(a.estado)+'</div>');
            });

            // Se actualizan los totales de la sesion y la columna del alumno
            actualizarTotales(trabajo.id);

            if($('body').hasClass('visualizando') == false)
                pluginDatepicker(elementSesion.find('[name="fecha"]'), function (obj) {
                    var tr = $(obj).parents('[data-trabajo-id]');

                    _a.editar(tr.attr('data-trabajo-id'), tr.find('[name="fecha"]').val(), tr.find('[name="observacion"]').val(), tr.find('[name="nombre"]').val(), tr.find('[name="tipo"]').val(), tr.find('[name="actividad"]').val());

                });
            else
                elementSesion.find('input,textarea').attr('disabled',true)

          

            if (focus == true) {
                $('html, body').animate({
                    scrollTop: elementSesion.offset().top
                }, 1000);

                elementSesion.resaltar('info', 3000)
                             .find('input:first')
                             .focus();
            }
        });




    }

    this.desplegarResultados = function (grupo, selector) {

        // Se generan rows y tds en base a los alumnos

        // Estos html se generaran cada vez que se ejecute desplegar resultados, 
        // una vez generado ya todas las acciones que requieran estos html solo utilizaran el html que se genero
        rowNombres = '';
        rowNumeros = '';
        tdsCaptura = '';
        rowAsistencias = '';
        rowMedios = '';

        rowInacisistencia = '';

        Alumnos.listar(_grupo).then(function () {

            for (var i = 0; i < Alumnos.data.length; i++) {
                var alumn = Alumnos.data[i];
                rowNumeros += '<th>{0}</th>'.format([i + 1]);
                rowNombres += '<th><div class="w100"><span data-alumno="{id}" class="semaforo" style="background-color:{semaforo}"></span><span class="nombre">{apellidoPaterno} {apellidoMaterno} {nombre}</span></div></th>'.format(alumn);
                rowInacisistencia += '<td><div data-alumno-noentrego="{id}" class="w100">0</div></th>'.format(alumn);
                rowAsistencias += '<td><div data-alumno-entrego="{id}" class="w100">0</div></th>'.format(alumn);
                rowMedios += '<td><div data-alumno-medio="{id}" class="w100">0</div></th>'.format(alumn);
                tdsCaptura += '<td><div data-alumno-id="{id}" data-alumno-trabajo-estado="" tabindex="0" class="w100"><span class="fa fa-check"></span></div></td>'.format(alumn);
            }

            Templates.load('tablaTrabajo', '/Scripts/apps/Bimestre/views/tablaTrabajo.html').then(function (template) {

                var datos = {
                    numerosHeaders: rowNumeros,
                    nombresHeaders: rowNombres,
                    rowAsistencia: rowAsistencias,
                    rowMedio: rowMedios,
                    rowInacisistencia: rowInacisistencia
                };

                $(selector).html(template.format(datos));

                _a.listar(grupo).then(function (data) {
                    data.map(function (s) {
                        _a.generarTrabajo(selector, s, grupo);
                    });

                    $(selector).find('.loading').remove();


                });


            });
        });
    }

    var obtenerHtmlEstado = function (estado) {
        var estadosResultado = ['<span class="fa fa-close"></span>', '<span class="fa fa-check"></span>', '&frac12'];
        if (estado == null) {
            return '<span class="fa fa-close"></span>';
        }
        return estadosResultado[estado];
    }

    var actualizarAsistencia = function (alumno, sesion, estado, grupo) {

        if (estado == '' || alumno == '' || sesion == '') {

            return;
        }

        // Brincarse inmediatamente al otro elemento sin esperar a que termine
        $('[data-trabajo-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').focusout()
        if ($('[data-trabajo-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').parents('td:first').next().find('[tabindex]').length == 0) {
            // En el ultimo td ya no hace el brinco hacia el textarea
            $('[data-trabajo-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').parent().find('textarea').focus();
        }
        else {
            $('[data-trabajo-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').parents('td:first').next().find('[tabindex]').focus();
        }


        $.ajax({
            url: '/Trabajos/actualizarEstado',
            type: 'post',
            data: {
                alumno: alumno,
                sesion: sesion,
                estado: estado
            },
            beforeSend: function () {
                $('[data-trabajo-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').html('<span class="fa fa-spin fa-refresh"></span>');
            },
            complete: function () {
                $('[data-trabajo-id="' + sesion + '"] [data-alumno-id="' + alumno + '"] span.fa-spin').remove();
            },
            success: function (response) {
                if (response.result == true) {
                    // Se despliega la sesion actualizada con efecto
                    $('[data-trabajo-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]')
                        .attr('data-alumno-trabajo-estado', estado)
                        .html('<div class="w100">' +obtenerHtmlEstado(estado) + '</div>')
                        .resaltar('info', 800);

                    // Se actualiza el dato actualizado en el cache
                    actualizaDataEnCache(grupo, alumno, sesion, estado);

                    actualizarTotales(sesion);
                    Alumnos.cargarSemaforo('trabajos', alumno);
                }
                else {
                    AlertError(response.message);
                    $('[data-trabajo-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').focusin();
                }
            }
        })

    }

    var actualizaDataEnCache = function () {
        // Se complica actualizar los datos, asi que se elimina el cache y la proxima vez se volvera a descargar todo
        Cache.vaciar(generarNombreCache());
    }

    var actualizarTotales = function (sesion) {
        var selector = $('[data-tabla="trabajo"]');

        Alumnos.data.map(function (alumno) {
            var totalAsistenciaAlumno = $(selector).find('[data-alumno-id="' + alumno.id + '"][data-alumno-trabajo-estado="1"]').length;
            var totalFaltasAlumno = $(selector).find('[data-alumno-id="' + alumno.id + '"][data-alumno-trabajo-estado="0"]').length;
            var totalMedio = $(selector).find('[data-alumno-id="' + alumno.id + '"][data-alumno-trabajo-estado="2"]').length;

            $(selector).find('[data-alumno-entrego="' + alumno.id + '"]').html(totalAsistenciaAlumno);
            $(selector).find('[data-alumno-noentrego="' + alumno.id + '"]').html(totalFaltasAlumno);
            $(selector).find('[data-alumno-medio="' + alumno.id + '"]').html(totalMedio);
        });

        var totalSesionAsistencia = $(selector).find('[data-trabajo-id="' + sesion + '"] [data-alumno-id][data-alumno-trabajo-estado="1"]').length*10;
        var totalnoentregado = $(selector).find('[data-trabajo-id="' + sesion + '"] [data-alumno-id][data-alumno-trabajo-estado="0"]').length*0;
        var totalmedio = $(selector).find('[data-trabajo-id="' + sesion + '"] [data-alumno-id][data-alumno-trabajo-estado="2"]').length*5;
        var totalSesionAlumnos = $(selector).find('[data-trabajo-id="' + sesion + '"] [data-alumno-id]').length;
        $(selector).find('[data-trabajo-total="' + sesion + '"]').html((totalSesionAsistencia + totalnoentregado + totalmedio)/10.0);
        $(selector).find('[data-trabajo-promedio="' + sesion + '"]').html((((totalSesionAsistencia + totalnoentregado + totalmedio) / (totalSesionAlumnos*10))* 100).toFixed(2) + '%');
    }

    var ordenar = function (selector) {

        var sort_by_date = function (a, b) {
            var dateA = a.getAttribute('data-trabajo-ticks');
            var dateB = b.getAttribute('data-trabajo-ticks');

            return dateA < dateB;
        }

        var list = $(selector).find('[data-trabajo-fecha]').get();
        list.sort(sort_by_date);
        for (var i = 0; i < list.length; i++) {
            list[i].parentNode.appendChild(list[i]);
            var hay = $(list[i]).find('td:first').find('span');
            if (hay.length == 0) {
                $(list[i]).find('td:first').find('div').append('<a><span data-trabajo-option="editardescripcion" class="fa fa-edit visor-oculto" title="Editar Descripción"></span></a><a><span data-asitencia-option="eliminar" class="fa fa-trash visor-oculto" title="Eliminar sesión"></span></a> ');
                
            }

        }
    }

    this.nuevo = function (selector, estado,tipo,actividad) {
        $.ajax({
            url: '/trabajos/nuevo',
            type: 'post',
            data: {
                grupo: _grupo,
                bimestre: _bimestre,
                estado: estado,
                tipo: tipo,
                actividad:actividad
            },
            beforeSend: function () {
                Loading('Generando sesión');
            },
            complete: function () {
                Loading();
            },
            success: function (response) {
                if (response.result == true) {

                    _a.generarTrabajo(selector, response.data, _grupo, true, true);

                    // Agregar al cache
                    var data = Cache.validarCache(generarNombreCache());
                    if (data != false) {
                        data.push(response.data);
                        Cache.almacenarCache(data, generarNombreCache(), 5);
                    }
                }
                else {
                    AlertError(response.message, 'Trabajos');
                }
            }
        });
    }

    this.editar = function (id, fecha, observacion, nombre,tipo,actividad) {
        $.ajax({
            url: '/Trabajos/editar',
            type: 'post',
            data: {
                id: id,
                fecha: fecha,
                observacion: observacion,
                nombre: nombre,
                tipo: tipo,
                actividad:actividad
            },
            success: function (response) {
                if (response.result == true) {
                    $('[data-trabajo-id="' + id + '"]').attr('data-trabajo-fecha', fecha).resaltar('info', 800);
                    actualizaDataEnCache();
                    Trabajo.desplegarResultados(_grupo, '#tabla-trabajo');
                }
                else {
                    AlertError(response.message, 'Asistencias');
                }
            }
        });
    }

    this.eliminar = function (id) {
        ConfirmDialog.show({
            title: 'Eliminar Trabajo',
            text: '<h3 class="text-center">Esta intentando eliminar un trabajo permanentemente, el cual ya no se podrá recuperar. ¿Desea continuar?</h3>',
            positiveButtonClass: 'btn btn-danger',
            positiveButtonText: 'Si',
            negativeButtonClass: 'btn btn-success',
            negativeButtonText: 'No',
            closeModalOnAction: false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: '/Trabajos/eliminar',
                        type: 'post',
                        data: {
                            id: id
                        },
                        beforeSend: function () {
                            $('#modalConfirm .modal-footer button[data-confirm="true"]').append(' <span class="fa fa-refresh fa-spin"></span>');
                            $('#modalConfirm .modal-footer button').attr('disabled', true);
                        },
                        complete: function () {
                            $('#modalConfirm .modal-footer button span').remove()
                            $('#modalConfirm .modal-footer button').attr('disabled', false);
                        },
                        success: function (response) {
                            if (response.result == true) {
                                $('[data-trabajo-id="' + id + '"]').removeConEfecto();
                                AlertSuccess('Se ha eliminado el trabajo', 'Trabajos');
                                ConfirmDialog.hide();
                                //ordenar('#tabla-trabajo');
                                actualizaDataEnCache();
                            }
                            else {
                                AlertError(response.message, 'Trabajos');
                            }

                        }
                    });
                }
                else {
                    ConfirmDialog.hide();
                }
            }
        });

    }

    this.modaleditar=function(id){
        Loading("Cargando");
        $.ajax({
            type: 'GET',
            url: '/Trabajos/CargarTrabajo',
            data: {                
                ID: id,
            }
        })
        .done(function (data) {
            $('#formularioTrabajo .modal-body').empty(data);
            $('#formularioTrabajo .modal-body').append(data);
            Loading();
            $('#formularioTrabajo').modal("show")
        });
    }

    this.guardar = function () {
        var form = $("#formularioTrabajo form");
        if (form.valid()) {
            $.ajax({
                url: '/Trabajos/GuardarTrabajo',
                data: form.serializeArray(),
                method: "POST",
                beforeSend: function () {
                    Loading('Guardando');
                },
                success: function (data) {
                    Loading();
                    if (data > 0) {
                        AlertSuccess('Se ha guardado el registro')
                        $('#formularioTrabajo').modal("hide");                        
                    } else {
                        AlertError('No se ha podido guardar: ' + data);

                    }
                }
            })
        }
    }



    // Generar select para la captura
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-trabajo-estado]', 'focusin', function () {
        var td = $(this);
        if (td.find('select').length == 0) {
            Templates.load('CapturaTrabajo', '/Scripts/apps/Bimestre/views/CapturaTrabajo.html').then(function (template) {
                td.empty();
                $(template).appendTo(td)
                    .focus(function () {
                        // Esto hace que se carge el select abierto.. no encontre otra manera
                        $(this).attr('size', $(this).attr("expandto"));
                    })
                    .focus();
            });
        }
    });

    // Reestablecer el td para mostrar unicamente el texto del estado
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-trabajo-estado] select', 'focusout', function () {

        var estado = $(this).parent('div').attr('data-alumno-trabajo-estado');
        $(this).parent('div').html(obtenerHtmlEstado(estado));
    });

    // Cambiar el estado inmediatamente al seleccionar opcion
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-trabajo-estado] select', 'change', function () {

        if ($(this).val() != '') {
            var alumno = $(this).parent('div').attr('data-alumno-id');
            var sesion = $(this).parents('tr').attr('data-trabajo-id');
            var grupo = $(this).parents('tr').attr('data-grupo');
            actualizarAsistencia(alumno, sesion, $(this).val(), grupo);
        }
        else
            $(this).focusout();
    });

    $('body:not(.visualizando)').delegate('[data-trabajo-id] [name="observacion"],[data-trabajo-id] [name="fecha"],[data-trabajo-id] [name="nombre"],[data-trabajo-id] [name="tipo"],[data-trabajo-id] [name="actividad"]', 'change', function () {
        var tr = $(this).parents('[data-trabajo-id]');

        _a.editar(tr.attr('data-trabajo-id'), tr.find('[name="fecha"]').val(), tr.find('[name="observacion"]').val(), tr.find('[name="nombre"]').val(), tr.find('[name="tipo"]').val(), tr.find('[name="actividad"]').val());
    });


    $('body:not(.visualizando)').delegate('[data-trabajo-id] [data-asitencia-option="eliminar"]', 'click', function () {
        var tr = $(this).parents('[data-trabajo-id]');
        _a.eliminar(tr.attr('data-trabajo-id'));
    });
    $('body:not(.visualizando)').delegate('[data-trabajo-id] [data-trabajo-option="editardescripcion"]', 'click', function () {
        var tr = $(this).parents('[data-trabajo-id]');
        _a.modaleditar(tr.attr('data-trabajo-id'));
    });

    
    // Se precarga template 
    Templates.load('rowTrabajo', '/Scripts/apps/Bimestre/views/rowTrabajo.html')

    this.Imprimir = function () {
        open('/trabajos/imprimir?grupo=' + _grupo + '&bimestre=' + _bimestre);
    }

    this.exportar = function () {

        // Se clona porque se va a editar el html para transformar los inputs en texto
        var tabla = $('#tabla-trabajo table').clone();

        tabla.find('td').each(function () {
            var td = $(this);
            td.find('input,textarea').each(function(){
                td.append($(this).val());
                $(this).remove();
            });

            td.find('.fa-check, .fa-close').each(function () {
                if ($(this).hasClass('fa-check'))
                    td.append(String.fromCharCode(10003));
                else
                    td.append(String.fromCharCode(10005));
                $(this).remove();
            });
        });

        tableToExcel(tabla[0], 'Trabajos');
    }

 
}

$('body:not(.visualizando)').delegate('[data-trabajo="nuevo"]', 'click', function () {
    Trabajo.nuevo('#tabla-trabajo', 1);
});

$('body:not(.visualizando)').delegate('[data-trabajo="suspencion"]', 'click', function () {
    Trabajo.nuevo('#tabla-trabajo', 3);
});