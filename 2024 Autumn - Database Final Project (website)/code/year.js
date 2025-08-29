$("#type-option").on("change", function () {
    var type = $(this).val();
    var url = "year_query.php?m=getYears&type=" + encodeURIComponent(type);

    $.ajax({
        url: url,
        method: "GET",
        dataType: "json",
        success: function (data) {
            $("#year-option").empty().append('<option value="0">Select Year</option>');
            if (data.length > 0) {
                $.each(data, function (index, year) {
                    $("#year-option").append('<option value="' + year + '">' + year + '</option>');
                });
            } else {
                $("#year-option").append('<option value="0">No Available Years</option>');
            }

            $("#sport-option").empty().append('<option value="0">Select Sport</option>');
        },
        error: function () {
            alert("Failed to load years. Please check the server.");
        }
    });
});

$("#year-option").on("change", function () {
    var year = $(this).val();
    var type = $("#type-option").val();
    $("#year-option-default").remove();

    if (year !== "0" && type !== "0") {
        var url = "year_query.php?m=getSports&type=" + encodeURIComponent(type) + "&year=" + year;

        $.ajax({
            url: url,
            method: "GET",
            dataType: "json",
            success: function (data) {
                $("#sport-option").empty().append('<option value="0">Select Sport</option>');
                if (data.length > 0) {
                    $.each(data, function (index, sport) {
                        $("#sport-option").append('<option value="' + sport + '">' + sport + '</option>');
                    });
                } else {
                    $("#sport-option").append('<option value="0">No Available Sports</option>');
                }
            },
            error: function () {
                alert("Failed to load sports. Please check the server.");
            }
        });
    }
});

$("#sport-option").on("change", function () {
    $("#sport-option-default").remove();
})

$("#submit").on("click", function (event) {
    event.preventDefault();
    var type = $("#type-option").val();
    var year = $("#year-option").val();
    var sport = $("#sport-option").val();

    if (type !== "0" && year !== "0" && sport !== "0") {
        var queryUrl = "year_query.php?m=getResults&type=" + encodeURIComponent(type) + "&year=" + year + "&sport=" + encodeURIComponent(sport);

        $("#table-content").load(queryUrl, function (response, status, xhr) {
            if (status === "error") {
                alert("Error loading results: " + xhr.status + " " + xhr.statusText);
            }
        });
        $("#default").remove();
    } else {
        alert("Please select valid options for type, year, and sport.");
    }
});

$("#table").on("click", ".individual-td", function() {
    if ($(this).attr("data-value") != "") {
        $(this).addClass("select");
        // get player id
        var playerId = $(this).attr("data-value");

        // show profile animation
        $("#profile").css("height", "100%").css("opacity", "100%");
        $("#profile-content").css("marginTop", "100px").css("marginBottom", "50px").css("opacity", "100%");

        // load player profile from server
        $("#profile-req").load("player_query.php?m=profile&p=" + playerId);
    }
})

// close profile
$("body").on("click", "#profile", function(event) {
    if (!$(event.target).parents().is("#profile")) {
        // unmark as selected
        $(".select").removeClass("select");

        $("#profile").css("height", "0").css("opacity", "0");
        $("#profile-content").css("marginTop", "130px").css("marginBottom", "20px").css("opacity", "0");
        // clear profile content
        $("#profile-req").html("");
    }
})