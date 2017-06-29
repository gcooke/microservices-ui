$("#loading").hide();
$(':submit').show();

$(function () {
    // When a Button is clicked on your page, disable everything and display your loading element
    $(':submit').click(function () {
        // Disable everything
        $("#loading").show();
        $(':submit').hide();
    });
});