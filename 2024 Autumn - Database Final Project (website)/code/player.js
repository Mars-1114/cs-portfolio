var hover = false;
var enterEdit = false;
var mode = "";
var currentMonth;
var search = {
    name: "",
    country: "",
    sex: ""
};

// load country list
$(window).on("load", function() {
    $("#form .filter-country").load("options_query.php?m=country");
});

// search player
$("#submit").on("click", function(event) {
    // load search query
    event.preventDefault();
    var name = $("#name").val();
    var country = $("#form .filter-country").val();
    var sex = $("#form .filter-sex").val();
    if (name.trim() != "") {
        search.name = name;
        search.country = country;
        search.sex = sex;
        // uncheck edit mode
        $("#edit-enable").prop("checked", false);
        $(".edit, .delete, .new").css("opacity", "0");
        
        var url = "player_query.php?m=search";
        $("#table-content").load(url, {
            "player": name,
            "country": country,
            "sex": sex
        });
        $("#default").remove();
    }
})

// open profile
$("#table").on("click", ".row", function() {
    $(this).addClass("select");
    if (!hover) {
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

// open edit ui
$("#table").on("click", ".edit", function() {
    mode = "edit";
    var playerId = $(this).parents(".row").attr("data-value");
    $("#edit-ui").css("height", "100%").css("opacity", "100%");
    $("#edit-ui-content").css("marginTop", "100px").css("marginButtom", "50px").css("opacity", "100%");
    $("#edit-ui-content > h2").html("Edit Profile");
    // load autofill
    $("#edit-req").load("player_query.php?m=edit-ui&p=" + playerId);
})

// close edit ui
$("body").on("click", "#edit-ui", function(event) {
    if (!$(event.target).parents().is("#edit-ui")) {
        enterEdit = false;
        // unmark as selected
        $(".select").removeClass("select");

        $("#edit-ui").css("height", "0").css("opacity", "0");
        $("#edit-ui-content").css("marginTop", "130px").css("marginBottom", "20px").css("opacity", "0");
        // clear content
        $("#edit-req").html("");
    }
})

// open delete ui
$("table").on("click", ".delete", function() {
    $("#delete-ui").css("height", "100%").css("opacity", "100%");
    $("#delete-warning").css("marginTop", "250px").css("opacity", "100%");
})

// close delete ui
$("body").on("click", "#delete-ui", function(event) {
    if (!$(event.target).parents().is("#delete-ui")) {
        // unmark as selected
        $(".select").removeClass("select");

        $("#delete-ui").css("height", "0").css("opacity", "0");
        $("#delete-warning").css("marginTop", "280px").css("opacity", "0");
    }
})

// open new ui
$(".new").on("click", function() {
    mode = "new";
    $("#edit-ui").css("height", "100%").css("opacity", "100%");
    $("#edit-ui-content").css("marginTop", "100px").css("marginButtom", "50px").css("opacity", "100%");
    $("#edit-ui-content > h2").html("Create Profile");
    $("#edit-req").load("player_query.php?m=new-ui");
})

$("#cancel").on("click", function() {
    enterEdit = false;
    // unmark as selected
    $(".select").removeClass("select");
    // close animation
    $("#edit-ui").css("height", "0").css("opacity", "0");
    $("#edit-ui-content").css("marginTop", "130px").css("marginBottom", "20px").css("opacity", "0");
    // clear content
    $("#edit-req").html("");
})

$("#delete-cancel").on("click", function() {
    // unmark as selected
    $(".select").removeClass("select");

    $("#delete-ui").css("height", "0").css("opacity", "0");
    $("#delete-warning").css("marginTop", "280px").css("opacity", "0");
})

$(".slider").on("click", function() {
    if (!$("#edit-enable").prop("checked")) {
        $(".edit").css("right", "-45px").css("opacity", "100%");
        $(".delete").css("right", "-75px").css("opacity", "100%");
        $(".new").css("right", "-80px").css("opacity", "100%");
    }
    else {
        $(".edit, .delete, .new").css("right", "-1000px").css("opacity", "0");
    }
})

$("#table").on("mouseenter", ".edit, .delete", function() {
    hover = true;
})

$("#table").on("mouseleave", ".edit, .delete", function() {
    hover = false;
})

// change days of month
$("#edit-ui-content").on("click", "#edit-bmonth", function() {
    if (!enterEdit) {
        currentMonth = $("#edit-bmonth").val();
        enterEdit = true;
    }
    var year = $("#edit-byear").val();
    var month = $(this).val();
    if (currentMonth != month) {
        $("#edit-bday").html(" ");
        for (var i = 1; i <= daysInMonth(month, year); i++) {
            $("#edit-bday").append($("<option>", {
                value: i,
                text: i
            }))
        }
        currentMonth = month;
    }
})

$("#edit-ui-content").on("mousedown", function() {
    maintainInput("#edit-byear", "int");
    maintainInput("#edit-height", "int");
    maintainInput("#edit-weight", "int");
    maintainInput(".edit-event-year-textbox", "int");
    maintainInput(".edit-record", "float");
})

// send update request to database
$("#confirm").on("click", function() {
    var hasEmpty = false;

    // check event profile
    var newEvents = $(".edit-new");
    newEvents.each(function(){
        var e = $(this);
        var year = e.find(".edit-new-event-year").val();
        var sport = e.find(".edit-new-event-sport").val();
        var event = e.find(".edit-new-event-event").val();
        var yearNew = e.find(".edit-event-year-textbox").val();
        var sportNew = e.find(".edit-event-sport-textbox").val();
        var eventNew = e.find(".edit-event-event-textbox").val();
        var grade = e.find(".edit-record").val();

        var hasRecord = e.find(".edit-record-container").attr("data-value");

        if (event == "" || (year == "new" && yearNew == "") || (sport == "new" && sportNew == "") || (event == "new" && eventNew == "") || (hasRecord == "1" && grade == "")) {
            hasEmpty = true;
        }
    })
    
    // check basic profile
    var name = $("#edit-name").val();
    var id = $("#edit-req > h3").attr("data-value");
    var height = $("#edit-height").val();
    var weight = $("#edit-weight").val();
    var byear = $("#edit-byear").val();
    var bmonth = $("#edit-bmonth").val().padStart(2, "0");
    var bday = $("#edit-bday").val().padStart(2, "0");
    var noc = $("#edit-country").val();
    var sex = $("#edit-sex").val();

    if (name == "" || height == "" || weight == "" || byear == "") {
        hasEmpty = true;
    }

    if (hasEmpty) {
        alert("Some fields are empty! Fill before submit");
    }
    else {
        var success = true;
        // update basic profile
        $.ajax(
            {
                url: "player_query.php?m=edit",
                type: "POST",
                data: {
                    "target": "basic",
                    "mode": mode,
                    "id": id,
                    "name": name,
                    "sex": sex,
                    "birthday": byear + "-" + bmonth + "-" + bday,
                    "height": height,
                    "weight": weight,
                    "noc": noc
                },
                success: function(data) {
                    if (data != "1") {
                        success = false;
                    }
                }
            }
        );

        // update event profile
        newEvents.each(function(){
            if (success) {
                var e = $(this);
                var hasNew = false, hasNewYear = false, hasNewSport = false, hasNewEvent = false;
                var hasRecord = e.find(".edit-record-container").attr("data-value");
                var season = e.find(".edit-new-event-season").val();
                var year, sport, event;
                if (e.find(".edit-new-event-year").val() == "new") {
                    year = e.find(".edit-event-year-textbox").val();
                    hasNewYear = true;
                }
                else {
                    year = e.find(".edit-new-event-year").val();
                }
                if (e.find(".edit-new-event-sport").val() == "new") {
                    sport = e.find(".edit-event-sport-textbox").val();
                    hasNewSport = true;
                    hasNew = true;
                }
                else {
                    sport = e.find(".edit-new-event-sport").val();
                }
                if (e.find(".edit-new-event-event").val() == "new") {
                    event = e.find(".edit-event-event-textbox").val();
                    hasNewEvent = true;
                    hasNew = true;
                }
                else {
                    event = e.find(".edit-new-event-event").val();
                }
                var grade = e.find(".edit-record").val();
                
                $.ajax({
                    url: "player_query.php?m=edit",
                    type: "POST",
                    data: {
                        "target": "event",
                        "id": $(".select").attr("data-value"),
                        "season": season,
                        "year": year,
                        "sport": sport,
                        "event": event,
                        "grade": grade,
                        "new": hasNew,
                        "new-year": hasNewYear,
                        "new-sport": hasNewSport,
                        "new-event": hasNewEvent,
                        "rec": hasRecord,
                        "noc": noc,
                        "athlete": name,
                        "athleteID": id
                    },
                    success: function(data) {
                        //console.log(data);
                    }
                })
            }
        })

        // remove event
        $(".edit-events-remove").each(function() {
            if($(this).prop("checked")) {
                var resultID = $(this).val();
                $.ajax({
                    url: "player_query.php?m=delete",
                    type: "POST",
                    data: {
                        "target": "event",
                        "resultID": resultID,
                        "playerID": id
                    },
                    success: function(data) {
                        //console.log(data);
                    }
                });
            }
        });

        // close modal
        if (success) {
            enterEdit = false;
            // unmark as selected
            $(".select").removeClass("select");
            // close animation
            $("#edit-ui").css("height", "0").css("opacity", "0");
            $("#edit-ui-content").css("marginTop", "130px").css("marginBottom", "20px").css("opacity", "0");
            // clear content
            $("#edit-req").html("");
            
            alert("update successfully!");
            // reload search
            if (mode == "new") {
                search.name = name;
                $("#default").remove();
            }
            $("#table-content").load("player_query.php?m=search", {
                "player": search.name,
                "country": search.country,
                "sex": search.sex
            }, function() {
                $(".edit").css("right", "-45px").css("opacity", "100%");
                $(".delete").css("right", "-75px").css("opacity", "100%");
                $(".new").css("right", "-80px").css("opacity", "100%");
            });
        }
        else {
            alert("update failed");
        }
    }
})

$("#delete-confirm").on("click", function() {
    var id = $(".select").attr("data-value");
    $.ajax({
        url: "player_query.php?m=delete",
        type: "POST",
        data: {
            "target": "player",
            "id": id
        },
        success: function() {
            // unmark as selected
            $(".select").removeClass("select");
            // close animation
            $("#delete-ui").css("height", "0").css("opacity", "0");
            $("#delete-warning").css("marginTop", "280px").css("opacity", "0");

            alert("delete successfully!");
            // reload search
            $("#table-content").load("player_query.php?m=search", {
                "player": search.name,
                "country": search.country,
                "sex": search.sex
            }, function() {
                $(".edit").css("right", "-45px").css("opacity", "100%");
                $(".delete").css("right", "-75px").css("opacity", "100%");
                $(".new").css("right", "-80px").css("opacity", "100%");
            });
        }
    })
})

$("#edit-ui-content").on("click", "#edit-new-event-btn", function() {
    // backslash used for format purpose
    var rowString = "\
    <tr>\
        <td>\
            <button class='remove-new-event'>\
                <span class='material-symbols-outlined'>\
                    close\
                </span>\
            </button>\
        </td>\
        <td class='edit-new'>\
            <select class='edit-new-event-season left' style='width: 95px'>\
                <option value='' class='empty'>(Season)</option>\
                <option value='Summer'>Summer</option>\
                <option value='Winter'>Winter</option>\
            </select>\
            <span class='edit-event-container'>\
                <select class='edit-new-event-year' style='width: 85px'>\
                    <option class='empty'>(Year)</option>\
                </select>\
                <input type='text' class='edit-event-year-textbox bottom' style='width: 85px' placeholder='(Year)'>\
            </span>\
            <span class='edit-event-container'>\
                <select class='edit-new-event-sport' style='width: 200px'>\
                    <option value='' class='empty'>(Sport)</option>\
                </select>\
                <input type='text' class='edit-event-sport-textbox bottom' style='width: 200px' placeholder='(Sport)'>\
            </span>\
            <span class='edit-event-container'>\
                <select class='edit-new-event-event right' style='width: 320px'>\
                    <option value='' class='empty'>(Event)</option>\
                </select>\
                <input type='text' class='edit-event-event-textbox bottom' style='width: 320px' placeholder='(Event)'>\
            </span>\
            <span class='edit-record-container' data-value='0'>\
                Result: \
                <input type='text' class='edit-record single' style='width: 100px'>\
                <span class='edit-record-unit'></span>\
            </span>\
        </td>\
    </tr>";

    $("#edit-events-table tbody").prepend(rowString);
})

// dynamically create selection
$("#edit-ui-content").on("change", ".edit-new-event-season", function() { getYearOption($(this).parents("td")); });
$("#edit-ui-content").on("change", ".edit-new-event-year", function() { getSportOption($(this).parents("td")); });
$("#edit-ui-content").on("change", ".edit-new-event-sport", function() { getEventOption($(this).parents("td")); });
$("#edit-ui-content").on("change", ".edit-new-event-event", function() { checkNewRecord($(this).parents("td")); });

// change event option when sex is modified as well
$("#edit-ui-content").on("change", "#edit-sex", function() {
    $(".edit-new-event-event").each(function() {
        if ($(this).val() != "") {
            getEventOption($(this).parents("td"));
        }
    });
})

// FUNCTION //

/**
 * 
 * @param {JQuery} row 
 */
function getYearOption(row) {
    var season = row.find(".edit-new-event-season").val();
    row.find(".edit-new-event-season").children(".empty").remove();
    row.find(".edit-new-event-year").load("options_query.php?m=year&s=" + season, function() { getSportOption(row); });
}

/**
 * 
 * @param {JQuery} row 
 */
function getSportOption(row) {
    var season = row.find(".edit-new-event-season").val();
    var year = row.find(".edit-new-event-year").val();
    row.find(".edit-new-event-sport").load("options_query.php?m=sport&s=" + season + "&y=" + year,
        function() { getEventOption(row); });
    newInstance(row.find(".edit-new-event-year"), row.find(".edit-event-year-textbox"));
}

/**
 * 
 * @param {JQuery} row 
 */
function getEventOption(row) {
    var season = row.find(".edit-new-event-season").val();
    var year = row.find(".edit-new-event-year").val();
    var sport = row.find(".edit-new-event-sport").val();
    row.find(".edit-new-event-event").load("options_query.php?m=event", {
        "season": season,
        "year": year,
        "sport": sport,
        "sex": $("#edit-sex").val()
    }, function() { checkNewRecord(row); });
    newInstance(row.find(".edit-new-event-sport"), row.find(".edit-event-sport-textbox"));
}

/**
 * 
 * @param {JQuery} row 
 */
function checkNewRecord(row) {
    var sport = row.find(".edit-new-event-sport").val();
    var event = row.find(".edit-new-event-event").val();
    $.ajax({
        type: "POST",
        url: "options_query.php?m=record",
        data: {
            "sport": sport,
            "event": event
        },
        success: function(str) {
            if (str == '0') {
                row.find(".edit-record-container").css("display", "none").attr("data-value", "0");
            }
            else {
                // get unit
                var id = str.indexOf("(");
                var unit = str.substring(id + 1, str.length - 1);
                row.find(".edit-record-container").css("display", "block").attr("data-value", "1").find(".edit-record-unit").html(unit);
            }
            newInstance(row.find(".edit-record-container"), "");
        }
    })
    newInstance(row.find(".edit-new-event-event"), row.find(".edit-event-event-textbox"));
}

$("#edit-ui-content").on("click", ".remove-new-event", function() {
    $(this).parents("tr").remove();
})

function daysInMonth(month, year) {
    var y = Number(year);
    var m = Number(month);
    return new Date(y, m, 0).getDate();
}

// make sure that the text box is a positive number
function maintainInput(selector, mode) {
    $(selector).each(function() {
        var val = Number($(this).val());
        if (mode == "int") {
            if (isNaN(val) || val % 1 != 0 || val <= 0) {
                $(this).val("");
            }
            else {
                $(this).val(val);
            }
        }
        else if (mode == "float") {
            if (isNaN(val) || val <= 0) {
                $(this).val("");
            }
            else {
                $(this).val(val);
            }
        }
    })
}

// if a selection value=new, display the text box for user to type in the new option
// element - <jQuery selector> - the select box
/**
 * @param {JQuery<any>} checkElement 
 * @param {JQuery<any>} targetElement
 */
function newInstance(checkElement, targetElement) {
    var row = checkElement.parents("tr");
    if (targetElement != "") {
        if (checkElement.val() == "new") {
            targetElement.css("display", "block");
        }
        else {
            targetElement.css("display", "none");
        }
    }

    // check if all four possible textboxes are open
    if (row.find(".edit-new-event-year").val() == "new" || row.find(".edit-new-event-sport").val() == "new" || row.find(".edit-new-event-event").val() == "new" || row.find(".edit-record-container").attr("data-value") == "1") {
        if (row.find(".edit-record-container").attr("data-value") == "1" && row.find(".edit-new-event-year").val() == "new") {
            if (row.css("border-bottom-width") != "72px") {
                row.find(".edit-record-container").css("top", "+=32");
            }
            row.css("border-bottom", "72px solid transparent");
        }
        else {
            if (row.css("border-bottom-width") == "72px") {
                row.find(".edit-record-container").css("top", "-=32");
            }
            row.css("border-bottom", "40px solid transparent");
        }
    }
    else {
        row.css("border-bottom", "0");
    }
}