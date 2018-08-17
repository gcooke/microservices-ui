$(document).ready(function () {


    var serverBaseAdd = 'AddServer';
    var controllerBaseAdd = 'AddController';


    function onDropdownChange(resourceName, name, typeName) {
        var buttonId = 'add-' + name;

        console.log(resourceName);
        console.log(name);
        console.log(typeName);

        var buttonWrapper = $('#' + buttonId);

        if (!resourceName) {
            buttonWrapper.hide();
        }
        buttonWrapper.show();
        var baseUrl = typeName == 'Server' ? serverBaseAdd : controllerBaseAdd;

        var newRef = baseUrl + '/' + name + '/' + resourceName;

        buttonWrapper.children().attr('href', newRef);
    }

    $('.server-resource-button').hide();

    $('.server-resource-dropdown').change(function() {
        onDropdownChange($(this).val(), $(this).attr('config-name'), $(this).attr('type-name'));
    });

});