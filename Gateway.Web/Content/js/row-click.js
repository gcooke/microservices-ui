$(function () {
    $('.table tr[data-href]').each(function () {
        $(this).css('cursor', 'pointer').hover(
            function () {
                $(this).addClass('active');
            },
            function () {
                $(this).removeClass('active');
            }).click(function () {
                document.location = $(this).attr('data-href');
            }
        );
    });
});


$(function () {
    $('.table td[data-href]').each(function () {
        $(this).css('cursor', 'pointer').hover(
            function () {
                $(this).addClass('active');
            },
            function () {
                $(this).removeClass('active');
            }).click(function () {
                document.location = $(this).attr('data-href');
            }
            );
    });
});


function popup(url, controllerName, maxPriority) {
    myWindow = window.open(url, controllerName, "dialogHeight:600px;dialogHeight:400px;dialogWidth:200px;dialogTop:300px;dialogLeft:200px;edge:Raised;center:Yes;help:No;Resiable:No;Status:No;");
    // Resizes the new window
    myWindow.resizeTo(550, 220 + (maxPriority * 38));
    myWindow.focus();
}