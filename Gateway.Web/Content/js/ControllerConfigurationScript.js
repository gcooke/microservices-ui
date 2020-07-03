$('#scaling').change(function () {
    var value = $(this).val();
    if (value == '1') {

        // Automatic - hide priority limits section
        $('#PriorityLimitsLabel').hide();
        $('#PriorityLimitsGrid').hide();
        $('#MaxInstancesGroup').show();
        $('#MaxPrioritiesGroup').show();
        $('#NestedPriorityGroup').show();

        $('#AutomaticLabel').show();
        $('#FixedLabel').hide();
        $('#CloudLabel').hide();

        $('#PodConfigurationGroup').hide();
    } else if (value == '2') {

        // Fixed - hide maximum instances field
        $('#PriorityLimitsLabel').text('Running workers per server');
        $('#PriorityLimitsTableHeaderLabel').text('Running workers per server');
        $('#PriorityLimitsLabel').hide();
        $('#PriorityLimitsGrid').show();
        $('#MaxInstancesGroup').hide();
        $('#NestedPriorityGroup').show();
        $('#MaxPrioritiesGroup').show();

        $('#AutomaticLabel').hide();
        $('#FixedLabel').show();
        $('#CloudLabel').hide();

        $('#PodConfigurationGroup').hide();
    } else {

        // Cloud - hide max instances, max priority and priority limits
        $('#PriorityLimitsLabel').text('Desired replicas per cluster');
        $('#PriorityLimitsTableHeaderLabel').text('Desired replicas per cluster');
        $('#PriorityLimitsLabel').hide();
        $('#PriorityLimitsGrid').show();
        $('#MaxInstancesGroup').hide();
        $('#NestedPriorityGroup').hide();
        $('#MaxPrioritiesGroup').hide();
        $('#AutomaticLabel').hide();
        $('#FixedLabel').hide();
        $('#CloudLabel').show();

        $('#PodConfigurationGroup').show();
    }
}).change();