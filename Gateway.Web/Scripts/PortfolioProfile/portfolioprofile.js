function setupCalendar() {
    var options = {
        selectedDate: $("#profile-paginator").data("current-date"),
        endDate: moment(),
        injectStyle: false,
        showOffDays: false,
        text: "MMM<br/>DD",
        onSelectedDateChanged: function (event, date) {
            window.location.href = "?site=SOUTH_AFRICA&valuationDate=" + moment(date).format("YYYY-MM-DD");
        }
    }
    $("#profile-paginator").datepaginator(options);
    $("#profile-paginator li").each(function (index, value) {
        var $date = $(value).find("a");
        var date = $date.data("moment");
        if (typeof (date) !== "undefined") {
            $date.attr("id", "business-date-" + date);
        }
    });
}