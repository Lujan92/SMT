var licencias = $("#_licencias_");
var $paquetes = $("input.mes1, input.mes3, input.mes4, input.mes6, input.mes10");
var qty_licencias = $("#qty_licencias")
    qty_licencias = qty_licencias.length ? qty_licencias : { val: function () { return 1 }, mousemove: function () { } };

$("#cantidad").html("$97.00");

qty_licencias.mousemove(function () {
    $paquetes.filter(function (i, v) {
        return $(v).is(":checked")
    }).click();
})

var formatMoney = function (money) {
    money = money.constructor == Number ? money : parseFloat(money);
    var formatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2
    });
    return formatter.format(isNaN(money) ? 0 : money);
}

$(".mes1").click(function () {
    $("#cantidad").html((formatMoney(97 * qty_licencias.val())));
    $('#periodo').prop('checked', false);
    $('.mes1').prop('checked', true);
});
$(".mes3").click(function () {
    $("#cantidad").html((formatMoney(745 * qty_licencias.val())));

    $('#periodo').prop('checked', false);
    $('.mes3').prop('checked', true);
});
$(".mes4").click(function () {
    $("#cantidad").html((formatMoney(847 * qty_licencias.val())));
    $('#periodo').prop('checked', false);
    $('.mes4').prop('checked', true);
});
$(".mes6").click(function () {
    $("#cantidad").html((formatMoney(1089 * qty_licencias.val())));
    $('#periodo').prop('checked', false);
    $('.mes6').prop('checked', true);
});
$(".mes10").click(function () {
    $("#cantidad").html((formatMoney(1800 * qty_licencias.val())));
    $('#periodo').prop('checked', false);
    $('.mes10').prop('checked', true);
});
$("#realizar").click(function (ev) {
    ev.preventDefault();
    var paquete = $paquetes
        .map(function (i, v) { return { i, v } })
        .filter(function(i, v) {return $(v.v).is(":checked")})
        .toArray()[0].i;
   
    var $form = $("<form method=post>");
    $(document.body).append($form);
    var $licencias = $("<input name='Licencias' value='" + qty_licencias.val() + "'>").appendTo($form);
    var $licencias = $("<input name='Paquete' value='" + paquete + "'>").appendTo($form);

    $form.submit();
})