app.factory("requestTimingsService", ["$http", function ($http) {
    var requestTimingsService = {};
    requestTimingsService.GetRequestTree = function () {
        return $http.get("/Request/GetRequestTree");
    };
    return requestTimingsService;

}]);