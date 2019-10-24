$('#scaling').change(function () {
    var value = $(this).val();
    if (value == '1') {
        $('#lblInstances').show();
        $('#lblRunning').hide();
    } else {
        $('#lblInstances').hide();
        $('#lblRunning').show();
    }
}).change();