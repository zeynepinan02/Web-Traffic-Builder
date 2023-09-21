// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//sayfa hazır olduğunda bu jquery çalışır
$(document).ready(function () {

    //sayfa ilk yüklendiğinde kayıtlı hostları getirir
    getHosts();

    const openButton = $('#openModal'); //modalın açılması için kullanılan button
    const closeButton = $('.close');
    const modal = $('#myModal');

    openButton.on("click", function () {
        modal[0].showModal();
    });

    closeButton.on("click", function () {
        modal[0].close();
    });

    $('#saveButton').click(function () {
        var thisButton = $(this);
        var host = $('#host').val();

        thisButton.attr("disabled", "true"); //bu işlemler asenkron olarak çalışıyor dolayısıyla bir kayıt işlemi için birden fazla butona basıldıysa herhangi biri bir diğerini beklemeyeceği için arka arkaya aynı insert işlemi yapılabilir. Bunu önlemek için yeni bir attiribute ekliyoruz ve butonu disable ediyoruz

        var WebSite = {
            url: host
        };

        if (isEmpty(host)) {
            alert("This field is required");
            thisButton.removeAttr("disabled");

        }
        else if (!isURL(host)) {
            alert("Please enter a valid url!");
            thisButton.removeAttr("disabled");
        }
        else {
            $.ajax({
                type: "POST",
                url: "/Home/AddHost",
                data: WebSite,
                success: function (result) {
                    thisButton.removeAttr("disabled");
                    //console.log(WebSite);
                    if (result) {
                        $('<li/>', {
                            'class': 'list-group-item',
                            'text': host
                        }).appendTo('#hostList');
                    }
                    else {
                        alert("This host already exist");
                    }
                },
                error: function (xhr, status, error) {
                    thisButton.removeAttr("disabled");
                    console.error("AJAX hatası:", status, error);
                }
            });
        }
    });
    function isEmpty(host) {
        if (host == '') {
            return true;
        }
        else {
            return false;
        }
    }
    function isURL(str) {
        // URL doğrulama için düzenli ifade
        var urlPattern = /^(https?:\/\/)?([a-z0-9\-]+\.)+[a-z]{2,6}(\/.*)?$/i;

        // URL düzenli ifadeyle eşleşiyorsa true döndürür, aksi halde false döndürür
        return urlPattern.test(str);
    }

    function getHosts() {

        var WebSite = {
            url: ""
        };

        $.ajax({
            type: "Get",
            url: "/Home/GetAllHost",
            success: function (result) {

                // Veriyi döngü ile dolaşarak, her bir öğeyi bir li (list item) olarak ekler
                result.forEach(function (item) {
                    WebSite = item;
                    $('<li/>', {
                        'class': 'list-group-item',
                        'text': WebSite.url
                    }).appendTo('#hostList');
                });
            },
            error: function (xhr, status, error) {
                console.error("AJAX hatası:", status, error);
            }
        });
    }

    $('#startTask').click(function () {
        startTask();
        setInterval(function () {
            var VisitResult = {
                Url: "",
                Size: 0,
                VisitTime: "",
                RequestTotalDuration: ""
            };
            var output = {};
            $.ajax({
                type: "Get",
                url: "/Home/GetVisit",
                success: function (result) {
                    $("#ListAtMonitor").empty();

                    // Veriyi döngü ile dolaşarak, her bir öğeyi bir li (list item) olarak ekler
                    result.forEach(function (item) {
                        VisitResult = JSON.parse(item);
                        output = "Host: " + VisitResult.Url + "| Size: " +
                            VisitResult.Size + "| Time: " + VisitResult.VisitTime +
                            "| Request Total Duration: " + VisitResult.RequestTotalDuration;
                        $('<li/>', {
                            'class': 'list-group-item',
                            'text': output,
                            'style': 'background-color:black; color:#4fdee5; padding: 10px 20px; font-family:"Times New Roman", Times, serif'
                        }).appendTo('#ListAtMonitor');
                    });
                },
                error: function (xhr, status, error) {
                    console.error("AJAX hatası:", status, error);
                }
            });

        }, 1000);
    });

    function startTask() {
        var randomVisit_ = false;
        if ($('#randomVisit').is(":checked")) {
            randomVisit_ = true;
            
        }
        $.ajax({
            type: "Post",
            url: "/Home/StartVisit",
            data: { "randomVisit": randomVisit_ },
            success: function (result) {
                console.log("StartVisit success");
            },
            error: function (xhr, status, error) {
                console.error("AJAX hatası:", status, error);
            }
        });

    }
});
