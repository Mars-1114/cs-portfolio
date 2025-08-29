<!DOCTYPE html>
<html lang="zh-TW">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Olympics</title>
    <link href="style.css" rel="stylesheet" type="text/css">
    <!-- Import Icons -->
    <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">
    <!-- Import Fonts -->
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Alatsi&family=Oxygen:wght@300;400;700&display=swap');
    </style>
</head>

<body>
    <!-- NavBar -->
    <div id="navBarContainer">
        <a class="menuBtn" href="index.php" style="width: 0; padding-left: 0; padding-right: 0; overflow: hidden"><i class="material-icons home">h</i></a>
        <p class="center"><img src="olympics.png" style="width:60px; height:auto; padding-right: 10px"><b>OLYMPICS</b></p>
    </div>

    <!-- Main Content -->
    <div id="content" style="padding-top: 80px; padding-left: 20px; padding-right: 20px">
        <div style="text-align: center; font-size: 18px; padding-bottom: 20px; width: 80%; margin: 0 auto">
            <h1>Welcome!</h1>
            <p>The Olympic Games are considered the world's foremost sports competition, 
                with more than 200 teams, representing sovereign states and territories, participating.
                This website serves the purpose of quickly search the profile, record, and medals of players and countries.
                You can also add/delete/update records for future events.
                Click on any of the button below to browse the features.
            </p>
        </div>
        <div id="form">
            <a href="player.php" class="menu-button">Athletes</a>
            <a href="year.php" class="menu-button">Games</a>
            <a href="country.php" class="menu-button">Countries</a>
            <a href="record.php" class="menu-button">Record</a>
        </div>
    </div>

    <!-- JavaScript -->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.7.1/jquery.min.js">
        // import jquery //
    </script>
    <script>
        // initialize the database if there's none
        // you can change the reset parameter to 1 to rebuild the database (will take a long time)
        $.ajax({url: "init.php?reset=0"});
    </script>
</body>
</html>
