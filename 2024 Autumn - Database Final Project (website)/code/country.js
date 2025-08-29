$("#submit").on("click", function(event) {
    event.preventDefault();
    
    var option = $("#country-sort-option").val();  
    var order = $("#country-sort-order").val();    
    
    var url = "country_query.php?m=search&sort=" + option + "&order=" + order;
    console.log("Request URL: " + url); 

    $("#table-content").load(url);

    $("#country-sort-option").val(option);
    $("#country-sort-order").val(order);
    $("#default").remove();  
});

$(window).on("load", function() {
    $("#table-content").load("country_query.php?m=search");
    $("#default").remove();
});

var players = document.getElementsByClassName("row");
var profile = document.getElementById("profile");
var profileContent = document.getElementById("profile-content");

$("#table").on("click", ".row", function(event) {
    event.currentTarget.classList.add("select");
    var country = this.dataset.value;  

    profile.style.height = "100%";
    profile.style.opacity = "100%";
    profileContent.style.marginTop = "100px";
    profileContent.style.marginBottom = "50px";
    profileContent.style.opacity = "100%";

    $("#profile-req").load("country_query.php?m=profile&c=" + country);
});

$("#profile-content").on("click", ".country-profile-collapse-btn", function() {
    var dropdown = $(this).next();
    if (dropdown.hasClass("toggle")) {
        dropdown.css("height", "0").removeClass("toggle");
        $(this).find(".collapse-btn-indicator").html("keyboard_arrow_down");
    }
    else {
        dropdown.css("height", dropdown[0].scrollHeight).addClass("toggle");
        $(this).find(".collapse-btn-indicator").html("keyboard_arrow_up");
    }
});

$("#profile-content").on("click", ".country-sport-collapse-btn", function() {
    var dropdown = $(this).next();
    var parent = $(this).parent();
    if (dropdown.hasClass("toggle")) {
        dropdown.css("height", "0").removeClass("toggle");
        parent.css("height", "-=" + dropdown[0].scrollHeight);
        $(this).find(".collapse-btn-indicator").html("keyboard_arrow_down");
    }
    else {
        dropdown.css("height", dropdown[0].scrollHeight).addClass("toggle");
        parent.css("height", "+=" + dropdown[0].scrollHeight);
        $(this).find(".collapse-btn-indicator").html("keyboard_arrow_up");
    }
});

window.onclick = function(event) {
    if (event.target == profile) {
        document.getElementsByClassName("select")[0].classList.remove("select");
        profile.style.height = "0";
        profile.style.opacity = "0";
        profileContent.style.marginTop = "130px";
        profileContent.style.marginBottom = "20px";
        profileContent.style.opacity = "0";
        document.getElementById("profile-req").innerHTML = "";
    }
};
