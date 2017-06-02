var Credenciales = (function () {

    var $frmBusqueda = $('#frmBusqueda');
    var pag = new Paginacion({
        content: '.paginacion',
        search: function () {
            $frmBusqueda.submit();
        },
        info: true,
        pageSize: 10
    });

    Templates.load('rowMaestro', '/scripts/apps/credenciales/views/RowCredencial.html');

    var cargarDatos = function (params) {
        return $.ajax({
            url: '/licencia/CargarDatos',
            type: 'get',
            data: params,
            beforeSend: function () {
                Loading('Cargando licencias');
            },
            complete: function () {
                Loading();
            }
        }).promise();
    }

    var desplegarDatos = function (data) {
        var t = $('#tCredenciales tbody').empty();
 
        if (data.cuentas.length > 0) {
            data.cuentas.map(function (e) {
                e.classActivo = e.activo == true ? 'success' : 'danger';
                e.credencial = e.persona == null ? 'Sin asignar' : 'Asignada a ' + e.persona;
                e.asignar = e.persona == null ? '':'hide';
                e.ver = e.persona == null || e.tipo == 'TUTOR' ? 'hide' : '';
                e.eliminar = e.persona == null ? 'hide' : '';
                t.append(Templates.items.rowMaestro.format(e));
            });
        }
        else {
            t.append('<tr><td class="text-center" colspan="10">No se han encontrado licencias</td></tr>');
        }
        pag.updateControls(data.total);
    }


   

    $('#tCredenciales').delegate('[data-opcion="asignar"]', 'click', function () {
        var id = $(this).attr('data-id');

        ConfirmDialog.show({
            title: 'Asignar licencia',
            text: '<form><div><label>Ingresa el email del usuario a quien asignaras la licencia</label><input type="email" name="email" class="form-control" required placeholder="Email" /></div></form>',
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
                            url: '/licencia/asignar',
                            type: 'post',
                            data: params,
                            beforeSend: function () {
                                Loading('Asignando licencia');
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

    $('#tCredenciales').delegate('[data-opcion="ver"]', 'click', function () {
        $.ajax({
            url: '/licencia/EstablecerVizor',
            type: 'post',
            data: { usuario: $(this).attr('data-id') },
            beforeSend: function () {
                Loading('Estableciendo visor');
            },
            complete: function () {
                Loading();
            },
            success: function () {
                Cache.vaciar();
                location = '/grupos';
            }
        });
    });

    $('#tCredenciales').delegate('[data-opcion="activar"]', 'click', function () {
        var activo = $(this).attr('data-activo') == 'true';
        var tr = $(this).parents('tr');
        $.ajax({
            url: '/licencia/activar',
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

    $('#tCredenciales').delegate('[data-opcion="eliminar"]', 'click', function () {
        var id = $(this).attr('data-id');
        var tr = $(this).parents('tr');
        ConfirmDialog.show({
            title: 'Eliminar licencia',
            text: '<h3 class="text-center">¿Desea removerle la licencia a esta persona?</h3>',
            positiveButtonText: 'Si',
            positiveButtonClass: 'btn btn-danger',
            negativeButtonText: 'No',
            negativeButtonClass: 'btn btn-success',
            closeModalOnAction:false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: '/licencia/eliminar2',
                        type: 'post',
                        data: { id: id },
                        
                        beforeSend: function () {
                            console.log(id);
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