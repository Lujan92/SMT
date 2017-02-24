var Reporte = new function () {
    var _a = this;

   

    this.desplegarResultados = function (grupo, selector) {
        Loading('Cargando Reporte');
        Alumnos.listar(_grupo).then(function () {
            Control.cargarDatos().then(function (data) {


                Templates.load('TablaReporte', '/Scripts/apps/Bimestre/views/RowReporte.html').then(function (template) {
                    $(selector).empty();
                    var index = 1;
                    data.alumnos.map(function (calif) {

                        for (var m in Alumnos.data) {
                            var alumno = Alumnos.data[m];

                            if (alumno.id == calif.id) {
                                calif.nombre = alumno.apellidoPaterno + ' ' + alumno.apellidoMaterno + ' ' + alumno.nombre;
                                calif.GradoGrupo = _GradoGrupo;
                                calif.Materia = _Materia;
                                calif.index = index++;

                                for (var m in calif) {
                                    if (typeof (calif[m]) == 'number' && calif[m] % 1 !== 0) {
                                        calif[m] = calif[m].toFixed(1);
                                    }
                                }

                                var tr = $(template.format(calif)).appendTo(selector);

                                tr.find('.calificacion').each(function () {
                                    if (parseInt($(this).text()) <= 5) {
                                        $(this).addClass('reprobado');
                                    }

                                })

                                if (index % 7 == 0) {
                                    $(selector).append('<div style="page-break-after:always"></div>')
                                }

                                break;
                            }
                        }

                    });

                    // Mostrar los rows que son opcionales
                    data.headers.map(function (e) {

                        $(selector).find('tr[data-mostrar="' + e.key + '"]').removeClass('hide');
                    });


                    $(selector).find('.loading').remove();
                    Loading();

                });

            });

        });
    }

 
}