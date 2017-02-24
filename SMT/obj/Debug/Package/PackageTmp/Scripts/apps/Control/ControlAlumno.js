var ControlAlumno = new function () {

    var _control = this;

    var nameCache = function () {
        return 'control-alumno-' + alumno.id;
    }

    this.data = [];

    this.cargarDatos = function () {
        return new Promise(function (success) {

            var data = Cache.validarCache(nameCache());
            if (data == false) {

                $.ajax({
                    url: '/alumnos/CargarReporte',
                    type: 'get',
                    data: { grupo: _grupo,alumno: alumno.id },
                    success: function (data) {
                        if (data != null) {
                            
                            _control.data = data;

                            if (data.result == false) {
                                $.confirm({
                                    title: 'Control',
                                    content: 'Se ha detectado un error en la generación del reporte de control, para solucionar este problema se necesita ejecutar un proceso que puede demorar unos minutos para generar los datos necesarios para este reporte. ¿Desea ejecutar este proceso?',
                                    confirmButton: 'Aceptar',
                                    confirmButtonClass: 'btn-info',
                                    cancelButton: 'Cancelar',
                                    icon: 'fa fa-question-circle',
                                    animation: 'scale',
                                    confirm: function () {
                                        actualizarGrupo();
                                    }
                                });
                            }
                            else {
                                Cache.almacenarCache(data, nameCache(), 1);
                            }

                            success(data);
                        }
                    }
                });
            }
            else {
                _control.data = data;
                success(data);
            }

        });
    }

    var generarTablaControl = function (selector) {
        Templates.load('tablaControl', '/scripts/apps/control/views/tablaControlAlumno.html').then(function (tabla) {

            var row = '';

            _control.data.headers.map(function (head) {
                row += '<tr><td><div class=" text-center">'+head.name+'</div></td><td><div class="">{' + head.key + '}</div></td></tr>';
            });

            $(selector).prepend('<div class="col-md-6 col-md-offset-3">' + tabla + '</div>');


            
            for (var a in _control.data.alumnos) {
                var reporte = _control.data.alumnos[a];
                if (reporte.id == alumno.id) {
                    reporte.nombre = alumno.nombre;

                    for (var m in reporte) {
                        if (typeof (reporte[m]) == 'number' && reporte[m] % 1 !== 0) {
                            reporte[m] = reporte[m].toFixed(1);
                        }
                    }

                    $(selector).find('#tControl tbody').append(row.format(reporte));
                    break;
                }

            }
              

           
            $(selector).find('h3.loading').remove();
        });
    }

    var generarTablaResumen = function (selector) {
        Templates.load('tablaResumen', '/scripts/apps/control/views/tablaResumen.html').then(function (tabla) {
            Templates.load('rowResumen', '/scripts/apps/control/views/rowResumen.html').then(function (row) {
                var t = $(tabla);

                $(selector).find('#tControl').parent().after(t);

                _control.data.resumen.map(function (resumen) {
                    for (var m in resumen) {
                        if (typeof (resumen[m]) == 'number' && resumen[m] % 1 !== 0) {
                            resumen[m] = resumen[m].toFixed(1);
                        }
                    }

                    $(t).find('tbody').append(row.format(resumen));
                });

            }); 
        });
    }

    var generarGraficaControl = function(selector,impresion){


        var control = $('<div id="grafica-control" class="' + (impresion == true ? 'col-lg-12' : 'col-lg-6') + ' "  style="height: 450px;"></div>').appendTo(selector);

        var resumen = $('<div id="grafica-resumen"  class="' + (impresion == true ? 'col-lg-12' : 'col-lg-6') + ' " style="height: 450px;"></div>').appendTo(selector);

        var data = [],
            labels = ['Alumnos'];

        var alumnosSubieron = 0,
            alumnosBajaron = 0,
            alumnosBajaronYReprobaron = 0,
            alumnosBajaronYAprobaron = 0,
            alumnosSubieronYReprobaron = 0,
            alumnosReprobaronBimestre = 0,
            alumnos7 = 0,
            alumnos8 = 0,
            alumnos9 = 0,
            alumnos10 = 0,
            promedioBimestral = 0;

        _control.data.resumen.map(function (e) {
            alumnosSubieron += e.alumnosSubieron;
            alumnosBajaron += e.alumnosBajaron;
            alumnosBajaronYReprobaron += e.alumnosBajaronYReprobaron;
            alumnosBajaronYAprobaron += e.alumnosBajaronYAprobaron;
            alumnosSubieronYReprobaron += e.alumnosSubieronYReprobaron;
            alumnosReprobaronBimestre += e.alumnosReprobaronBimestre;
            alumnos7 += e.alumnos7;
            alumnos8 += e.alumnos8;
            alumnos9 += e.alumnos9;
            alumnos10 += e.alumnos10;
            promedioBimestral += e.promedioBimestral;
        });

        data = [
            {
                key: 'Alumnos que subieron calificación',
                value: alumnosSubieron

            },
            {
                key: 'Alumnos que bajaron calificación',
                value: alumnosBajaron

            },
            {
                key: 'Alumnos que bajaron calificación y reprobaron',
                value: alumnosBajaronYReprobaron

            },
            {
                key: 'Alumnos que bajaron calificación y aprobaron',
                value: alumnosBajaronYAprobaron

            },
            {
                key: 'Alumnos que subieron calificación y reprobaron',
                value: alumnosSubieronYReprobaron
            }
        ]

        new Morris.Bar({
            element: control,
            data: data,
            xkey: 'key',
            ykeys: ['value'],
            labels:labels,
            barColors: ['#588990', '#868686', '#d0a55a', '#b97455', '#927b5a']
        });

        data = [
            {
                key: 'Alumnos que reprobaron bimestre',
                value: alumnosReprobaronBimestre

            },
            {
                key: 'Alumnos que tienen 6 a 6.9',
                value: alumnos8

            },
            {
                key: 'Alumnos que tienen 7 a 7.9',
                value: alumnos8

            },
            {
                key: 'Alumnos que tienen 8 a 8.9',
                value: alumnos9

            },
            {
                key: 'Alumnos que tienen 9 a 10',
                value: alumnos10
            }
        ]

        new Morris.Bar({
            element: resumen,
            data: data,
            xkey: 'key',
            ykeys: ['value'],
            labels: labels,
            barColors: ['#4f9d8d']
        });

    }
    

    this.inicializar = function (selector) {
        $(selector).html('<h3 class="text-center loading"><span class="fa fa-refresh fa-spin"></span></h3>');

        _control.cargarDatos().then(function (data) {
           
            generarTablaControl(selector);
        });

    }

    this.imprimir = function () {


        var table = $("#tControl"),
                tableWidth = table.outerWidth(),
                pageWidth = 1800,
                pageCount = Math.ceil(tableWidth / pageWidth),
                printWrap = $("#divInternoImpresion").empty(),
                i,
                printPage;
        for (i = 0; i < pageCount; i++) {
            printPage = $("<div></div>").css({
                "overflow": "hidden",
                "width": pageWidth,
                "page-break-before": i === 0 ? "auto" : "always"
            }).appendTo(printWrap);
            table.clone().removeAttr("id").appendTo(printPage).css({
                "position": "relative",
                "left": -i * pageWidth
            });
        }


        var w = window.open();
        var html = $("#divInternoImpresion").html();

        $('link').each(function () {

            $(w.document.head).append('<link href="' + this.href + '" rel="stylesheet" type="text/css">');
        });

        $(w.document.head).append('<link href="' + location.origin + '/content/impresionTabla.css" rel="stylesheet" type="text/css">');

        $(w.document.head).append($('style').clone());
        $(w.document.body).html(html);

        var grafica = $('<div></div>');

        $(w.document.body).append(grafica);

       

        setTimeout(function () {
           
            w.print();
        }, 800);
    }

    var actualizarGrupo = function () {

        $.ajax({
            url: '/alumnos/ActualizarDesempenio',
            type:'get',
            data: {grupo:_grupo},
            beforeSend: function () {
                Loading('Actualizando grupo espere por favor.');
            },
            complete: function () {
                Loading();
            },
            success: function (response) {
                AlertInfo('Se ha actualizo el reporte');

                autoCargarSeccion();
            }
        });
    }
}