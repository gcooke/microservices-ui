$("#loading").hide();

$(function () {
    $("form").submit(function () {

        $(this).find(":input[type=submit]").prop("disabled", "disabled");
        $(":submit").attr("disabled", "disabled");
        $("#loading").show();
    });
});