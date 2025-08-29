var attr = [
    "date_onset",
    "sex",
    "age",
    "county",
    "township",
    "village",
    "countyID",
    "townshipID",
    "villageID",
    "enumID",
    "enum_lat",
    "enum_long",
    "county_infect",
    "township_infect",
    "village_infect",
    "county_infectID",
    "township_infectID",
    "village_infectID",
    "isImport",
    "country_infected",
    "num_cases",
    "type"
];

var lookup_sex = {
    F: "Female",
    M: "Male"
}

var lookup_age = {
    "0": "0-4",
    "5": "5-9",
    "10": "10-14",
    "15": "15-19",
    "20": "20-24",
    "25": "25-29",
    "30": "30-34",
    "35": "35-39",
    "40": "40-44",
    "45": "45-49",
    "50": "50-54",
    "55": "55-59",
    "60": "60-64",
    "65": "65-69",
    "70": "70+"
}

// parameters
var selectedTimeParam = {
    margin: {
        top: 10,
        right: 10,
        bottom: 20,
        left: 10
    },
    height: 100,
    width: 650
}

var fullTimeParam = {
    margin: {
        top: 5,
        right: 10,
        bottom: 5,
        left: 10
    },
    height: 50,
    width: 650
}
var realWidth = fullTimeParam.width - fullTimeParam.margin.left - fullTimeParam.margin.right;
var realHeight = fullTimeParam.height - fullTimeParam.margin.top - fullTimeParam.margin.bottom;
var width = selectedTimeParam.width - selectedTimeParam.margin.left - selectedTimeParam.margin.right;

var brush_range = {
    left: 0,
    right: realWidth
};
var brush_head = 0;
var brush_tail = realWidth;
var brush_center = 0;
var pointer_pos = width / 2;

var brush_ready = false;
var left_resize_ready = false;
var right_resize_ready = false;
var pointer_ready = false;

var currentMousePos = {
    x: -1,
    y: -1
};
var anchorMousePos = {
    x: -1,
    y: -1
};

var color = {
    sex: {
        F: "rgb(232, 68, 68)", 
        M: "rgb(77, 93, 233)"
    },
    age: {
        "0": "rgb(244,233,165)",
        "5": "rgb(236, 209, 127)",
        "10": "rgb(239, 187, 117)",
        "15": "rgb(238,157,90)",
        "20": "rgb(235, 135, 80)",
        "25": "rgb(230, 126, 51)",
        "30": "rgb(224, 115, 38)",
        "35": "rgb(216,108,49)",
        "40": "rgb(200,94,46)",
        "45": "rgb(185,80,42)",
        "50": "rgb(171,73,38)",
        "55": "rgb(161,66,35)",
        "60": "rgb(140,56,27)",
        "65": "rgb(120,48,19)",
        "70": "rgb(107,41,12)"
    },
    type: {
        None: "rgb(150, 150, 150)",
        DENV1: "rgb(255, 152, 41)",
        DENV2: "rgb(60, 255, 73)",
        DENV3: "rgb(83, 103, 255)",
        DENV4: "rgb(255, 65, 185)"
    }
}

const communicate_day = 5;
const last_day = 30;

// get user variables
var sexFilter = [];
var ageFilter = [];
var typeFilter = [];
var dateRange = Array<Date>(2);
var days = 0;
var startDate, endDate, currentDate;

var data_filtered = [];
var stack = [];
var case_list = [];

// initialize
var selectedTimeSVG = d3.select("#scaled-time")
                      .append("svg")
                      .attr("width", selectedTimeParam.width)
                      .attr("height", selectedTimeParam.height)
                      .append("g")
                      .attr("class", "main")
                      .attr("transform", "translate(" + selectedTimeParam.margin.left + "," + selectedTimeParam.margin.top + ")");

const map = L.map('map').setView([23.7, 121.0], 7);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 14,
    minZoom: 7,
    attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors'
}).addTo(map);
map.setMaxBounds(map.getBounds().pad(0.2));

//import csv
var data = [];
d3.csv("Dengue_Daily_EN.csv", function(row) {
    // process age group
    // convert them to the multiple of 5 (0, 5, 10, ...)
    var ageGroup = row["Age_Group"];
    var age = 0;
    if (ageGroup.length != 1) {
        var index = ageGroup.indexOf("-");
        if (index == -1) {
            age = +ageGroup.substring(0, 2);
        }
        else {
            age = +ageGroup.substring(0, index);
        }
    }
    // output object
    return {
        "date_onset": new Date(row["Date_Onset"].replaceAll("/", "-")),
        "sex": row["Sex"],
        "age": age,
        "county": row["MOI_County_living"],
        "township": row["MOI_Township_living"],
        "village": row["Village_Living"],
        "countyID": row["MOI_County_living_Code"],
        "TownshipID": row["MOI_Township_living_Code"],
        "VillageID": row["Village_Living_Code"],
        "enumID": row["Enumeration_unit"],
        "enum_lat": +row["Enumeration_unit_lat"],
        "enum_long": +row["Enumeration_unit_long"],
        "county_infect": row["MOI_County_infected"],
        "township_infect": row["MOI_Township_infected"],
        "village_infect": row["Village_infected"],
        "county_infectID": row["MOI_County_infected_Code"],
        "township_infectID": row["MOI_Township_infected_Code"],
        "isImport": row["Imported"],
        "country_infected": row["Country_infected"],
        "num_cases": +row["Number_of_confirmed_cases"],
        "type": row["Serotype"]
    };
}).then(function(arr) {
    // store data
    for (var row of arr) {
        data.push(row);
    }

    // clear loader
    $("#load").remove();

    // bind event listener
    $("#interface").on("mousemove", function(event) {
        currentMousePos.x = event.pageX;
        currentMousePos.y = event.pageY;
    });
    $(window).on("mouseup", function() {
        brush_ready = false;
        left_resize_ready = false;
        right_resize_ready = false;
        pointer_ready = false;
    });
    $(window).on("mousemove", function() {
        if (left_resize_ready) {
            var diff = currentMousePos.x - anchorMousePos.x;
            // check if out of range
            if (brush_head + diff < brush_range.left) {
                diff = brush_range.left - brush_head;
            }
            if (brush_head + diff > brush_tail - 15) {
                    diff = brush_tail - brush_head - 15;
            }
            // move brush
            $("#brush-left").css("left", "+=" + diff);
            $("#brush").css("left", "+=" + diff / 2).css("width", "-=" + diff);
            brush_center += diff / 2;
            brush_head += diff;
            anchorMousePos.x = currentMousePos.x;
            anchorMousePos.y = currentMousePos.y;
        }
        if (right_resize_ready) {
            var diff = currentMousePos.x - anchorMousePos.x;
            // check if out of range
            if (brush_tail + diff < brush_head + 15) {
                diff = brush_head + 15 - brush_tail;
            }
            if (brush_tail + diff > brush_range.right) {
                    diff = brush_range.right - brush_tail;
            }
            // move brush
            $("#brush-right").css("left", "+=" + diff);
            $("#brush").css("left", "+=" + diff / 2).css("width", "+=" + diff);
            brush_center += diff / 2;
            brush_tail += diff;
            anchorMousePos.x = currentMousePos.x;
            anchorMousePos.y = currentMousePos.y;
        }
        if (brush_ready) {
            var diff = currentMousePos.x - anchorMousePos.x;
            // check if out of range
            if (brush_head + diff < brush_range.left) {
                diff = brush_range.left - brush_head;
            }
            if (brush_tail + diff > brush_range.right) {
                diff = brush_range.right - brush_tail;
            }
            // move brush
            $("#brush").css("left", "+=" + diff);
            $("#brush-left").css("left", "+=" + diff);
            $("#brush-right").css("left", "+=" + diff);
            brush_center += diff;
            brush_head += diff;
            brush_tail += diff;
            anchorMousePos.x = currentMousePos.x;
            anchorMousePos.y = currentMousePos.y;
        }
        if (pointer_ready) {
            var diff = currentMousePos.x - anchorMousePos.x;
            // check if out of range
            if (pointer_pos + diff > width) {
                diff = width - pointer_pos;
            }
            if (pointer_pos + diff < 0) {
                diff = -pointer_pos;
            }
            // move pointer
            $("#pointer").css("left", "+=" + diff);
            $("#pointer-line").css("left", "+=" + diff);
            pointer_pos += diff;
            anchorMousePos.x = currentMousePos.x;
            anchorMousePos.y = currentMousePos.y;
            renderMap();
        }

        if (left_resize_ready || right_resize_ready || brush_ready) {
            $("#pointer").css("left", "+=" + (width / 2 - pointer_pos));
            pointer_pos = width / 2;
            renderRiver();
        }
    })
    
    // initial render
    renderHorizon();
})

// call render when value changes
$("#data-settings").on("click", function() {
    renderHorizon();
})
$("#time-settings").on("click", function() {
    renderHorizon();
})

function renderHorizon() {
    sexFilter = [];
    ageFilter = [];
    typeFilter = [];
    case_list = [];
    $("#sex").find("input:checked").each(function() {
        sexFilter.push($(this).val());
    })
    $("#age").find("input:checked").each(function() {
        ageFilter.push(+$(this).val());
    })
    $("#type").find("input:checked").each(function() {
        typeFilter.push($(this).val());
    })

    // filter data
    data_filtered = [];
    for (var d of data) {
        if (sexFilter.includes(d["sex"]) && ageFilter.includes(d["age"]) && typeFilter.includes(d["type"])) {
            data_filtered.push(d);
        }
    }
    
    // draw horizon chart for full time scale
    var colors = ["#313695", "#4575b4", "#74add1", "#abd9e9", "#fee090", "#fdae61", "#f46d43", "#d73027"]
    // rearrange data
    dateRange = d3.extent(data_filtered, d => d["date_onset"]);
    days = (dateRange[1].getTime() - dateRange[0].getTime()) / (1000 * 3600 * 24);
    var series = Array(days).fill(0);
    // counter
    for (var d of data_filtered) {
        // find index
        var targetDate = d["date_onset"];
        var index = (targetDate.getTime() - dateRange[0].getTime()) / (1000 * 3600 * 24);
        series[index]++;
    }

    var horizon = d3.horizonChart()
                    .height(realHeight)
                    .step(realWidth / days)
                    .colors(colors);

    // clear visualization
    $(".horizon").remove();
    $(".spawn").remove();
    $(".dot").remove();
    $("#full-time").find("svg").remove();

    // draw
    var canva = d3.select("#full-time")
                  .selectAll("_horizon_")
                  .data([series])
                  .enter()
                  .append("div")
                  .attr("class", "horizon")
                  .each(horizon);
    
    // create brush
    canva.append("div")
    .attr("id", "brush");

    $("#brush").css("width", realWidth + "px")
               .css("height", (realHeight + 10) + "px")
               .css("top", "-=5")
               // move the brush back
               .css("left", "+=" + brush_center)
               .css("width", "-=" + ((brush_range.right - brush_range.left) - (brush_tail - brush_head)))
               .on({
                   mousedown: function() {
                       brush_ready = true;
                       anchorMousePos.x = currentMousePos.x;
                       anchorMousePos.y = currentMousePos.y;
                   }
               })
               .after("<div id='brush-left'></div><div id='brush-right'></div>");
               
    // add resizer
    $("#brush-left").css("top", "-=5")
                    // move the resizer back
                    .css("left", "+=" + (brush_center - (brush_head + brush_tail) / 2 + brush_head))
                    .on({
                        mousedown: function() {
                            left_resize_ready = true;
                            anchorMousePos.x = currentMousePos.x;
                            anchorMousePos.y = currentMousePos.y;
                        }
                    });
    $("#brush-right").css("top", "-=5")
                    // move the resizer back
                    .css("left", "+=" + (brush_center - (brush_head + brush_tail) / 2 + brush_tail))
                    .on({
                        mousedown: function() {
                            right_resize_ready = true;
                            anchorMousePos.x = currentMousePos.x;
                            anchorMousePos.y = currentMousePos.y;
                        }
                    })
                    // not the best way to draw axis, but d3 scale seems to be broken
                    .after("\
                            <div id='full-axis'>\
                                <table style='width:100%'>\
                                    <tbody style='font-size:12px; color:rgb(199, 199, 199); font-weight: 700'>\
                                        <tr>\
                                            <td>1999</td>\
                                            <td>2003</td>\
                                            <td>2007</td>\
                                            <td>2011</td>\
                                            <td>2015</td>\
                                            <td>2019</td>\
                                            <td>2023</td>\
                                        </tr>\
                                    </tbody>\
                                </table>\
                            </div>\
                            ");
    
    $("#full-axis").css("width", realWidth + "px")
                   .css("top", "+=26");
    
    // call render theme river
    renderRiver();
}

function renderRiver() {
    // draw theme river (bar chart) for selected time
    // find date range
    var startDate_relative = brush_head / brush_range.right;
    startDate = new Date((dateRange[1].getTime() - dateRange[0].getTime()) * startDate_relative + dateRange[0].getTime());
    startDate = new Date(startDate.getTime() - startDate.getTime() % 86400000);
    var endDate_relative = brush_tail / brush_range.right;
    endDate = new Date((dateRange[1].getTime() - dateRange[0].getTime()) * endDate_relative + dateRange[0].getTime());
    endDate = new Date(endDate.getTime() - endDate.getTime() % 86400000);
    var dateDiff = (endDate - startDate) / 86400000;

    // group data
    var group = $("#group").find("input:checked").val();
    stack = [];
    // store date
    for (var i = 0; i < dateDiff; i++) {
        if (group == "sex") {
            stack.push({
                date: new Date(startDate.getTime() + i * 24 * 60 * 60 * 1000),
                M: 0,
                F: 0,
                casesID: []
            });
        }
        else if (group == "age") {
            stack.push({
                date: new Date(startDate.getTime() + i * 24 * 60 * 60 * 1000),
                "0": 0,
                "5": 0,
                "10": 0,
                "15": 0,
                "20": 0,
                "25": 0,
                "30": 0,
                "35": 0,
                "40": 0,
                "45": 0,
                "50": 0,
                "55": 0,
                "60": 0,
                "65": 0,
                "70": 0,
                casesID: []
            });
        }
        else if (group == "type") {
            stack.push({
                date: new Date(startDate.getTime() + i * 24 * 60 * 60 * 1000),
                None: 0,
                DENV1: 0,
                DENV2: 0,
                DENV3: 0,
                DENV4: 0,
                casesID: []
            });
        }
    }
    var n = 0;
    for (var d of data_filtered) {
        // check if in range
        if (startDate.getTime() <= d["date_onset"].getTime() && d["date_onset"].getTime() <= endDate.getTime()) {
            var ID = (d["date_onset"] - startDate) / 86400000;
            if (stack[ID] !== undefined) {
                stack[ID][d[group]]++;
                // store the index(filtered data) of the full info of the patient for later use
                stack[ID].casesID.push(n);
            }
        }
        n++;
    }
    
    // create stack
    // get keys
    var keys = Object.keys(stack[0])
    keys.splice(keys.indexOf("date"), 1);
    keys.splice(keys.indexOf("casesID"), 1);
    var series = d3.stack()
                   .keys(keys)
                   .order(d3.stackOrderNone)
                   .offset(d3.stackOffsetSilhouette)
                   (stack);
    
    // create axis
    var height = selectedTimeParam.height - selectedTimeParam.margin.bottom - selectedTimeParam.margin.top;
    var x = d3.scaleTime()
              .domain([startDate, endDate])
              .range([0, width]);
    
    // find max
    var maxVal = d3.max(series[series.length - 1], d => d[1]) / 2 * 1.5;
    var y = d3.scaleLinear()
              .domain([-maxVal, maxVal])
              .range([height, 0]);
    
    //console.log(series)
              
    // create river
    var area = d3.area()
                 .x(function(d) { return x(d.data.date); })
                 .y0(function(d) { return y(d[0]); })
                 .y1(function(d) { return y(d[1]); });

    // clear visualization
    $(".area").remove();
    $(".axis").remove();
    $(".tooltip").remove();
    $("#pointer").remove();
    $("#pointer-line").remove();
    
    // render x axis
    selectedTimeSVG.append("g")
                   .attr("transform", "translate(0," + height + ")")
                   .attr("class", "axis")
                   .call(d3.axisBottom(x).tickSize(-height))
                   .select(".domain").remove();
    selectedTimeSVG.selectAll(".tick line").attr("stroke", "#b8b8b8");

    // create a tooltip
    var Tooltip = selectedTimeSVG.append("text")
                                 .attr("x", 0)
                                 .attr("y", 0)
                                 .attr("class", "tooltip")
                                 .style("opacity", 0)
                                 .style("font-size", 17);

    // Three function that change the tooltip when user hover / move / leave a cell
    var mouseover = function(d, i) {
        Tooltip.style("opacity", 1);
        d3.selectAll(".area").style("opacity", .5);
        d3.select(this)
            .style("stroke", "black")
            .style("opacity", 1);

        $(".dot").css("opacity", "0.1");
        $(".type-" + i.key).css("opacity", "1");
    }
    var mousemove = function(d, i) {
        if (group == "sex") {
            Tooltip.text(lookup_sex[i.key]);
        }
        else if (group == "age") {
            Tooltip.text(lookup_age[i.key]);
        }
        else {
            Tooltip.text(i.key);
        }
    }
    var mouseleave = function(d) {
        Tooltip.style("opacity", 0)
        d3.selectAll(".area").style("opacity", 1).style("stroke", "none")
        $(".dot").css("opacity", "1");
    }

    // render
    selectedTimeSVG.selectAll("_layer_")
                   .data(series)
                   .enter()
                   .append("path")
                   .attr("class", "area")
                   .style("fill", function(d) { return color[group][d.key] })
                   .attr("d", area)
                   .on("mouseover", mouseover)
                   .on("mousemove", mousemove)
                   .on("mouseleave", mouseleave);
    
    // add a control bar
    $("#scaled-time").append("<div id='pointer'></div><div id='pointer-line'></div>")
    $("#pointer").css("top", "-=10px")
                 .css("left", "+=" + (pointer_pos - width / 2))
                 .on("mousedown", function() {
                    pointer_ready = true;
                    anchorMousePos.x = currentMousePos.x;
                    anchorMousePos.y = currentMousePos.y;
                 });
    $("#pointer-line").css("top", "-=10px")
                      .css("left", "+=" + (pointer_pos - width / 2));

    $("#scaled-time").find("svg").on({
        mouseenter: function() {
            $("#pointer").css("opacity", "1");
        },
        mouseleave: function() {
            $("#pointer").css("opacity", "0");
            $("#pointer:hover").css("opacity", "1");
        }
    })

    // render map
    renderMap();
}

function renderMap() {
    // get date
    var ratio = pointer_pos / width;
    currentDate = new Date((endDate.getTime() - startDate.getTime()) * ratio + startDate.getTime());
    currentDate = new Date(currentDate.getTime() - currentDate.getTime() % 86400000);
    
    $("#display-date").html(currentDate.toLocaleDateString('en-ZA'));

    // get case id
    var ID = (currentDate - startDate) / 86400000;
    var newCaseList = [];
    // get all caseID within the period
    for (var i = 0; i < last_day; i++) {
        if (ID < 0) {
            break;
        }
        else {
            for (var j = 0; j < stack[ID].casesID.length; j++) {
                if (!case_list.includes(stack[ID].casesID[j])) {
                    newCaseList.push(stack[ID].casesID[j]);
                    case_list.push(stack[ID].casesID[j]);
                }
            }
        }
    }

    // plot points
    $(".dot").remove();
    for (var id of case_list) {
        var long = data_filtered[id]["enum_long"];
        var lat = data_filtered[id]["enum_lat"];
        var onset = data_filtered[id]["date_onset"];
        var group = $("#group").find("input:checked").val();
        var type = data_filtered[id][group];
        var isNew = newCaseList.includes(id);

        // remove if too old or not yet happened
        if (onset.getTime() > currentDate.getTime() || (currentDate.getTime() - onset.getTime()) / 86400000 > last_day || isNaN(long)) {
            case_list.splice(case_list.indexOf(id), 1);
        }
        else {
            drawPoint(long, lat, onset, group, type, isNew);
        }
    }

    $('.spawn').each(function() {
        fadeout($(this));
    });
}

/**
 * Draw a point on the map
 * @param {Number} long 
 * @param {Number} lat 
 * @param {Date} onset 
 * @param {String} group
 * @param {String} type
 * @param {Boolean} newPoint
 */
function drawPoint(long, lat, onset, group, type, newPoint) {
    var day = (currentDate.getTime() - onset.getTime()) / 86400000;
    var opacity = (day <= communicate_day) ? 0.8 : (1 - (day - communicate_day) / (last_day - communicate_day)) * 0.8;
    L.circleMarker([lat, long], {
        radius: 6,
        stroke: true,
        weight: 1,
        color: "#555555",
        opacity: opacity,
        fillColor: color[group][type],
        fillOpacity: opacity,
        className: 'dot type-' + type
    }).addTo(map);

    if (newPoint) {
        L.circleMarker([lat, long], {
            radius: 8,
            stroke: 0,
            fillColor: '#ffffff',
            fillOpacity: 1,
            className: 'spawn',
        }).addTo(map);
    }
}

/**
 * 
 * @param {JQuery<any>} target 
 */
async function fadeout(target) {
    setTimeout(() => {
        target.css("opacity", "0");
        setTimeout(() => {
            target.remove();
        }, 1000)
    }, 50);
}