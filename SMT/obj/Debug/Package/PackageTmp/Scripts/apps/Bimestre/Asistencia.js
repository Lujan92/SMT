var Asistencia = new function () {
    var _a = this;

    var tdsCaptura = '',
        rowNumeros = '',
        rowNombres = '',
        rowInacisistencia = '',
        rowAsistencias = '';

    var generarNombreCache = function () {
        return 'asistencia-' + _grupo + '-'+_bimestre;
    }

    this.listar = function () {
        return new Promise(function (success) {
             _data = [];
            var consultaParcial = function ( page) {
                $.ajax({
                    url: '/asistencias/cargarSesiones',
                    data: {
                        grupo: _grupo,
                        bimestre:_bimestre,
                        page: page
                    },
                    beforeSend:function(){
                        Loading('Cargando asistencias');
                    },
                    error: function () {
                        success(_data);
                    },
                    success: function (data) {
                        _data = _data.concat(data);
                        if (data.length > 0) {
                            // Hacer recursivo hasta que ya no hallan registros
                            page++;
                            consultaParcial( page);
                        }
                        else {
                            Loading();
                            success(_data);
                        }
                    }
                });
            }

            // Se inicia el cargado de datos por partes
            consultaParcial(0);
        });
    }

    this.generarSesion = function (selector,sesion,grupo,focus,autoOrdenar) {
        Templates.load('rowAsistencia', '/Scripts/apps/Bimestre/views/rowAsistencia.html').then(function (template) {
            sesion.num = $(selector).find('[data-sesion-id]').length + 1;
            sesion.tds = tdsCaptura;
            sesion.grupo = grupo;

            var elementSesion = $(template.format(sesion)).appendTo($(selector).find('tbody'));

            // Actualizar los tds a los datos reales
            sesion.asistencia.map(function (a) {
                elementSesion.find('[data-alumno-id="' + a.id + '"]').attr('data-alumno-estado', a.estado).html('<div class="w100">' + obtenerHtmlEstado(a.estado) + '</div>');
            });

            // Se actualizan los totales de la sesion y la columna del alumno
            actualizarTotales(sesion.id);

            if ($('body').hasClass('visualizando') == false)
                pluginDatepicker(elementSesion.find('[name="fecha"]'), function (obj) {
                    var tr = $(obj).parents('[data-sesion-id]');
                    _a.editar(tr.attr('data-sesion-id'), tr.find('[name="fecha"]').val(), tr.find('[name="observacion"]').val());
                });
            else
                elementSesion.find('[name="fecha"]').attr('disabled',true);


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
        rowInacisistencia = '';

        Alumnos.listar(_grupo).then(function () {

            for (var i = 0; i < Alumnos.data.length; i++) {
                var alumn = Alumnos.data[i];
                rowNumeros += '<th><div class="w50">{0}</div></th>'.format([i + 1]);
                rowNombres += '<th><div class="w100"><span data-alumno="{id}" class="semaforo"></span>{apellidoPaterno} {apellidoMaterno} {nombre}</div></th>'.format(alumn);
                rowInacisistencia += '<td ><div data-alumno-inasistencia="{id}" class="w100">0</div></th>'.format(alumn);
                rowAsistencias += '<td><div data-alumno-asistencia="{id}" class="w100">0</div></th>'.format(alumn);
                tdsCaptura += '<td  ><div tabindex="0" data-alumno-id="{id}" data-alumno-estado="" class="w100"><span class="fa fa-check"></span></div></td>'.format(alumn);
            }

            Templates.load('tablaAsistencia', '/Scripts/apps/Bimestre/views/tablaAsistencia.html').then(function (template) {

                var datos = {
                    numerosHeaders: rowNumeros,
                    nombresHeaders: rowNombres,
                    rowAsistencia: rowAsistencias,
                    rowInacisistencia: rowInacisistencia
                };

                // Se despliega el esqueleto de la asitencias
                $(selector).html(template.format(datos));


                // Cargar y desplegar datos de sesiones
                _a.listar(grupo).then(function (data) {
                    data.map(function (s) {
                        _a.generarSesion(selector, s, grupo);
                    });

                    setTimeout(function () {
                        var selector = $('[data-tabla="asistencia"]').get(0);

                        Alumnos.data.map(function (alumno) {
                            var totalAsistenciaAlumno = selector.querySelectorAll('[data-alumno-id="' + alumno.id + '"]:not([data-alumno-estado="0"])').length;
                            var totalFaltasAlumno = selector.querySelectorAll('[data-alumno-id="' + alumno.id + '"][data-alumno-estado="0"]').length;

                            selector.querySelector('[data-alumno-asistencia="' + alumno.id + '"]').innerHTML = totalAsistenciaAlumno;
                            selector.querySelector('[data-alumno-inasistencia="' + alumno.id + '"]').innerHTML = totalFaltasAlumno;
                        });
                    }, 500);

                    $(selector).find('.loading').remove();

                });

            });
        });
    }

    var obtenerHtmlEstado = function (estado) {
        var estadosResultado = ['<span class="fa fa-close"></span>', '<span class="fa fa-check"></span>', 'RET', 'SUSP', 'JUST'];
 
        return estadosResultado[estado === '' ? 1 : estado];
    }

    var actualizarAsistencia = function (alumno,sesion,estado,grupo) {
       
        if (estado == '' || alumno == '' || sesion == '') {

            return;
        }

        // Brincarse inmediatamente al otro elemento sin esperar a que termine
        $('[data-sesion-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').focusout()
        if($('[data-sesion-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').parents('td:first').next('td').find('[tabindex]').length == 0){
            // En el ultimo td ya no hace el brinco hacia el textarea
            $('[data-sesion-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').parents('tr').find('textarea').focus();
        }
        else{
            $('[data-sesion-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').parents('td:first').next('td').find('[tabindex]').focus();
        }
                        

        $.ajax({
            url: '/asistencias/actualizarEstado',
            type: 'post',
            data: {
                alumno: alumno,
                sesion: sesion,
                estado:estado
            },
            beforeSend: function () {
                $('[data-sesion-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').html('<span class="fa fa-spin fa-refresh"></span>');
            },
            complete: function () {
                $('[data-sesion-id="' + sesion + '"] [data-alumno-id="' + alumno + '"] span.fa-spin').remove();
            },
            success: function (response) {
                if (response.result == true) {
                    // Se despliega la sesion actualizada con efecto
                    $('[data-sesion-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]')
                        .attr('data-alumno-estado', estado)
                        .html(obtenerHtmlEstado(estado))
                        .resaltar('info', 800);

                    // Se actualiza el dato actualizado en el cache
                    actualizaDataEnCache(grupo, alumno, sesion, estado);


                    Alumnos.cargarSemaforo('asistencia', alumno);
                    actualizarTotales(sesion);
                    setTimeout(function () {
                        var selector = $('[data-tabla="asistencia"]').get(0);

                        Alumnos.data.map(function (alumno) {
                            var totalAsistenciaAlumno = selector.querySelectorAll('[data-alumno-id="' + alumno.id + '"]:not([data-alumno-estado="0"])').length;
                            var totalFaltasAlumno = selector.querySelectorAll('[data-alumno-id="' + alumno.id + '"][data-alumno-estado="0"]').length;

                            selector.querySelector('[data-alumno-asistencia="' + alumno.id + '"]').innerHTML = totalAsistenciaAlumno;
                            selector.querySelector('[data-alumno-inasistencia="' + alumno.id + '"]').innerHTML = totalFaltasAlumno;
                        });
                    }, 200);
                }
                else {
                    AlertError(response.message);
                    $('[data-sesion-id="' + sesion + '"] [data-alumno-id="' + alumno + '"]').focusin();
                }
            }
        })
       
    }

    var actualizaDataEnCache = function () {
        // Se complica actualizar los datos, asi que se elimina el cache y la proxima vez se volvera a descargar todo
        Cache.vaciar(generarNombreCache());
    }

    var actualizarTotales = function (sesion) {
        var selector = $('[data-tabla="asistencia"]').get(0);
        var totalSesionAsistencia = selector.querySelectorAll('[data-sesion-id="' + sesion + '"] [data-alumno-id]:not([data-alumno-estado="0"])').length;
        var totalSesionAlumnos = selector.querySelectorAll('[data-sesion-id="' + sesion + '"] [data-alumno-id]').length;

        selector.querySelector('[data-sesion-total="' + sesion + '"]').innerHTML = totalSesionAsistencia;
        selector.querySelector('[data-sesion-promedio="' + sesion + '"]').innerHTML = (totalSesionAsistencia * 100 / totalSesionAlumnos).toFixed(2) + '%';
    }

    var ordenar = function (selector) {
        
        var sort_by_date = function (a, b) {
            var dateA = a.getAttribute('data-sesion-fecha').toDate2();
            var dateB = b.getAttribute('data-sesion-fecha').toDate2();

            return dateA < dateB ;
        }

        var list = $(selector).find('[dat<div class="w50">1</div>a-sesion-fecha]').get();
        list.sort(sort_by_date);
        for (var i = 0; i < list.length; i++) {
            list[i].parentNode.appendChild(list[i]);
            $(list[i]).find('td:first div').html('<a><span data-asitencia-option="eliminar" class="fa fa-trash visor-oculto" title="Eliminar sesión"></span></a> ' +( i + 1 ));
        }
    }

    this.nuevo = function (selector,estado) {
        $.ajax({
            url:'/asistencias/nuevo',
            type: 'post',
            data: {
                grupo: _grupo,
                bimestre: _bimestre,
                estado:estado
            },
            beforeSend: function () {
                Loading('Generando sesión');
            },
            complete: function () {
                Loading();
            },
            success: function (response) {
                if (response.result == true) {
                    
                    _a.generarSesion(selector, response.data, _grupo,true,true);

                    // Agregar al cache
                    var data = Cache.validarCache(generarNombreCache());
                    if (data != false) {
                        data.push(response.data);
                        Cache.almacenarCache(data, generarNombreCache(), 5);
                    }
                }
                else {
                    AlertError(response.message,'Asistencias');
                }
            }
        });
    }

    this.editar = function (id,fecha,observacion) {
        $.ajax({
            url: '/asistencias/editar',
            type: 'post',
            data: {
                id: id,
                fecha: fecha,
                observacion: observacion
            },
            success: function (response) {
                if (response.result == true) {
                    $('[data-sesion-id="' + id + '"]').attr('data-sesion-fecha',fecha).resaltar('info', 800);
                    Asistencia.desplegarResultados(_grupo, '#tabla-asistencia');
                    actualizaDataEnCache();
                }
                else {
                    AlertError(response.message,'Asistencias');
                }
            }
        });
    }

    this.eliminar = function (id) {
        ConfirmDialog.show({
            title: 'Eliminar sesión de asistencia',
            text:'<h3 class="text-center">Esta intentando eliminar una sesión permanentemente, la cual ya no se podrá recuperar. ¿Desea continuar?</h3>',
            positiveButtonClass: 'btn btn-danger',
            positiveButtonText: 'Si',
            negativeButtonClass: 'btn btn-success',
            negativeButtonText: 'No',
            closeModalOnAction: false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: '/asistencias/eliminar',
                        type: 'post',
                        data: {
                            id: id
                        },
                        beforeSend: function () {
                            $('#modalConfirm .modal-footer button[data-confirm="true"]').append(' <span class="fa fa-refresh fa-spin"></span>');
                            $('#modalConfirm .modal-footer button').attr('disabled',true);
                        },
                        complete: function () {
                            $('#modalConfirm .modal-footer button span').remove()
                            $('#modalConfirm .modal-footer button').attr('disabled',false);
                        },
                        success: function (response) {
                            if (response.result == true) {
                                $('[data-sesion-id="' + id + '"]').removeConEfecto();
                                AlertSuccess('Se ha eliminado la sesión', 'Asistencias');
                                ConfirmDialog.hide();
                                actualizaDataEnCache();
                            }
                            else {
                                AlertError(response.message, 'Asistencias');
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

    // Generar select para la captura
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-estado]', 'focusin', function () {
        var td = $(this);
        if (td.find('select').length == 0) {
            Templates.load('CapturaAsistencia', '/Scripts/apps/Bimestre/views/capturaAsistencia.html').then(function (template) {
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
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-estado] select', 'focusout', function () {
        
        var estado = $(this).parent('div').attr('data-alumno-estado');
        $(this).parent('div').html(obtenerHtmlEstado(estado));
    });

    // Cambiar el estado inmediatamente al seleccionar opcion
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-estado] select', 'change', function () {
        
        if ($(this).val() != '') {
            var alumno = $(this).parent('div').attr('data-alumno-id');
            var sesion = $(this).parents('tr').attr('data-sesion-id');
            var grupo = $(this).parents('tr').attr('data-grupo');
            actualizarAsistencia(alumno, sesion, $(this).val(),grupo);
        }
        else
            $(this).focusout();
    });

    $('body:not(.visualizando)').delegate('[data-sesion-id] [name="observacion"],[data-sesion-id] [name="fecha"]', 'change', function () {
        var tr = $(this).parents('[data-sesion-id]');
        _a.editar(tr.attr('data-sesion-id'), tr.find('[name="fecha"]').val(), tr.find('[name="observacion"]').val());
    });


    $('body:not(.visualizando)').delegate('[data-sesion-id] [data-asitencia-option="eliminar"]', 'click', function () {
        var tr = $(this).parents('[data-sesion-id]');
        _a.eliminar(tr.attr('data-sesion-id'));
    });


    // Se precarga template 
    Templates.load('rowAsistencia', '/Scripts/apps/Bimestre/views/rowAsistencia.html')

    this.Imprimir = function () {        
        open('/asistencias/imprimir?grupo=' + _grupo + '&bimestre=' + _bimestre);
    }
}


$('body:not(.visualizando)').delegate('[data-asistencia="nuevo"]', 'click', function () {
    Asistencia.nuevo('#tabla-asistencia', 1);
});

$('body:not(.visualizando)').delegate('[data-asistencia="suspencion"]', 'click', function () {
    Asistencia.nuevo('#tabla-asistencia', 3);
});