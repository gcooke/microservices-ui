$(document).ready(function () {
    if (!hasStorageCapabilities()) {
        $(".selection-filter").hide();
        return;
    }

    renderFilters();

    var addFilterButton = $(".selection-filter-label");
    var clearFiltersButton = $("#selection-filter-clear");

    addFilterButton.on("click", function () {
        addFilter($(this));
    });

    clearFiltersButton.on("click", function () {
        clearFilters();
    });
});

function addFilter(element) {
    var addedClass = "label-success";
    var defaultClass = "label-info";
    var value = element.data("value").toLowerCase();
    var selectedFilters = getFilters();

    var index = selectedFilters.indexOf(value);

    if (index > -1) {
        element.removeClass(addedClass);
        element.addClass(defaultClass);
        selectedFilters.splice(index, 1);
        persistFilters(selectedFilters);
        renderFilters();
        return;
    }

    if (selectedFilters.length < 5) {
        element.removeClass(defaultClass);
        element.addClass(addedClass);
        selectedFilters.push(value);
        persistFilters(selectedFilters);
        renderFilters();
        return;
    }
}

function clearFilters() {
    persistFilters([]);
    renderFilters();
}

function persistFilters(filters) {
    localStorage.setItem(getLocalStorageKey(), JSON.stringify(filters));
    $(document).trigger("selectionFiltersChanged", [getLocalStorageKey()]);
}

function getFilters() {
    return JSON.parse(localStorage.getItem(getLocalStorageKey())) || [];
}

function renderFilters() {
    var filters = getFilters();

    if (filters.length === 0) {
        $("#selection-filter-viewing-label").html("No custom queues selected");
    } else {
        $("#selection-filter-viewing-label").html("Viewing ");
    }

    for (var i = 0; i < filters.length; i++) {
        $("#selection-filter-" + i).html(filters[i]);
    }

    for (var j = filters.length; j < 5; j++) {
        $("#selection-filter-" + j).html("");
    }

    var filterButtons = $(".selection-filter-label");
    filterButtons.each(function () {
        var value = $(this).data("value").toLowerCase();
        var index = filters.indexOf(value);

        if (index > -1) {
            $(this).removeClass("label-info");
            $(this).addClass("label-success");
        } else {
            $(this).addClass("label-info");
            $(this).removeClass("label-success");
        }
    });

}

function getLocalStorageKey() {
    return $(".selection-filter-main").first().data("storage-key");
}

function hasStorageCapabilities() {
    return typeof (Storage) !== "undefined";
}

function getFiltersQueryStringParameters(localStorageKey, parameterName) {
    var items = JSON.parse(localStorage.getItem(localStorageKey)) || [];
    var queryStringParameters = "";
    for (var i = 0; i < items.length; i++) {
        queryStringParameters += parameterName + "=" + items[i] + "&";
    }
    return queryStringParameters.substring(0, queryStringParameters.length - 1);
}
