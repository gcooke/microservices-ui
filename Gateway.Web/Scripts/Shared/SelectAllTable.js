$(document).ready(function() {

    setupSelectAllTable();

});


function setupSelectAllTable() {
    $(".select-all-table th input:checkbox").click(
       
        function () {
            if ($(this).is(":checked")) {
                $(".select-all-table td input:checkbox").prop("checked", true);
            } else {
                $(".select-all-table td input:checkbox").prop("checked", false);
            }
        });
    $(".select-all-table td input:checkbox").click(
        function() {
            var checkedCount = $(".select-all-table td input:checkbox:checked").length;
            var totalCount = $(".select-all-table td input:checkbox").length;
            $(".select-all-table th input:checkbox").prop("checked", checkedCount === totalCount);
        });
}