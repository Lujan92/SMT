(function () {
    "use strict";

    var url = '/panel/vendedores/';

    var $frmBusqueda = $('#frmBusqueda'),
        $tabla = $('#tResults'),

        pag = new Paginacion({
            content: $tabla.find('.paginacion'),
            search: function () {
                $frmBusqueda.submit();
            },
            info: true,
            pageSize:10
        });

    Templates.load('row', '/areas/panel/scripts/apps/vendedores/rowVendedor.html');
   

    $frmBusqueda.submit(function (e) {
        e.preventDefault();

        var params = $(this).serializeArray();
        params = params.concat([{
            name: 'page',
            value: pag.getCurrentPage()
        },
        {
            name: 'pageSize',
            value: pag.getPageSize()
        }]);

        $.ajax({
            url: url + 'ListarUsuarios',
            type:'get',
            data: params,
            beforeSend: function () {
                Loading('');
            },
            complete:function(){
                Loading();
            },
            success: function (response) {
                var t = $tabla.find('tbody').empty();
                if (response.total == 0)
                    t.html('<tr><td class="text-center" colspan="4">No se han encontrado resultados</td></tr>');
                else
                    response.data.map(function (e) {
                        if (_root == true) {
                            e.mostrarPagada = '';
                            e.textoPagada = e.pagada == 'No' ? 'Pagada' : 'No pagada';
                        }
                        else
                            e.mostrarPagada = 'hide';

                        t.append(Templates.items.row.format(e));
                    });

                pag.updateControls(response.total);
            }
            
        });

    }).submit();

    $('body').delegate('[data-option="pagada"]', 'click', function () {
        var id = $(this).attr('data-id');
        var text = $(this).text();

        ConfirmDialog.show({
            title: 'Licencia',
            text: '<h3 class="text-center">¿Desea marcar como '+ text.toLocaleLowerCase()  +' esta licencia?</h3>',
            positiveButtonText: 'Si, marcar como ' + text.toLocaleLowerCase(),
            closeModalOnAction: false,
            callback: function (result) {
                if (result == true) {
                    $.ajax({
                        url: url + 'MarcarPagada',
                        type: 'post',
                        data: {id:id},
                        beforeSend: function () {
                            Loading('');
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
            }
        });

    });

})();