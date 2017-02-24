var Control = new function () {

    var _control = this;

    var nameCache = function () {
        return 'control-' + _grupo + '-' + _bimestre;
    }

    this.data = [];

    this.cargarDatos = function () {
        return $.ajax({
            url: '/alumnos/CargarReporte',
            type: 'get',
            data: { grupo: _grupo, bimestre: _bimestre },
            beforeSend: function () {
                Loading('Cargando datos');
            },
            complete: function () {
                Loading();
            },
            success: function (data) {
                _control.data = data;
            }
        }).promise();
        
    }

    var generarTablaControl = function (selector) {
        var $def = new $.Deferred();

        Templates.load('tablaControl', '/scripts/apps/control/views/tablaControl.html').then(function (tabla) {

            Alumnos.listar(_grupo).then(function () {

                // generar el header y cuerpo de row
                var row = '<td><div class="w150">{nombre}</div></td>';
                var header = '';
                _control.data.headers.map(function (head) {

                    header += '<th><div class="w100 text-center">' + head.name + '</div></th>';

                    row += '<td><div class="w100">{' + head.key + '}</div></td>';
                });

                header += '<th><div class="w100 text-center">Promedio final del bimestre</div></th>';
                row += '<td><div class="w100">{promedioFinal}</div></td>';

                $(selector).prepend('<div style="overflow:auto">' + tabla.format({ headers: header }) + '</div>');

                var fromUrl = {};
                if (window.location.search !== "") {
                    fromUrl = {};
                    window.location.search.substr(1).split("&").map(function (v) {
                        var token = v.split("=")
                        fromUrl[token[0]] = token[1];
                    });
                }

                var lsPorcName = _grupo + "-" + _bimestre + "-porcentaje";
                var lsData = localStorage.getItem(lsPorcName);
                var lsDataPorc = lsData ? JSON.parse(lsData) : {};
                var porcentajes = _control.data.headers.map(function (header, index) {
                    var porcentaje = fromUrl[header.key] !== void 0 ?
                        (lsDataPorc[header.key] = parseFloat(fromUrl[header.key])) :
                            lsDataPorc[header.key] !== void 0 ?
                            lsDataPorc[header.key] :
                        (lsDataPorc[header.key] = 10);

                    return '<td><input data-porcentaje="' + header.key + '" style="width:30px;" value=' + porcentaje + '>%</td>';
                });
                porcentajes.pop();

                var trPorcentajes = '' +
                    '<tr>' +
                        '<td>Ponderación</td>' +
                        porcentajes.join('') +
                    '</tr>';

                $(trPorcentajes).prependTo($(selector).find('#tControl thead'));

                var headerKeys = _control.data.headers.map(function (h) { return h.key });
                var calcular = function (alum) {
                    for (var a in _control.data.alumnos) {
                        var reporte = _control.data.alumnos[a];
                        var promedioFinal = 0;
                        var promedioFinalCalificaciones = 0;
                        if (reporte.id == alum.id) {
                            reporte.nombre = alum.apellidoPaterno + ' ' + alum.apellidoMaterno + ' ' + alum.nombre;

                            for (var m in reporte) {
                                if (m != "totalFaltas" && headerKeys.indexOf(m) !== -1) {
                                    promedioFinalCalificaciones++;
                                    promedioFinal += reporte[m] * lsDataPorc[m] / 100.0;
                                }

                                if (typeof (reporte[m]) == 'number' && reporte[m] % 1 !== 0) {
                                    reporte[m] = reporte[m].toFixed(1);
                                }
                            }

                            reporte.promedioFinal = promedioFinal > 10 ? 10 : promedioFinal.toFixed(1);

                            _control.data.promediosFinales.filter(function (v) { return v.id == reporte.id }).map(function (e) {
                                e.promedioFinal = promedioFinal > 10 ? 10 : promedioFinal;
                                return e;
                            })

                            var tr = $('<tr>' + row.format(reporte) + '</tr>').appendTo($(selector).find('#tControl tbody'));

                            $(' <span class="fa fa-pie-chart" title="Ver grafica"></span>').appendTo(tr.find('td:first'))
                                                                                          .addData(reporte)
                                                                                          .click(function () {
                                                                                              graficarExamen($(this).getDataAsObject());
                                                                                          });
                            break;
                        }
                    }
                };

                Alumnos.data.map(calcular);

                $(document).on("change keyup keydown", "[data-porcentaje]", function (e, i) {
                    var $me = $(this);
                    lsDataPorc[$me.data("porcentaje")] = $me.val();
                    localStorage.setItem(lsPorcName, JSON.stringify(lsDataPorc));
                    $(selector).find('#tControl tbody').empty();
                    Alumnos.data.map(calcular);
                });

                $(selector).find('h3.loading').remove();
                $def.resolve();
            });
        });

        return $def;
    }

    var estaEnRango = function (calificacion, inclusive) {
        return function (alumno) {
            var prom = parseFloat(alumno.promedioFinal);
            return prom >= calificacion && (!inclusive ? prom < calificacion + 1 : prom <= calificacion + 1);
        };
    }

    var generarTablaResumen = function (selector) {
        var $def = new $.Deferred();

        Templates.load('tablaResumen', '/scripts/apps/control/views/tablaResumen.html').then(function (tabla) {
            Templates.load('rowResumen', '/scripts/apps/control/views/rowResumen.html').then(function (row) {
                var t = $(tabla);

                $(selector).find('#tControl').parent().after(t);
                var alumnos7 = _control.data.alumnos.filter(estaEnRango(6)).length,
                alumnos8 = _control.data.alumnos.filter(estaEnRango(7)).length,
                alumnos9 = _control.data.alumnos.filter(estaEnRango(8)).length,
                alumnos10 = _control.data.alumnos.filter(estaEnRango(9, true)).length,
                promedioBimestral = _control.data.alumnos.reduce((a, b) => parseFloat(a) + parseFloat(b.promedioFinal), 0) / _control.data.alumnos.length;
                var asignar = function (resumen) {
                    resumen["alumnos7"] = alumnos7;
                    resumen["alumnos8"] = alumnos8;
                    resumen["alumnos9"] = alumnos9;
                    resumen["alumnos10"] = alumnos10;
                    resumen["promedioBimestral"] = parseFloat(promedioBimestral.toFixed(1));
                };
                _control.data.resumen.map(function (resumen) {
                    if (resumen.bimestre == "Primer" && _bimestre == 1) asignar(resumen);
                    if (resumen.bimestre == "Segundo" && _bimestre == 2) asignar(resumen);
                    if (resumen.bimestre == "Tercer" && _bimestre == 3) asignar(resumen);
                    if (resumen.bimestre == "Cuarto" && _bimestre == 4) asignar(resumen);
                    if (resumen.bimestre == "Quinto" && _bimestre == 5) asignar(resumen);

                    for (var m in resumen) {
                        if (typeof (resumen[m]) == 'number'/*  && resumen.existe == false */) {
                            //resumen[m] = "N/A";
                            //resumen[m]
                        }
                        else {
                            if (typeof (resumen[m]) == 'number' && resumen[m] % 1 !== 0) {
                                resumen[m] = resumen[m].toFixed(1);
                            }
                        }
                    }


                    $(t).find('tbody').append(row.format(resumen));
                });
    
                $def.resolve();
            }); 
        });

        return $def;
    }

    var generarGraficaControl = function(selector,impresion){
   

        var control = $('<div id="grafica-control" class="' + (impresion == true ? 'col-lg-12 col-md-12' : 'col-lg-6 col-md-6') + ' "  style="height: 450px;"></div>').appendTo(selector);

        var resumen = $('<div id="grafica-resumen"  class="' + (impresion == true ? 'col-lg-12 col-lg-12' : 'col-lg-6  col-md-6') + ' " style="height: 450px;"></div>').appendTo(selector);

        var califas = $('<div id="grafica-califas"  class="' + (impresion == true ? 'col-lg-12 col-md-12' : 'col-lg-12 col-md-12') + ' " style="height: 450px;"></div>').appendTo(selector);

        var grupal = $('<div id="grafica-grupal"  class="' + (impresion == true ? 'col-lg-12 col-md-12' : 'col-lg-12 col-md-12') + ' " style="height: 450px;"></div>').appendTo(selector);

        var alumnosSubieron = 0,
            alumnosBajaron = 0,
            alumnosBajaronYReprobaron = 0,
            alumnosBajaronYAprobaron = 0,
            alumnosSubieronYReprobaron = 0,
            alumnosReprobaronBimestre = 0,
            alumnos7 = _control.data.alumnos.filter(estaEnRango(6)).length,
            alumnos8 = _control.data.alumnos.filter(estaEnRango(7)).length,
            alumnos9 = _control.data.alumnos.filter(estaEnRango(8)).length,
            alumnos10 = _control.data.alumnos.filter(estaEnRango(9, true)).length,
            promedioBimestral = _control.data.alumnos.reduce((a, b) => parseFloat(a) + parseFloat(b.promedioFinal), 0) / _control.data.alumnos.length;

        promedioBimestral = parseFloat(promedioBimestral.toFixed(1));

        _control.data.resumen.map(function (e) {
            alumnosSubieron += ~~e.alumnosSubieron;
            alumnosBajaron += ~~e.alumnosBajaron;
            alumnosBajaronYReprobaron += ~~e.alumnosBajaronYReprobaron;
            alumnosBajaronYAprobaron += ~~e.alumnosBajaronYAprobaron;
            alumnosSubieronYReprobaron += ~~e.alumnosSubieronYReprobaron;
            alumnosReprobaronBimestre += ~~e.alumnosReprobaronBimestre;
        });

        var series = [{
            name: 'Alumnos',
            data: [
                {
                    name: 'Alumnos que subieron calificación',
                    y: alumnosSubieron,
                    color: '#588990'

                },
                {
                    name: 'Alumnos que bajaron calificación',
                    y: alumnosBajaron,
                    color: '#868686'

                },
                {
                    name: 'Alumnos que bajaron calificación y reprobaron',
                    y: alumnosBajaronYReprobaron,
                    color: '#d0a55a'

                },
                {
                    name: 'Alumnos que bajaron calificación y aprobaron',
                    y: alumnosBajaronYAprobaron,
                    color: '#b97455'

                },
                {
                    name: 'Alumnos que subieron calificación y reprobaron',
                    y: alumnosSubieronYReprobaron,
                    color: '#927b5a'
                }
            ]

        }];
       

        $(control).highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: ''
            },
            credits: false,
            xAxis:{
                categories: ['Alumnos que subieron calificación', 'Alumnos que bajaron calificación', 'Alumnos que bajaron calificación y reprobaron', 'Alumnos que bajaron calificación y aprobaron', 'Alumnos que subieron calificación y reprobaron']
            },
            yAxis: {
                allowDecimals: false,
                title: {
                    text: 'Alumnos'
                }
            },
            plotOptions: {
                column: {
                    dataLabels: {
                        enabled: true
                    },
                    enableMouseTracking: false
                }
            },
            legend: {
                enabled:false
            },
            tooltip: {
                formatter: function () {
                    return '<b>' + this.point.y + '</b> ' +this.series.name;
                }
            },
            series: series
        });


        series = [{
            name: 'Alumnos',
            data: [
                {
                    name: 'Alumnos que reprobaron bimestre',
                    y: alumnosReprobaronBimestre,
                    color: '#4f9d8d'

                },
                {
                    name: 'Alumnos que tienen 6 a 6.9',
                    y: alumnos7,
                    color: '#588990'

                },
                {
                    name: 'Alumnos que tienen 7 a 7.9',
                    y: alumnos8,
                    color: '#4f9d8d'

                },
                {
                    name: 'Alumnos que tienen 8 a 8.9',
                    y: alumnos9,
                    color: '#588990'

                },
                {
                    name: 'Alumnos que tienen 9 a 10',
                    y: alumnos10,
                    color: '#4f9d8d'
                }

            ]

        }]

        $(resumen).highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: ''
            },
            credits: false,
            xAxis: {
                categories: ['Alumnos que reprobaron bimestre', 'Alumnos que tienen 6 a 6.9', 'Alumnos que tienen 7 a 7.9', 'Alumnos que tienen 8 a 8.9', 'Alumnos que tienen 9 a 10']
            },
            yAxis: {
                allowDecimals: false,
                title: {
                    text: 'Alumnos'
                }
            },
            legend: {
                enabled: false
            },
            plotOptions: {
                column: {
                    dataLabels: {
                        enabled: true
                    },
                    enableMouseTracking: false
                }
            },
            tooltip: {
                formatter: function () {
                    return '<b>' + this.point.y + '</b> ' + this.series.name;
                }
            },
            series: series
        });

    
        series = [{
            name: 'Alumnos',
            data: []
        }];

        var alumnos = [];

        Alumnos.data.map(function (e) {
            alumnos.push(e.apellidoPaterno + ' ' + e.apellidoMaterno + ' ' + e.nombre);
            var total = 0, sumatoria = 0;
            _control.data.promediosFinales.map(function (h) {
                if (e.id == h.id) {
                    series[0].data.push({
                        color: '#377ad3',
                        y: parseFloat(h.promedioFinal.toFixed(1))
                    });
                }
                
            });

           
        });

        $(califas).highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: ''
            },
            credits: false,
            xAxis: {
                categories: alumnos
            },
            yAxis: {
                allowDecimals: false,
                title: {
                    text: 'Promedio final'
                }
            },
            legend: {
                enabled: false
            },
            plotOptions: {
                column: {
                    dataLabels: {
                        enabled: true
                    },
                    enableMouseTracking: false
                }
            },
            tooltip: {
                formatter: function () {
                    return '<b>' + this.point.y + '</b> ' + this.series.name;
                }
            },
            series: series
        });

        series = [{
            name: 'Bimestre',
            data: [],

            color: '#377ad3',
        }];

        series[0].data.push({
            y: _bimestre == 1 ? parseFloat(promedioBimestral.toFixed(1)) : parseFloat(_control.data.promedioGrupal.bimestre1.toFixed(1))
        });
        series[0].data.push({
            y: _bimestre == 2 ? parseFloat(promedioBimestral.toFixed(1)) : parseFloat(_control.data.promedioGrupal.bimestre2.toFixed(1))
        });
        series[0].data.push({
            y: _bimestre == 3 ? parseFloat(promedioBimestral.toFixed(1)) : parseFloat(_control.data.promedioGrupal.bimestre3.toFixed(1))
        });
        series[0].data.push({
            y: _bimestre == 4 ? parseFloat(promedioBimestral.toFixed(1)) : parseFloat(_control.data.promedioGrupal.bimestre4.toFixed(1))
        });
        series[0].data.push({
            y: _bimestre == 5 ? parseFloat(promedioBimestral.toFixed(1)) : parseFloat(_control.data.promedioGrupal.bimestre5.toFixed(1))
        });


        $(grupal).highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: ''
            },
            credits: false,
            xAxis: {
                categories: ['Bimestre 1', 'Bimestre 2', 'Bimestre 3', 'Bimestre 4', 'Bimestre 5']
            },
            yAxis: {
                allowDecimals: false,
                title: {
                    text: 'Promedio del grupo'
                }
            },
            legend: {
                enabled: false
            },
            plotOptions: {
                column: {
                    dataLabels: {
                        enabled: true
                    },
                    enableMouseTracking: false
                }
            },
            tooltip: {
                formatter: function () {
                    return '<b>' + this.point.y + '</b> ' + this.series.name;
                }
            },
            series: series
        });

  
    }
    

    this.inicializar = function (selector) {
        $(selector).html('<h3 class="text-center loading"><span class="fa fa-refresh fa-spin"></span></h3>');
        return new Promise(function (ok, no) {
            _control.cargarDatos().then(function (data) {
                return generarTablaControl(selector).then(function () {
                    return generarTablaResumen(selector).then(function () {
                        ok();
                        return generarGraficaControl(selector);
                    });
                });
            })
        })
    }

    this.imprimir = function () {

        if (this.data.length == 0) {
            AlertWarning('Aun no se ha terminado de cargar los datos, por favor espere un momento','Control');
            return;
        }

        var w = window.open();
       
        $('link').each(function () {

            $(w.document.head).append('<link href="' + this.href + '" rel="stylesheet" type="text/css">');
        });

        $(w.document.head).append('<link href="' + location.origin + '/content/impresionTabla.css" rel="stylesheet" type="text/css">');

        $(w.document.head).append($('style').clone());
        $(w.document.body).append($('.header-control').clone().removeClass('visible-print'));
        $(w.document.body).append($("#tControl").clone());
        

        $(w.document.body).append($('#tabla-resumen').clone());

        var grafica = $('<div style="width:80%;margin:10px 0;"></div>');


        $(w.document.body).append(grafica);

        $(w.document.body).find('.fa').remove();
        $(w.document.body).find('td,th').css({
            padding: '2px',
            'text-align':'center',
            fontsize: '10px'
        });

        $(w.document.body).find('table:eq(1) th:not(:first)').addClass('vertical');
        $(w.document.body).find('table:eq(1) .w100').removeClass('w100');

        $(w.document.body).find('table:not(:first)').removeClass().attr('border',1).css('margin-bottom','10px');

        setTimeout(function () {
            generarGraficaControl(grafica,true);
            setTimeout(function () {
                w.print();
            }, 500);
        }, 1200);
    }

    this.imprimirWS = function () {
        var div = $("#cuerpo_impresion");
        var grafica = $('<div style="width:80%;margin:10px 0;"></div>');
        div.append(grafica);

        setTimeout(function () {
            generarGraficaControl(grafica, true);
            setTimeout(function () {
                print();
            }, 500);
        }, 1200);
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

    var graficarExamen = function (_data) {

      
        ConfirmDialog.show({
            title: 'Examenes de ' + _data.nombre ,
            text: '<fieldset><button id="btnImprimirModal" class="btn btn-danger"><span class="fa fa-print"></span> Imprimir</button><div id="grafica" style="height:300px;" class="col-lg-12"></div></fieldset>',
            beforeOpen: function () {
                $('#modalConfirm #btnImprimirModal').click(function () {
                    ImprimirExamen(_data);
                });

                setTimeout(function () {
                    generarGraficaExamen('#modalConfirm #grafica',_data);
                }, 500);


            },
            negativeButton: false,
            positiveButtonText:'Aceptar'
        });
    }

    var generarGraficaExamen = function (content, _data) {
        var data = [],
            labels = [];

               

        _control.data.headers.map(function (e) {
                   
            if (e.examen == true) {
                data.push({
                    name: e.name,
                    y: parseFloat(_data[e.key])
                });


                labels.push(e.name);
            }

        });

        // Agregar 4 instrumentos
        var total = 0;
        _control.data.headers.map(function (e) {

            if (e.instrumentos == true && total < 4) {
                data.push({
                    name: e.name,
                    y: parseFloat(_data[e.key])
                });


                labels.push(e.name);
                total++;
            }

        });

        // Agregar los otros a mano
        var extras =  {
            'promedioTrabajo':'Trabajos',
            'promedioCoevaluacion': 'Coevaluación',
            'promedioAutoevaluacion': 'Autoevaluación'
        };

        for (var m in extras) {
            data.push({
                name: extras[m],
                y: parseFloat(_data[m])
            });


            labels.push(extras[m]);
        }

      

        $(content).highcharts({
            chart: {
                type: 'line'
            },
            title: {
                text: 'Calificaciones de ' + _data.nombre
            },
            xAxis: {
                categories: labels
            },
            credits: false,
            yAxis: {
                allowDecimals: false,
                title: {
                    text: 'Calificación'
                }
            },
            plotOptions: {
                line: {
                    dataLabels: {
                        enabled: true
                    },
                    enableMouseTracking: false
                }
            },
            tooltip: {
                formatter: function () {
                    return '<b>' + this.series.name + '</b>' +
                        this.point.y;
                }
            },
            series: [{
                name: 'Calificación',
                data: data
            }]
        });
    }

    var ImprimirExamen = function (data) {



        var w = window.open();

        var grafica = $('<div></div>');

        $(w.document.body).append($('.header-control').clone())
        $(w.document.body).append(grafica);



        setTimeout(function () {
            generarGraficaExamen(grafica, data);
            setTimeout(function(){w.print()}, 800);
        }, 800);

    }

    var obtenerColorPorCalificacion = function (val) {
       
        var red = new Color(232, 9, 26),
            white = new Color(255, 255, 255),
            green = new Color(6, 170, 60),
            start = red,
            end = green;

        val = val -5;
      
        var startColors = start.getColors(),
            endColors = end.getColors();
        var r = Interpolate(startColors.r, endColors.r, 5, val);
        var g = Interpolate(startColors.g, endColors.g, 5, val);
        var b = Interpolate(startColors.b, endColors.b, 5, val);

       return  "rgb(" + r + "," + g + "," + b + ")";
       
    }

    function Interpolate(start, end, steps, count) {
        var s = start,
            e = end,
            final = s + (((e - s) / steps) * count);
        return Math.floor(final);
    }

    function Color(_r, _g, _b) {
        var r, g, b;
        var setColors = function (_r, _g, _b) {
            r = _r;
            g = _g;
            b = _b;
        };

        setColors(_r, _g, _b);
        this.getColors = function () {
            var colors = {
                r: r,
                g: g,
                b: b
            };
            return colors;
        };
    }
}