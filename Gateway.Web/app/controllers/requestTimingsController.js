app.controller("requestTimingsController", function ($scope, requestTimingsService) {
    $scope.gridOptions = [];
    $scope.dataLoading = true;

    $scope.GetRequestTree = function () {
        $scope.gridOptions = {
            enableSorting: false,
            enableFiltering: false,
            showTreeExpandNoChildren: false,
            //paginationPageSizes: [5, 10],
            //paginationPageSize: 5,
            columnDefs: [
                { name: "Controller" },
                { name: "StartUtc", cellFilter: "date:\'yyyy-MM-dd HH:mm:ss\'" },
                { name: "EndUtc", cellFilter: "date:\'yyyy-MM-dd HH:mm:ss\'" },
                { name: "Size", cellFilter: "nullFilter", cellTemplate: "<div>{{row.entity.Size + ' ' + row.entity.SizeUnit }}</div>" },
                { name : "QueueTimeMs" },
                { name : "ProcessingTimeMs" },
                { name: "TotalTimeMs" }
            ]
        };

        var id = "00000000-0000-0000-0000-000000000000";

        var writeoutNode = function (childArray, currentLevel, dataArray) {
            childArray.forEach(function(childNode) {
                console.log(childNode);
                if (childNode.ChildRequests !== null  && childNode.ChildRequests !== undefined) {
                    if (childNode.ChildRequests.length > 0) {
                        childNode.$$treeLevel = currentLevel;
                        id = childNode.CorrelationId;
                        if (childNode.CorrelationId === childNode.ParentCorrelationId) {
                            childNode.ParentCorrelationId = "";
                        }
                    }
                } else {
                    if ((id !== childNode.ParentCorrelationId) || (childNode.CorrelationId === childNode.ParentCorrelationId)) {
                        if (childNode.CorrelationId === childNode.ParentCorrelationId) {
                            childNode.ParentCorrelationId = "";
                        }
                    childNode.$$treeLevel = currentLevel;
                    }
                }
                dataArray.push(childNode);
                if (childNode.ChildRequests !== null && childNode.ChildRequests !== undefined)
                    writeoutNode(childNode.ChildRequests, currentLevel +1, dataArray);
                });
            };

        var loadGrid = function(responseData) {
            var array = new Array();
            console.log("response data", responseData);
            array.push(responseData);
            var jsonArray = JSON.parse(JSON.stringify(array));
            writeoutNode(jsonArray, 0, $scope.gridOptions.data);
        };

        requestTimingsService.GetRequestTree().then(
            function (response) {
                $scope.gridOptions.data =[];
                if (response.data !== "") 
                    loadGrid(response.data);
                $scope.dataLoading = false;
            },
            function(error) {
                //TODO: Do more
                $scope.dataLoading = false;
                console.log("Error: " +error);
            });
        }

    $scope.dataLoading = true;
    $scope.GetRequestTree();
});