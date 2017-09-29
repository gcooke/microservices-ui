$(document).ready(function () {
    var controller = $("#historical-version-queues-chart").data("controller");
    var localStorageKey = "selected-versions-"+controller;

    displayHistoricalQueueChart();
    displayLiveQueueChart();

    $(document).on("selectionFiltersChanged", function (event, localStorageKeyData) {
        if (localStorageKey !== localStorageKeyData)
            return;
        displayHistoricalQueueChart();
        displayLiveQueueChart();
    });
});

function displayHistoricalQueueChart() {
    var chartElementId = "#historical-version-queues-chart";

    if ($(chartElementId).length <= 0)
        return;

    var progressBar = $("#historical-version-queues-chart-progress");
    var chartElement = $(chartElementId);
    var timestampLabel = $("#historical-version-queue-chart-last-updated");
    var dateTimeFormat = "hh:mm:ss A";
    var controller = chartElement.data("controller");

    var options = {
        element: chartElementId,
        type: "bar",
        dataUrl: "/Controller/GetHistoricalQueueData?controllerName=" + controller + "&" + getFiltersQueryStringParameters("selected-versions-" + controller, "versions"),
        dataFormatFunction: function(data) {
            return formatDataFunction(data.Data);
        },
        additionalOptions: {
            axis: {
                x: {
                    type: "category",
                    categories: getHistoricalCategories(),
                    label: {
                        text: "Time (Past 24 Hours)",
                        position: "outer-center"
                    }
                },
                y: {
                    label: {
                        text: "Number of Requests",
                        position: "outer-middle"
                    }
                }
            },
            padding: {
                right: 30
            },
            size: {
                height: 480
            }
        },
        onChartRenderStarted: function () {
            progressBar.show();
            chartElement.hide();
        },
        onChartRenderComplete: function (chart, data) {
            var groups = [];
            data.forEach(function (element) {
                groups.push(element[0]);
            });
            chart.groups([groups]);
            chartElement.show();
            progressBar.hide();
            timestampLabel.html(moment().format(dateTimeFormat));
        }
    }

    renderChart(options);
}

function displayLiveQueueChart() {
    var chartElementId = "#live-version-queues-chart";

    if ($(chartElementId).length <= 0)
        return;

    var progressBar = $("#live-version-queues-chart-progress");
    var chartElement = $(chartElementId);
    var timestampLabel = $("#live-version-queue-chart-last-updated");
    var dateTimeFormat = "hh:mm:ss A";
    var controller = chartElement.data("controller");
    var startTime = moment().format("YYYY-MM-DDTHH:00:00");
    var endTime = moment(startTime).add(1, "hours").format("YYYY-MM-DDTHH:00:00");

    var options = {
        element: chartElementId,
        type: "line",
        dataUrl: "/Controller/GetLiveQueueData?startDateTime=" + startTime + "&endDateTime=" + endTime + "&controllerName=" + controller + "&" + getFiltersQueryStringParameters("selected-versions-"+controller, "versions"),
        dataFormatFunction: function (data) {
            var result = formatDataFunction(data.Data);
            return result;
        },
        additionalOptions: {
            axis: {
                x: {
                    label: {
                        text: "Time",
                        position: "outer-middle"
                    },
                    tick: {
                        count: 4
                    }
                },
                y: {
                    label: {
                        text: "Number of Requests",
                        position: "outer-middle"
                    }
                }
            }
        },
        onChartRenderStarted: function () {
            progressBar.show();
            chartElement.hide();
        },
        onChartRenderComplete: function (chart, data) {
            chartElement.show();
            progressBar.hide();
            timestampLabel.html(moment().format(dateTimeFormat));
        },
        refreshInterval: 1000
    }

    renderChart(options);
}

function formatDataFunction(data) {
    var multiArray = [];
    for (var key in data) {
        if (data.hasOwnProperty(key)) {
            multiArray.push([].concat.apply([], [key, data[key] === 0 ? null : data[key]]));
        }
    }
    return multiArray;
}