
var pag = new Paginacion({
    content: '#paginador',
    search: function () {
        $('#frmBusqueda').submit();
    },
    info: true
});

var getUsuario = function (id, callback) {
    $.ajax({
        url: '/panel/usuarios/Get',
        type: 'get',
        data: { id: id },
        beforeSend: function () {
            Loading('Cargando usuario');
        },
        complete: function () {
            Loading();
        },
        success: function (data) {
            if (callback)
                callback(data);
        }

    });

}

var nuevoUsuario = function () {
    $('#modalRegistro').find('.modal-title').html('Nuevo Usuarios');
    $('#modalRegistro').find('form')[0].reset();
    $('#modalRegistro').find('#Id').val('');
    $('#modalRegistro').find('input[type="checkbox"],input[type="radio"]').attr('checked', false);
    $('#modalRegistro').find('#Password').removeClass('ignore');
    $('#modalRegistro').find('#TotalCuentas').attr('readonly',false);
    $('#modalRegistro').modal('show');

    $.data($('#modalRegistro form')[0], 'validator').settings.ignore = ".ignore";
}

var desactivar = function (id, activar) {
    var text = $(activar).html();
    ConfirmDialog.show({
        text: '<h3 class="text-center">¿Desea ' + text.toLowerCase() + ' al usuario?</h3>',
        title: text + ' usuario',
        callback: function (result) {
            if (result == true) {

                $.ajax({
                    url: '/panel/usuarios/CambiarEstado',
                    type: 'post',
                    data: { id: id },
                    beforeSend: function () {
                        Loading('Actualizando usuario');
                    },
                    complete: function () {
                        Loading();
                    },
                    success: function (response) {
                        if (response.result == true) {
                            $('#modalConfirm').modal('hide');
                            AlertSuccess('Se ha ' + (activar == true ? 'activado' : 'desactivado') + ' exitosamente', 'Usuarios');
                            $(activar).toggleClass('btn-success btn-danger').html(text == 'Activar' ? 'Desactivar' : 'Activar');

                        }
                        else {
                            AlertWarning(response.message, 'Usuarios');
                        }
                    }
                });
            }
            else {
                $('#modalConfirm').modal('hide');
            }
        },
        positiveButtonText: 'Si',
        positiveButtonClass: 'btn btn-primary',
        negativeButtonText: 'No',
        closeModalOnAction: false,
    });
}


var eliminar = function (id) {

    ConfirmDialog.show({
        text: '<h3 class="text-center">¿Desea eliminar al usuario permanentemente?</h3>',
        title:  'Eliminar usuario',
        callback: function (result) {
            if (result == true) {

                $.ajax({
                    url: '/panel/usuarios/eliminar',
                    type: 'post',
                    data: { id: id },
                    beforeSend: function () {
                        Loading('Eliminando usuario');
                    },
                    complete: function () {
                        Loading();
                    },
                    success: function (response) {
                        if (response.result == true) {
                            $('#modalConfirm').modal('hide');
                            $('[data-id="' + id + '"]').remove();
                            AlertSuccess('Se ha eliminado exitosamente', 'Usuarios');
                        }
                        else {
                            AlertWarning(response.message, 'Usuarios');
                        }
                    }
                });
            }
            else {
                $('#modalConfirm').modal('hide');
            }
        },
        positiveButtonText: 'Si',
        positiveButtonClass: 'btn btn-danger',
        negativeButtonText: 'No',
        closeModalOnAction: false,
    });
}



$('#frmBusqueda').submit(function (e) {
    e.preventDefault();

    $(this).find('#page').val(pag.getCurrentPage());
    $(this).find('#pageSize').val(pag.getPageSize());


    $.ajax({
        url: '/panel/usuarios/Buscar',
        type: 'get',
        data: $(this).serialize(),
        beforeSend:function(){
            Loading('Buscando usuarios');
        },
        complete:function(){
            Loading();
        },
        success: function (response) {
            var t = $('#tResults tbody').empty();

            if (response.total == 0) {
                t.html('<tr><td class="text-center" colspan="10">No se encontraron resultados</td></tr>');
            }
            else {
                response.data.map(function (e) {

                    e.opciones = '<div class="btn-group pull-right">';

                    e.opciones += '<button class="btn btn-default" data-opcion="editar">Editar</button>';
                    if (e.Disabled == true)
                        e.opciones += '<button class="btn btn-success" data-opcion="bloquear">Activar</button>';
                    else
                        e.opciones += '<button class="btn btn-danger" data-opcion="bloquear">Desactivar</button>';

                    e.opciones += '<button class="btn btn-danger" data-opcion="eliminar">Eliminar</button>';
                    e.opciones += '</div>';

                    t.append('<tr data-id="{Id}"><td>{Username}</td><td>{Nombre}</td><td>{Email}</td><td>{Vigencia}</td><td>{Roles}</td><td>{opciones}</td></tr>'.format(e));
                });
            }

            pag.updateControls(response.total);

        }
    });

}).submit();

$('#modalRegistro form').submit(function (e) {
    e.preventDefault();
    var id = $(this).find('#Id').val();
    var url = id == '' || id == undefined ? '/panel/usuarios/Crear' : '/panel/usuarios/Editar';

    if ($(this).valid()) {

        var params = $(this).serializeArray();

        $('#modalRegistro form [name="Roles"]:checked').each(function () {
            params.push({
                name: 'IdRoles',
                value: this.value
            });
        });


        $.ajax({
            url: url,
            type: 'post',
            data: params,
            beforeSend: function () {
                Loading('Guardando usuario');
            },
            complete: function () {
                Loading();
            },
            success: function (response) {
                if (response.result == true) {
                    $('#UserName').val($('#modalRegistro').find('#Username').val());
                    AlertSuccess('Se ha guardado exitosamente', 'Usuarios');
                    $('#frmBusqueda').submit();
                    $('#modalRegistro').modal('hide');
                }
                else {
                    var errors = '';
                    if (response.Errors !== undefined) {
                        response.Errors.map(function (e) {
                            errors += e + '<br>';
                        });
                        AlertError(errors);
                    } else {
                        AlertWarning(response.message, 'Usuarios');
                    }
                }

            },
            error: function (e) {
                AlertError(e, 'Usuarios');
            }
        })
    }

});

$('#tResults').delegate('[data-opcion]', 'click', function () {
    var id = $(this).parents('tr').data('id');

    switch (this.getAttribute('data-opcion')) {
        case 'editar':
            getUsuario(id, function (data) {
                nuevoUsuario();
                $('#modalRegistro').find('.modal-title').html('Editar Usuario');
                $('#modalRegistro').find('#Password').addClass('ignore');
                $('#modalRegistro').find('#TotalCuentas').attr('readonly', true);

                for (var m in data) {
                    if ($('#modalRegistro').find('[name="' + m + '"][type="checkbox"]').length > 0 ||
                        $('#modalRegistro').find('[name="' + m + '"][type="radio"]').length > 0) {
                        $('#modalRegistro').find('[name="' + m + '"][value="' + data[m] + '"]').attr('checked', true);
                    }
                    else {
                        $('#modalRegistro').find('[name="' + m + '"]').val(data[m] != null ? data[m].toString() :'');
                    }
                }

                for (var m in data.IdRoles) {
                    $('#modalRegistro').find('[name="Roles"][value="' + data.IdRoles[m] + '"]').attr('checked', true);
                    $('#modalRegistro').find('[name="Roles"][value="' + data.IdRoles[m] + '"]').prop('checked', true);
                }


                $('#modalRegistro').find('#Pais').change();
                $('#modalRegistro').find('#Estado').val(data.Estado);
                $('#modalRegistro').find('input[name="RegistroCompleto"][value="' + data.RegistroCompleto + '"]').parent().addClass('active');
            });
            break;
        case 'bloquear':
            desactivar(id, this);
            break;
        case 'eliminar':
            eliminar(id);
            break;
    }
});