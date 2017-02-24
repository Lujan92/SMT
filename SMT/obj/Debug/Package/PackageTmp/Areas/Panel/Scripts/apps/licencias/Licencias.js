var Licencias = (function () {

    var $frmBusqueda = $('#frmBusqueda');
    var pag = new Paginacion({
        content: '.paginacion',
        search: function () {
            $frmBusqueda.submit();
        },
        info: true,
        pageSize:10
    });

    Templates.load('rowLicencia', '/areas/panel/scripts/apps/licencias/views/RowLicencia.html');
    Templates.load('formCaptura', '/areas/panel/scripts/apps/licencias/views/FormNuevaLicencia.html');
    Templates.load('frmLicencias', '/areas/panel/scripts/apps/licencias/views/FormAgregarLicencias.html');
    Templates.load('frmModificar', '/areas/panel/scripts/apps/licencias/views/FormModificarLicencias.html');

    var cargarDatos = function (params) {
        return $.ajax({
            url: '/panel/licencias/CargarDatos',
            type: 'get',
            data:params,
            beforeSend: function () {
                Loading('Cargando licencias');
            },
            complete: function () {
                Loading();
            }
        }).promise();
    }

    var desplegarDatos = function (data) {
        var t = $('#tLicencias tbody').empty();

        if (data.cuentas.length > 0) {
            
            data.cuentas.map(function (e) {
                e.classActivo = e.activo == true ? 'success' : 'danger';
                e.licencia = e.persona == null ? 'Sin asignar' : 'Asignada a ' + e.persona;
                e.eliminarLicencias = e.licencia == 0 ? 'hide' : '';
                t.append(Templates.items.rowLicencia.format(e));
            });
        }
        else {
            t.append('<tr><td class="text-center" colspan="10">No se han encontrado licencias</td></tr>');
        }
        pag.updateControls(data.total);
    }


    var generarLicencia = function (params) {
        return $.ajax({
            url: '/panel/licencias/generar',
            type: 'post',
            data:params,
            beforeSend: function () {
                Loading('Generando licencia');
            },
            complete: function () {
                Loading();
            },
            success: function (response) {
                if (response.result == true) {
                    AlertSuccess('Se ha agregado la nueva licencia', 'Licencias');
                    $frmBusqueda.submit();
                }
                else {
                    AlertError(response.message,'Licencias');
                }
            }

        }).promise();
    }


    $('#tLicencias').delegate('[data-opcion="agregar"]', 'click', function () {
        var id = $(this).attr('data-id');

        ConfirmDialog.show({
            title: 'Agregar licencias',
            text: Templates.items.frmLicencias,
            closeModalOnAction: false,
            positiveButtonClass:'btn btn-primary',
            beforeOpen:function(){
                $('#modalConfirm form').submit(function (e) {
                    e.preventDefault();

                    var params = $(this).serializeArray();
                    params.push({
                        name: 'id',
                        value:id
                    });

                    if ($(this).valid()) {
                        $.ajax({
                            url: '/panel/licencias/agregar',
                            type: 'post',
                            data: params,
                            beforeSend: function () {
                                Loading('Agregando licencias');
                            },
                            complete: function () {
                                Loading();
                            },
                            success: function (response) {
                                if (response.result == true) {
                                    $frmBusqueda.submit();
                                    ConfirmDialog.hide();
                                }
                                else {
                                    AlertError(response.message);
                                }
                            }
                        });
                    }
                });
            },
            callback: function (result) {
                if (result == true) {
                    $('#modalConfirm form').submit();
                }
                else {
                    ConfirmDialog.hide();
                }
            }
        });

    })


    
    $('#tLicencias').delegate('[data-opcion="renovar"]', 'click', function () {
        var id = $(this).attr('data-id');
        
       
        ConfirmDialog.show({
            title: 'Renovar licencias',
            text: Templates.items.frmModificar,
            closeModalOnAction: false,
            positiveButtonClass:'btn btn-primary',
            beforeOpen:function(){
                $('#modalConfirm form').submit(function (e) {
                    e.preventDefault();

                    var params = $(this).serializeArray();
                    alert(params);
                    console.log(params);
                    params.push({
                        name: 'id',
                        value:id
                    });

                    if ($(this).valid()) {
                        $.ajax({
                            url: '/panel/licencias/renovar',
                            type: 'post',
                            data: params,
                            beforeSend: function () {
                                Loading('Renovando licencias');
                            },
                            complete: function () {
                                Loading();
                            },
                            success: function (response) {
                                if (response.result == true) {
                                    $frmBusqueda.submit();
                                    ConfirmDialog.hide();
                                }
                                else {
                                    AlertError(response.message);
                                }
                            }
                        });
                    }
                });
            },
            callback: function (result) {
                if (result == true) {
                    $('#modalConfirm form').submit();
                }
                else {
                    ConfirmDialog.hide();
                }
            }
        });

    })




    $('#tLicencias').delegate('[data-opcion="activar"]', 'click', function () {
        var activo = $(this).attr('data-activo') == 'true';
        var tr = $(this).parents('tr');
        $.ajax({
            url: '/panel/licencias/activar',
            type: 'post',
            data: { id: $(this).attr('data-id') },
            beforeSend: function () {
                Loading( (activo ==  true ? 'Desactivando' : 'Activando') + ' licencia');
            },
            complete: function () {
                Loading();
            },
            success: function () {
                tr.toggleClass('success danger');
            }
        });
    })

    $('#tLicencias').delegate('[data-opcion="eliminar"]', 'click', function () {
        var id = $(this).attr('data-id');
        var tr = $(this).parents('tr');
        ConfirmDialog.show({
            title: 'Eliminar licencia',
            text: '<h3 class="text-center">¿Desea eliminar la licencia principal de esta persona?</h3>',
            positiveButtonText: 'Si',
            positiveButtonClass: 'btn btn-danger',
            negativeButtonText: 'No',
            negativeButtonClass: 'btn btn-success',
            closeModalOnAction:false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: '/panel/licencias/eliminar',
                        type: 'post',
                        data: { id: id },
                        beforeSend: function () {
                            Loading('Removiendo licencia');
                        },
                        complete: function () {
                            Loading();
                        },
                        success: function () {
                            tr.remove();
                            ConfirmDialog.hide();
                        }
                    });
                }
                else {
                    ConfirmDialog.hide();
                }
            }
        });
       
    })

    $('#tLicencias').delegate('[data-opcion="eliminar-licencias"]', 'click', function () {
        var id = $(this).attr('data-id');
        var tr = $(this).parents('tr');
        ConfirmDialog.show({
            title: 'Eliminar licencia',
            text: '<h3 class="text-center">¿Desea eliminar las licencias que están sin asignar?</h3>',
            positiveButtonText: 'Si',
            positiveButtonClass: 'btn btn-danger',
            negativeButtonText: 'No',
            negativeButtonClass: 'btn btn-success',
            closeModalOnAction: false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: '/panel/licencias/eliminarSinAsingar',
                        type: 'post',
                        data: { id: id },
                        beforeSend: function () {
                            Loading('Removiendo licencias');
                        },
                        complete: function () {
                            Loading();
                        },
                        success: function () {
                            $frmBusqueda.submit();
                            ConfirmDialog.hide();
                        }
                    });
                }
                else {
                    ConfirmDialog.hide();
                }
            }
        });

    })

    $('[data-nueva]').click(function () {

        var tipo = $(this).data('nueva');

        ConfirmDialog.show({
            title: 'Generar nueva licencia de ' + tipo,
            text: Templates.items.formCaptura,
            closeModalOnAction:false,
            beforeOpen: function () {

                if (tipo == 'escuela') {
                    $('#modalConfirm form [data-tipo="escuela"]').removeClass('hide');
                }
                else {
                    $('#modalConfirm form [data-tipo="escuela"]').addClass('hide');
                }

                $.validator.unobtrusive.parse($('#modalConfirm form'));
                $('#modalConfirm form').submit(function (e) {
                    e.preventDefault();
                    if ($(this).valid()) {
                        var params = $(this).serializeArray();
                        params.push({
                            name: 'tipoPrincipal',
                            value: tipo
                        });

                        generarLicencia(params).then(function (response) {
                            if (response.result == true) ConfirmDialog.hide();
                        });
                    }
                });
            },
            callback: function (response) {
                if (response == true) {
                    $('#modalConfirm form').submit();
                }
                else {
                    ConfirmDialog.hide();
                }
            }
        });
    });

    $frmBusqueda.submit(function (e) {
        e.preventDefault();

        var params = $(this).serializeArray();

        params.push({
            name: 'page',
            value: pag.getCurrentPage()
        });

        params.push({
            name: 'pageSize',
            value: pag.getPageSize()
        });

        cargarDatos(params).then(function (data) {
            desplegarDatos(data);
        });
    }).submit();
})();