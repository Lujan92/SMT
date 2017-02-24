var Alumnos = new function () {
    
    var _a = this;

    this.data = [];

    this.limpiar = function (grupo) {
        Cache.vaciar();
        _a.data = [];
    }

    this.listar = function (grupo) {
        return new Promise(function (success) {
            //var lista = Cache.validarCache('alumnos-' + grupo);
            //if (lista == false) {
                $.ajax({
                    url: '/alumnos/listasimple',
                    data: { grupo: grupo },
                    beforeSend:function(){
                        Loading('Cargando alumnos');
                    },
                    complete:function(){
                        Loading();
                    },
                    success: function (data) {
                        if (data.length > 0)
                            Cache.almacenarCache(data, 'alumnos-' + grupo, 10);
                        _a.data = data;
                        success(data);
                    }
                });
            //}
            //else {
            //    _a.data = lista;
            //    success(lista);
            //}
        });
    }

    this.desplegarLista = function (grupo, selector) {
        // Cargar html para generar los rows antes de ejecutar la busqueda
        var template = "/Scripts/apps/Bimestre/views/rowAlumno.html";
        if ($("#GrupoEsTaller").val()=="true") {
            template = "/Scripts/apps/Bimestre/views/RowAlumnoTaller.html";
        }
        Templates.load('rowAlumno', template).then(function (template) {
 
            _a.listar(grupo).then(function (data) {
                if (data.length == 0) {
                    $(selector).html('<tr><td class="text-center" colspan="5">No se encontraron alumnos</td></tr>');




                    if (Alumnos.data.length == 0) {
                        $('#tabs a:not(:first), #btnPerfil,#btnImprimirAlumnos').addClass('hide');
                        AlertInfo('Este grupo no cuenta con alumnos registrados, para que se muestren todas las opciones debes capturar a los alumnos del grupo');
                        $('[data-alumno="nuevo"]').popover({
                            title: 'Alumnos',
                            content: 'Da clic para crear un alumno en el grupo',
                            trigger: 'manual',
                            placement: 'top'
                        }).popover('show');

                        setTimeout(function () {
                            $('[data-alumno="nuevo"]').popover('hide');
                        },5000);
                        return;
                    }
                }
                else {

                    $('#tabs a, #btnPerfil,#btnImprimirAlumnos').removeClass('hide');
                    

                    $(selector).empty();
                    var estados = ['#f5caf5', '#c0d9b7', '#f0ad4e', '#d9534f'];
                    for (var i = 0; i < data.length; i++) {
                        var alum = data[i];

                        alum.num = i + 1;
                        alum.color = alum.colorSemaforo;

                        

                        $(selector).append(template.format(alum));

                    }
                }

            });
        });
  
    }
    
    this.cargarAlumno = function (ID) {
        Loading("Cargando");
        $.ajax({
            type: 'GET',
            url: '/alumnos/CargarAlumno',
            data: {
                ID: ID,
                IDGrupo: $("#grupo").val(),
                Tipo:$("#GrupoEsTaller").val()
            }
        })
        .done(function (data) {
            $('#formularioAlumno .modal-body').empty(data);
            $('#formularioAlumno .modal-body').append(data);
            Loading();
            $('#formularioAlumno').modal("show")
        });
    }

    this.guardarAlumno = function () {
        var form = $("#formularioAlumno form");
        if (form.valid()) {
            $.ajax({
                url: '/alumnos/GuardarAlumno',
                data: form.serializeArray(),
                method: "POST",
                beforeSend: function () {
                    Loading('Guardando');
                },
                success: function (data) {
                    Loading();
                    if (data && data.length == 36) {
                        AlertSuccess('Se ha guardado el registro')
                        $('#formularioAlumno').modal("hide");
                        Alumnos.limpiar(_grupo)                        
                        Alumnos.desplegarLista(_grupo, '[data-alumno-lista]');
                        Dropzone.forElement("#dropzone").removeAllFiles(true);
                        $('#formularioAlumno').modal("hide")
                    } else {
                        AlertError('No se ha podido guardar: ' + data);
                    }
                }
            })
        }
    }

    this.eliminarAlumno = function (ID) {
        $.confirm({
            title: 'Alumnos',
            content: "¿Esta seguro de querer eliminarlo?",
            confirmButton: 'Aceptar',
            confirmButtonClass: 'btn-info',
            cancelButton: 'Cancelar',
            icon: 'fa fa-question-circle',
            animation: 'scale',
            confirm: function () {
                $.ajax({
                    url: '/Alumnos/EliminarAlumno',
                    data: {                        
                        ID: ID
                    },
                    method: "post",
                    beforeSend: function () {
                        Loading('Guardando');
                    },
                    success: function (data) {
                        Loading();

                        if (data && data.length == 36) {
                            AlertSuccess("Se ha eliminado correctamente");
                        } else {
                            AlertError(data);
                        }

                        Alumnos.limpiar(_grupo)
                        Alumnos.desplegarLista(_grupo, '[data-alumno-lista]');
                    }
                });
            }
        });
    }

    this.asignarTutor = function (id) {
        var $form;
        var $modal;
        var fetchTutores = function () {
            return new Promise(function (ok, no) {
                $.post(__url__ + 'Tutor/ListarTutores?idAlumno=' + id, function (res) {
                    if (res.result) ok(res.data);
                    else {
                        no("Ocurrió un problema al cargar los tutores");
                        console.error(res.message);
                    }
                });
            });
        };
        var tutores = fetchTutores();
        var removeTutor = function () {
            var $me = $(this)
            var $row = $me.parents("[data-id]");
            var idTutor = $row.data("id")
            var data = { idTutor, idAlumno: id };

            Loading("...");
            $.post(__url__ + 'Tutor/QuitarAlumno', data, function (res) {
                if (res.result) {
                    $row.remove(); 
                    AlertSuccess("Se quitó el tutor");
                    if (!$modal.find("[data-id]").length) updateTable(Promise.resolve([]));
                }
                else AlertError(res.message);
            }).fail(function (r) {
                AlertError("Ocurrió un problema al quitar el tutor");
            }).always(function () { Loading() })
        }

        var updateTable = function (promise) {
            var info =
                $modal.find(".info").length ? $modal.find(".info") :
                $('<div class="info alert alert-info text-center" role="alert"></div>')
                    .appendTo($modal.find("[data-tutores]"));

            promise.catch(function (err) {
                info.removeClass("alert-info").addClass("alert-danger");
                info.html('<i class="fa fa-close"></i> ' + err);
            });
            promise.then(function (data) {
                if (!data.length) return info
                    .html('<i class="fa fa-info-circle"></i> sin tutores');

                $modal.find('[data-tutores]').html(data.map(function (tutor) {
                    var html = '<div data-id="{id}">{label} <i class="fa fa-close pull-right"></i></div>';
                    var obj = { id: tutor.Id, label: tutor.Nombre.bold() + ' - ' + tutor.Email };
                    return html.format(obj);
                }));

                $modal.find('[data-tutores] [data-id] .fa-close').click(removeTutor);
            });
            return promise;
        };

        var prepareForm = function () {
            $form = $modal.find("form");
            $form.find('[name=Id]').val(id);

            $.validator.unobtrusive.parse($form);
            updateTable(tutores);

            $form.submit(function (ev) {
                if (!$form.valid()) return false;
                ev.preventDefault();
                Loading("Enviando...");
                $.post($form.attr("action"), $form.serializeArray(), function (res) {
                    if (res.result) {
                        AlertSuccess("Se agregó correctamente");
                        updateTable(fetchTutores()).then(function () { Loading() });
                        $form.find('[name=Email]').val("");
                    } else { Loading(); AlertError(res.message); }
                }).fail(function () {
                    AlertError("No se pudo comunicar con el servidor");
                    Loading();
                })
            });
        }

        var dialogConfig = {
            title: 'Asignar tutor',
            closeModalOnAction: false,
            positiveButtonText: "Agregar",
            negativeButtonText: "Cerrar",
            callback: function (result) {
                if (result == true) $form.submit();
                else ConfirmDialog.hide();
            },
            beforeOpen: function ($m) { $modal = $m; prepareForm() }
        };

        Loading("Cargando formulario");
        Templates.load('asignarTutorAlumno', __url__ + 'Tutor/AgregarAlumno').then(function (template) {
            Loading();
            ConfirmDialog.show($.extend(dialogConfig, { text: template }));
        });
    }

    this.modalImportar = function () {
        $("#formularioimportarAlumno").modal("show");
    }

    this.importarAlumno = function () {
        var form = $("#formularioimportarAlumno form")
        var files = form.find('.dropzone')[0].dropzone.getAcceptedFiles();
        var formData = new FormData();

        form.serializeArray().map(function (e) {
            formData.append(e.name, e.value);
        });

        $.each(files, function (i, v) {
            formData.append('archivo', v);
        });

        $.ajax({
            type: 'POST',
            url: '/Alumnos/ImportarAlumnos',
            data: formData,
            processData: false,
            contentType: false,
            xhr: function () {
                myXhr = $.ajaxSettings.xhr();

                return myXhr;
            },
            beforeSend: function () {
                Loading("Guardando");
            },
            complete: function () {
                Loading();                
            },
            success: function (data) {
                if (data > 0) {
                    AlertSuccess('Se ha guardado la exitosamente.');
                    Alumnos.limpiar(_grupo)
                    Alumnos.desplegarLista(_grupo, '[data-alumno-lista]');
                    $("#formularioimportarAlumno").modal("hide");
                    Alumnos.limpiar(_grupo)
                    Alumnos.desplegarLista(_grupo, '[data-alumno-lista]');
                } else
                    AlertError(data);
            },
            error: function () {
                AlertError('No se pudo guardar. Intente nuevamente.', '@ViewBag.Title');
            }
        });
    }
  
    this.cargarSemaforo = function (seccion,alumno) {
        $.ajax({
            url:'/alumnos/cargarSemaforo',
            type:'get',
            data: {
                grupo: _grupo,
                bimestre: _bimestre,
                seccion: seccion,
                alumno:alumno
            },
            success: function (data) {
                data.map(function(e){
                    $('#' + seccion).find('[data-alumno="' + e.id + '"].semaforo').css('background-color',e.semaforo);
                });
            }

        });
    }

    this.imprimir = function () {

        open('/alumnos/imprimir?grupo='+_grupo);
    }
}


$('body').delegate('[data-alumno="editar"]', 'click', function (e) {
    e.preventDefault();

    
});

$('body').delegate('[data-alumno="eliminar"]', 'click', function (e) {
    e.preventDefault();

});