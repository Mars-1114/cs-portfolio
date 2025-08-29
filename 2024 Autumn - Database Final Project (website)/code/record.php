<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width">
    <title>Olympic Records</title>
    <link href="style.css" rel="stylesheet" type="text/css">
    <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Alatsi&family=Oxygen:wght@300;400;700&display=swap');
    </style>
</head>
<body>
    <div id="navBarContainer">
        <a class="menuBtn" href="index.php"><i class="material-icons home">home</i></a>
        <p class="center"><img src="olympics.png" style="width:60px; height:auto; padding-right: 10px"><b>OLYMPICS</b></p>
    </div>
    <div id="content" style="padding-top: 80px; padding-left: 20px; padding-right: 20px">
        <div style="text-align: center; font-size: 18px; padding-bottom: 20px">
            Here displays the Olympic records for all sports.
        </div>
        <div id="table">
            <table>
                <colgroup>
                    <col style="width: 25%">
                    <col style="width: 25%">
                    <col style="width: 25%">
                    <col style="width: 15%">
                    <col style="width: 10%">
                </colgroup>
                <thead>
                    <tr class="attr">
                        <th>Event</th>
                        <th>Athlete Name</th>
                        <th>Country</th>
                        <th>Grade</th>
                        <th>Date</th>
                    </tr>
                </thead>
                <tbody id="table-content">
                    <!-- 查詢結果將顯示於此 -->
                </tbody>
            </table>
            <div id="default">Loading Query...</div>
        </div>
    </div>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="record.js"></script>
</body>
</html>
