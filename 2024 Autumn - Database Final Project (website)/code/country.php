<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width">
    <title>Country Search</title>
    <link rel="stylesheet" href="style.css">
    <!-- Import Icons -->
    <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">
    <link rel="preload" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" as="style">
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined">
    <!-- Import Fonts -->
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Alatsi&family=Oxygen:wght@300;400;700&display=swap');
    </style>
</head>
<body>
    <div id="navBarContainer">
        <a class="menuBtn" href="index.php"><i class="material-icons home">home</i></a>
        <p class="center"><img src="olympics.png" style="width:60px; height:auto; padding-right: 10px"><b>OLYMPICS</b></p>
    </div>

    <div id="content" style="padding-top: 80px; padding-left: 20px; padding-right: 20px;">
        <div style="text-align: center; font-size: 18px; padding-bottom: 20px">
            This page shows the list of all countries ever attended in Olympics.
            You can rank the list by <span class="tooltip">points<span class="tooltiptext">Golds*3+Silver*2+Bronze*1</span></span>, medal counts, or names.
            Click on the country to get its profile.
        </div>   

        <div id="form">
            <form name="form">
                <label class="oxygen-bold">Sort By</label>
                <select id="country-sort-option" class="left">
                    <option value="points">Points</option>
                    <option value="gold">Gold</option>
                    <option value="silver">Silver</option>
                    <option value="bronze">Bronze</option>
                    <option value="country">Name</option>
                </select>
                <select id="country-sort-order">
                    <option value="DESC">Descend</option>
                    <option value="ASC">Ascend</option>
                </select>
                <input type="submit" value="Go" id="submit" class="oxygen-bold">
            </form>
        </div>
        
        <!-- Table to show rankings -->
        <div id="table">
            <table>
                <colgroup>
                    <col style="width: 8%">
                    <col style="width: 32%">
                    <col style="width: 20%">
                    <col style="width: 20%">
                    <col style="width: 20%">
                </colgroup>
                <thead class="attr">
                    <tr>
                        <th>Rank</th>
                        <th>Country</th>
                        <th style="background-color: #FFE142">Gold</th>
                        <th style="background-color: #C0C0C0">Silver</th>
                        <th style="background-color: #D18B41">Bronze</th>
                    </tr>
                </thead>
                <tbody id="table-content"></tbody>
            </table>
        </div>
    </div>
    
    <div id="default">Search to see results</div>
    
    <div id="profile">
        <div id="profile-content">
            <div id="profile-req"></div>
        </div>
    </div>

    <!-- Include jQuery -->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    
    <!-- Include JavaScript file -->
    <script src="country.js"></script>
</body>
</html>
