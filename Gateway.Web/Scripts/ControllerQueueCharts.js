$(document).ready(function () {
    displayLiveQueueChart();
});

function displayLiveQueueChart() {
    var chart = c3.generate({
        bindto: "#live-queues-chart",
        data: {
            x: "x",
            columns: [
            [
                "x", moment().add(-240, "s"), moment().add(-180, "s"), moment().add(-120, "s"), moment().add(-60, "s"), moment()],
                ["data1", 500, 500, 500, 500, 500],
                ["data2", 100, 100, 100, 100, 100],
                ["data3", 200, 200, 200, 200, 200],
                ["data4", 200, 200, 200, 200, 200],
                ["data5", 200, 200, 200, 200, 200]
            ]
        },
        axis: {
            x: {
                type: "timeseries",
                tick: {
                    format: "%H:%M:%S",
                    rotate: 90
                }
            }
        }
    });

    setInterval(function() {
        chart.flow({
            columns: [
              ["x", moment()],
              ["data1", Math.floor(Math.random() * 500)],
              ["data2", Math.floor(Math.random() * 500)],
              ["data3", Math.floor(Math.random() * 500)],
              ["data4", Math.floor(Math.random() * 500)],
              ["data5", Math.floor(Math.random() * 500)]
            ],
            length: 0
        });
    }, 60000);
}