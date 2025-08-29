<?php
    include "database_connection.php";

    $mode = $_REQUEST["m"];
    switch ($mode) {
        case "year":
            $season = $_REQUEST["s"];
            $sql = "select year, edition_id
                    from Games
                    where edition LIKE '%$season%' and competition_date != '—'
                    order by year desc";
            $result = $conn->query($sql);
            while ($row = $result->fetch_assoc()) {
                echo "<option value=".$row["edition_id"].">".$row["year"]."</option>";
            }
            echo "<option value='new'>(Other)</option>";
            break;

        case "sport":
            $season = $_REQUEST["s"];
            $year = ($_REQUEST["y"] != "new") ? $_REQUEST["y"] : "%";
            $sql = "select distinct d.sport
                    from  Details d
                    order by d.sport";
            $result = $conn->query($sql);
            while ($row = $result->fetch_assoc()) {
                echo "<option value='".$row["sport"]."'>".$row["sport"]."</option>";
            }
            echo "<option value='new'>(Other)</option>";
            break;
        
        case "event":
            $season = $_POST["season"];
            $year = ($_POST["year"] != "new") ? $_POST["year"] : "%";
            $sport = $_POST["sport"];
            $sex_opposite = ($_POST["sex"] == "Male") ? "Women" : "Men";
            $sql = "select distinct d.event, max(result_id) as result_id
                    from Details d
                    where d.sport = '$sport'
                    and d.event not like '%, $sex_opposite'
                    group by d.event
                    order by d.event";
            $result = $conn->query($sql);
            while ($row = $result->fetch_assoc()) {
                echo "<option value='".$row["result_id"]."'>".$row["event"]."</option>";
            }
            echo "<option value='new'>(Other)</option>";
            break;
        
        case "record":
            $event = $_POST["event"];
            $sql = "SELECT Q.sport AS sport, Q.event AS event, grade
                    FROM (SELECT DISTINCT d.sport AS sport, d.event AS event, grade
                        FROM AthleteRecords a, Details d
                        WHERE a.result_id = d.result_id
                        ORDER BY sport) AS Q,
                        (SELECT DISTINCT d.sport, d.event
                        FROM Details d
                        WHERE d.result_id = '$event'
                        ORDER BY sport) AS P
                    WHERE Q.sport = P.sport AND Q.event = P.event";
            $result = $conn->query($sql);
            if ($result->num_rows > 0) {
                while ($row = $result->fetch_assoc()) {
                    echo $row["grade"];
                }
            }
            else {
                echo '0';
            }
            break;
        
        case "country":
            $query = $conn->query("SELECT DISTINCT country
                                          FROM Medal
                                          ORDER BY country ASC");
            echo '<option value="all">(All Countries)</option>';
            while ($country = $query->fetch_assoc()) {
                echo '<option value="'.$country["country"].'">'.$country["country"].'</option>';
            }
    }
    
    // close connection
    $conn->close();
?>