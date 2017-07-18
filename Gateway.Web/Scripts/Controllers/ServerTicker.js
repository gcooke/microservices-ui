// A simple templating method for replacing placeholders enclosed in curly braces.
if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

$(function () {
    var ticker = $.connection.serverInfoTicker,
        $serverTable = $('#serverTable'),
        rowTemplate = "<center><div class='widget'>" +
                        "<div class='content alert alert-{Status}'>" +
                            "<span class='glyphicon glyphicon-hdd'>" +
                                "<span class='h3'>&nbsp;</span>" +
                                "<span class='h3 text-uppercase'>{Node}</span>" +
                            "</span>" +
                        "</div>" +
                        "<div class='content'style='margin-bottom: 20px; margin-top: 20px;'>" +
                           "<div class='glyphicon glyphicon-list-alt'>&nbsp;Workers:&nbsp;<span class='server-info-label'>{Workers}</span>&nbsp;</div>" +
                           "<div class='glyphicon glyphicon-dashboard'>&nbsp;Queues:&nbsp;<span class='server-info-label'>{Queues}</span>&nbsp;</div><br/>" +
                           "<div class='glyphicon glyphicon-th'>&nbsp;CPU:&nbsp;<span class='server-info-label'>{Cpu}</span>&nbsp;</div>" +
                           "<div class='glyphicon glyphicon-tasks'>&nbsp;Memory:&nbsp;<span class='server-info-label'>{Memory}</span>&nbsp;</div>" +
                         "</div>" +
                        "<div class='content alert alert-{Status}'style='margin-bottom: 0px; margin-top: 0px;;'>" +
                            "<span class='h6'><b>{Output}</b></span>" +
                        "</div>" +
                    "</div></center>";

    function formatServer(server) {
        server.Status =
            server.Status !== null &&
            server.Status.toLowerCase() === "passing" ?
            "success" : "warning";
        return server;
    }

    function init() {
        ticker.server.getAllServers().done(function (servers) {
            $serverTable.empty();
            $.each(servers, function () {
                var server = formatServer(this);
                $serverTable.append(rowTemplate.supplant(server));
            });
        });
    }

    // Add a client-side hub method that the server will call
    ticker.client.updateServerInfo = function (server) {
        var displayStock = formatServer(server),
            $row = $(rowTemplate.supplant(displayStock));

        var target = $serverTable.find('div[data-symbol=' + server.Node + ']');
        if (!target) {
            $serverTable.append($row);
        } else {
            target.replaceWith($row);
        }
    }

    // Start the connection
    $.connection.hub.start().done(init);
});