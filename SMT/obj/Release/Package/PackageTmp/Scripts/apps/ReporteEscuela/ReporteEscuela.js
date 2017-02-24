var ReporteEscuela = (function () {
    var $tGrupos;
    var $bimestre;
    var $trigger;
    var $info;
    var lista;
    var chartData;
    var $general

    var init = function () {
        $tGrupos = $("#tGrupos");
        if ($tGrupos.length) initListaGrupos();
        else initReporte();
    }

    var initListaGrupos = function () {
        $trigger = $("button[data-generar]");
        $bimestre = $('[name=numBimestre]');
        $info = {
            tr: $tGrupos.find("tr[data-info]"),
            div: $tGrupos.find("tr[data-info] .info")
        }
        lista = fetchData();
        updateTable();
        $tGrupos.stupidtable();
        $trigger.click(generar)
    };

    /* ------------ 
    Graficas de reporte
       ------------ */
    var hcdefault = {
        chart: { type: 'column' },
        title: { text: '' },
        credits: false,
        legend: { enabled: false },
        series: [],
        xAxis: { type: "category" },
        plotOptions: {
            series: {
                borderWidth: 0,
                dataLabels: {
                    enabled: true,
                    format: '{point.y:.2f}'
                }
            }
        },
        tooltip: {
            headerFormat: '<span style="font-size:11px">{series.name}</span>',
            pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y:.2f}</b>'
        }
    };

    var tabularPromediosGenerales = function () {
        $(".promedios-generales").highcharts($.extend({}, hcdefault, {
            yAxis: {
                min: 0,
                max: 10,
                title: { text: 'Promedio' }
            },
            series: [{
                name: " ",
                data: chartData.map(function (i) {
                    return {
                        name: i.Grupo,
                        y: i.Promedio,
                    }
                })
            }],
        }))
    };

    var tabularPromediosDesgloce = function () {
        $(".promedios-desgloce").highcharts($.extend({}, hcdefault, {
            plotOptions: { series: { dataLabels: { enabled: false } } },
            tooltip: {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y:.0f}</b></td></tr>',
                footerFormat: '</table>',
                shared: true,
                useHTML: true
            },
            legend: { enabled: true },
            xAxis: {
                categories: chartData.map(function (i) { return i.Grupo }),
                crosshair: true
            },
            yAxis: {
                min: 0,
                title: { text: 'Cantidad de alumnos' }
            },
            series: [{
                name: 'Reprobados',
                data: chartData.map(function (i) { return i.NoAprovados })
            }, {
                name: '6',
                data: chartData.map(function (i) { return i.P6 })
            }, {
                name: '7',
                data: chartData.map(function (i) { return i.P7 })
            }, {
                name: '8',
                data: chartData.map(function (i) { return i.P8 })
            }, {
                name: '9 - 10',
                data: chartData.map(function (i) { return i.P9Y10 })
            }]
        }))
    };

    var initReporte = function () {
        tabularPromediosGenerales();
        tabularPromediosDesgloce();
        console.log(chartData)
    };


    /* ------------ 
      Lista de Grupos
       ------------ */
    var showLoading = function () {
        displayInfo(message, "alert-info", "fa-refresh", true)
    }
    var showInfo = function (message) {
        displayInfo(message, "alert-info", "fa-info-circle")
    }
    var showError = function (message) {
        displayInfo(message, "alert-danger", "fa-info-circle")
    }
    var displayInfo = function (text, alertClass, iconClass, loading) {
        var $icon = $info.div.find(".fa");
        var $text = $info.div.find(".text");

        $info.tr.show("slow");
        $info.div.attr("class", alertClass + " info alert text-center");
        $icon.attr("class", iconClass + (loading ? " fa fa-spin fa-refresh" : " fa"))
        $text.html(text)
    }

    var generar = function () {
        var bimestre = $bimestre.html();
        var idsGrupos = $tGrupos.find("tbody tr input:checked").map(function (i, e) {
            return $(e).data("id")
        }).toArray();

        if (!idsGrupos.length) {
            AlertError("Debe seleccionar por lo menos a un profesor", "Ocurrió un problema");
            return;
        }

        var url = __url__ + 'ReporteEscuela/Reporte';
        var $form = $("<form method=post action='" + url + "'>");
        $(document.body).append($form);
        $("<input>", {
            name: "ids",
            val: idsGrupos.join(","),
            appendTo: $form
        });
        $("<input>", {
            name: "bim",
            val: bimestre,
            appendTo: $form
        });

        $form.submit();
    }

    var updateTable = function () {
        lista.catch(function (err) {
            showError(err);
            AlertError(err);
        })
        lista.then(function (data) {
            if (data.length) $info.tr.hide();
            else showInfo("No se encontraron profesores");

            $tGrupos.find("tbody").append(data.map(function (grupo) {
                var $tr = $(("" +
                    "<tr>" +
                        "<td><input data-id='{IDGrupo}' type=checkbox></td>" +
                        "<td>{Materia}</td>" +
                        "<td>{Nombre}</td>" +
                        "<td>{Grupo}</td>" +
                        "<td><a href='javascript:void(0);' class='btn btn-default btn-circle'>" +
                            " <span class='fa fa-plus'></span> " +
                        "</a></td>" +
                    "</tr>").format(grupo));

                $tr.data("raw", grupo);
                $tr.click(function (ev) {
                    var $chk = $(this).find("input");
                    var $a = $(this).find("a");
                    if (ev.target == $chk.get(0) || ev.target == $a.get(0)) return;
                    else $chk.click();
                });

                return $tr;
            }));
        })
    }

    var fetchData = function () {
        return new Promise(function (ok, no) {
            $.post(__url__ + "ReporteEscuela/ListarGrupos", function (res) {
                if (res.result) ok(res.data)
                else no(res.message)
            }).fail(function () { no("Ocurrió un problema al cargar la información") })
        });
    }

    $(init);
    return { // public api
        cambiar: function (num) {
            $bimestre.html(num);
        },
        promedios: function (data) {
            chartData = data;
        }
    };
}());