var Alerta = function (mensaje, titulo, delay,type, icon,desktop) {
  
    PNotify.prototype.options.styling = "fontawesome";

    if (desktop == true) {
        PNotify.desktop.permission();
    }

    var stack_bar_bottom = { "dir1": "up", "dir2": "right", "spacing1": 0, "spacing2": 0 };

    new PNotify({
        title: titulo,
        text: mensaje,
        icon: icon,
        type: type,
        desktop: {
            desktop: desktop
        },
        addclass: "stack-bar-bottom",
        stack: stack_bar_bottom
    });

}

var AlertError = function (mensaje, titulo, delay,desktop) {
    Alerta(mensaje, titulo, delay,'error', 'fa fa-times-circle',desktop);
}

var AlertSuccess = function (mensaje, titulo, delay, desktop) {
    Alerta(mensaje, titulo, delay, 'success', 'fa fa-check', desktop);
}

var AlertInfo = function (mensaje, titulo, delay, desktop) {
    Alerta(mensaje, titulo, delay, 'info', 'fa fa-info-circle', desktop);
}

var AlertWarning = function (mensaje, titulo, delay, desktop) {
    Alerta(mensaje, titulo, delay, 'warning', 'fa fa-exclamation-triangle', desktop);
}

String.prototype.format = function (data) {
    /***
        Imitar la función de c#. v1.5
    ***/

    var text = this;

    for (var index in data) {

        var pattern = '{' + index + '}';
        var valor = data[index];
        while (text.indexOf(pattern) != -1) {
            text = text.replace(pattern, valor == null || valor == undefined ? '' : valor);
        }

    }

    return text;
}

$.fn.extend({
    addData: function (data) {
        var element = this;
        $.each(data, function (key, value) {
            element.data(key, value);
        });

        return element;
    },
    getDataAsObject: function () {
        var obj = {};
        $.each($(this).data(), function (key, value) {
            obj[key] = value;
        });

        return obj;
    }
});

var ConfirmDialog = {
    html: '<div id="modalConfirm" class="modal fade"  tabindex="-1" role="dialog"><div class="modal-dialog" role="document"><div class="modal-content"><div class="modal-header"><h4 class="modal-title" >{title}</h4></div><div class="modal-body">{text}</div><div class="modal-footer">{buttons}</div></div></div></div>',
    show: function (options) {

        var _opDefault = {
            text: '',
            title: '',
            modalClass: '',
            buttons: undefined,
            callback: function () { },
            positiveButton: true,
            positiveButtonText: 'Aceptar',
            positiveButtonClass: 'btn btn-success',
            negativeButton: true,
            negativeButtonText: 'Cancelar',
            negativeButtonClass: 'btn btn-default',
            closeModalOnAction: true,
            beforeOpen: function () { },
        };

        $.extend(_opDefault, options, true);

        if ($('#modalConfirm').length > 0)
            $('#modalConfirm').remove();

        if (_opDefault.buttons == undefined) {
            _opDefault.buttons = '';
            if (_opDefault.positiveButton == true) {
                _opDefault.buttons += '<button class="' + _opDefault.positiveButtonClass + '" data-confirm="true">' + _opDefault.positiveButtonText + '</button>';
            }
            if (_opDefault.negativeButton == true) {
                _opDefault.buttons += '<button class="' + _opDefault.negativeButtonClass + '" data-confirm="false">' + _opDefault.negativeButtonText + '</button>';
            }

            if (_opDefault.buttons.length == 0)
                throw 'No se especificaron los botones de acción';
            else {

            }

        }

        var $modal = $(ConfirmDialog.html.format(_opDefault));
        if (_opDefault.modalClass)
            $modal.addClass(_opDefault.modalClass);

        $(document.body).append($modal);

        $('#modalConfirm').delegate('[data-confirm]', 'click', function () {
            if (_opDefault.closeModalOnAction == true) $('#modalConfirm').modal('hide');
            if (_opDefault.callback) {
                _opDefault.callback(this.getAttribute('data-confirm') == 'true');
            }
        });

        $modal = $modal.modal({
            backdrop: 'static',
            keyboard: false,
            show: false
        });

        if (_opDefault.beforeOpen)
            _opDefault.beforeOpen($modal);

        $modal.modal("show");
    },
    hide: function () {
        $('#modalConfirm').modal('hide');

    }
}


var Loading = function (message, target) {
    /***
        Muestra/Oculta el loading. v1.6
    ***/
    var content = target == undefined ? document.body : target;
    var loading = $(content).find('#modal_loading');

    if (loading.length == 0) {
        $(content).append('<div id="modal_loading"><div class="bks"></div><div class="content"><h1><span class="fa fa-refresh fa-spin"></span></h1><div class="modal-body"> <p>Guardando registro</p></div> </div></div>');
        loading = $(content).find('#modal_loading');
    }
    if (message != undefined) {
        loading.show();
        loading.find('.modal-body').html(message);
    }
    else
        loading.hide();
}

var Cache = {
    listar: function (regex) {
        var keys = Object.keys(localStorage);
        if (regex != undefined)
            keys = $.grep(keys, function (e) { return e.match(regex); })

        return keys;
    },
    vaciar: function (name) {
        if (name == undefined)
            localStorage.clear();
        else
            localStorage.removeItem(name);
    },
    almacenarCache: function (data, name, minutes) {
        var expiracion = new Date();
        var cache = {
            expiracion: expiracion.setMinutes(expiracion.getMinutes() + minutes),
            data: data
        }
        localStorage.setItem(name, JSON.stringify(cache));
    },
    validarCache: function (name) {
        if (typeof (Storage) !== "undefined") {
            var cache = JSON.parse(localStorage.getItem(name));

            if (cache != null) {
                if (typeof (cache.expiracion) !== "undefined" && (new Date(cache.expiracion) > new Date())) {
                    return cache.data;
                }
            }
        }

        return false
    }
}

// Validar Promise
if (typeof Promise === 'undefined') {
    head.load('/Scripts/rsvp.js', function () {
        Promise = RSVP.Promise; // Para navegadores que no soportan Promise
    });
}

var Templates = new function (tipo) {

    var _templates = {};

    var promise;

    if (typeof Promise === 'undefined') {
        head.load('/Scripts/rsvp.js', function () {
            promise = RSVP.Promise; // Para navegadores que no soportan Promise
        });
    }

    else {
        promise = Promise;
    }
    this.load = function (tipo, url) {

        return new promise(function (success) {
            $.ajax({
                url: url,
                type: 'get',
                beforeSend: function (e) {
                    if (_templates[tipo] != undefined) {
                        e.abort();
                        success(_templates[tipo]);
                    }
                },
                success: function (html) {
                    _templates[tipo] = html;
                    success(_templates[tipo]);
                },
            });
        });
    }

    this.items = _templates;
}

String.prototype.toDate2 = function(){
    var date;
                
    if (/\d{4}-\d{2}-\d{2}/.test(this)) { // YYY-MM-DD
        date = new Date(this.substr(0, 4) + '-' +
                         this.substr(6, 2) + '-' +
                         this.substr(8, 2));
    } else if (/\d{2}(-|\/)?\d{2}(-|\/)\d{4}/.test(this)) { // DD-MM-YYYY
        date = new Date(this.substr(3, 2) + '-' +
                         this.substr(0, 2) + '-' +
                         this.substr(6, 4));
    }
    else
        date = Date.parse(this);
    
    return date;
}

function formatDate(date) {
    var d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [day, month, year].join('/');
}


// Esto arregla el problema de doble modal al mismo tiempo
$(document).on('hidden.bs.modal', '.modal', function () {
    $('.modal:visible').length && $(document.body).addClass('modal-open');
});

// Resaltar cosas
$(document).delegate('.resaltar input, .resaltar textarea,.resaltar td', 'focusin', function () {
    $(this).parents('tr').addClass('success');
});
$(document).delegate('.resaltar input, .resaltar textarea,.resaltar td', 'focusout', function () {
    $(this).parents('tr').removeClass('success');
});

$.fn.extend({
    resaltar: function (clase, expira) {
        var element = this;
        $(element).addClass(clase);
        setTimeout(function () {
            $(element).removeClass(clase);
        }, expira);

        return this;
    },
    removeConEfecto: function () {
        var element = this;
        
        $(element).find('*').css('background-color', '#f99');
        $(element).css({ opacity: 0.3, 'background-color': '#f99' });
        
        setTimeout(function () {
            $(element).remove();
        }, 1500);
    }
});

var QueryString = function () {
    // This function is anonymous, is executed immediately and 
    // the return value is assigned to QueryString!
    var query_string = {};
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        // If first entry with this name
        if (typeof query_string[pair[0]] === "undefined") {
            query_string[pair[0]] = decodeURIComponent(pair[1]);
            // If second entry with this name
        } else if (typeof query_string[pair[0]] === "string") {
            var arr = [query_string[pair[0]], decodeURIComponent(pair[1])];
            query_string[pair[0]] = arr;
            // If third or later entry with this name
        } else {
            query_string[pair[0]].push(decodeURIComponent(pair[1]));
        }
    }
    return query_string;
}();

function processAjaxData(response, urlPath) {
    document.getElementById("content").innerHTML = response.html;
    document.title = response.pageTitle;
    window.history.pushState({ "html": response.html, "pageTitle": response.pageTitle }, "", urlPath);
}

var tableToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
      , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--><meta http-equiv="content-type" content="text/plain; charset=UTF-8"/></head><body><table>{table}</table></body></html>'
      , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
      , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    return function (table, name) {
        if (!table.nodeType) table = document.getElementById(table)
        var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML }
        window.location.href = uri + base64(format(template, ctx))
    }
})()

function romanize(num) {
    if (!+num)
        return false;
    var digits = String(+num).split(""),
		key = ["", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM",
		       "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC",
		       "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX"],
		roman = "",
		i = 3;
    while (i--)
        roman = (key[+digits.pop() + (i * 10)] || "") + roman;
    return Array(+digits.join("") + 1).join("M") + roman;
}

String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};

$.ajaxSetup({
    error: function (xhr) {
        if(xhr.responseText != "")
            AlertError(xhr.responseText.replaceAll('\n',''));
    }
});