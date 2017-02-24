var Visor = (function () {

    var cargarVisor = function () {

            $.ajax({
                url: '/licencia/getVisorActual',
                type: 'get',
                success: function (data) {

                    mostrarVisor(data);
                }
            });
        
       
    }

    var mostrarVisor = function (data) {
        if (data == null) return;

        Templates.load('visor','/scripts/apps/credenciales/views/visorActual.html').then(function(html){
            $('body').append(html.format(data));
        });

        $('body').addClass('visualizando');
        $('.visor-oculto').remove();
    }

    cargarVisor();

    $('body').delegate('#desactivarVisor', 'click', function () {
        $.ajax({
            url: '/licencia/EstablecerVizor',
            type: 'post',
            data: { usuario: null },
            beforeSend: function () {
                Loading('Quitando visor');
            },
            complete: function () {
                Loading();
            },
            success: function () {
                Cache.vaciar();
                location = '/grupos'
            }
        });
    })
    
})();