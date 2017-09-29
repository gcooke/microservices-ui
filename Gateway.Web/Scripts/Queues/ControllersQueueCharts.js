$(document).ready(function () {
    var localStorageKey = "selected-controllers";

    displayCurrentQueueChart();
    displayHistorialQueueChart();

    $(document).on("selectionFiltersChanged", function (event, localStorageKeyData) {
        if (localStorageKey !== localStorageKeyData)
            return;

        displayCurrentQueueChart();
        displayHistorialQueueChart();
    });
});

function displayCurrentQueueChart() {
    var chartElementId = "#current-queues-chart";

    if ($(chartElementId).length <= 0)
        return;

    var progressBar = $("#current-queues-chart-progress");
    var chartElement = $(chartElementId);
    var timestampLabel = $("#current-queues-chart-last-updated");
    var dateTimeFormat = "hh:mm:ss A";

    var options = {
        element: chartElementId,
        type: "donut",
        dataUrl: "Controllers/CurrentQueueData?" + getFiltersQueryStringParameters("selected-controllers", "controllers"),
        dataFormatFunction: dataFormatFunction,
        additionalOptions: {
            donut: {
                label: {
                    threshold: 0.05
                }
            },
            tooltip: {
                format: {
                    value: function(value) {
                        return value + " requests";
                    }
                }
            },
            legend: {
                position: "right"
            }
        },
        onChartRenderStarted: function() {
            progressBar.show();
            chartElement.hide();
        },
        onChartRenderComplete: function() {
            chartElement.show();
            progressBar.hide();
            timestampLabel.html(moment().format(dateTimeFormat));
        },
        refreshInterval: 5000
    }

    renderChart(options);
}

function displayHistorialQueueChart() {
    var chartElementId = "#historical-queues-chart";

    if ($(chartElementId).length <= 0)
        return;

    var progressBar = $("#historical-queues-chart-progress");
    var chartElement = $(chartElementId);
    var timestampLabel = $("#historical-queues-chart-last-updated");
    var dateTimeFormat = "hh:mm:ss A";

    var options = {
        element: chartElementId,
        type: "bar",
        dataUrl: "Controllers/HistoricalQueueData?" + getFiltersQueryStringParameters("selected-controllers", "controllers"),
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
                        position: "outer-center",
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

function dataFormatFunction(data) {
    var multiArray = [];
    for (var key in data) {
        if (data.hasOwnProperty(key)) {
            multiArray.push([].concat.apply([], [key, data[key] === 0 ? null : data[key]]));
        }
    }
    return multiArray;
}
