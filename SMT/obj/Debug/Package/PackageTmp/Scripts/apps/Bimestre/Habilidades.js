var Habilidades = new function () {
    var _a = this;

    var tdsCaptura = '',
        rowNumeros = '',
        rowNombres = '',
        rowInacisistencia = '',
        rowMedios='',
        rowAsistencias = '';

    var generarNombreCache = function () {
        return 'habilidades-' + _grupo + '-' + _bimestre;
    }

    this.listar = function () {
        return new Promise(function (success) {
            var _data = Cache.validarCache(generarNombreCache());            
            if (_data == false) {
                _data = [];
                var consultaParcial = function (page) {
                    $.ajax({
                        url: '/Habilidades/cargarHabilidades',
                        data: {
                            grupo: _grupo,
                            bimestre: _bimestre,
                            page: page
                        },
                        error: function () {
                            success(_data);
                        },
                        success: function (data) {
                            _data = _data.concat(data);
                            
                            Cache.almacenarCache(_data, generarNombreCache(), 1);
                            success(_data);
                            
                        }
                    });
                }

                // Se inicia el cargado de datos por partes
                consultaParcial(0);
            }
            else {
                success(_data);
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

        for (var i = 0; i < Alumnos.data.length; i++) {
            var alumn = Alumnos.data[i];
            rowNumeros += '<th>{0}</th>'.format([i + 1]);
            rowNombres += '<th>{apellidoPaterno} {apellidoMaterno} {nombre}</th>'.format(alumn);
            rowInacisistencia += '<td data-alumno-noentrego="{id}">0</th>'.format(alumn);
            rowAsistencias += '<td data-alumno-entrego="{id}">0</th>'.format(alumn);
            rowMedios += '<td data-alumno-medio="{id}">0</th>'.format(alumn);
            tdsCaptura += '<td data-alumno-id="{id}" data-alumno-habilidadid="" data-clase="" data-alumno-habilidad="" data-alumno-habilidades-estado="" tabindex="0"><span class="fa fa-check"></span></td>'.format(alumn);
        }

        Templates.load('tablaHabilidades', '/Scripts/apps/Bimestre/views/tablaHabilidades.html').then(function (template) {

            var datos = {
                numerosHeaders: rowNumeros,
                nombresHeaders: rowNombres,
                rowAsistencia: rowAsistencias,
                rowMedio: rowMedios,
                rowInacisistencia: rowInacisistencia
            };

            // Se despliega el esqueleto de la asitencias
            $(selector).empty().html(template.format(datos));


            // Cargar y desplegar datos de sesiones
            _a.listar(grupo).then(function (data) {
                
                _a.generarTrabajo(selector, data, grupo);
                

                $(selector).find('.loading').remove();

                setTimeout(function () {
                    // Ligero delay para que se termine de generar las sesiones
                    
                }, 500);
            });

        });
    }

    this.generarTrabajo = function (selector, habilidades, grupo, focus, autoOrdenar) {

        Templates.load('rowHabilidades', '/Scripts/apps/Bimestre/views/rowHabilidades.html').then(function (template) {
            $(selector).find('tbody').empty();
            habilidades.num = $(selector).find('[data-habilidades-id]').length + 1;
            habilidades.tds = tdsCaptura;
            habilidades.grupo = grupo;
            var nombres = ["Se involucra en clase", "Requiere Apoyo Matemáticas ", "Requiere Apoyo Escritura", "Requiere Apoyo Lectura", "Argumentacion ", "Sintesis", "Conocimiento", "Coevaluación", "Autoevaluación"];
            for (var i = 1; i <=9; i++) {                
                habilidades["grupo"] = grupo;
                habilidades["id"] = i;
                habilidades["nombre"] = nombres[i - 1];
                
                var elementSesion = $(template.format(habilidades)).prependTo($(selector).find('tbody'));
                var tipo = 0;
                var clase = 0;
                habilidades.map(function (k) {
                    var estado = "";
                    switch (i) {
                        case 1:
                            estado = k.SeInvolucraClase;
                            tipo = 3;
                            clase = 3;
                            break;
                        case 2:
                            estado = k.ApoyoMatematicas;
                            tipo = 1;
                            clase = 3;
                            break;
                        case 3:
                            estado = k.ApoyoEscritura;
                            tipo = 1;
                            clase = 3;
                            break;
                        case 4:
                            estado = k.ApoyoLectura;
                            tipo = 1;
                            clase = 3;
                            break;
                        case 5:
                            estado = k.Argumentacion;
                            tipo = 2;
                            clase = 2;
                            elementSesion.addClass("separadorHabilidades");
                            break;
                        case 6:
                            estado = k.Sintesis;
                            tipo = 2;
                            clase = 2;
                            break;
                        case 7:
                            estado = k.Conocimiento;
                            tipo = 2;
                            clase = 2;
                            break;                        
                        case 8:
                            estado = k.Coevaluacion;
                            tipo = 1;
                            clase = 1;
                            elementSesion.addClass("separadorHabilidades");
                            break;
                        case 9:
                            estado = k.Autoevaluacin;
                            tipo = 1;
                            clase = 1;
                            elementSesion.addClass("separadorHabilidades");
                            break;                        
                    }                    
                    if (estado == null || estado==undefined) {
                        estado = "";
                    }
                    elementSesion.find('[data-alumno-id="' + k.IDAlumno + '"]').attr('data-clase', clase);
                    elementSesion.find('[data-alumno-id="' + k.IDAlumno + '"]').attr('data-alumno-habilidad', tipo);
                    elementSesion.find('[data-alumno-id="' + k.IDAlumno + '"]').attr('data-alumno-habilidadid', i);
                    
                    elementSesion.find('[data-alumno-id="' + k.IDAlumno + '"]').attr('data-alumno-habilidades-estado', estado).html(obtenerHtmlEstado(estado, tipo));
                    if (i!=1) {
                        actualizarTotales(i);
                    }

                    if ($('body').hasClass('visualizando'))
                        elementSesion.find('input').attr('disabled',true);
                    
                })
            }

            

            // Se actualizan los totales de la sesion y la columna del alumno
            
            ordenar(selector);
            

            if (focus == true) {
                $('html, body').animate({
                    scrollTop: elementSesion.offset().top
                }, 1000);

                elementSesion.resaltar('info', 3000)
                             .find('input:first')
                             .focus();
            }

            var contador = 1;
            $(".separadorHabilidades").each(function () {
                if ($(this).next(".separador").length == 0 && $(this).prev(".separador").length == 0) {

                    switch (contador) {
                        case 1:
                            var sim = $("<tr class='separador'><td colspan='1000'><h4 class='pull-left'>Evaluación</h4></td></tr>");
                            sim.insertBefore($(this));
                            break;
                        case 2:
                            var sim = $("<tr class='separador'><td colspan='1000'><h4 class='pull-left'>Comprensión de Lectora</h4></td></tr>");
                            sim.insertAfter($(this));
                            break;
                        case 3:
                            var sim = $("<tr class='separador'><td colspan='1000'><h4 class='pull-left'>Apoyo</h4></td></tr>");
                            sim.insertAfter($(this));
                            break;
                    }
                }
                contador++;
            })
        });
    }

    var ordenar = function (selector) {        

        //var list = $(selector).find('[data-habilidades-fecha]').get();        
        //for (var i = 0; i < list.length; i++) {
        //    list[i].parentNode.appendChild(list[i]);
        //    var hay = $(list[i]).find('td:first').find('span');
        //    if (hay.length == 0) {
        //        //$(list[i]).find('td:first').append('<a><span data-habilidades-option="eliminar" class="fa fa-trash" title="Eliminar sesión"></span></a> ');

        //    }

        //}
    }

    var obtenerHtmlEstado = function (estado, tipo) {
        var estadosResultado = ['<span class="fa fa-close"></span>', '<span class="fa fa-check"></span>', '&frac12'];
        if (estado == null || estado=="") {
            return ' ';
        }
        if (estado==true || estado=='true') {
            return 'Si';
        } else if (estado == false || estado == 'false') {
            return 'No';
        }
        return estado;
    }

    var actualizarAsistencia = function (alumno, sesion, estado, grupo, habilidad) {

        if (estado == '' || alumno == '' || sesion == '') {

            return;
        }

        // Brincarse inmediatamente al otro elemento sin esperar a que termine
        $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').focusout()
        
        var clase = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').data("clase");
        var cellIndex = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').index()
        switch (clase) {
            case 1:
                var nxt = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').closest('tr').next().children().eq(cellIndex);
                var nxtClase = nxt.data("clase");
                if (nxtClase==1) {
                    nxt.focus();
                }
                else if ($('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').next('td[tabindex]').length > 0) {
                    nxt = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').closest('tr').prev().children().eq(cellIndex + 1);
                    nxt.focus();
                }
                else {
                    nxt = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').closest('tr').next().next().children().eq(1)
                    nxt.focus();
                }
                break;
            case 2:
                var nxt = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').closest('tr').next().children().eq(cellIndex);
                var nxtClase = nxt.data("clase");
                if (nxtClase == 2) {
                    nxt.focus();

                }
                else if ($('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').next('td[tabindex]').length > 0) {
                    nxt = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').closest('tr').prev().prev().children().eq(cellIndex + 1);
                    nxt.focus();
                }
                else {
                    nxt = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').closest('tr').next().next().children().eq(1)
                    nxt.focus();
                }
                break;
            case 3:
                var nxt = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').closest('tr').next().children().eq(cellIndex);
                var nxtClase = nxt.data("clase");
                if (nxtClase == 3) {
                    nxt.focus();
                } else {
                    nxt = $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').closest('tr').prev().prev().prev().children().eq(cellIndex + 1);
                    nxt.focus();
                }
                break;
        
        }
            
            
        


        $.ajax({
            url: '/Habilidades/actualizarEstado',
            type: 'post',
            data: {
                alumno: alumno,
                sesion: sesion,
                estado: estado,
                habilidad: habilidad,
                bimestre: _bimestre,
                grupo: grupo

            },
            beforeSend: function () {
                $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').html('<span class="fa fa-spin fa-refresh"></span>');
            },
            complete: function () {
                $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"] span.fa-spin').remove();
            },
            success: function (response) {
                if (response.result == true) {
                    // Se despliega la sesion actualizada con efecto
                    $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]')
                        .attr('data-alumno-habilidades-estado', estado)
                        .html(obtenerHtmlEstado(estado))
                        .resaltar('info', 800);

                    // Se actualiza el dato actualizado en el cache
                    actualizaDataEnCache(grupo, alumno, sesion, estado);

                    actualizarTotales(habilidad);
                }
                else {
                    AlertError(response.message);
                    $('[data-alumno-habilidadid="' + habilidad + '"][data-alumno-id="' + alumno + '"]').focusin();
                }
            }
        })

    }

    var actualizaDataEnCache = function () {
        // Se complica actualizar los datos, asi que se elimina el cache y la proxima vez se volvera a descargar todo
        Cache.vaciar(generarNombreCache());
    }

   
    var actualizarTotales = function (sesion) {
        var selector = $('[data-tabla="habilidades"]');
        if (sesion != 1) {



            var totalSesionAsistencia = $(selector).find('[data-alumno-habilidadid="' + sesion + '"]').length;
            var total = 0
            $(selector).find('[data-alumno-habilidadid="' + sesion + '"]').each(function () {
                var val = $(this).attr("data-alumno-habilidadid-estado");
                if (val!=null) {
                    total += parseInt(val) ;    
                }
                
            });
            var promedio = (((total) / (totalSesionAsistencia * 10)) * 100).toFixed(2);
            
            $(selector).find('[data-habilidades-total="' + sesion + '"]').html(total>0?total:0);
            $(selector).find('[data-habilidades-promedio="' + sesion + '"]').html(promedio != "NaN" ? promedio + "%" : 0);
        }
        
    }
   
   

    this.editar = function (id, fecha, observacion, nombre) {
        $.ajax({
            url: '/Habilidades/editar',
            type: 'post',
            data: {
                id: id,
                fecha: fecha,
                observacion: observacion,
                nombre: nombre
            },
            success: function (response) {
                if (response.result == true) {
                    $('[data-habilidades-id="' + id + '"]').attr('data-habilidades-fecha', fecha).resaltar('info', 800);                    
                    actualizaDataEnCache();
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
            text: '<h3 class="text-center">Esta intentando eliminar una sesión permanentemente, la cual ya no se podrá recuperar. ¿Desea continuar?</h3>',
            positiveButtonClass: 'btn btn-danger',
            positiveButtonText: 'Si',
            negativeButtonClass: 'btn btn-success',
            negativeButtonText: 'No',
            closeModalOnAction: false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: '/Habilidades/eliminar',
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
                                $('[data-habilidades-id="' + id + '"]').removeConEfecto();
                                AlertSuccess('Se ha eliminado la sesión', 'Habilidades');
                                ConfirmDialog.hide();                                
                                actualizaDataEnCache();
                            }
                            else {
                                AlertError(response.message, 'Habilidades');
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
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-habilidades-estado]', 'focusin', function () {
        var td = $(this);
        var url = '';        
        if (td.find('select').length == 0) {
            switch ($(this).attr("data-alumno-habilidad")) {
                case "1":
                    url='/Scripts/apps/Bimestre/views/CapturaHabilidades1.html';
                    break;
                case "2":
                    url = '/Scripts/apps/Bimestre/views/CapturaHabilidades2.html';
                    break;
                case "3":
                    url = '/Scripts/apps/Bimestre/views/CapturaHabilidades3.html';
                    break;        
            }
                  
            Templates.load('CapturaHabilidades' + $(this).attr("data-alumno-habilidad"), url).then(function (template) {
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
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-habilidades-estado] select', 'focusout', function () {

        var estado = $(this).parents('td').attr('data-alumno-habilidades-estado');
        $(this).parents('td').html(obtenerHtmlEstado(estado));
    });

    // Cambiar el estado inmediatamente al seleccionar opcion
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-habilidades-estado] select', 'change', function () {

        if ($(this).val() != '') {
            var alumno = $(this).parents('td').attr('data-alumno-id');
            var sesion = $(this).parents('tr').attr('data-habilidades-id');
            var grupo = $(this).parents('tr').attr('data-grupo');
            var habilidad = $(this).parents('td').attr('data-alumno-habilidadid');
            actualizarAsistencia(alumno, sesion, $(this).val(), grupo, habilidad);
        }
        else
            $(this).focusout();
    });

    $('body:not(.visualizando)').delegate('[data-habilidades-id] [name="observacion"],[data-habilidades-id] [name="fecha"],[data-habilidades-id] [name="nombre"]', 'change', function () {
        var tr = $(this).parents('[data-habilidades-id]');

        var fecha = tr.find('[name="fecha"]').val();
        if (tr.find('[name="fecha"]').attr('type') != 'text') {
            fecha = fecha.substr(8, 2) + '-' +
                    fecha.substr(5, 2) + '-' +
                    fecha.substr(0, 4);
        }

        _a.editar(tr.attr('data-habilidades-id'), fecha, tr.find('[name="observacion"]').val(), tr.find('[name="nombre"]').val());
    });

    // Convertir el text a date
    $('body:not(.visualizando)').delegate('[data-habilidades-id] [name="fecha"]', 'focusin', function () {
        if ($(this).attr('type') != 'date') {
            var valor = this.value;

            valor = valor.substr(6, 4) + '-' +
                    valor.substr(3, 2) + '-' +
                    valor.substr(0, 2);

            $(this).val(valor).attr('type', 'date').css('width', 160);
        }
    });

    // Convertir el date a text
    $('body:not(.visualizando)').delegate('[data-habilidades-id] [name="fecha"]', 'focusout', function () {
        if ($(this).attr('type') != 'text') {
            var valor = this.value;

            valor = valor.substr(8, 2) + '-' +
                    valor.substr(5, 2) + '-' +
                    valor.substr(0, 4);

            $(this).attr('type', 'text').val(valor).css('width', 100);
        }
    });

    $('body:not(.visualizando)').delegate('[data-habilidades-id] [data-habilidades-option="eliminar"]', 'click', function () {
        var tr = $(this).parents('[data-habilidades-id]');
        _a.eliminar(tr.attr('data-habilidades-id'));
    });
    $('body:not(.visualizando)').delegate('[data-habilidades-id] [data-habilidades-option="editardescripcion"]', 'click', function () {
        var tr = $(this).parents('[data-habilidades-id]');
        _a.modaleditar(tr.attr('data-habilidades-id'));
    });

    
    // Se precarga template 
    Templates.load('rowTrabajo', '/Scripts/apps/Bimestre/views/rowTrabajo.html')

    this.Imprimir = function () {
        open('/habilidades/imprimir?grupo=' + _grupo + '&bimestre=' + _bimestre);
    }
}

$('body:not(.visualizando)').delegate('[data-habilidades="nuevo"]', 'click', function () {
    Trabajo.nuevo('#tabla-habilidades', 1);
});

$('body:not(.visualizando)').delegate('[data-habilidades="suspencion"]', 'click', function () {
    Trabajo.nuevo('#tabla-trabajo', 3);
});