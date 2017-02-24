// Se debe cargar este script después de los de validador
if (jQuery.validator != undefined) {
    jQuery.extend(jQuery.validator.messages, {
        required: "Campo Obligatorio.",
        remote: "Please fix this field.",
        email: "Formato de email invalido.",
        url: "Formato de URL invalido.",
        date: "Formato de decha invalido.",
        dateISO: "Please enter a valid date (ISO).",
        number: "No es un numero.",
        digits: "Please enter only digits.",
        creditcard: "Please enter a valid credit card number.",
        equalTo: "Please enter the same value again.",
        accept: "Please enter a value with a valid extension.",
        maxlength: jQuery.validator.format("La longitud máxima es de {0} caracteres."),
        minlength: jQuery.validator.format("La longitud mínima es de {0} caracteres."),
        rangelength: jQuery.validator.format("Please enter a value between {0} and {1} characters long."),
        range: jQuery.validator.format("El valor debe estar entre {0} y {1}."),
        max: jQuery.validator.format("El valor debe ser menor o igual a {0}."),
        min: jQuery.validator.format("El valor debe ser mayor o igual a {0}.")
    });

    jQuery.extend(jQuery.validator.methods, {
        maxlength: function (value, element, param) {
            if (element && !element.attributes.maxlength)
                $(element).attr('maxlength', param); // Poner el maxlength por default
            return this.optional(element) || this.getLength($.trim(value), element) <= param;
        },
    })
}