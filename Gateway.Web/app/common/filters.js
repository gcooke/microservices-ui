var filters = angular.module("filters", []);

filters.filter("nullDateFilter", function ($filter) {
    return function (input) {
        var originalFilter = $filter("date");
        return input == null ? "" : originalFilter(input);
    }
});

filters.filter("nullFilter", function () {
    return function (input) {
        return input == null ? "" : input;
    };
});