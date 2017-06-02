var Examen = new function () {
    var _a = this;

    var tdsCaptura = '',
        rowNumeros = '',
        rowNombres = '',
        rowCalificacionFinal = '',
        rowAprobados = '',
        rowNoAprobados = '',
        rowExamen = '',
        tdsTotales = '';

    $("#examenes").keydown(function (e) {
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
                        } else if ($(e.target).closest('td').next().find("[data-examen-total]").length > 0) {
                            previo.next().next().next().next().children().eq(7).find("input").focus();
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

    var generarNombreCache = function () {
        return 'examen-' + _grupo + '-' + _bimestre;
    }

    this.listar = function () {
        return new Promise(function (success) {

            _data = [];
            $.ajax({
                url: '/Examenes/listar',
                data: {
                    grupo: _grupo,
                    bimestre: _bimestre
                },
                error: function () {
                    success(_data);
                },
                success: function (data) {
                    success(data);
               
                }
            });

        });
    }

    this.generar = function (selector, examen, focus, autoOrdenar) {
       
        var elementosExamen = $(selector).find('tbody');
        var regAnterior = undefined;
        if (autoOrdenar == true)
            regAnterior = obtenerPosicion(selector, examen.FechaEntregaDesplegable);


        examen.Temas.map(function (tema) {
            tema.num = 0;
            tema.IDExamen = examen.IDExamen;
            tema.Tipo = examen.Tipo;
            tema.Titulo = examen.Titulo;
            tema.FechaEntregaDesplegable = examen.FechaEntregaDesplegable;
            tema.FechaEntrega = examen.FechaEntrega;
            tema.tds = tdsCaptura.format(tema);

            var element = $(rowExamen.format(tema)).appendTo(elementosExamen).addData(tema);

            if (regAnterior != undefined)
                $(regAnterior).before(element);
            else
                $(elementosExamen).append(element);

            tema.Alumnos.map(function (al) {
                elementosExamen.find('[data-alumno-id="' + al.IDAlumno + '"][data-alumno-tema="' + tema.IDTema + '"] input').val(al.Calificacion);

            });

            $(elementosExamen).find('[title]').tooltip();

            if ($('body').hasClass('visualizando'))
                $(elementosExamen).find('input').attr('disabled', true);
        });

        if (focus == true) {
            $('html, body').animate({
                scrollTop: $(selector).find('[data-examen-id="' + examen.IDExamen + '"]:first').offset().top
            }, 1000);

            $(selector).find('[data-examen-id="' + examen.IDExamen + '"]:first').resaltar('info', 3000)
                        .find('input:first')
                        .focus();
        }

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
                rowNumeros += '<th><div class="w100">{0}</div></th>'.format([i + 1]);
                rowNombres += '<th><div class="w100"><span data-alumno="{id}" class="semaforo" style="background-color:{semaforo}"></span>{apellidoPaterno} {apellidoMaterno} {nombre}</div></th>'.format(alumn);
                rowCalificacionFinal += '<td><div class="w100" data-alumno-final="{id}">0<div></th>'.format(alumn);
                rowAprobados += '<td><div class="w100" data-alumno-aprobado="{id}">0<div></th>'.format(alumn);
                rowNoAprobados += '<td><div class="w100" data-alumno-reprobado="{id}">0<div></th>'.format(alumn);
                tdsTotales += '<td><div class="w100" data-alumno-total="{id}">0<div></th>'.format(alumn);
                tdsCaptura += '<td data-alumno-id="{id}" data-alumno-tema="{IDTema}" ><input name="calificacion" tabindex="0" type="number" min="0" max="100" required="required" class="form-control form-control-oculto w100" value="0" /></td>'.format(alumn);
            }
            Loading('Cargando examenes');
            Templates.load('tablaExamen', '/Scripts/apps/Bimestre/views/tablaExamen.html').then(function (template) {

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
                        _a.generar(selector, s);
                    });

                    $(selector).find('.loading').remove();
                    Loading();

                    setTimeout(function () {
                        // Ligero delay para que se termine de generar las sesiones
                        ajustarRows(selector);
                    }, 250);
                });

            });
        });
    }

    var actualizarCalificacion = function (alumno, calificacion, tema, examen) {

        if (calificacion == '' || alumno == '' || tema == undefined) {

            return;
        }


        var input = $('[data-examen-id] [data-alumno-id="' + alumno + '"][data-alumno-tema="' + tema + '"] input');

        $.ajax({
            url: '/examenes/actualizarCalificacion',
            type: 'post',
            data: {
                alumno: alumno,
                tema: tema,
                calificacion: calificacion,
                grupo: _grupo
            },
            beforeSend: function () {
                $('[data-examen-id] [data-alumno-id="' + alumno + '"][data-alumno-tema="' + tema + '"]').html('<span class="fa fa-spin fa-refresh"></span>');
            },
            complete: function () {
                $('[data-examen-id] [data-alumno-id="' + alumno + '"][data-alumno-tema="' + tema + '"] span.fa-spin').remove();
            },
            success: function (response) {
                if (response.result == true) {
                    // Se despliega la sesion actualizada con efecto
                    $('[data-examen-id] [data-alumno-id="' + alumno + '"][data-alumno-tema="' + tema + '"]')
                        .html(input)
                        .resaltar('info', 800);


                    actualizaDataEnCache();

                    actualizarTotales(examen);
                    Alumnos.cargarSemaforo('examenes', alumno);
                }
                else {
                    AlertError(response.message);
                    $('[data-examen-id] [data-alumno-id="' + alumno + '"]').focusin();
                }
            }
        })

    }

    var actualizaDataEnCache = function () {
        // Se complica actualizar los datos, asi que se elimina el cache y la proxima vez se volvera a descargar todo
        Cache.vaciar(generarNombreCache());
    }

    var actualizarTotales = function (id) {
        var selector = $('[data-tabla="examen"]');

        // Calcular total de rectivos de un examen
        $(selector).find('[data-total-reactivos="' + id + '"]').each(function () {
            var total = 0;

            $('[data-examen-id="' + id + '"]').find('[data-reactivos]').each(function () {
                total += parseInt(this.innerHTML);
            });

            this.innerHTML = total;
        });

        // Promedio de trabajos en un tema
        $(selector).find('[data-examen-promedio="' + id + '"]').each(function () {
            var total = $(this).parents('tr').find('[data-alumno-id][data-alumno-tema="' + $(this).parents('tr').attr('data-tema') + '"]').length;
            var reactivos = parseInt($(this).parents('tr').find('[data-reactivos]').text());
            var sumatoria = 0;
            var falsa = 0;

            $(this).parents('tr').find('[data-alumno-id][data-alumno-tema="' + $(this).parents('tr').attr('data-tema') + '"] input').each(function () {
                sumatoria += parseInt($(this).val());
                var promedioFalso = parseInt($(this).val()) * 10 / reactivos;
                falsa += promedioFalso < 5 ? 5 : promedioFalso > 10 ? 10 : promedioFalso;
            });

            $(this).parents('tr').find('[data-examen-total="' + id + '"]').html(sumatoria);
            total = total == 0 ? 1 : total;
            falsa = falsa / total;
            var promedio = sumatoria * 10 / reactivos / total;
            $(this).html((falsa < 5 ? 5 : falsa > 10 ? 10 : falsa.toFixed(1)) + (falsa != promedio ? '(' + promedio.toFixed(1) + ')' : ''));
        });


        // Calificaciones de todos los temas de cada alumno en el examen
        $(selector).find('[data-examen-id="' + id + '"][data-total-agregado] [data-alumno-total]').each(function () {
            var numeroAspectos = $(selector).find('[data-examen-id="' + id + '"]').length - 1;
            var calificacion = 0;
            $(selector).find('[data-examen-id="' + id + '"] [data-alumno-id="' + this.getAttribute('data-alumno-total') + '"][data-alumno-tema] input').each(function () {
                calificacion += parseInt(this.value);
            });
            var promedio = calificacion * 10 / parseInt($('[data-total-reactivos="' + id + '"]').text());
            $(this).html(promedio < 5 || promedio > 10 ? (promedio > 10 ? 10 : 5) + ' (' + promedio.toFixed(1) + ')' : promedio.toFixed(1)).attr('data-real', promedio < 5 ? 5 : promedio > 10 ? 10 : promedio.toFixed(1));
        });

        // Aprobados y reprobados
        $(selector).find('[data-alumno-aprobado]').each(function () {
            var alumno = this.getAttribute('data-alumno-aprobado');
            var aprobados = 0, reprobados = 0;
            $(selector).find('[data-examen-id][data-total-agregado] [data-alumno-total="' + alumno + '"]').each(function () {

                aprobados += parseFloat($(this).attr('data-real')) >= 6 ? 1 : 0;
                reprobados += parseFloat($(this).attr('data-real')) < 6 ? 1 : 0;
            });

            $(selector).find('[data-alumno-aprobado="' + alumno + '"]').html(aprobados);
            $(selector).find('[data-alumno-reprobado="' + alumno + '"]').html(reprobados);
        });

    }

    var obtenerPosicion = function (selector, fecha) {
        var list = $(selector).find('[data-examen-fecha][data-visible="true"]').get();
        var date = fecha.toDate2();
        var tr = undefined;
        for (var m = 0; m < list.length - 1; m++) {

            if (date <= list[m].getAttribute('data-examen-fecha').toDate2() && date > list[m + 1].getAttribute('data-examen-fecha').toDate2()) {
                tr = list[m + 1];
                break;
            }
            else if (date > list[m].getAttribute('data-examen-fecha').toDate2() && m == 0) {
                tr = list[m];
                break;
            }
        }

        return tr;
    }

    var ajustarRows = function (selector) {
        var ids = [];

        $(selector).find('[data-examen-id]').each(function () {
            if (ids.indexOf(this.getAttribute('data-examen-id')) == -1) ids.push(this.getAttribute('data-examen-id'));
        });

        // eliminamos todos los rows de total agregados
        $(selector).find('[data-total-agregado]').remove();

        // Ajustar los rowspan
        var index = 1;
        ids.map(function (id) {

            // agregar row para totales a cada conjunto de examen
            $(selector).find('[data-examen-id="' + id + '"]:last').after('<tr data-total-agregado="" data-examen-id="' + id + '"><td colspan="3" rowspan=""></td><th><div class="w100">Calificación</div></th><td><div data-total-reactivos="' + id + '" class="w100">0</div></th>' + tdsTotales + '</tr>');

            var total = $(selector).find('[data-examen-id="' + id + '"]').length;

            $(selector).find('[data-examen-id="' + id + '"]').each(function (i, k) {
                if (i == 0) {
                    $(k).attr('data-visible', true);
                    $(k).find('td[rowspan]').removeClass('hide').attr('rowspan', total);
                    $(k).find('td:first').html('<div class="w50"><a><span data-option="download" class="fa fa-download visor-oculto" title="Descargar examen"></span></a><a><span data-option="editar" class="fa fa-edit  visor-oculto" title="Editar examen"></span></a><a><span data-option="eliminar" class="fa fa-trash  visor-oculto" title="Eliminar examen"></span></a> ' + index + '</div>');
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
        Templates.load('nuevoExamen', '/examenes/nuevo').then(function (template) {
            Loading();
            ConfirmDialog.show({
                text: 'Nuevo examen',
                text: template,
                closeModalOnAction: false,
                callback: function (result) {
                    if (result == true) {
                        $('#frmExamen').submit();
                    }
                    else {
                        ConfirmDialog.hide();
                    }
                },
                beforeOpen: function () {
                    var form = $('#frmExamen');
                    $.validator.unobtrusive.parse(form);

                    pluginDatepicker(form.find('[name="FechaEntrega"]'));

                    form.find('#IDBimestre').val(_bimestre);
                    form.find('#Grupo').val(_grupo);
                    form.find('#FechaEntrega').val(formatDate(new Date()));
                    form.find('[data-option="pregunta"]').click();
                    form.find('[name="Titulo"]').val(form.find('[name="Tipo"]').val() + ' ' + _bimestre);

                    form.on("change", "[data-no-instrucciones]", function (ev) {
                        var $me = $(this)
                        var $related = form.find("[name='" + $me.data("no-instrucciones") + "']");
                        var isFirstQuestion = form.find("[data-pregunta]").eq(0).get(0) == $me.parents("[data-pregunta]").get(0);
                        if (!isFirstQuestion) {
                            $related.prop("disabled", $me.is(":checked"));
                        } else {
                            $me.prop("checked", false);
                            AlertError("No puede deshabilitar las instrucciones de la primera pregunta");
                        }
                    })

                    form.submit(function (e) {
                        e.preventDefault();

                        if ($(this).find('[data-pregunta]').length == 0) {
                            AlertWarning('Debes capturar por lo menos una pregunta', 'Exámenes');
                            return;
                        }

                        if ($(this).valid()) {

                            $.ajax({
                                url: '/examenes/Nuevo',
                                type: 'post',
                                cache: false,
                                dataType: 'json',
                                processData: false,
                                contentType: false,
                                data: generarFormData($(this)),
                                beforeSend: function () {
                                    $('#modalConfirm .modal-footer button').attr('disabled', true).eq(0).append(' <span class="fa fa-spin fa-refresh"></span>');
                                },
                                complete: function () {
                                    $('#modalConfirm .modal-footer button').attr('disabled', false).find('span').remove();
                                },
                                success: function (response) {

                                    if (response.result == true) {
                                        //_a.generar estara comentado hasta que se haya guardado examen en base de datos
                                        // _a.generar(selector, response.data, true, true);
                                        ConfirmDialog.hide();
                                        Examen.desplegarResultados(_grupo, '#tabla-examen');
                                    }
                                    else {
                                        AlertError(response.message, 'Examen');
                                    }

                                }
                            });
                        }

                    }).validate({
                        ignore: '.hide'
                    });

                    form.find('[name="Tipo"]').change(function () {
                        form.find('[name="Titulo"]').val(form.find('[name="Tipo"]').val() + ' ' + _bimestre);
                    });
                }

            });

        });

    }

    this.editar = function (selector, data) {
        Loading('Cargando formulario');

        Templates.load('nuevoExamen', '/examenes/nuevo').then(function (template) {
            Loading();
            ConfirmDialog.show({
                title: 'Editar examen',
                text: template,
                closeModalOnAction: false,
                callback: function (result) {
                    if (result == true) {
                        $('#frmExamen').submit();
                    }
                    else {
                        ConfirmDialog.hide();
                    }
                },
                beforeOpen: function () {
                    var form = $('#frmExamen');


                    $.validator.unobtrusive.parse(form);
                    pluginDatepicker(form.find('[name="FechaEntrega"]'));

                    form.find('#Grupo').val(_grupo);
                    form.find('#IDBimestre').val(_bimestre);

                    for (var m in data[0]) {
                        form.find('[name="' + m + '"]').val(data[0][m]);

                    }

                    data.map(function (e) {
                        e.index = form.find('[data-pregunta]').length;
                        form.find('#preguntas').append(generarPregunta(e));
                    });

                    form.on("change", "[data-no-instrucciones]", function () {
                        var $me = $(this)
                        var $related = form.find("[name='" + $me.data("no-instrucciones") + "']");
                        var isFirstQuestion = form.find("[data-pregunta]").eq(0).get(0) == $me.parents("[data-pregunta]").get(0);
                        if (isFirstQuestion) {
                            $me.prop("checked", false);
                            AlertError("No puede deshabilitar las instrucciones de la primera pregunta");
                        } else {
                            $related.prop("disabled", $me.is(":checked"));
                        }
                    });

                    form.submit(function (e) {
                        e.preventDefault();

                        if ($(this).valid()) {
                            $.ajax({
                                url: '/Examenes/editar',
                                type: 'post',
                                cache: false,
                                dataType: 'json',
                                processData: false,
                                contentType: false,
                                data: generarFormData(form),
                                beforeSend: function () {
                                    $('#modalConfirm .modal-footer button').attr('disabled', true).eq(0).append(' <span class="fa fa-spin fa-refresh"></span>');
                                },
                                complete: function () {
                                    $('#modalConfirm .modal-footer button').attr('disabled', false).find('span').remove();
                                },
                                success: function (response) {
                                    if (response.result == true) {
                                        $('[data-examen-id="' + response.data.IDExamen + '"]').remove();
                                        _a.generar(selector, response.data, true, true);
                                        ConfirmDialog.hide();

                                        Examen.desplegarResultados(_grupo, '#tabla-examen');
                                    }
                                    else {
                                        AlertError(response.message, 'Examenes');
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
            title: 'Eliminar Examen',
            text: '<h3 class="text-center">Esta intentando eliminar este examen permanentemente, el cual ya no se podrá recuperar. ¿Desea continuar?</h3>',
            positiveButtonClass: 'btn btn-danger',
            positiveButtonText: 'Si',
            negativeButtonClass: 'btn btn-success',
            negativeButtonText: 'No',
            closeModalOnAction: false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: '/Examenes/eliminar',
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
                                $('[data-examen-id="' + id + '"]').removeConEfecto();
                                AlertSuccess('Se ha eliminado el examen', 'Examenes');
                                ConfirmDialog.hide();
                                Examen.desplegarResultados(_grupo, selector);
                            }
                            else {
                                AlertError(response.message, 'Examenes');
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

    var generarPregunta = function (data) {
        var template = $('[data-template="pregunta"]').html();

        if (data != undefined) {
            template = template.format(data);
        }

        var pregunta = $(template);

        for (var m in data) {
            pregunta.find('[name$="' + m + '"]').val(data[m]);
        }

        pregunta.find('select[name$="TipoTema"]').change(function () {
            $(this).parents('[data-pregunta]').find('[data-respuesta]').addClass('hide');
            var activo = $(this).parents('[data-pregunta]').find('[data-respuesta*="' + $(this).val() + '"]');
            activo.removeClass('hide');
            if ($(this).val() == 'Multiple' && activo.find('textarea').val() == '') {
                activo.find('textarea').val('a) ');
            }
        }).change();

        if (data.UrlArchivo != null) {
            pregunta.find('input:file').after('<img src="' + data.UrlArchivo + '" class="img-thumbnail" >');
        }
       
        return pregunta;
    }
    var generarExamenes = function (data) {

        //var template = $('[data-template="examen"]').html();
        //if (data != undefined) {
        //    template = template.format(data);
        //}

        //var pregunta = $(template);

        //for (var m in data) {
        //    pregunta.find('[name$="' + m + '"]').val(data[m]);

        //}

        //pregunta.find('select[name$="TipoTema"]').change(function () {
        //    $(this).parents('[data-examen]').find('[data-respuesta]').addClass('hide');
        //    var activo = $(this).parents('[data-examen]').find('[data-respuesta*="' + $(this).val() + '"]');
        //    activo.removeClass('hide');
        //    if ($(this).val() == 'Multiple' && activo.find('textarea').val() == '') {
        //        activo.find('textarea').val('a) ');
        //    }
        //}).change();

        //if (data.UrlArchivo != null) {
        //    pregunta.find('input:file').after('<img src="' + data.UrlArchivo + '" class="img-thumbnail" >');
        //}
        //console.log(pregunta);
        var template = $('#exa').html();
        var pregunta = "<br>" + $('#exa').html();

        return pregunta;
    }
    var generarFormData = function (form) {
        var formData = new FormData();

        form.find('[name]').each(function () {
            if ($(this).is(':file'))
                formData.append(this.name, this.files[0]);
            else {
                formData.append(this.name, this.disabled ? "" : this.value);
            }
        });

        formData.append("bimestre", _bimestre);

        return formData;
    }

    var ajustarIndex = function () {
        $('#frmExamen').find('[data-pregunta]').each(function (i, k) {
            $(this).find('[name]').each(function () {
                this.name = 'ExamenTema[' + i + ']' + this.name.substring(this.name.indexOf('.'), this.name.length);
            });
        });
    }
    var ajustarIndexExamen = function () {
        $('#frmExamen').find('[data-examen]').each(function (i, k) {
            $(this).find('[name]').each(function () {
                this.name = 'ExamenTema[' + i + ']' + this.name.substring(this.name.indexOf('.'), this.name.length);
            });
        });
    }
    // Cambiar el estado inmediatamente al seleccionar opcion
    $('body:not(.visualizando)').delegate('[data-alumno-id][data-alumno-tema] input', 'change', function () {

        if ($(this).val() >= 0 && $(this).val() <= 100) {
            var alumno = $(this).parents('td').attr('data-alumno-id');
            var tema = $(this).parents('td').attr('data-alumno-tema');

            actualizarCalificacion(alumno, $(this).val(), tema, $(this).parents('[data-examen-id]').attr('data-examen-id'));
            $(this).removeClass('input-validation-error');
        }
        else
            $(this).addClass('input-validation-error');
    });

    $('body:not(.visualizando)').delegate('[data-examen-id] [data-option="eliminar"]', 'click', function () {

        var tr = $(this).parents('[data-examen-id]');
        _a.eliminar(tr.parents('table'), tr.attr('data-examen-id'));
    });
    $('body:not(.visualizando)').delegate('[data-examen-id] [data-option="editar"]', 'click', function () {
        var tr = $(this).parents('[data-examen-id]');
        var data = [];

        $('[data-examen-id="' + tr.attr('data-examen-id') + '"]:not([data-total-agregado])').each(function () {
            data.push($(this).getDataAsObject());
        });

        _a.editar(tr.parents('table'), data);
    });

    // Agregar preguntas
    $('body:not(.visualizando)').delegate('[data-option="pregunta"]', 'click', function () {

        var total = $('form [data-pregunta]').length;
        var total2 = $('form [name$="Instrucciones"]:not([disabled])').length;

        $('#preguntas').append(generarPregunta({
            index: total,
            Nombre: 'Tema / Subtema ' + (total + 1)
        }));

        $('#preguntas [data-pregunta] .panel-collapse ').removeClass('in');
        $('#preguntas [data-pregunta]:last .panel-collapse ').addClass('in');
        $('#preguntas [data-pregunta]:last').find('[name$="Instrucciones"]').val(romanize(total2 + 1) + '. ');

        $.validator.unobtrusive.parse($('#frmExamen'));
    });
    // Agregar examenes
    var total = 0;
    $('body:not(.visualizando)').delegate('[data-option="examen"]', 'click', function () {

        total = $('[data-examen]').length + total;
        var total2 = $('form [name$="Instrucciones"]:not([disabled])').length;
        var totalPregunta = $('form [data-pregunta]').length;


        var html =
        '<div data-examen="' + total + '" class="panel panel-default resaltar">' +
            '<div class="panel-heading"  role="tab" id="heading' + total + '">' +
                    '<h4 class="panel-title">' +
                    '<a data-toggle="collapse" data-parent="#accordion2" href="#collapseInnerTwo' + total + '"' +
                     '<span class="nombrePregunta' + total + '">pregunta </span></a>' +
                     '<a href="#' + total + '" id="Aeliminar">' +
                     '<span id="' + total + '" data-id="facturaCancelacion" data-option="eliminar-preguntaExamen" class="pull-right fa fa-trash"></span>' +
                    ' </a></h4></div>' +
    '<div id="collapseInnerTwo' + total + '" class="panel-collapse collapse">' +
        '<div class="panel-body">' +
            '<input type="hidden" name="ExamenTema' + total + '.IDTema" />' +
        '<input type="hidden" name="ExamenTema' + total + '.Archivo" />' +
        //    '<div class="form-group">' +
        //'<label>Tema / Subtema</label>' +
        //        '<input type="text" name="ExamenTema' + total + '.nombrePregunta" maxlength="50" minlength="3" required class="form-control" />' +
        //    '</div>' +
        //    '<div class="form-group">' +
        //'<label>Tipo de pregunta</label>' +
        //        '<select name="ExamenTema' + total + '.TipoTema" class="form-control">' +
        //'<option value="Sin personalizar">Sin personalizar</option>' +
        //'           <option value="Multiple">Multiple</option>' +
        //'           <option value="Columnas">Columnas</option>' +
        //'           <option value="Laguna">Laguna</option>' +
        //'           <option value="Abierta">Abierta</option>' +
        //'       </select>' +
        //'   </div>' +
        // '   <div class="form-group">' +
        //'      <label>Instrucciones</label>' +
        //'       <label class="pull-right">' +
        //'           Sin instrucciones' +
        //'           <input data-no-instrucciones="ExamenTema' + total + '.Instrucciones" type="checkbox" />' +
        //'       </label>' +
        //'       <input type="text" name="ExamenTema' + total + '.Instrucciones" class="form-control" />' +
        //'   </div>' +
        '   <div class="form-group">' +
        '       <label id="pregunta">Pregunta</label>' +
        '       <textarea id="tApregunta" name="ExamenTema' + total + '.Pregunta" minlength="3" required class="form-control"></textarea>' +
        '   </div>' +
        '    <div data-respuesta="Multiple,Laguna" class="hide">' +
        '       <div class="form-group">' +
        '           <textarea name="ExamenTema' + total + '.Respuesta" class="form-control"></textarea>' +
        '       </div>' +
        '   </div>' +
        '   <div data-respuesta="Columnas" class="hide row">' +
        '       <div class="form-group col-lg-6">' +
        '           <label>Columna 1</label>' +
        '           <textarea id="col1" name="ExamenTema' + total + '.Respuesta1" class="form-control"></textarea>' +
        '       </div>' +
        '       <div class="form-group col-lg-6">' +
        '           <label>Columna 2</label>' +
        '           <textarea id="col2" name="ExamenTema' + total + '.Respuesta2" class="form-control"></textarea>' +
        '       </div>' +
        '   </div>' +
        '   <div data-respuesta="Abierta" class="hide">' +
        '       <label>Archivo</label>' +
        '       <input type="file" name="ExamenTema' + total + '.file" />' +
        '   </div>' +
        //'   <div class="form-group">' +
        //'       <label>Reactivos</label>' +
        //'       <input type="number" name="ExamenTema' + total + '.Reactivos" max="100" min="0" required class="form-control" />' +
        //'   </div>' +
        '   </div>' +
        '   </div>' +
        '   </div>' +
        '   </div>' +
        '</div></div>';
        var variable = totalPregunta - 1;
        var idVariable = "prueba" + variable;

        $("#" + String(idVariable) + "").append(html);

    });
    //$('body:not(.visualizando)').delegate('[name$="Nombre"]', 'keyup', function () {

    //    $(this).parents('[data-pregunta=""]').find('span.nombre').html(this.value);
    //});

    $('body:not(.visualizando)').delegate('[name$="nombrePregunta"]', 'keyup', function () {

        $(this).parents('[data-pregunta=""]').find('span.nombrePregunta').html(this.value);
    });
    $('body:not(.visualizando)').delegate('#frmExamen [data-option="eliminar-pregunta"]', 'click', function () {
        var pregunta = $(this).parents('[data-pregunta]');

        $.confirm({
            title: 'Eliminar pregunta',
            content: '¿Esta seguro de eliminar esta pregunta?',
            confirmButton: 'Si',
            confirmButtonClass: 'btn-danger',
            cancelButton: 'No',
            icon: 'fa fa-question-circle',
            animation: 'scale',
            confirm: function () {
                pregunta.remove();
                ajustarIndex();
            }
        });

    });
    $('body:not(.visualizando)').delegate('#frmExamen #Aeliminar', 'click', function () {
        //se obtiene el valor href que esta en el icono y de una vez se le quita el # 
        var href = $(this).attr('href').replace('#', '');
        //pregunta obtiene el atributo data-examen que posee el total en este caso el total es el que obtenemos de href
        var pregunta = $(this).parents('[data-examen="' + href + '"]');

        $.confirm({
            title: 'Eliminar pregunta',
            content: '¿Esta seguro de eliminar esta pregunta?',
            confirmButton: 'Si',
            confirmButtonClass: 'btn-danger',
            cancelButton: 'No',
            icon: 'fa fa-question-circle',
            animation: 'scale',
            confirm: function () {
                pregunta.remove();
            }
        });


    });

    $('#Aeliminar').click(function (e) {
        alert('df');
        var href = $(this).attr('href');

    });

    $('body:not(.visualizando)').delegate('[data-option="download"]', 'click', function () {

        open('/examenes/download?examen=' + $(this).parents('[data-examen-id]').attr('data-examen-id'));
    });

    // Se precarga template 
    Templates.load('rowExamen', '/Scripts/apps/Bimestre/views/rowExamen.html').then(function (template) {
        rowExamen = template;


        Examen.desplegarResultados(_grupo, '#tabla-examen');
    });

    this.Imprimir = function () {
        open('/examenes/imprimir?grupo=' + _grupo + '&bimestre=' + _bimestre);
    }
}

$('body:not(.visualizando)').delegate('[data-examen="nuevo"]', 'click', function () {
    Examen.nuevo('#tabla-examen', 1);
});

$('body:not(.visualizando)').delegate('#frmExamen textarea', 'keypress', function (e) {
    if (e.keyCode == 13) {
        var texts = $(this).val().split('\n');
        var reg = /^(\w|\d)(\)|\.-?)/ig;

        var ultimaLinea = texts[texts.length - 1];
        var sinEspacios = ultimaLinea.trim();

        if (reg.test(sinEspacios)) {

            var matches = reg.exec(sinEspacios);
            if (matches == null) matches = reg.exec(sinEspacios); // Nose porque la primera vez da null y la segunda ya da bien el resultado...
            var valor = /\d/.test(matches[1]) ? matches[1] + 1 : matches[1].charCodeAt(0);
            var tipo = matches[2];

            var resultado = String.fromCharCode(valor + 1) + tipo;

            // Ahora detectar espacios
            var espacios = /\s+?_+/;
            if (espacios.test(ultimaLinea)) {
                var match = espacios.exec(ultimaLinea);
                if (match == null) match = espacios.exec(ultimaLinea);
                resultado += match[0];
            }

            $(this).val($(this).val() + '\n' + resultado + ' ');
            return false;
        }

    }
});

var cargarComponenteUpload = function (select) {
    alert('ohh!!');
    $(select).addClass('dropzone').dropzone({

        url: '/Home',
        maxFiles: 1,
        acceptedFiles: 'image/jpg, image/jpeg, image/png',
        dictDefaultMessage: 'Arrastre o seleccione la imagen a subir',
        init: function () {
            this.on('success', function (file) {
                var reader = new FileReader();
                reader.onload = function (e) {

                    $(select).before('<img data-preview="' + $(select).attr('data-imagen') + '" src="{0}" class="option-container default" />'.format([e.target.result]));

                    $(select).parent().find('[data-preview]').cropper({
                        aspectRatio: 'free',
                        movable: true,
                        zoomable: true,
                        rotatable: false,
                        scalable: false,
                        strict: false,
                        guides: false,
                        highlight: false,
                        dragCrop: false,
                        cropBoxMovable: true,
                        cropBoxResizable: true,
                        minCropBoxWidth: 200,
                        minCropBoxHeight: 112.50
                    });

                    $(select).hide();

                }
                reader.readAsDataURL(file);
            });
            this.on("error", function (file) {
                if (!file.accepted) {
                    this.removeFile(file);
                    AlertError('Solo se permiten imagenes');
                }
            });
        }
    });
}