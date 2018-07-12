$(document).ready(function () {
    $(".monitoring-report-trigger").on("click",
        function () {
            var $groupTrigger = $(this);
            $("." + $(this).data("group-trigger")).each(function () {
                if ($(this).css("visibility") === "collapse") {
                    $(this).css("visibility", "visible");
                    $groupTrigger.html("[-]");
                } else {
                    $(this).css("visibility", "collapse");
                    $groupTrigger.html("[+]");
                }
            });
        });
});