﻿$(document).ready(function() {
    setupSelectAllTrigger();
    setupBatchTable();
    setupCalendar();
    pollResults();
    setupRowFunctions();
    setupTabs();
});

function setupBatchTable() {
    $(".batch-table").treetable({ expandable: true });
}

function setupTabs()
{
    $('.tab-link[data-toggle="tab"]').on('shown.bs.tab',
        function (e) {
            $(this).parent().removeClass("active");
        });

    $("#myTabs a").click(function (e) {
        e.preventDefault();
        $(this).tab("show");
    });
}

function setupSelectAllTrigger() {
    $(".select-all-trigger").on("click",
        function () {
            var childClass = $(".select-all-trigger").data("children");
            if ($(".select-all-trigger").is(":checked")) {
                $("." + childClass).prop("checked", true);
                $(".report-table-detail-row-hidden").removeClass("report-table-detail-row-hidden");
                $(".monitoring-report-trigger").html("[-]");
            } else {
                $("." + childClass).prop("checked", false);
            }
        });
}

function setupRowFunctions() {

    $(".editable-table").on("click", "a",
        function (e) {
            var $link = $(e.target);
            var table = $link.parent().parent().parent().children();

            if (table.length <= 1)
                return;

            var row = $(this).parent().parent();
            row.remove();
        });


    $(".add-row-button").on("click",
        function () {
            var table = $(this).data("table-body");
            var $table = $(table);
            var $lastRow = $(table + " tr:last");
            var index = $table.children().length;
            var html = $lastRow.html();

            html = html.replace(/_[0-9]*__/gi, "_" + index + "__");
            html = html.replace(/\[[0-9]\]*/gi, "[" + index + "]");

            $table.append("<tr>" + html + "</tr>");
        });  
}

function setupCalendar() {
    var options = {
        selectedDate: $("#paginator").data("current-date"),
        endDate: moment(),
        injectStyle: false,
        showOffDays: false,
        text: "MMM<br/>DD",
        onSelectedDateChanged: function (event, date) {
            window.location.href = "?businessDate=" + moment(date).format("YYYY-MM-DD");
        }
    }
    $("#paginator").datepaginator(options);
    $("#paginator li").each(function (index, value) {
        var $date = $(value).find("a");
        var date = $date.data("moment");
        if (typeof (date) !== "undefined") {
            $date.attr("id", "business-date-" + date);
        }
    });
}

function pollResults() {
    var shouldPollResults = $("#pollResults").val();
    console.log(shouldPollResults);
    if (shouldPollResults !== "true")
        return;

    var businessDate = $("#businessDate").data("date");
    $.get("Schedule/Status?includeDailySummaries=true&businessDate=" + businessDate, function (data) {
        updateView(data);
    });

    setInterval(function () {
        var businessDate = $("#businessDate").data("date");
        $.get("Schedule/Status?includeDailySummaries=true&businessDate=" + businessDate, function (data) {
            updateView(data);
        });
    }, 5000);
}

function updateView(data) {
    var statusList = JSON.parse(data);
    $.each(statusList,
        function (index, value) {
            $("#status-" + value.ScheduleId).html(getStatusHtml(value));
            $("#timing-" + value.ScheduleId).html(getTimingsHtml(value));

            $("#paginator li").each(function (index, item) {
                var date = $(item).find("a").data("moment");
                var $date = $(item).find("a");

                if (typeof (date) !== "undefined") {
                    if (typeof (value.DailySummaries[date]) !== "undefined") {
                        var $icon = $date.find("span");
                        if (value.DailySummaries[date] !== "") {
                            var color = getColor(value.DailySummaries[date]);
                            $icon.css("color", color);
                            $icon.css("display", "inline");
                        }
                    }
                }
            });
        });
}

function getStatusHtml(value) {
    var html = "";
    var color = getColor(value.Status);
    var icon = getIcon(value);

    if (value.RequestId !== null) {
        html = "<a target='_blank' href='~/Request/Summary?correlationId=" + value.RequestId + "'>" +
            "<span style='color: " + color + "'>" +
            "<span class='" + icon +"'></span> " +
            value.Status +
            "</span>" +
            "</a>";
    } else {
        html = "<span style='color: " + color + "'>" +
            "<span class='"+icon+"'></span> " +
            value.Status +
            "</span>";
    }

    if ((value.Status === 'Succeeded' || value.Status === 'Failed') && value.Retries !== null && value.Retries > 0) {
        html += " <span style='padding: 0 !important; color: " + color + ";'><span class='glyphicon glyphicon-retweet'></span> " + value.Retries + "</span>";
    }

    return html;
}

function getTimingsHtml(value) {
    var html = "";
    var color = getColor(value.Status);

    var start = value.StartedAt === null ? "" : value.StartedAt;
    var end = value.FinishedAt === null ? "" : value.FinishedAt;
    if (value.StartedAt !== null && value.FinishedAt !== null) {
        html = "<span style = 'color: " + color + ";'>" +
            "<span class='glyphicon glyphicon-dashboard'></span> " +
            start + " - " + end + " (" + value.TimeTakenFormatted + ")" +
            "</span>";
        return html;
    }

    if (value.StartedAt !== null) {
        html = "<span style = 'color: " + color + ";'>" +
            "<span class='glyphicon glyphicon-dashboard'></span> " +
            start + " - present" +
            "</span>";
        return html;
    }

    return html;
}

function getColor(status) {
    var color = "";
    if (status === "Succeeded") {
        color = "green";
    }
    else if (status === "Failed") {
        color = "red";
    }
    else {
        color = "orange";
    }
    return color;
}

function getIcon(value) {
    var icon = "";
    if (value.Status === "Succeeded") {
        icon = "glyphicon glyphicon-ok";
    }
    else if (value.Status === "Failed") {
        icon = "glyphicon glyphicon-remove";
    }
    else {
        icon = "glyphicon glyphicon-hourglass";
    }
    return icon;
}

function asyncGet(url) {
    $.get(url);
}

function bulkFunction(childClass, baseUrl, isAsync) {
    var items = [];
    $("." + childClass).each(function (index) {
        if ($(this).is(":checked")) {
            var id = $(this).data("id");
            items.push(id);
        }
    });

    if (items.length === 0)
        return;

    var url = baseUrl + "?items=" + items.join();

    if (isAsync) {
        window.location.href = url;
    } else {
        this.document.location.href = url;
    }
}