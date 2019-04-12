$(document).ready(function () {
    $(".monitoring-report-trigger").on("click",
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

    $(".visibility-toggle").on("click",
        function () {
            var link = $(this).data("link");
            console.log(link);

            var $item = $(".visibility-" + link);
            console.log($item.hasClass("hide-item"));

            if ($item.hasClass("hide-item")) {
                $item.removeClass("hide-item");
                $item.addClass("show-item");
                $(this).html("Hide Remediation");
            } else {
                $item.removeClass("show-item");
                $item.addClass("hide-item");
                $(this).html("View Remediation");
            }
        });
});

