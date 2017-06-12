var app = angular.module("app", ["ui.grid", "ui.grid.treeView", "ui.grid.pagination"]);

app.filter("nullDateFilter", function ($filter) {
    return function (input) {
        var originalFilter = $filter("date");
        return input == null ? "" : originalFilter(input);
    }
});

app.filter("nullFilter", function () {
    return function (input) {
        console.log(input);
        return input == null ? "" : input;
    };
});