$('#scaling').change(function () {
    var value = $(this).val();
    if (value == '1') {

        // Automatic - hide priority limits section
        $('#PriorityLimitsLabel').hide();
        $('#PriorityLimitsGrid').hide();
        $('#MaxInstancesGroup').show();
        $('#MaxPrioritiesGroup').show();

        $('#AutomaticLabel').show();
        $('#FixedLabel').hide();
        $('#CloudLabel').hide();

    } else if (value == '2') {

        // Fixed - hide maximum instances field
        $('#PriorityLimitsLabel').show();
        $('#PriorityLimitsGrid').show();
        $('#MaxInstancesGroup').hide();
        $('#MaxPrioritiesGroup').show();

        $('#AutomaticLabel').hide();
        $('#FixedLabel').show();
        $('#CloudLabel').hide();

    } else {

        // Cloud - hide max instances, max priority and priority limits
        $('#PriorityLimitsLabel').hide();
        $('#PriorityLimitsGrid').hide();
        $('#MaxInstancesGroup').hide();
        $('#MaxPrioritiesGroup').hide();

        $('#AutomaticLabel').hide();
        $('#FixedLabel').hide();
        $('#CloudLabel').show();
    }
}).change();