var tInstrumento = 0;
var Portafolio = new function () {
    var _a = this;

    var tdsCaptura = '',
        rowNumeros = '',
        rowNombres = '',
        rowCalificacionFinal = '',
        rowAprobados = '',
        rowNoAprobados = '',
        rowPortafolio = '',
        tdsTotales = '';


    var generarNombreCache = function () {
        return 'portafolio-' + _grupo + '-' + _bimestre;
    }
    $("#portafolio").keydown(function (e) {
        var code = e.keyCode || e.which;
        if (code == '9') {
            e.preventDefault();
            e.stopPropagation();
            var cellIndex = $(e.target).closest('td').index();
            var nxt = $(e.target).closest('tr').next().children().eq(cellIndex);
            if (nxt.data("alumno-id") != undefined) {
                nxt.find("input").focus()
            } else {
                var previo = $(e.target).closest('tr').prev();
                if (previo == undefined || previo.data("total-agregado") != undefined) {
                    $(e.target).closest('td').next().find("input").focus()
                } else {
                    var contador = 0;
                    var c = 0;
                    do {
                        var anterior = $(previo);
                        previo = $(previo).prev();
                        if (previo.data("total-agregado") != undefined) {
                            $(previo).next().children().eq(cellIndex + 1).find("input").focus()
                            contador = 1;
                        } else if (previo.length == 0) {
                            $(anterior).children().eq(cellIndex + 1).find("input").focus()
                            contador = 1;
                        } else if (nxt.length == 0) {
                            previo.next().next().next().next().children().eq(3).find("input").focus();
                            contador = 1;
                        }
                        c++;
                        if (c == 10) {
                            contador = 1;
                        }

                    } while (contador == 0);

                }
            }

        }
    })

    this.listar = function () {
        return new Promise(function (success) {

            _data = [];
            var consultaParcial = function (page) {
                $.ajax({
                    url: '/Instrumentos/listarPortafolio',
                    data: {
                        grupo: _grupo,
                        bimestre: _bimestre,
                        page: page
                    },
                    error: function () {
                        success(_data);
                    },
                    beforeSend: function () {
                        Loading('Cargando instrumentos');
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
            consultaParcial(0);

        });
    }


    this.generarTrabajo = function (selector, portafolio, focus, autoOrdenar) {


        portafolio.num = $(selector).find('[data-portafolio-id]').length + 1;
        portafolio.tds = tdsCaptura;
        portafolio.grupo = _grupo;

        var elementosPortafolio = $(selector).find('tbody');

        var actualizarAspecto = function (a, key, nombre, template, criterios) {
            var element = $(template.format(a)).appendTo(elementosPortafolio).addData(a);
            
            $(elementosPortafolio).append(element);
      
            element.attr('data-aspecto', key);

            a.entrega.map(function (a) {

                element.find('[data-alumno-id="' + a.id + '"]')
                            .attr('data-alumno-portafolio-estado', a.estado)
                            .attr('data-alumno-aspecto', key)
                            .find('input').val(a[key]);
                element.find('[data-alumno-id="' + a.id + '"]')
                        .prev('[data-aspecto]')
                        .html('<div class="">' + nombre + '</br> ' + criterios + ' </div>');


            });
        }
    
        if (portafolio.Activo1 == true) {
            portafolio.observacion = portafolio.Observacion1;
            actualizarAspecto(portafolio, 'Aspecto1', portafolio.Aspecto1, rowPortafolio, portafolio.Criterio1);
            
        }
        if (portafolio.Activo2 == true) {
            portafolio.observacion = portafolio.Observacion2;
            actualizarAspecto(portafolio, 'Aspecto2', portafolio.Aspecto2, rowPortafolio, portafolio.Criterio2);
        }
        if (portafolio.Activo3 == true) {
            portafolio.observacion = portafolio.Observacion3;
            actualizarAspecto(portafolio, 'Aspecto3', portafolio.Aspecto3, rowPortafolio, portafolio.Criterio3);
        }
        if (portafolio.Activo4 == true) {
            portafolio.observacion = portafolio.Observacion4;
            actualizarAspecto(portafolio, 'Aspecto4', portafolio.Aspecto4, rowPortafolio, portafolio.Criterio4);
        }
        if (portafolio.Activo5 == true) {
            portafolio.observacion = portafolio.Observacion5;
            actualizarAspecto(portafolio, 'Aspecto5', portafolio.Aspecto5, rowPortafolio, portafolio.Criterio5);
        }

        $('span[title]').tooltip();


        if (focus == true) {
            $('html, body').animate({
                scrollTop: $(selector).find('[data-portafolio-id="' + portafolio.IDPortafolio + '"]:first').offset().top
            }, 1000);

            $(selector).find('[data-portafolio-id="' + portafolio.IDPortafolio + '"]:first').resaltar('info', 3000)
                        .find('input:first')
                        .focus();
        
        }

        if ($('body').hasClass('visualizando') == true)
            $('input,textarea').attr('disabled', true);

    }

    this.desplegarResultados = function (grupo, selector) {

        // Se generan rows y tds en base a los alumnos

        // Estos html se generaran cada vez que se ejecute desplegar resultados, 
        // una vez generado ya todas las acciones que requieran estos html solo utilizaran el html que se genero
        rowNombres = '';
        rowNumeros = '';
        tdsCaptura = '';
        rowCalificacionFinal = '';
        rowAprobados = '';
        rowNoAprobados = '';
        tdsTotales = '';

        Alumnos.listar(_grupo).then(function () {
 
            for (var i = 0; i < Alumnos.data.length; i++) {
                var alumn = Alumnos.data[i];
           
        
                rowNumeros += '<th>{0}</th>'.format([i + 1]);
                rowNombres += '<th><div class="w100"><span data-alumno="{id}" class="semaforo" style="background-color:{semaforo}"></span>{apellidoPaterno} {apellidoMaterno} {nombre}</div></th>'.format(alumn);
                rowCalificacionFinal += '<td><div data-alumno-final="{id}" class="w100">0</div></th>'.format(alumn);
                rowAprobados += '<td><div data-alumno-aprobado="{id}" class="w100"></div></th>'.format(alumn);
                rowNoAprobados += '<td><div data-alumno-reprobado="{id}" class="w100">0</div></th>'.format(alumn);
                tdsTotales += '<td><div  data-alumno-total="{id}"class="w100"></div></th>'.format(alumn);
                tdsCaptura += '<td data-alumno-id="{id}" data-alumno-aspecto="" data-alumno-portafolio-estado="" ><input name="calificacion" tabindex="0" type="number" min="0" max="10" required="required" class="form-control form-control-oculto" class="w100" /></td>'.format(alumn);
            }

            Templates.load('tablaPortafolio', '/Scripts/apps/Bimestre/views/tablaPortafolio.html').then(function (template) {

                var datos = {
                    numerosHeaders: rowNumeros,
                    nombresHeaders: rowNombres,
                    rowAprobados: rowAprobados,
                    rowNoAprobados: rowNoAprobados
                };

                // Se despliega el esqueleto de la asitencias
                $(selector).html(template.format(datos));


                // Cargar y desplegar datos de sesiones
                _a.listar(grupo).then(function (data) {

                    data.map(function (s) {
                        _a.generarTrabajo(selector, s);
                    });

                    $(selector).find('.loading').remove();

                    setTimeout(function () {
                        // Ligero delay para que se termine de generar las sesiones
                        ajustarRows(selector);
                    }, 250);
                });

            });
        });
    }

    var obtenerHtmlEstado = function (estado) {
        var estadosResultado = ['<span class="fa fa-close"></span>', '<span class="fa fa-check"></span>', '1/2'];
        if (estado == null) {
            return '<span class="fa fa-close"></span>';
        }
        return estadosResultado[estado];
    }

    var actualizarCalificacion = function (alumno, portafolio, calificacion, grupo, aspecto) {

        if (calificacion == '' || alumno == '' || portafolio == '' || aspecto == undefined || aspecto == '') {

            return;
        }
        var sumatoria = 0;
        var selector = $('[data-tabla="portafolio"]');
       
        
     

        var input = $('[data-portafolio-id="' + portafolio + '"] [data-alumno-id="' + alumno + '"][data-alumno-aspecto="' + aspecto + '"] input');

    
        $.ajax({
            url: '/Instrumentos/actualizarCalificacion',
            type: 'post',
            data: {
                alumno: alumno,
                portafolio: portafolio,
                aspecto: aspecto,
                calificacion: calificacion
            },
            beforeSend: function () {
                $('[data-portafolio-id="' + portafolio + '"] [data-alumno-id="' + alumno + '"][data-alumno-aspecto="' + aspecto + '"]').html('<span class="fa fa-spin fa-refresh"></span>');
            },
            complete: function () {
                $('[data-portafolio-id="' + portafolio + '"] [data-alumno-id="' + alumno + '"][data-alumno-aspecto="' + aspecto + '"] span.fa-spin').remove();
            },
            success: function (response) {
                if (response.result == true) {

                    // Se despliega la sesion actualizada con efecto
                    $('[data-portafolio-id="' + portafolio + '"] [data-alumno-id="' + alumno + '"][data-alumno-aspecto="' + aspecto + '"]')
                        .html(input)
                        .resaltar('info', 800);


                    actualizaDataEnCache();

                    actualizarTotales(portafolio);
                    Alumnos.cargarSemaforo('portafolio', alumno);
                }
                else {
                    AlertError(response.message);
                    $('[data-portafolio-id="' + portafolio + '"] [data-alumno-id="' + alumno + '"]').focusin();
                }
            }
        })

        $(selector).find('[data-portafolio-id][data-total-agregado] [data-alumno-total="' + alumno + '"]').each(function () {

            sumatoria += parseInt($(this).attr('data-real'));

        });
        actualizaDataEnCache();
            actualizarTotales(portafolio);
        Alumnos.cargarSemaforo('portafolio', alumno);
        
    }

    var actualizaDataEnCache = function () {
        // Se complica actualizar los datos, asi que se elimina el cache y la proxima vez se volvera a descargar todo
        Cache.vaciar(generarNombreCache());
    }

    var actualizarTotales = function (sesion) {
        var selector = $('[data-tabla="portafolio"]');
        var x = 0;
        $.ajax({
            url: '/Instrumentos/reactivos',
            type: 'post',
            data: {
                id: sesion

            },
            success: function (response) {
                
                x = response;
               

                // Promedio de trabajos en un aspecto
                $(selector).find('[data-portafolio-promedio="' + sesion + '"]').each(function () {

                    var total = $(this).parents('tr').find('[data-alumno-id][data-alumno-aspecto="' + $(this).parents('tr').attr('data-aspecto') + '"]').length;
                    var sumatoria = 0;
                    var falsa = 0;

                    $(this).parents('tr').find('[data-alumno-id][data-alumno-aspecto="' + $(this).parents('tr').attr('data-aspecto') + '"] input').each(function () {
                        sumatoria += parseInt($(this).val());
                        falsa += parseInt($(this).val()) < 5 ? 5 : parseInt($(this).val()) > 10 ? 10 : parseInt($(this).val());

                    });

                    $(this).prev('[data-portafolio-total="' + sesion + '"]').html(falsa);
                    var promedio = sumatoria / total;
                    falsa = falsa / total;



                    if (falsa > 0) {
                        falsa = falsa.toFixed(1)
                    }
                    $(this).html(falsa < 5 ? 5 : falsa > 10 ? 10 : falsa + (falsa != promedio ? '(' + promedio.toFixed(2) + ')' : ''));
                });

               

                // Calificaciones de todos los aspectos de cada alumno en el proyecto
                $(selector).find('[data-portafolio-id="' + sesion + '"][data-total-agregado] [data-alumno-total]').each(function () {
                    var numeroAspectos = $(selector).find('[data-portafolio-id="' + sesion + '"]').length - 1;
                    var calificacion = 0;
                    $(selector).find('[data-portafolio-id="' + sesion + '"] [data-alumno-id="' + this.getAttribute('data-alumno-total') + '"][data-alumno-aspecto] input').each(function () {
                        calificacion += parseInt(this.value);

                    });



                    var promedio = ((calificacion / x) * 10);
                    var promedioFixed = promedio.toFixed(2);
                  
                    $(this).html(promedioFixed < 5 || promedioFixed > 10 ? (promedioFixed > 10 ? 10 : 5) + ' (' + promedioFixed + ')' : promedio.toFixed(1)).attr('data-real', promedioFixed < 5 ? 5 : promedioFixed > 10 ? 10 : promedio.toFixed(1));

                   
                });
                // Aprobados y reprobados
                $(selector).find('[data-alumno-aprobado]').each(function () {
                    var alumno = this.getAttribute('data-alumno-aprobado');
                    var aprobados = 0, reprobados = 0;
                    $(selector).find('[data-portafolio-id][data-total-agregado] [data-alumno-total="' + alumno + '"]').each(function () {

                        aprobados += parseFloat($(this).attr('data-real')) >= 6 ? 1 : 0;
                        reprobados += parseFloat($(this).attr('data-real')) < 6 ? 1 : 0;
                    });
                    tInstrumento= aprobados + reprobados;
                    $(selector).find('[data-alumno-aprobado="' + alumno + '"]').html(aprobados);
                    $(selector).find('[data-alumno-reprobado="' + alumno + '"]').html(reprobados);
                })


            }

        })
    

    }

    var obtenerPosicion = function (selector, fecha) {

        var list = $(selector).find('[data-portafolio-fecha][data-visible="true"]').get();
        var date = fecha.toDate2();
        var tr = undefined;
        for (var m = 0; m < list.length - 1; m++) {

            if (date <= list[m].getAttribute('data-portafolio-fecha').toDate2() && date > list[m + 1].getAttribute('data-portafolio-fecha').toDate2()) {
                tr = list[m + 1];
                break;
            }
            else if (date > list[m].getAttribute('data-portafolio-fecha').toDate2() && m == 0) {
                tr = list[m];
                break;
            }
        }



        return tr;
    }

    var ajustarRows = function (selector) {


        var ids = [];
        $(selector).find('[data-portafolio-id]').each(function () {
            if (ids.indexOf(this.getAttribute('data-portafolio-id')) == -1) ids.push(this.getAttribute('data-portafolio-id'));
        });

        // eliminamos todos los rows de total agregados
        $(selector).find('[data-total-agregado]').remove();


        // Ajustar los rowspan
        var index = 1;
        ids.map(function (id) {

            // agregar row para totales a cada conjunto de portafolio
            $(selector).find('[data-portafolio-id="' + id + '"]:last').after('<tr data-total-agregado="" data-portafolio-id="' + id + '"><td colspan="3" rowspan=""></td><th><div class="" style="width: 315px;">Calificación</div></th>' + tdsTotales + '</tr>');

            var total = $(selector).find('[data-portafolio-id="' + id + '"]').length;

            $(selector).find('[data-portafolio-id="' + id + '"]').each(function (i, k) {
                if (i == 0) {
                    $(k).attr('data-visible', true);
                    $(k).find('td[rowspan]').removeClass('hide').attr('rowspan', total);
                    $("#headerAspectos")
                    $(k).find('td:first').html('<div class="w50"><a><span data-option="editar" class="fa fa-edit  visor-oculto" title="Editar instrumento"></span></a><a><span data-option="eliminar" class="fa fa-trash  visor-oculto" title="Eliminar instrumento"></span></a> ' + index + '</div>');
                    index++;
                }
                else {
                    $(k).attr('data-visible', false);
                    $(k).find('td[rowspan]').addClass('hide');
                }
            });


            actualizarTotales(id);
        });

    }

    this.nuevo = function (selector, estado) {

        Loading('Cargando formulario');
        Templates.load('nuevoPortafolio', '/Instrumentos/nuevo').then(function (template) {
            Loading();
            ConfirmDialog.show({
                title: 'Nuevo instrumento',
                text: template,
                closeModalOnAction: false,
                callback: function (result) {
                    if (result == true) {
                        $('#frmPortafolio').submit();
                    }
                    else {
                        ConfirmDialog.hide();
                    }
                },
                beforeOpen: function () {
                    var form = $('#frmPortafolio');


                    $('#modalConfirm button[data-confirm="true"]').before('<button class="btn btn-info pull-left" data-defecto="">Guardar configuración</button>');

                    pluginDatepicker(form.find('[name="FechaEntrega"]'));

                    form.find('#IDTipoPortafolio').change();


                    form.find('#IDGrupo').val(_grupo);
                    form.find('#IDBimestre').val(_bimestre);
                    form.find('#FechaEntrega').val(formatDate(new Date()));


                    $.validator.unobtrusive.parse(form);


                    form.submit(function (e) {
                        e.preventDefault();
                        var items = $(this).serializeArray();
                        items.push({ name: "bimestre", value: _bimestre })
               
                        if ($(this).valid()) {
                            $.ajax({
                                url: '/Instrumentos/GuardarPortafolio',
                                type: 'post',
                                data: items,
                                beforeSend: function () {
                                    $('#modalConfirm .modal-footer button').attr('disabled', true).eq(0).append(' <span class="fa fa-spin fa-refresh"></span>');
                                },
                                complete: function () {
                                    $('#modalConfirm .modal-footer button').attr('disabled', false).find('span').remove();
                                },
                                success: function (response) {
                                    if (response.result == true) {

                                        _a.generarTrabajo(selector, response.data, true, true);
                                        ConfirmDialog.hide();

                                        Portafolio.desplegarResultados(_grupo, '#tabla-portafolio');

                                    }
                                    else {
                                        AlertError(response.message, 'Instrumentos');
                                    }

                                }
                            });
                        }

                    });
                }

            });

        });

    }

    this.editar = function (selector, data) {
        Loading('Cargando formulario')
        var id = data.IDPortafolio;

        Templates.load('nuevoPortafolio', '/portafolio/editar?id=' + id + '').then(function (template) {
           
            Loading();
            ConfirmDialog.show({
                title: 'Editar instrumento',
                text: template,
                closeModalOnAction: false,
                callback: function (result) {
                    if (result == true) {
                        $('#frmPortafolio').submit();
                    }
                    else {
                        ConfirmDialog.hide();
                    }
                },
                beforeOpen: function () {
                    var form = $('#frmPortafolio');

                    $.validator.unobtrusive.parse(form);

                    pluginDatepicker(form.find('[name="FechaEntrega"]'));

                    for (var name in data) {
                        if (name.startsWith('activo') && data[name] == true) {
                            form.find('[name="' + name + '"]').attr('checked', true).change();
                        }
                        else {
                            form.find('[name="' + name + '"]').val(data[name]);
                        }

                    }

                    $('#modalConfirm button[data-confirm="true"]').before('<button class="btn btn-info pull-left" data-defecto="">Guardar configuración</button>');





                    form.find('#IDGrupo').val(_grupo);
                    form.find('#IDBimestre').val(_bimestre);
                    form.submit(function (e) {
                        e.preventDefault();

                        form.find('input[type="checkbox"]').each(function () {
                            form.find('input[name="' + this.name + '"]').val($(this).is(':checked'));
                        });

                        if ($(this).valid()) {
                            $.ajax({
                                url: '/Instrumentos/editar',
                                type: 'post',
                                data: $(this).serializeArray(),
                                beforeSend: function () {
                                    $('#modalConfirm .modal-footer button').attr('disabled', true).eq(0).append(' <span class="fa fa-spin fa-refresh"></span>');
                                },
                                complete: function () {
                                    $('#modalConfirm .modal-footer button').attr('disabled', false).find('span').remove();
                                },
                                success: function (response) {
                                    if (response.result == true) {
                                        $('[data-portafolio-id="' + response.data.IDPortafolio + '"]').remove();
                                        _a.generarTrabajo(selector, response.data, true, true);
                                        ConfirmDialog.hide();
                                        Portafolio.desplegarResultados(_grupo, '#tabla-portafolio');
                                        
                                    }
                                    else {
                                        AlertError(response.message, 'Instrumentos');
                                    }
                                }
                            });
                        }
                    });
                }

            });

        });


    }

    this.eliminar = function (selector, id) {
        ConfirmDialog.show({
            title: 'Eliminar instrumento',
            text: '<h3 class="text-center">Esta intentando eliminar un instrumento permanentemente, la cual ya no se podrá recuperar. ¿Desea continuar?</h3>',
            positiveButtonClass: 'btn btn-danger',
            positiveButtonText: 'Si',
            negativeButtonClass: 'btn btn-success',
            negativeButtonText: 'No',
            closeModalOnAction: false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: '/Instrumentos/eliminar',
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
                                $('[data-portafolio-id="' + id + '"]').removeConEfecto();
                                AlertSuccess('Se ha eliminado el instrumento', 'Instrumentos');
                                ConfirmDialog.hide();
                                Portafolio.desplegarResultados(_grupo, selector);
                            }
                            else {
                                AlertError(response.message, 'Instrumentos');
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

    this.actualizarObservacion = function (id, aspecto, observacion) {
        $.ajax({
            url: '/Instrumentos/actualizarObservacion',
            type: 'post',
            data: {
                id: id,
                aspecto: aspecto,
                observacion: observacion
            },
            success: function (response) {
                if (response.result == true) {
                    $('[data-portafolio-id="' + id + '"][data-aspecto="' + aspecto + '"]').resaltar('info', 800);
                    actualizaDataEnCache();
                }
                else {
                    AlertError(response.message, 'Instrumentos');
                }
            }

        });
    }

    var guardarDefecto = function () {

        if ($('#frmPortafolio [name="IDTipoPortafolio"]').val() == undefined || $('#frmPortafolio [name="IDTipoPortafolio"]').val() == null || $('#frmPortafolio [name="IDTipoPortafolio"]').val() == '') {
            AlertWarning('Debes seleccionar el tipo de trabajo');
            return;
        }

        $.ajax({
            url: '/Instrumentos/GuardarPorDefecto',
            type: 'post',
            data: $("#frmPortafolio").serializeArray(),
            beforeSend: function () {
                $("#modalConfirm button[data-defecto]").attr('disabled', true).append(' <span class="fa fa-refresh fa-spin"></span>');
            },
            complete: function () {
                $("#modalConfirm button[data-defecto]").attr('disabled', false).find('span').remove();
            },
            success: function (response) {
                if (response.result == true) {
                    AlertSuccess('Se ha guardado la configuración por defecto', 'Instrumentos');
                }
                else {
                    AlertError(response.message, '');
                }
            }

        });
    }

    var getDefecto = function (tipo) {
        return new Promise(function (success) {
            $.ajax({
                url: '/Instrumentos/GetDefecto',
                type: 'get',
                data: { tipo: tipo },
                success: function (response) {
                    success(response);
                }
            });

        });

    }

    // Cambiar el estado inmediatamente al seleccionar opcion
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-portafolio-estado] input', 'change', function () {

        if ($(this).val() >= 0 && $(this).val() <= 10) {
            var alumno = $(this).parents('td').attr('data-alumno-id');
            var sesion = $(this).parents('tr').attr('data-portafolio-id');
            var grupo = $(this).parents('tr').attr('data-grupo');
            var aspecto = $(this).parents('td').attr('data-alumno-aspecto');

            actualizarCalificacion(alumno, sesion, $(this).val(), grupo, aspecto);
         
            $(this).removeClass('input-validation-error');
        }
        else
            $(this).addClass('input-validation-error');
    });

    $('body:not(.visualizando)').delegate('form [name="IDTipoPortafolio"]', 'change', function () {
        getDefecto($(this).val()).then(function (data) {

            for (var name in data) {
                if ($('#frmPortafolio [name="' + name + '"]').is(':checkbox')) {
                    $('#frmPortafolio [name="' + name + '"]').prop('checked', data[name]);
                }
                else {
                    $('#frmPortafolio [name="' + name + '"]').val(data[name]);
                }
            }

        });
    });

    $('body:not(.visualizando)').delegate('#modalConfirm button[data-defecto]', 'click', function () {
        guardarDefecto();
    });

    $('body:not(.visualizando)').delegate('[data-portafolio-id] [name="observacion"],[data-portafolio-id] [name="fecha"],[data-portafolio-id] [name="nombre"]', 'change', function () {
        var tr = $(this).parents('[data-portafolio-id]');
        _a.actualizarObservacion(tr.attr('data-portafolio-id'), tr.attr('data-aspecto'), this.value);
    });

    // Convertir el text a date
    $('body:not(.visualizando)').delegate('[data-portafolio-id] [name="fecha"]', 'focusin', function () {
        if ($(this).attr('type') != 'date') {
            var valor = this.value;

            valor = valor.substr(6, 4) + '-' +
                    valor.substr(3, 2) + '-' +
                    valor.substr(0, 2);

            $(this).val(valor).attr('type', 'date').css('width', 160);
        }
    });

    // Convertir el date a text
    $('body:not(.visualizando)').delegate('[data-portafolio-id] [name="fecha"]', 'focusout', function () {
        if ($(this).attr('type') != 'text') {
            var valor = this.value;

            valor = valor.substr(8, 2) + '-' +
                    valor.substr(5, 2) + '-' +
                    valor.substr(0, 4);

            $(this).attr('type', 'text').val(valor).css('width', 100);
        }
    });

    $('body:not(.visualizando)').delegate('[data-portafolio-id] [data-option="eliminar"]', 'click', function () {
        var tr = $(this).parents('[data-portafolio-id]');
        _a.eliminar(tr.parents('table'), tr.attr('data-portafolio-id'));
    });
    $('body:not(.visualizando)').delegate('[data-portafolio-id] [data-option="editar"]', 'click', function () {
        var tr = $(this).parents('[data-portafolio-id]');
        _a.editar(tr.parents('table'), tr.getDataAsObject());
    });

    // Agregar o quitar validacion de captura de aspecto
    $('body:not(.visualizando)').delegate('#frmPortafolio input[type="checkbox"][name^="Activo"]', 'change', function (e) {
        if ($('#frmPortafolio input[type="checkbox"][name^="Activo"]:checked').length < 2 && !$(this).is(':checked')) {
            AlertError('Debe ingresar como mínimo dos aspectos', 'Instrumentos');
            $(this).prop('checked', true);
            return;
        }

        if ($(this).is(':checked')) {
            $(this).parents('tr').find('input,textarea').removeClass('ignore');
        }
        else {
            $(this).parents('tr').find('input,textarea').addClass('ignore').removeClass('input-validation-error');
        }
    });


    // Se precarga template 
    Templates.load('rowPortafolio', '/Scripts/apps/Bimestre/views/rowPortafolio.html').then(function (template) {
        rowPortafolio = template;
        Portafolio.desplegarResultados(_grupo, '#tabla-portafolio');
    });

    this.Imprimir = function () {
        open('/Instrumentos/imprimir?grupo=' + _grupo + '&bimestre=' + _bimestre);
    }
}

$('body:not(.visualizando)').delegate('[data-portafolio="nuevo"]', 'click', function () {
    Portafolio.nuevo('#tabla-portafolio', 1);
});
