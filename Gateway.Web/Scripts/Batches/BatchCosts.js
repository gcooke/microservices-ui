$(document).ready(function () {
    $(".cost-detail-trigger").on("click",
        function () {
            var $groupTrigger = $(this);
            $("." + $(this).data("group-trigger")).each(function () {
                if ($(this).hasClass("report-table-detail-row-hidden")) {
                    $(this).removeClass("report-table-detail-row-hidden");
                    $groupTrigger.html("[-]");
                } else {
                    $(this).addClass("report-table-detail-row-hidden");
                    $groupTrigger.html("[+]");
                }
            });
        });
});

