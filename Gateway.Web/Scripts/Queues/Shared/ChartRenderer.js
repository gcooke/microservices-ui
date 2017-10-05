function renderChart(options) {
    validateOptions(options);

    var basicChartOptions = {
        bindto: options.element,
        data: {
            columns: [],
            type: options.type,
            empty: {
                label: {
                    text: "No data could be found."
                }
            }
        }
    };

    jQuery.extend(basicChartOptions, options.additionalOptions);

    var chart = c3.generate(basicChartOptions);

    options.onChartRenderStarted(chart);

    loadChartData(chart, options);
    setInterval(function () {
        refreshChartData(chart, options);
    }, options.refreshInterval || 30000);
}

function loadChartData(chart, options) {
    $.get(options.dataUrl, function (result) {
        var data = options.dataFormatFunction(result);

        if (!options.loadChartDataFunction) {
            chart.load({
                columns: data
            });
        } else {
            options.loadChartDataFunction(chart, data);
        }

        options.onChartRenderComplete(chart, data);
    });
}

function refreshChartData(chart, options) {
    var refreshUrl = typeof (options.getDataRefreshUrl) === "undefined" ? options.dataRefreshUrl : options.getDataRefreshUrl();
    $.get(refreshUrl || options.dataUrl, function (result) {
        var data = options.dataFormatFunction(result);

        if (!options.refreshChartDataFunction) {
            chart.load({
                columns: data
            });
        } else {
            options.refreshChartDataFunction(chart, data);
        }

        options.onChartRenderComplete(chart, data);
    });
}

function validateOptions(options) {
    if (!options.element || options.element == null)
        throw "HTML chart element not specified.";

    if (!options.type || options.type == null)
        throw "Chart type not specified.";

    if (!options.dataUrl || options.dataUrl == null)
        throw "Chart data URL not specified.";

    if (!options.dataFormatFunction || options.dataFormatFunction == null)
        throw "Chart data format function not specified.";
}

function addToCurrentDate(part, increment) {
    return moment().add(increment, part);
}

function getHistoricalCategories() {
    var timeIntervals = [];
    for (var i = 24; i >= 1; i--) {
        timeIntervals.push(addToCurrentDate(-1 * i, "hours").format("HHa"));
    }
    return timeIntervals;
}